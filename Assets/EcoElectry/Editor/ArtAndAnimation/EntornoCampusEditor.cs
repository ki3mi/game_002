using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

namespace EcoElectry.Editor.ArtAndAnimation
{
    internal static class LC10_EntornoCampusEditor
    {
        private const string ContentRoot = "Assets/EcoElectry/Content";
        private const string EnvPrefabsDir = ContentRoot + "/Prefabs/Environment";
        private const string EnvAnimDir = ContentRoot + "/Animations/Environment";

        [MenuItem("Tools/EcoElectry/LC-10 Generar Entorno Campus Rural")]
        private static void BuildCampusEnvironment()
        {
            EnsureFolders();

            RuntimeAnimatorController windController = CreateFoliageWindAnimator();

            GameObject pfMontanaAlta = CreateEnvVariant(
                "Assets/ResourcePack/Montana/Prefabs/MontanaAlta.prefab",
                EnvPrefabsDir + "/PF_MontanaAlta.prefab",
                castShadows: false, isStatic: true, animator: null);

            GameObject pfMontanaFondo = CreateEnvVariant(
                "Assets/ResourcePack/Montana/Prefabs/MontanaFondo.prefab",
                EnvPrefabsDir + "/PF_MontanaFondo.prefab",
                castShadows: false, isStatic: true, animator: null);

            GameObject pfLomaFondo = CreateEnvVariant(
                "Assets/ResourcePack/Montana/Prefabs/LomaFondo.prefab",
                EnvPrefabsDir + "/PF_LomaFondo.prefab",
                castShadows: false, isStatic: true, animator: null);

            GameObject pfArbolGrande = CreateEnvVariant(
                "Assets/ResourcePack/Arbol/Prefabs/ArbolGrande.prefab",
                EnvPrefabsDir + "/PF_ArbolGrande_Brisa.prefab",
                castShadows: true, isStatic: false, animator: windController);

            GameObject pfArbolMediano = CreateEnvVariant(
                "Assets/ResourcePack/Arbol/Prefabs/ArbolMediano.prefab",
                EnvPrefabsDir + "/PF_ArbolMediano_Brisa.prefab",
                castShadows: true, isStatic: false, animator: windController);

            GameObject pfArbustoFlorido = CreateEnvVariant(
                "Assets/ResourcePack/Arbusto/Prefabs/ArbustoFlorido.prefab",
                EnvPrefabsDir + "/PF_ArbustoFlorido_Brisa.prefab",
                castShadows: false, isStatic: false, animator: windController);

            GameObject pfCaminoRural = CreateEnvVariant(
                "Assets/ResourcePack/CaminoPatio/Prefabs/CaminoAfirmado.prefab",
                EnvPrefabsDir + "/PF_CaminoRural.prefab",
                castShadows: false, isStatic: true, animator: null);

            GameObject pfSueloBase = CreateEnvVariant(
                "Assets/ResourcePack/SueloPatio/Prefabs/SueloGrava.prefab",
                EnvPrefabsDir + "/PF_TerrenoRuralBase.prefab",
                castShadows: false, isStatic: true, animator: null);

            GameObject pfCercaMadera = CreateEnvVariant(
                "Assets/ResourcePack/Cerca/Prefabs/CercaMadera.prefab",
                EnvPrefabsDir + "/PF_CercaPerimetral.prefab",
                castShadows: true, isStatic: true, animator: null);

            AssembleMasterCampusPrefab(
                pfMontanaAlta, pfMontanaFondo, pfLomaFondo,
                pfArbolGrande, pfArbolMediano, pfArbustoFlorido,
                pfCaminoRural, pfSueloBase, pfCercaMadera);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("LC-10: Entorno visual del campus rural generado en Assets/EcoElectry/Content/Prefabs/Environment/PF_EntornoCampus_LC10.prefab");
        }

        private static void EnsureFolders()
        {
            string[] dirs =
            {
                ContentRoot + "/Models/Environment",
                EnvPrefabsDir,
                EnvAnimDir
            };
            foreach (string d in dirs)
            {
                if (!AssetDatabase.IsValidFolder(d))
                    Directory.CreateDirectory(d);
            }
            AssetDatabase.Refresh();
        }

        private static RuntimeAnimatorController CreateFoliageWindAnimator()
        {
            string clipPath = EnvAnimDir + "/Anim_Vegetacion_Brisa.anim";
            string ctrlPath = EnvAnimDir + "/AC_Vegetacion_Brisa.controller";

            var existingCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ctrlPath);
            if (existingCtrl != null) return existingCtrl;

            var clip = new AnimationClip { frameRate = 24f };
            var curveZ = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(1.8f, 1.4f),
                new Keyframe(3.6f, -1.1f),
                new Keyframe(5.4f, 0f));
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.z", curveZ);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, clipPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            AnimatorState state = controller.layers[0].stateMachine.AddState("BrisaSuave");
            state.motion = clip;
            controller.layers[0].stateMachine.defaultState = state;
            return controller;
        }

        private static GameObject CreateEnvVariant(
            string sourcePath, string targetPath,
            bool castShadows, bool isStatic, RuntimeAnimatorController animator)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
            if (existing != null) return existing;

            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (source == null)
            {
                Debug.LogWarning($"Falta asset origen: {sourcePath}. Ejecuta 'git lfs pull'.");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }

            if (isStatic)
            {
                SetStaticRecursive(instance,
                    StaticEditorFlags.BatchingStatic |
                    StaticEditorFlags.OccludeeStatic |
                    StaticEditorFlags.ContributeGI);
            }

            if (animator != null)
            {
                var anim = instance.GetComponent<Animator>() ?? instance.AddComponent<Animator>();
                anim.runtimeAnimatorController = animator;
                anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(instance, targetPath);
            Object.DestroyImmediate(instance);
            return saved;
        }

        private static void AssembleMasterCampusPrefab(
            GameObject montanaAlta, GameObject montanaFondo, GameObject lomaFondo,
            GameObject arbolGrande, GameObject arbolMediano, GameObject arbustoFlorido,
            GameObject caminoRural, GameObject sueloBase, GameObject cercaMadera)
        {
            string masterPath = EnvPrefabsDir + "/PF_EntornoCampus_LC10.prefab";
            var root = new GameObject("PF_EntornoCampus_LC10");

            var grpHorizonte = CreateChildGroup(root.transform, "01_HorizonteMontanoso");
            var grpTerreno = CreateChildGroup(root.transform, "02_TerrenoYCaminos");
            var grpPerimetro = CreateChildGroup(root.transform, "03_CercaPerimetral");
            var grpVegetacion = CreateChildGroup(root.transform, "04_VegetacionLowPoly");

            Place(montanaAlta, grpHorizonte, new Vector3(-45f, -1f, 65f), 15f, new Vector3(2.2f, 2.4f, 2.2f));
            Place(montanaFondo, grpHorizonte, new Vector3(0f, -1f, 75f), -10f, new Vector3(2.5f, 2.1f, 2.5f));
            Place(montanaAlta, grpHorizonte, new Vector3(50f, -1f, 62f), -35f, new Vector3(2.0f, 2.3f, 2.0f));
            Place(montanaFondo, grpHorizonte, new Vector3(-68f, -1f, 20f), 75f, new Vector3(2.1f, 1.8f, 2.1f));
            Place(montanaFondo, grpHorizonte, new Vector3(70f, -1f, 18f), -80f, new Vector3(2.1f, 1.8f, 2.1f));

            Place(lomaFondo, grpHorizonte, new Vector3(-28f, -0.5f, 36f), 12f, new Vector3(1.6f, 1.3f, 1.6f));
            Place(lomaFondo, grpHorizonte, new Vector3(26f, -0.5f, 38f), -18f, new Vector3(1.7f, 1.4f, 1.7f));
            Place(lomaFondo, grpHorizonte, new Vector3(-40f, -0.5f, -2f), 85f, new Vector3(1.5f, 1.1f, 1.5f));
            Place(lomaFondo, grpHorizonte, new Vector3(40f, -0.5f, -4f), -85f, new Vector3(1.5f, 1.1f, 1.5f));

            Place(sueloBase, grpTerreno, new Vector3(0f, -0.05f, 0f), 0f, new Vector3(18f, 1f, 16f));
            for (int i = 0; i < 4; i++)
            {
                Place(caminoRural, grpTerreno, new Vector3(0f, 0.01f, -14f - (i * 3f)), 0f, Vector3.one);
            }

            for (int x = -15; x <= 15; x += 3)
            {
                if (Mathf.Abs(x) > 2)
                    Place(cercaMadera, grpPerimetro, new Vector3(x, 0f, -13f), 0f, Vector3.one);
                Place(cercaMadera, grpPerimetro, new Vector3(x, 0f, 15f), 180f, Vector3.one);
            }
            for (int z = -12; z <= 14; z += 3)
            {
                Place(cercaMadera, grpPerimetro, new Vector3(-16.5f, 0f, z), 90f, Vector3.one);
                Place(cercaMadera, grpPerimetro, new Vector3(16.5f, 0f, z), -90f, Vector3.one);
            }

            Place(arbolGrande, grpVegetacion, new Vector3(-12f, 0f, 11f), 20f, Vector3.one);
            Place(arbolMediano, grpVegetacion, new Vector3(-6f, 0f, 12.5f), 110f, Vector3.one);
            Place(arbolGrande, grpVegetacion, new Vector3(13f, 0f, 10f), -45f, Vector3.one);
            Place(arbolMediano, grpVegetacion, new Vector3(14f, 0f, -3f), 65f, Vector3.one);

            Place(arbustoFlorido, grpVegetacion, new Vector3(-4f, 0f, -12.2f), 15f, Vector3.one);
            Place(arbustoFlorido, grpVegetacion, new Vector3(4f, 0f, -12.2f), -30f, Vector3.one);
            Place(arbustoFlorido, grpVegetacion, new Vector3(-14.5f, 0f, -11f), 50f, Vector3.one);
            Place(arbustoFlorido, grpVegetacion, new Vector3(14.5f, 0f, -11f), -70f, Vector3.one);

            PrefabUtility.SaveAsPrefabAsset(root, masterPath);
            Object.DestroyImmediate(root);
        }

        private static Transform CreateChildGroup(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void Place(GameObject prefab, Transform parent, Vector3 pos, float rotY, Vector3 scale)
        {
            if (prefab == null) return;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(parent, false);
            inst.transform.localPosition = pos;
            inst.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);
            inst.transform.localScale = scale;
        }

        private static void SetStaticRecursive(GameObject go, StaticEditorFlags flags)
        {
            GameObjectUtility.SetStaticEditorFlags(go, flags);
            foreach (Transform child in go.transform)
                SetStaticRecursive(child.gameObject, flags);
        }
    }
}
