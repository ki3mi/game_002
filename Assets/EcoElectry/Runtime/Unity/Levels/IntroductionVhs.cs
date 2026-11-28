using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace EcoElectry.Unity.Levels
{
    internal sealed class IntroductionVhs
    {
        private enum State { OnBench, PickingUp, Holding, Aligning, Inserting, Playing }

        private const float Reach = 2.25f;
        private static readonly Quaternion HoldTilt = Quaternion.Euler(-42f, -12f, -11f);
        private static readonly Color PromptSurface = new(.046f, .077f, .091f, .98f);
        private static readonly Color PromptInk = new(.89f, .92f, .85f);
        private readonly Transform cassette;
        private readonly Transform deck;
        private readonly Transform shell;
        private readonly Transform label;
        private readonly Transform title;
        private readonly Transform flap;
        private readonly Renderer screen;
        private readonly BoxCollider pickupTarget;
        private readonly Vector3 slot;
        private readonly VideoPlayer video;
        private readonly Renderer picture;
        private readonly Material pictureMaterial;
        private readonly RenderTexture videoTexture;
        private State state;
        private Transform tape;
        private Pose start;
        private float elapsed;
        private string prompt;
        private GUIStyle promptStyle;
        private bool pausedForMenu;
        internal bool Finished { get; private set; }

        internal bool TryGetTutorialTarget(out Vector3 position, out string target, out string step)
        {
            position = default;
            target = step = null;
            switch (state)
            {
                case State.OnBench:
                    position = shell.position + Vector3.up * 0.16f;
                    target = "Cinta · Encargo 01";
                    step = "Mira la cinta de la mesa y pulsa E para recogerla.";
                    return true;
                case State.Holding:
                    position = deck.TransformPoint(slot + Vector3.up * 0.18f);
                    target = "Ranura del reproductor";
                    step = "Mira la videocasetera y pulsa E para insertar la cinta.";
                    return true;
                case State.Playing when !Finished:
                    position = screen.transform.position + Vector3.up * 0.36f;
                    target = "Televisor";
                    step = "Mira el encargo. El tutorial continúa cuando termine el video.";
                    return true;
                default:
                    return false;
            }
        }

        internal static IntroductionVhs Create(Transform work)
        {
            if (work == null) return null;
            Transform cassette = work.Find("Cinta VHS del encargo");
            Transform deck = work.Find("Videocasetera VHS");
            Renderer screen = work.Find("Televisor CRT/Vidrio oscuro")?.GetComponent<Renderer>();
            Transform casing = cassette?.Find("Carcasa de cinta");
            Transform label = cassette?.Find("Etiqueta");
            Transform title = cassette?.Find("Identificación de la cinta");
            Renderer chassis = deck?.Find("Chasis")?.GetComponent<Renderer>();
            Renderer tray = deck?.Find("Bandeja de cinta")?.GetComponent<Renderer>();
            if (casing == null || label == null || title == null || chassis == null || tray == null || screen == null)
                return null;
            return new IntroductionVhs(cassette, deck, casing, label, title, chassis, tray, screen);
        }

        private IntroductionVhs(Transform cassette, Transform deck, Transform shell, Transform label,
            Transform title, Renderer chassis, Renderer tray, Renderer screen)
        {
            this.cassette = cassette;
            this.deck = deck;
            this.shell = shell;
            this.label = label;
            this.title = title;
            this.screen = screen;

            slot = tray.transform.localPosition;
            flap = PrepareDeck(chassis, tray);
            pickupTarget = cassette.gameObject.AddComponent<BoxCollider>();
            pickupTarget.center = shell.localPosition;
            pickupTarget.size = shell.localScale + new Vector3(0.025f, 0.018f, 0.025f);

            VideoClip clip = Resources.Load<VideoClip>("Level00/Encargo01");
            if (clip == null)
            {
                Debug.LogError("Falta el video Encargo01 de la sala de mantenimiento.", screen);
                return;
            }

            videoTexture = new RenderTexture(650, 440, 0, RenderTextureFormat.ARGB32) { name = "Imagen del CRT" };
            videoTexture.Create();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = videoTexture;
            GL.Clear(false, true, Color.black);
            RenderTexture.active = previous;

            GameObject display = GameObject.CreatePrimitive(PrimitiveType.Quad);
            display.name = "Imagen VHS";
            display.transform.SetParent(screen.transform, false);
            display.transform.localPosition = new Vector3(0f, 0f, -0.535f);
            display.GetComponent<Collider>().enabled = false;
            picture = display.GetComponent<Renderer>();
            picture.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            picture.receiveShadows = false;
            picture.enabled = false;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            pictureMaterial = new Material(shader);
            pictureMaterial.mainTexture = videoTexture;
            picture.sharedMaterial = pictureMaterial;

            var audio = screen.transform.parent.gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 1f;
            audio.dopplerLevel = 0f;
            audio.rolloffMode = AudioRolloffMode.Linear;
            audio.minDistance = 1.2f;
            audio.maxDistance = 8f;

            video = display.AddComponent<VideoPlayer>();
            video.playOnAwake = false;
            video.clip = clip;
            video.renderMode = VideoRenderMode.RenderTexture;
            video.targetTexture = videoTexture;
            video.aspectRatio = VideoAspectRatio.FitInside;
            video.waitForFirstFrame = true;
            video.audioOutputMode = VideoAudioOutputMode.AudioSource;
            video.controlledAudioTrackCount = (ushort)Mathf.Min(1, clip.audioTrackCount);
            if (video.controlledAudioTrackCount > 0)
            {
                video.EnableAudioTrack(0, true);
                video.SetTargetAudioSource(0, audio);
            }
            video.prepareCompleted += OnVideoPrepared;
            video.errorReceived += OnVideoError;
            video.loopPointReached += OnVideoEnded;
        }

        internal void Tick(Camera view, bool canInteract)
        {
            if (view == null) return;
            if (state == State.PickingUp)
            {
                float t = Advance(0.52f);
                MoveTape(start, HeldPose(view), t, Vector3.up * 0.13f);
                if (t >= 1f) state = State.Holding;
                return;
            }

            if (state == State.Aligning)
            {
                float t = Advance(0.50f);
                MoveTape(start, SlotPose(-0.135f), t, Vector3.up * 0.09f);
                if (t >= 1f) Begin(State.Inserting);
                return;
            }

            if (state == State.Inserting)
            {
                float t = Advance(0.55f);
                MoveTape(start, SlotPose(0.103f), t, Vector3.zero);
                float open = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.08f, 0.46f, t));
                float close = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.80f, 1f, t));
                flap.localRotation = Quaternion.Euler(-82f * open * (1f - close), 0f, 0f);
                if (t >= 1f) StartPlayback();
                return;
            }

            if (state == State.Holding)
            {
                Pose held = HeldPose(view);
                tape.SetPositionAndRotation(held.position, held.rotation);
            }

            if (!canInteract || state == State.Playing ||
                !Physics.Raycast(view.transform.position, view.transform.forward, out RaycastHit hit, Reach,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                HidePrompt();
                return;
            }

            bool take = state == State.OnBench && hit.collider == pickupTarget;
            bool insert = state == State.Holding && hit.transform.IsChildOf(deck);
            if (!take && !insert)
            {
                HidePrompt();
                return;
            }

            prompt = take ? "E  Recoger cinta VHS" : "E  Insertar cinta VHS";
            if (Keyboard.current?.eKey.wasPressedThisFrame != true) return;
            HidePrompt();
            if (take) PickUp();
            else Begin(State.Aligning);
        }

        internal void DrawPrompt()
        {
            if (prompt == null) return;
            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, wordWrap = true };
                promptStyle.normal.textColor = PromptInk;
            }

            var area = new Rect((Screen.width - 196f) * 0.5f, Screen.height * 0.5f + 18f, 196f, 28f);
            Color saved = GUI.color;
            GUI.color = PromptSurface;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = saved;
            GUI.Label(area, prompt, promptStyle);
        }

        internal void HidePrompt() => prompt = null;

        internal void Dispose()
        {
            if (video != null)
            {
                video.prepareCompleted -= OnVideoPrepared;
                video.errorReceived -= OnVideoError;
                video.loopPointReached -= OnVideoEnded;
                video.Stop();
            }
            if (videoTexture != null)
            {
                videoTexture.Release();
                Object.Destroy(videoTexture);
            }
            if (pictureMaterial != null) Object.Destroy(pictureMaterial);
        }

        private void PickUp()
        {
            tape = new GameObject("Casete VHS · Encargo 01").transform;
            tape.SetParent(cassette, false);
            tape.SetPositionAndRotation(shell.position, cassette.rotation);
            Transform geometry = new GameObject("Geometría de la cinta").transform;
            geometry.SetParent(tape, false);
            geometry.localPosition = -shell.localPosition;
            // Use the original dimensions and materials with a fresh cube mesh. The authored cubes can be
            // static-batched into the scenery, so their in-scene sharedMesh is not safe to move.
            CopyCube(shell, geometry);
            CopyCube(label, geometry);
            CopyLabel(title, geometry);
            shell.gameObject.SetActive(false);
            label.gameObject.SetActive(false);
            title.gameObject.SetActive(false);

            pickupTarget.enabled = false;
            if (video != null) video.Prepare();
            Begin(State.PickingUp);
        }

        private void Begin(State next)
        {
            start = new Pose(tape.position, tape.rotation);
            elapsed = 0f;
            state = next;
        }

        private static Pose HeldPose(Camera view) =>
            new(view.ViewportToWorldPoint(new Vector3(0.67f, 0.24f, 0.69f)), view.transform.rotation * HoldTilt);

        private Pose SlotPose(float depth) =>
            new(deck.TransformPoint(slot + Vector3.forward * depth), deck.rotation);

        private float Advance(float duration)
        {
            elapsed += Time.deltaTime;
            return Mathf.Clamp01(elapsed / duration);
        }

        private void MoveTape(Pose from, Pose to, float t, Vector3 arc)
        {
            float eased = Mathf.SmoothStep(0f, 1f, t);
            Vector3 point = Vector3.Lerp(from.position, to.position, eased) + arc * (4f * eased * (1f - eased));
            tape.SetPositionAndRotation(point, Quaternion.Slerp(from.rotation, to.rotation, eased));
        }

        private void StartPlayback()
        {
            state = State.Playing;
            tape.SetParent(deck, true);
            if (video != null && video.isPrepared) OnVideoPrepared(video);
        }

        private void OnVideoPrepared(VideoPlayer player)
        {
            if (state != State.Playing) return;
            picture.enabled = true;
            player.Play();
        }

        private void OnVideoError(VideoPlayer player, string message) =>
            Debug.LogError("No se pudo reproducir el video del VHS: " + message, screen);

        private void OnVideoEnded(VideoPlayer player) => Finished = true;

        internal void SetMenuPaused(bool paused)
        {
            if (video == null) return;
            if (paused)
            {
                pausedForMenu = video.isPlaying;
                if (pausedForMenu) video.Pause();
            }
            else if (pausedForMenu)
            {
                pausedForMenu = false;
                video.Play();
            }
        }

        internal void JumpToTutorial(bool afterVideo)
        {
            HidePrompt();
            if (video != null) video.Stop();
            if (picture != null) picture.enabled = false;
            if (tape != null)
            {
                tape.gameObject.SetActive(false);
                Object.Destroy(tape.gameObject);
                tape = null;
            }
            flap.localRotation = Quaternion.identity;
            shell.gameObject.SetActive(!afterVideo);
            label.gameObject.SetActive(!afterVideo);
            title.gameObject.SetActive(!afterVideo);
            pickupTarget.enabled = !afterVideo;
            state = afterVideo ? State.Playing : State.OnBench;
            Finished = afterVideo;
        }

        private Transform PrepareDeck(Renderer chassis, Renderer tray)
        {
            Material body = chassis.sharedMaterial;
            // Keep the original collider for aiming. The visible shell has a cavity the tape can enter.
            Block("Base VHS", deck, new Vector3(0f, 0.030f, 0f), new Vector3(0.63f, 0.060f, 0.43f), body);
            Block("Tapa VHS", deck, new Vector3(0f, 0.124f, 0f), new Vector3(0.63f, 0.020f, 0.43f), body);
            Block("Lado izquierdo VHS", deck, new Vector3(-0.2765f, 0.087f, 0f), new Vector3(0.077f, 0.054f, 0.43f), body);
            Block("Lado derecho VHS", deck, new Vector3(0.1785f, 0.087f, 0f), new Vector3(0.273f, 0.054f, 0.43f), body);
            Block("Fondo de la ranura", deck, new Vector3(-0.098f, 0.087f, 0.116f), new Vector3(0.28f, 0.054f, 0.198f), body);

            Transform hinge = new GameObject("Bisagra de la ranura").transform;
            hinge.SetParent(deck, false);
            hinge.localPosition = slot + Vector3.up * 0.0245f;
            Block("Tapa de la ranura", hinge, Vector3.down * 0.0245f, tray.transform.localScale, tray.sharedMaterial);
            chassis.enabled = false;
            tray.enabled = false;
            return hinge;
        }

        private static void Block(string name, Transform parent, Vector3 position, Vector3 size, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = position;
            block.transform.localScale = size;
            block.GetComponent<Collider>().enabled = false;
            block.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CopyCube(Transform source, Transform parent)
        {
            GameObject copy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            copy.name = source.name;
            copy.transform.SetParent(parent, false);
            copy.transform.SetLocalPositionAndRotation(source.localPosition, source.localRotation);
            copy.transform.localScale = source.localScale;
            copy.GetComponent<Collider>().enabled = false;
            copy.GetComponent<Renderer>().sharedMaterial = source.GetComponent<Renderer>().sharedMaterial;
        }

        private static void CopyLabel(Transform source, Transform parent)
        {
            var copy = new GameObject(source.name);
            copy.transform.SetParent(parent, false);
            copy.transform.SetLocalPositionAndRotation(source.localPosition, source.localRotation);
            copy.transform.localScale = source.localScale;
            if (source.TryGetComponent(out TextMesh original))
            {
                TextMesh text = copy.AddComponent<TextMesh>();
                text.font = original.font;
                text.fontSize = original.fontSize;
                text.fontStyle = original.fontStyle;
                text.text = original.text;
                text.characterSize = original.characterSize;
                text.anchor = original.anchor;
                text.alignment = original.alignment;
                text.color = original.color;
                copy.GetComponent<Renderer>().sharedMaterial = source.GetComponent<Renderer>().sharedMaterial;
            }

            foreach (Transform child in source) CopyLabel(child, copy.transform);
        }
    }
}
