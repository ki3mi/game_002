using UnityEngine;

namespace EcoElectry.Unity.Levels
{
    internal sealed class IntroductionGuard
    {
        private const float VoicePitch = 0.94f;
        private static readonly (string clip, string text)[] Lines =
        {
            ("L00_Mauricio_01_Bienvenida", "Buenas noches. Soy Mauricio, el vigilante. Guille me avisó que venías para el mantenimiento."),
            ("L00_Mauricio_02_Ruta_Mantenimiento", "Sigue el sendero iluminado junto a la fachada. La sala de mantenimiento es la M-01. Verás la placa al lado de la puerta."),
            ("L00_Mauricio_03_Cinta_Primer_Turno", "Dentro hay un televisor y una videocasetera. Pon la cinta que dejó Guille. Ahí te explica por dónde empezar el turno.")
        };

        private readonly Transform booth;
        private readonly AudioSource voice;
        private int nextLine;
        private float lineTimeRemaining;
        private string currentLine;
        private bool away;

        internal bool Started { get; private set; }
        internal bool Finished { get; private set; }

        internal IntroductionGuard(Transform booth)
        {
            this.booth = booth;
            GameObject model = Resources.Load<GameObject>("Level00/VigilanteMauricio");
            GameObject mauricio = model != null ? Object.Instantiate(model, booth) : new GameObject();
            mauricio.name = "Vigilante Mauricio";
            mauricio.transform.SetParent(booth, false);
            mauricio.transform.localPosition = new Vector3(0.26f, 0.16f, -1.68f);
            foreach (MeshRenderer renderer in mauricio.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.name != "Mauricio_Piel") continue;
                if (Vector3.Dot(renderer.bounds.center - mauricio.transform.position, booth.forward) > 0f)
                    mauricio.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                break;
            }
            Material boothMaterial = booth.Find("Frente_inferior")?.GetComponent<Renderer>()?.sharedMaterial;
            if (boothMaterial != null) ApplyPalette(mauricio, boothMaterial);

            var lamp = new GameObject("Luz de ventanilla · Mauricio").AddComponent<Light>();
            lamp.transform.SetParent(booth, false);
            lamp.transform.localPosition = new Vector3(0.18f, 2.04f, -2.28f);
            lamp.transform.localRotation = Quaternion.LookRotation(new Vector3(0.08f, -0.23f, 0.55f));
            lamp.type = LightType.Spot;
            lamp.color = new Color32(255, 222, 180, 255);
            lamp.intensity = 2.6f;
            lamp.range = 1.85f;
            lamp.spotAngle = 72f;
            lamp.shadows = LightShadows.None;

            var speech = new GameObject("Voz de Mauricio");
            speech.transform.SetParent(mauricio.transform, false);
            speech.transform.localPosition = Vector3.up * 1.62f;
            voice = speech.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.pitch = VoicePitch;
            voice.dopplerLevel = 0f;
            speech.AddComponent<AudioLowPassFilter>().cutoffFrequency = 5800f;
            voice.spatialBlend = 0.65f;
            voice.rolloffMode = AudioRolloffMode.Linear;
            voice.minDistance = 3f;
            voice.maxDistance = 12f;
        }

        internal void Tick(Vector3 playerPosition, IntroductionDrivingCall dialogue)
        {
            if (Finished) return;
            Vector3 offset = playerPosition - IntroductionWaypoint.GuardApproach(booth);
            bool near = Vector3.ProjectOnPlane(offset, Vector3.up).sqrMagnitude <= 2.5f * 2.5f;
            if (!Started)
            {
                if (!near || !dialogue.TryYieldToGuard()) return;
                Started = true;
            }

            if (!near)
            {
                if (!away)
                {
                    voice.Pause();
                    dialogue.HideGuardLine();
                    away = true;
                }
                return;
            }

            if (away)
            {
                voice.UnPause();
                dialogue.ShowGuardLine(currentLine);
                away = false;
            }

            lineTimeRemaining -= Time.deltaTime;
            if (lineTimeRemaining > 0f || voice.isPlaying) return;
            if (nextLine == Lines.Length)
            {
                Finished = true;
                dialogue.HideGuardLine();
                return;
            }

            var line = Lines[nextLine++];
            currentLine = line.text;
            dialogue.ShowGuardLine(currentLine);
            voice.clip = Resources.Load<AudioClip>("Mauricio/" + line.clip);
            if (voice.clip != null) voice.Play();
            float seconds = voice.clip != null ? voice.clip.length / VoicePitch : Mathf.Max(4f, line.text.Length / 18f);
            lineTimeRemaining = seconds + 0.45f;
        }

        internal void Stop()
        {
            if (voice != null) voice.Stop();
        }

        internal void Skip()
        {
            Stop();
            Started = true;
            Finished = true;
        }

        private static void ApplyPalette(GameObject model, Material material)
        {
            var block = new MaterialPropertyBlock();
            foreach (MeshRenderer renderer in model.GetComponentsInChildren<MeshRenderer>())
            {
                Color32 color = renderer.name switch
                {
                    "Mauricio_Uniforme" => new Color32(53, 91, 121, 255),
                    "Mauricio_Pantalon" => new Color32(38, 57, 75, 255),
                    "Mauricio_Piel" => new Color32(184, 124, 92, 255),
                    "Mauricio_Detalle" => new Color32(245, 209, 141, 255),
                    "Mauricio_Pelo" => new Color32(73, 55, 47, 255),
                    _ => new Color32(33, 39, 47, 255)
                };
                renderer.sharedMaterial = material;
                block.SetColor("_BaseColor", color);
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
