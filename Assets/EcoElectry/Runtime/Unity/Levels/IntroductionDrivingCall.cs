using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EcoElectry.Unity.Levels
{
    internal sealed class IntroductionDrivingCall
    {
        private const int ParkingClip = 5;
        private const float AutoAnswerDelay = 6f;

        // The final cue leaves roughly 550 m before the arrival sequence takes over.
        private static readonly float[] Cues = { 0f, 0.22f, 0.39f, 0.56f, 0.80f };
        private static readonly Color32 Amber = new(237, 177, 96, 255);
        private static readonly Color32 Ink = new(24, 28, 29, 247);
        private static readonly Color32 Paper = new(246, 241, 229, 255);
        private static readonly string[][] Captions =
        {
            new[]
            {
                "¿Me escuchas? Bien. Te llamo para confirmar que vas rumbo al colegio.",
                "El viaje es largo, así que aprovecho para explicarte el trabajo antes de que llegues."
            },
            new[]
            {
                "Es un colegio rural grande, bastante lejos de la ciudad. No hay talleres ni proveedores cerca.",
                "Si algo falla, tendrás que resolverlo allí mismo con las herramientas y repuestos que encuentres."
            },
            new[]
            {
                "Por eso te quedarás en el recinto durante todo el contrato. Ya deben tenerte preparado un cuarto.",
                "Organiza tus cosas para pasar allí las semanas de trabajo. No cuentes con volver a la ciudad cada noche."
            },
            new[]
            {
                "Harás el mantenimiento de noche. Durante el día las clases siguen sin interrupción y no podemos meternos en medio.",
                "Cuando empieza el toque de queda, los estudiantes se quedan dentro y tienes vía libre para revisar las instalaciones y hacer tus tareas."
            },
            new[]
            {
                "Una cosa más. La gente de la zona cuenta que pasan cosas raras alrededor del colegio.",
                "No sé cuánto creerles, pero si oyes algo fuera, mantente alerta y avisa antes de salir a investigar.",
                "Ya casi llegas. Busca la entrada iluminada junto a la caseta."
            },
            new[]
            {
                "Bien, ya llegaste. Puedes bajar de la camioneta.",
                "¿Ves la caseta iluminada junto a la entrada? Ve para allá. El guardia te está esperando."
            },
            new[]
            {
                "Dile al guardia que vienes por el trabajo de mantenimiento. Él te indicará cómo llegar a la sala.",
                "Te dejé una cinta con las instrucciones para el primer turno. Hablamos luego."
            }
        };

        private readonly AudioClip[] clips;
        private readonly GameObject receiver;
        private readonly AudioSource voice;
        private readonly AudioSource ringer;
        private readonly AudioClip ringTone;
        private readonly AudioClip hangupTone;
        private readonly IntroductionWaypoint waypoint;
        private readonly RectTransform panel;
        private readonly RectTransform guide;
        private readonly RectTransform playbackLine;
        private readonly Text caption;
        private readonly Text contact;
        private readonly Text callStatus;
        private readonly RectTransform answerKey;
        private readonly Text answerPrompt;
        private bool ringing;
        private float autoAnswerAt;
        private bool answered;
        private int next;
        private int playing = -1;
        private float nextAllowedTime;
        private bool closed;
        private float closeTime;
        internal IntroductionWaypoint Waypoint => waypoint;

        internal bool TryYieldToGuard()
        {
            if (answered && (!closed || Time.time < closeTime)) return false;
            if (ringing) Stop();
            return true;
        }

        internal void ShowGuardLine(string line)
        {
            contact.text = "Vigilante Mauricio";
            callStatus.text = "CASETA";
            caption.text = line;
            caption.gameObject.SetActive(true);
            answerKey.gameObject.SetActive(false);
            answerPrompt.gameObject.SetActive(false);
            playbackLine.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
            panel.gameObject.SetActive(true);
        }

        internal void HideGuardLine() => panel.gameObject.SetActive(false);

        internal IntroductionDrivingCall(Transform parent, AudioClip[] clips)
        {
            this.clips = clips;
            receiver = new GameObject("Interfaz de conducción", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            SceneManager.MoveGameObjectToScene(receiver, parent.gameObject.scene);
            var canvas = receiver.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = receiver.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            var channel = new GameObject("Canal telefónico");
            channel.transform.SetParent(receiver.transform, false);
            voice = channel.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = 0f;
            channel.AddComponent<AudioHighPassFilter>().cutoffFrequency = 300f;
            channel.AddComponent<AudioLowPassFilter>().cutoffFrequency = 3400f;
            channel.AddComponent<AudioDistortionFilter>().distortionLevel = 0.10f;
            ringer = receiver.AddComponent<AudioSource>();
            ringer.playOnAwake = false;
            ringer.loop = true;
            ringer.volume = 0.55f;
            ringTone = CreateRingTone();
            hangupTone = CreateHangupTone();
            ringer.clip = ringTone;

            panel = Plate(receiver.transform, "Receptor", -410, 42, 820, 146, new Color32(10, 14, 15, 230));
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0f);
            panel.anchoredPosition = new Vector2(-410, 42);
            Plate(panel, "Frente", 0, 4, 820, 142, Ink);
            Plate(panel, "Borde superior", 10, 144, 800, 2, new Color32(97, 92, 76, 255));
            Plate(panel, "Canal lateral", 0, 4, 8, 142, Amber);
            Plate(panel, "Indicador 1", 28, 107, 4, 7, Amber);
            Plate(panel, "Indicador 2", 35, 107, 4, 11, Amber);
            Plate(panel, "Indicador 3", 42, 107, 4, 15, Amber);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            contact = Label(panel, "Contacto", font, 58, 99, 360, 32, 25, Amber);
            contact.text = "Guille";
            contact.fontStyle = FontStyle.Bold;
            callStatus = Label(panel, "Estado", font, 552, 101, 240, 29, 18, Paper);
            callStatus.alignment = TextAnchor.MiddleRight;
            caption = Label(panel, "Subtítulo", font, 28, 17, 764, 76, 24, Paper);
            answerKey = Keycap(panel, font, "E", 28, 34);
            answerPrompt = Label(panel, "Acción de llamada", font, 86, 34, 320, 44, 24, Paper);
            answerPrompt.text = "Responder";
            Plate(panel, "Pista", 8, 0, 812, 3, new Color32(79, 73, 61, 255));
            playbackLine = Plate(panel, "Progreso de voz", 8, 0, 0, 3, Amber);
            panel.gameObject.SetActive(false);

            guide = Plate(receiver.transform, "Guía de conducción", -273, 80, 546, 166, Ink);
            guide.anchorMin = guide.anchorMax = new Vector2(0.5f, 0f);
            guide.anchoredPosition = new Vector2(-273, 80);
            Plate(guide, "Borde luminoso", 0, 162, 546, 4, Amber);
            GuideRow(guide, font, 101, "W", "Acelerar");
            GuideRow(guide, font, 51, "S", "Frenar / retroceder");
            Keycap(guide, font, "A", 24, 8);
            Keycap(guide, font, "D", 75, 8);
            Label(guide, "Girar", font, 132, 8, 370, 44, 24, Paper).text = "Girar a izquierda / derecha";
            guide.gameObject.SetActive(false);
            waypoint = new IntroductionWaypoint((RectTransform)receiver.transform, font);
        }

        internal void ShowGuide(bool visible)
        {
            if (guide != null) guide.gameObject.SetActive(visible);
        }

        internal void Ring()
        {
            if (clips == null || clips.Length != Captions.Length || clips[0] == null || answered || ringing) return;
            ringing = true;
            autoAnswerAt = Time.time + AutoAnswerDelay;
            panel.gameObject.SetActive(true);
            callStatus.text = "LLAMADA ENTRANTE";
            caption.gameObject.SetActive(false);
            answerKey.gameObject.SetActive(true);
            answerPrompt.gameObject.SetActive(true);
            playbackLine.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
            ringer.Play();
        }

        internal void Tick(float roadProgress, bool canAnswer, bool parked, bool walking)
        {
            if (closed)
            {
                if (Time.time >= closeTime && panel.gameObject.activeSelf) panel.gameObject.SetActive(false);
                return;
            }
            if (ringing)
            {
                if (!canAnswer) return;
                bool answeredManually = Keyboard.current?.eKey.wasPressedThisFrame == true;
                if (!answeredManually && Time.time < autoAnswerAt) return;
                ringing = false;
                answered = true;
                ringer.Stop();
                callStatus.text = "EN LÍNEA";
                caption.gameObject.SetActive(true);
                answerKey.gameObject.SetActive(false);
                answerPrompt.gameObject.SetActive(false);
            }

            if (!answered) return;
            if (playing >= 0)
            {
                if (voice.isPlaying)
                {
                    PaintCaption((float)voice.timeSamples / voice.clip.samples);
                    return;
                }

                bool ended = playing == Captions.Length - 1;
                playing = -1;
                if (ended)
                {
                    closed = true;
                    closeTime = Time.time + 1.15f;
                    callStatus.text = "LLAMADA FINALIZADA";
                    caption.gameObject.SetActive(false);
                    ringer.PlayOneShot(hangupTone);
                    return;
                }
                panel.gameObject.SetActive(false);
                nextAllowedTime = Time.time + 1f;
            }

            if (parked) next = Mathf.Max(next, ParkingClip);
            if (next >= clips.Length || Time.time < nextAllowedTime) return;
            if (next < ParkingClip ? roadProgress < Cues[next] : !parked || (next > ParkingClip && !walking)) return;

            int index = next++;
            if (clips[index] == null) return;
            playing = index;
            voice.clip = clips[index];
            PaintCaption(0f);
            panel.gameObject.SetActive(true);
            voice.Play();
        }

        internal void Stop()
        {
            if (ringer != null) ringer.Stop();
            ringing = false;
            if (voice != null) voice.Stop();
            if (panel != null) panel.gameObject.SetActive(false);
            playing = -1;
        }

        internal void Dispose()
        {
            Object.Destroy(ringTone);
            Object.Destroy(hangupTone);
            Object.Destroy(receiver);
        }

        private static AudioClip CreateHangupTone()
        {
            const int rate = 24000;
            var samples = new float[rate / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                float time = (float)i / rate;
                float pulse = time < 0.20f ? time : time - 0.20f;
                if (pulse >= 0.12f) continue;
                float envelope = Mathf.Clamp01(Mathf.Min(pulse, 0.12f - pulse) / 0.014f);
                float frequency = time < 0.20f ? 510f : 360f;
                samples[i] = envelope * 0.32f * Mathf.Sin(2f * Mathf.PI * frequency * pulse);
            }
            AudioClip clip = AudioClip.Create("Fin de llamada", samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateRingTone()
        {
            const int rate = 24000;
            const float pulse = 0.22f;
            var samples = new float[rate * 3];
            for (int i = 0; i < samples.Length; i++)
            {
                float time = (float)i / rate;
                float phase = time < 0.34f ? time : time - 0.34f;
                if (phase >= pulse) continue;
                float envelope = Mathf.Clamp01(Mathf.Min(phase, pulse - phase) / 0.018f);
                samples[i] = envelope * 0.25f * (Mathf.Sin(2f * Mathf.PI * 660f * time) +
                    0.4f * Mathf.Sin(2f * Mathf.PI * 880f * time));
            }
            AudioClip clip = AudioClip.Create("Timbre de cabina", samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static void GuideRow(Transform parent, Font font, float y, string key, string action)
        {
            Keycap(parent, font, key, 24, y);
            Label(parent, action, font, 86, y, 416, 44, 24, Paper).text = action;
        }

        private static RectTransform Keycap(Transform parent, Font font, string key, float x, float y)
        {
            RectTransform cap = Plate(parent, "Tecla " + key, x, y, 44, 40, Amber);
            Text glyph = Label(cap, key, font, 0, 0, 44, 40, 24, Ink);
            glyph.text = key;
            glyph.fontStyle = FontStyle.Bold;
            glyph.alignment = TextAnchor.MiddleCenter;
            return cap;
        }

        private void PaintCaption(float progress)
        {
            playbackLine.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 812f * progress);
            string[] pages = Captions[playing];
            int totalLength = 0;
            foreach (string page in pages) totalLength += page.Length;

            float position = progress * totalLength;
            foreach (string page in pages)
            {
                if (position <= page.Length)
                {
                    if (caption.text != page) caption.text = page;
                    return;
                }
                position -= page.Length;
            }
        }

        internal static RectTransform Plate(Transform parent, string name, float x, float y, float width, float height, Color tint)
        {
            RectTransform rect = Place(parent, name, x, y, width, height);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = tint;
            image.raycastTarget = false;
            return rect;
        }

        internal static Text Label(Transform parent, string name, Font font, float x, float y, float width, float height,
            int size, Color tint)
        {
            var label = Place(parent, name, x, y, width, height).gameObject.AddComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.color = tint;
            label.alignment = TextAnchor.MiddleLeft;
            label.raycastTarget = false;
            return label;
        }

        private static RectTransform Place(Transform parent, string name, float x, float y, float width, float height)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
    }
}
