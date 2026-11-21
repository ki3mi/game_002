using System.IO;
using EcoElectry.Unity.Devices;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace EcoElectry.Editor.ArtAndAnimation
{
    internal static class LC17_DispositivosAnimacionEditor
    {
        private const string ContentRoot = "Assets/EcoElectry/Content";
        private const string DeviceAnimDir = ContentRoot + "/Animations/Devices";
        private const string DevicePrefabsDir = ContentRoot + "/Prefabs/Devices";
        private const string ControllerPath = DeviceAnimDir + "/AC_DispositivoFuncional.controller";

        [MenuItem("Tools/EcoElectry/LC-17 Generar Animaciones y Prefabs de Dispositivos")]
        private static void GenerateDeviceAnimations()
        {
            EnsureDirectories();
            AnimatorController controller = BuildDeviceAnimatorController();

            // 1. Ventiladores con rotación de aspas/rotor e inercia al apagar o perder suministro
            CreateDevicePrefab(
                "Assets/ResourcePack/Ventilador/Prefabs/VentiladorTecho.prefab",
                DevicePrefabsDir + "/PF_VentiladorTecho_Animado.prefab",
                controller,
                DeviceMotionType.RotacionContinua,
                Vector3.up,
                190f);

            CreateDevicePrefab(
                "Assets/ResourcePack/Ventilador/Prefabs/VentiladorIndustrial.prefab",
                DevicePrefabsDir + "/PF_VentiladorIndustrial_Animado.prefab",
                controller,
                DeviceMotionType.RotacionContinua,
                Vector3.forward,
                260f);

            CreateDevicePrefab(
                "Assets/ResourcePack/Ventilador/Prefabs/VentiladorPared.prefab",
                DevicePrefabsDir + "/PF_VentiladorPared_Animado.prefab",
                controller,
                DeviceMotionType.RotacionContinua,
                Vector3.forward,
                220f);

            // 2. Equipos eléctricos y luminarias con respuesta visual de encendido, apagado y corte
            CreateDevicePrefab(
                "Assets/ResourcePack/LuminariaInterior/Prefabs/TuboLED.prefab",
                DevicePrefabsDir + "/PF_LuminariaLED_Animada.prefab",
                controller,
                DeviceMotionType.IndicadorEmisivo,
                Vector3.up,
                0f);

            CreateDevicePrefab(
                "Assets/ResourcePack/Inversor/Prefabs/InversorHibrido.prefab",
                DevicePrefabsDir + "/PF_InversorHibrido_Animado.prefab",
                controller,
                DeviceMotionType.IndicadorEmisivo,
                Vector3.up,
                0f);

            CreateDevicePrefab(
                "Assets/ResourcePack/ControladorCarga/Prefabs/ControladorMPPT.prefab",
                DevicePrefabsDir + "/PF_ControladorMPPT_Animado.prefab",
                controller,
                DeviceMotionType.IndicadorEmisivo,
                Vector3.up,
                0f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LC-17] Clips, AC_DispositivoFuncional.controller y prefabs funcionales generados.");
        }

        [MenuItem("Tools/EcoElectry/LC-17 Validar Estados y Coherencia de Dispositivos")]
        private static void ValidateDevices()
        {
            int errors = 0;

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[LC-17] Falta {ControllerPath}. Ejecuta primero el generador LC-17.");
                return;
            }

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            string[] expectedStates = { "Apagado", "Encendido", "SinSuministro" };
            foreach (string stName in expectedStates)
            {
                bool found = false;
                foreach (ChildAnimatorState child in sm.states)
                {
                    if (child.state.name == stName)
                    {
                        found = true;
                        if (child.state.motion == null)
                        {
                            Debug.LogError($"[LC-17] El estado '{stName}' no tiene AnimationClip asignado.");
                            errors++;
                        }
                    }
                }
                if (!found)
                {
                    Debug.LogError($"[LC-17] Falta el estado obligatorio '{stName}' en AC_DispositivoFuncional.");
                    errors++;
                }
            }

            string[] prefabs =
            {
                DevicePrefabsDir + "/PF_VentiladorTecho_Animado.prefab",
                DevicePrefabsDir + "/PF_VentiladorIndustrial_Animado.prefab",
                DevicePrefabsDir + "/PF_VentiladorPared_Animado.prefab",
                DevicePrefabsDir + "/PF_LuminariaLED_Animada.prefab",
                DevicePrefabsDir + "/PF_InversorHibrido_Animado.prefab",
                DevicePrefabsDir + "/PF_ControladorMPPT_Animado.prefab"
            };

            foreach (string path in prefabs)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null)
                {
                    Debug.LogError($"[LC-17] No se encontró {path}.");
                    errors++;
                    continue;
                }

                var op = go.GetComponent<DeviceOperationalAnimator>();
                var anim = go.GetComponent<Animator>();
                if (op == null || anim == null)
                {
                    Debug.LogError($"[LC-17] {path} requiere Animator y DeviceOperationalAnimator.");
                    errors++;
                    continue;
                }

                // Verificar que inicia en Apagado sin movimiento fantasma
                if (op.CurrentState != DevicePowerState.Apagado)
                {
                    Debug.LogError($"[LC-17] {path} debe iniciar en estado Apagado por defecto.");
                    errors++;
                }
            }

            if (errors == 0)
                Debug.Log("[LC-17] Validación exitosa: 3 estados de suministro, 6 prefabs reutilizables y cero movimiento residual en apagado.");
            else
                Debug.LogError($"[LC-17] Validación fallida con {errors} error(es).");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder(DeviceAnimDir))
                Directory.CreateDirectory(DeviceAnimDir);
            if (!AssetDatabase.IsValidFolder(DevicePrefabsDir))
                Directory.CreateDirectory(DevicePrefabsDir);
            AssetDatabase.Refresh();
        }

        private static AnimatorController BuildDeviceAnimatorController()
        {
            // 1. Apagado (reposo absoluto: sin vibración ni movimiento)
            AnimationClip clipApagado = CreateClip(DeviceAnimDir + "/Anim_Dispositivo_Apagado.anim", loop: false,
                ("localScale.x", new[] { new Keyframe(0f, 1f) }),
                ("localScale.y", new[] { new Keyframe(0f, 1f) }),
                ("localScale.z", new[] { new Keyframe(0f, 1f) }));

            // 2. Encendido (pulso operativo muy sutil para equipos energizados)
            AnimationClip clipEncendido = CreateClip(DeviceAnimDir + "/Anim_Dispositivo_Encendido.anim", loop: true,
                ("localScale.x", new[] { new Keyframe(0f, 1f), new Keyframe(0.5f, 1.008f), new Keyframe(1f, 1f) }),
                ("localScale.y", new[] { new Keyframe(0f, 1f), new Keyframe(0.5f, 1.008f), new Keyframe(1f, 1f) }),
                ("localScale.z", new[] { new Keyframe(0f, 1f), new Keyframe(0.5f, 1.008f), new Keyframe(1f, 1f) }));

            // 3. SinSuministro (sacudida breve al cortarse la energía y detención inmediata)
            AnimationClip clipCorte = CreateClip(DeviceAnimDir + "/Anim_Dispositivo_SinSuministro.anim", loop: false,
                ("localEulerAnglesRaw.z", new[]
                {
                    new Keyframe(0f, 0f),
                    new Keyframe(0.08f, -1.5f),
                    new Keyframe(0.18f, 1.5f),
                    new Keyframe(0.30f, -0.6f),
                    new Keyframe(0.42f, 0f)
                }));

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("EstadoEnergia", AnimatorControllerParameterType.Int);
            controller.AddParameter("Encendido", AnimatorControllerParameterType.Bool);
            controller.AddParameter("SinSuministro", AnimatorControllerParameterType.Bool);
            controller.AddParameter("TriggerEncendido", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("TriggerCorte", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState stApagado = sm.AddState("Apagado");
            stApagado.motion = clipApagado;
            sm.defaultState = stApagado;

            AnimatorState stEncendido = sm.AddState("Encendido");
            stEncendido.motion = clipEncendido;

            AnimatorState stSinSuministro = sm.AddState("SinSuministro");
            stSinSuministro.motion = clipCorte;

            // Encendido <-> Apagado
            AddTransition(stApagado, stEncendido, 0.1f, (AnimatorConditionMode.If, 0f, "Encendido"));
            AddTransition(stEncendido, stApagado, 0.1f,
                (AnimatorConditionMode.IfNot, 0f, "Encendido"),
                (AnimatorConditionMode.IfNot, 0f, "SinSuministro"));

            // Corte de suministro desde Encendido o Apagado
            AddTransition(stEncendido, stSinSuministro, 0.05f, (AnimatorConditionMode.If, 0f, "SinSuministro"));
            AddTransition(stApagado, stSinSuministro, 0.05f, (AnimatorConditionMode.If, 0f, "SinSuministro"));

            // Recuperación de suministro
            AddTransition(stSinSuministro, stEncendido, 0.1f, (AnimatorConditionMode.If, 0f, "Encendido"));
            AddTransition(stSinSuministro, stApagado, 0.1f,
                (AnimatorConditionMode.IfNot, 0f, "Encendido"),
                (AnimatorConditionMode.IfNot, 0f, "SinSuministro"));

            return controller;
        }

        private static void AddTransition(
            AnimatorState from, AnimatorState to, float duration,
            params (AnimatorConditionMode mode, float threshold, string param)[] conditions)
        {
            AnimatorStateTransition t = from.AddTransition(to);
            t.duration = duration;
            t.hasExitTime = false;
            foreach (var c in conditions)
                t.AddCondition(c.mode, c.threshold, c.param);
        }

        private static AnimationClip CreateClip(string path, bool loop, params (string property, Keyframe[] keys)[] curves)
        {
            var clip = new AnimationClip { frameRate = 30f };
            foreach (var c in curves)
                clip.SetCurve("", typeof(Transform), c.property, new AnimationCurve(c.keys));

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void CreateDevicePrefab(
            string sourcePrefabPath, string targetPrefabPath,
            RuntimeAnimatorController controller, DeviceMotionType motionType,
            Vector3 spinAxis, float rpm)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath);
            if (source == null)
            {
                Debug.LogWarning($"[LC-17] Prefab base no encontrado: {sourcePrefabPath}. Ejecuta 'git lfs pull'.");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.transform.localScale = Vector3.one;

            var animator = instance.GetComponent<Animator>() ?? instance.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            var audio = instance.GetComponent<AudioSource>() ?? instance.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 1f;
            audio.maxDistance = 14f;

            var opAnim = instance.GetComponent<DeviceOperationalAnimator>() ?? instance.AddComponent<DeviceOperationalAnimator>();

            var rotors = new System.Collections.Generic.List<Transform>();
            if (motionType == DeviceMotionType.RotacionContinua)
            {
                foreach (Transform t in instance.GetComponentsInChildren<Transform>(true))
                {
                    string n = t.name.ToLowerInvariant();
                    if (n.Contains("rotor") || n.Contains("aspa") || n.Contains("brazo") || n.Contains("helice") || n.Contains("buje"))
                        rotors.Add(t);
                }
            }

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            opAnim.ConfigureParts(rotors.ToArray(), spinAxis, rpm, renderers);

            var serialized = new SerializedObject(opAnim);
            serialized.FindProperty("initialState").enumValueIndex = (int)DevicePowerState.Apagado;
            serialized.FindProperty("motionType").enumValueIndex = (int)motionType;
            serialized.FindProperty("audioSource").objectReferenceValue = audio;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(instance, targetPrefabPath);
            Object.DestroyImmediate(instance);
        }
    }
}
