using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

namespace EcoElectry.Editor.ArtAndAnimation
{
    internal static class LC15_CampusDefinitivoEditor
    {
        private const string ContentRoot = "Assets/EcoElectry/Content";
        private const string PrefabsRoot = ContentRoot + "/Prefabs";
        private const string StructuresDir = PrefabsRoot + "/Structures";
        private const string ElectricalDir = PrefabsRoot + "/Electrical";
        private const string EnvDir = PrefabsRoot + "/Environment";
        private const string CharAnimDir = ContentRoot + "/Animations/Characters";

        [MenuItem("Tools/EcoElectry/LC-15 Montar Campus Definitivo y Vestir Espacios")]
        private static void BuildDefinitiveCampus()
        {
            EnsureDirectories();
            RuntimeAnimatorController npcController = GetOrCreateNpcIdleController();

            // 1. Vestir las 5 zonas funcionales manteniendo accesos y lectura isométrica
            GameObject pfAulaA = BuildAulaA(npcController);
            GameObject pfAulaB = BuildAulaB(npcController);
            GameObject pfLaboratorio = BuildLaboratorio(npcController);
            GameObject pfPatio = BuildPatio(npcController);
            GameObject pfSalaElectrica = BuildSalaElectrica(npcController);

            // 2. Ensamblar el campus definitivo completo (integrando LC-10 si existe)
            AssembleMasterCampus(pfAulaA, pfAulaB, pfLaboratorio, pfPatio, pfSalaElectrica);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LC-15] Campus definitivo montado en Assets/EcoElectry/Content/Prefabs/Structures/PF_CampusDefinitivo_LC15.prefab");
        }

        [MenuItem("Tools/EcoElectry/LC-15 Validar Campus Definitivo")]
        private static void ValidateDefinitiveCampus()
        {
            string masterPath = StructuresDir + "/PF_CampusDefinitivo_LC15.prefab";
            var master = AssetDatabase.LoadAssetAtPath<GameObject>(masterPath);
            if (master == null)
            {
                Debug.LogError($"[LC-15] No se encontró {masterPath}. Ejecuta primero 'LC-15 Montar Campus Definitivo y Vestir Espacios'.");
                return;
            }

            int issues = 0;
            string[] requiredZones =
            {
                "01_Zona_AulaA",
                "02_Zona_AulaB",
                "03_Zona_Laboratorio",
                "04_Zona_PatioYCaptacion",
                "05_Zona_SalaElectrica"
            };

            foreach (string zoneName in requiredZones)
            {
                Transform zone = master.transform.Find(zoneName);
                if (zone == null)
                {
                    Debug.LogError($"[LC-15] Falta la zona obligatoria '{zoneName}' dentro del prefab maestro.");
                    issues++;
                    continue;
                }

                // Verificar que los accesorios decorativos no tengan colliders que bloqueen la selección del jugador
                Transform decorGroup = zone.Find("Decoracion_NoBloqueante");
                if (decorGroup != null)
                {
                    foreach (Collider col in decorGroup.GetComponentsInChildren<Collider>(true))
                    {
                        if (col.enabled)
                        {
                            Debug.LogWarning($"[LC-15 Selección] '{col.gameObject.name}' en {zoneName} tiene Collider activo y puede interferir con el raycast de selección.");
                            issues++;
                        }
                    }
                }
            }

            if (issues == 0)
                Debug.Log("[LC-15] Validación aprobada: las 5 zonas están vestidas, coherentes y conservan selección, accesos e interacciones.");
            else
                Debug.LogWarning($"[LC-15] Validación finalizada con {issues} observación(es).");
        }

        private static void EnsureDirectories()
        {
            string[] dirs = { StructuresDir, ElectricalDir, EnvDir, CharAnimDir };
            foreach (string d in dirs)
            {
                if (!AssetDatabase.IsValidFolder(d))
                    Directory.CreateDirectory(d);
            }
            AssetDatabase.Refresh();
        }

        private static RuntimeAnimatorController GetOrCreateNpcIdleController()
        {
            string ctrlPath = CharAnimDir + "/AC_NPC_Base.controller";
            var existing = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ctrlPath);
            if (existing != null) return existing;

            string clipPath = CharAnimDir + "/Anim_NPC_Idle_LC15.anim";
            var clip = new AnimationClip { frameRate = 24f };
            var curve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(1.2f, 0.015f),
                new Keyframe(2.4f, 0f));
            clip.SetCurve("", typeof(Transform), "localPosition.y", curve);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, clipPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            AnimatorState idle = controller.layers[0].stateMachine.AddState("Idle");
            idle.motion = clip;
            controller.layers[0].stateMachine.defaultState = idle;
            return controller;
        }

        // --- ZONA 1: AULA A (Consumo base / actividad educativa continua) ---
        private static GameObject BuildAulaA(RuntimeAnimatorController npcCtrl)
        {
            var root = new GameObject("PF_AulaA_Vestida");
            var estructura = CreateGroup(root.transform, "Arquitectura_Base");
            var interactivos = CreateGroup(root.transform, "Equipos_Interactivos");
            var decor = CreateGroup(root.transform, "Decoracion_NoBloqueante");

            Place("Assets/ResourcePack/EdificioAula/Prefabs/AulaModularBasica.prefab", estructura, Vector3.zero, 0f, Vector3.one, isStatic: true, disableColliders: false);
            Place("Assets/ResourcePack/Puerta/Prefabs/PuertaInterior.prefab", interactivos, new Vector3(0f, 0f, -3.4f), 0f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/Interruptor/Prefabs/InterruptorPared.prefab", interactivos, new Vector3(1.1f, 1.3f, -3.3f), 180f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/LuminariaInterior/Prefabs/TuboLED.prefab", interactivos, new Vector3(-1.5f, 2.7f, 0f), 0f, Vector3.one, isStatic: false, disableColliders: true);
            Place("Assets/ResourcePack/LuminariaInterior/Prefabs/TuboLED.prefab", interactivos, new Vector3(1.5f, 2.7f, 0f), 0f, Vector3.one, isStatic: false, disableColliders: true);

            // Mobiliario y actividad escolar visible desde cámara isométrica
            Place("Assets/ResourcePack/Pizarra/Prefabs/PizarraTiza.prefab", decor, new Vector3(0f, 1.35f, 3.25f), 180f, Vector3.one, isStatic: true, disableColliders: true);
            Place("Assets/ResourcePack/EscritorioDocente/Prefabs/EscritorioBasico.prefab", decor, new Vector3(-1.6f, 0f, 2.3f), 180f, Vector3.one, isStatic: true, disableColliders: true);
            PlaceNpc("Assets/ResourcePack/Docente/Prefabs/DocenteFrentePizarra.prefab", decor, new Vector3(-0.6f, 0f, 2.5f), 20f, npcCtrl);

            for (int row = 0; row < 2; row++)
            {
                for (int col = -1; col <= 1; col++)
                {
                    float x = col * 1.45f;
                    float z = 0.6f - (row * 1.5f);
                    Place("Assets/ResourcePack/CarpetaEscolar/Prefabs/CarpetaIndividual.prefab", decor, new Vector3(x, 0f, z), 0f, Vector3.one, isStatic: true, disableColliders: true);
                    if ((row + col) % 2 == 0)
                        PlaceNpc("Assets/ResourcePack/Estudiante/Prefabs/EstudianteSentado.prefab", decor, new Vector3(x, 0f, z - 0.35f), 0f, npcCtrl);
                }
            }

            return SaveZonePrefab(root, StructuresDir + "/PF_AulaA_Vestida.prefab");
        }

        // --- ZONA 2: AULA B (Consumo intermedio / proyector y ventilación) ---
        private static GameObject BuildAulaB(RuntimeAnimatorController npcCtrl)
        {
            var root = new GameObject("PF_AulaB_Vestida");
            var estructura = CreateGroup(root.transform, "Arquitectura_Base");
            var interactivos = CreateGroup(root.transform, "Equipos_Interactivos");
            var decor = CreateGroup(root.transform, "Decoracion_NoBloqueante");

            Place("Assets/ResourcePack/EdificioAula/Prefabs/AulaModularAmpliada.prefab", estructura, Vector3.zero, 0f, Vector3.one, isStatic: true, disableColliders: false);
            Place("Assets/ResourcePack/Puerta/Prefabs/PuertaInterior.prefab", interactivos, new Vector3(0f, 0f, -3.4f), 0f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/Interruptor/Prefabs/InterruptorPared.prefab", interactivos, new Vector3(1.1f, 1.3f, -3.3f), 180f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/Ventilador/Prefabs/VentiladorTecho.prefab", interactivos, new Vector3(0f, 2.85f, 0f), 0f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/LuminariaInterior/Prefabs/PanelLED.prefab", interactivos, new Vector3(-1.8f, 2.75f, 0.5f), 0f, Vector3.one, isStatic: false, disableColliders: true);
            Place("Assets/ResourcePack/LuminariaInterior/Prefabs/PanelLED.prefab", interactivos, new Vector3(1.8f, 2.75f, 0.5f), 0f, Vector3.one, isStatic: false, disableColliders: true);

            Place("Assets/ResourcePack/Pizarra/Prefabs/PizarraBlanca.prefab", decor, new Vector3(0f, 1.35f, 3.25f), 180f, Vector3.one, isStatic: true, disableColliders: true);
            Place("Assets/ResourcePack/Proyector/Prefabs/ProyectorTecho.prefab", decor, new Vector3(0f, 2.65f, 1.6f), 0f, Vector3.one, isStatic: true, disableColliders: true);
            Place("Assets/ResourcePack/EscritorioDocente/Prefabs/EscritorioConCajones.prefab", decor, new Vector3(1.7f, 0f, 2.3f), 180f, Vector3.one, isStatic: true, disableColliders: true);
            PlaceNpc("Assets/ResourcePack/Docente/Prefabs/DocenteDePie.prefab", decor, new Vector3(1.7f, 0f, 1.7f), 190f, npcCtrl);

            for (int row = 0; row < 2; row++)
            {
                for (int col = -1; col <= 1; col += 2)
                {
                    float x = col * 1.4f;
                    float z = 0.5f - (row * 1.55f);
                    Place("Assets/ResourcePack/CarpetaEscolar/Prefabs/CarpetaDoble.prefab", decor, new Vector3(x, 0f, z), 0f, Vector3.one, isStatic: true, disableColliders: true);
                    PlaceNpc("Assets/ResourcePack/Estudiante/Prefabs/EstudianteSentado.prefab", decor, new Vector3(x - 0.35f, 0f, z - 0.35f), 0f, npcCtrl);
                }
            }

            return SaveZonePrefab(root, StructuresDir + "/PF_AulaB_Vestida.prefab");
        }

        // --- ZONA 3: LABORATORIO (Alta demanda técnica / equipos informáticos y eléctricos) ---
        private static GameObject BuildLaboratorio(RuntimeAnimatorController npcCtrl)
        {
            var root = new GameObject("PF_Laboratorio_Vestido");
            var estructura = CreateGroup(root.transform, "Arquitectura_Base");
            var interactivos = CreateGroup(root.transform, "Equipos_Interactivos");
            var decor = CreateGroup(root.transform, "Decoracion_NoBloqueante");

            Place("Assets/ResourcePack/EdificioLaboratorio/Prefabs/LaboratorioElectrico.prefab", estructura, Vector3.zero, 0f, Vector3.one, isStatic: true, disableColliders: false);
            Place("Assets/ResourcePack/Puerta/Prefabs/PuertaExteriorMetalica.prefab", interactivos, new Vector3(0f, 0f, -3.6f), 0f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/Ventilador/Prefabs/VentiladorIndustrial.prefab", interactivos, new Vector3(-2.6f, 2.2f, 2.8f), 135f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/Tomacorriente/Prefabs/TomacorrienteIndustrial.prefab", interactivos, new Vector3(-2.8f, 1.0f, 0f), 90f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/Tomacorriente/Prefabs/TomacorrienteIndustrial.prefab", interactivos, new Vector3(2.8f, 1.0f, 0f), -90f, Vector3.one, isStatic: false, disableColliders: false);

            Place("Assets/ResourcePack/Pizarra/Prefabs/PizarraDigital.prefab", decor, new Vector3(0f, 1.4f, 3.45f), 180f, Vector3.one, isStatic: true, disableColliders: true);
            for (int i = -1; i <= 1; i += 2)
            {
                Place("Assets/ResourcePack/CarpetaEscolar/Prefabs/CarpetaConBandeja.prefab", decor, new Vector3(i * 1.6f, 0f, 0.8f), 0f, Vector3.one, isStatic: true, disableColliders: true);
                Place("Assets/ResourcePack/Computadora/Prefabs/ComputadoraEscritorio.prefab", decor, new Vector3(i * 1.6f, 0.76f, 0.85f), 0f, Vector3.one, isStatic: true, disableColliders: true);
                Place("Assets/ResourcePack/Monitor/Prefabs/MonitorBasico.prefab", decor, new Vector3(i * 1.6f, 0.76f, 1.05f), 0f, Vector3.one, isStatic: true, disableColliders: true);
                Place("Assets/ResourcePack/Silla/Prefabs/SillaEscolar.prefab", decor, new Vector3(i * 1.6f, 0f, 0.25f), 0f, Vector3.one, isStatic: true, disableColliders: true);
            }
            PlaceNpc("Assets/ResourcePack/Estudiante/Prefabs/EstudianteDePie.prefab", decor, new Vector3(0.4f, 0f, 1.8f), 160f, npcCtrl);

            return SaveZonePrefab(root, StructuresDir + "/PF_Laboratorio_Vestido.prefab");
        }

        // --- ZONA 4: PATIO Y CAPTACIÓN SOLAR (Articulador abierto sin saturar la gestión) ---
        private static GameObject BuildPatio(RuntimeAnimatorController npcCtrl)
        {
            var root = new GameObject("PF_Patio_Vestido");
            var estructura = CreateGroup(root.transform, "Arquitectura_Base");
            var interactivos = CreateGroup(root.transform, "Equipos_Interactivos");
            var decor = CreateGroup(root.transform, "Decoracion_NoBloqueante");

            Place("Assets/ResourcePack/SueloPatio/Prefabs/SueloConcreto.prefab", estructura, Vector3.zero, 0f, new Vector3(4.5f, 1f, 3.5f), isStatic: true, disableColliders: false);
            Place("Assets/ResourcePack/CaminoPatio/Prefabs/CaminoAdoquinado.prefab", estructura, new Vector3(0f, 0.02f, -3.5f), 0f, new Vector3(1.2f, 1f, 2.2f), isStatic: true, disableColliders: false);
            Place("Assets/ResourcePack/AccesoEscolar/Prefabs/PuertaPeatonal.prefab", interactivos, new Vector3(0f, 0f, -6.2f), 0f, Vector3.one, isStatic: false, disableColliders: false);

            // Luminarias solares exteriores solo en esquinas para mantener el centro despejado
            Place("Assets/ResourcePack/LuminariaExterior/Prefabs/LuminariaSolarPoste.prefab", decor, new Vector3(-5.8f, 0f, -4.8f), 45f, Vector3.one, isStatic: true, disableColliders: true);
            Place("Assets/ResourcePack/LuminariaExterior/Prefabs/LuminariaSolarPoste.prefab", decor, new Vector3(5.8f, 0f, -4.8f), -45f, Vector3.one, isStatic: true, disableColliders: true);
            PlaceNpc("Assets/ResourcePack/Estudiante/Prefabs/EstudianteCaminando.prefab", decor, new Vector3(-1.8f, 0f, -1.5f), 75f, npcCtrl);

            return SaveZonePrefab(root, EnvDir + "/PF_Patio_Vestido.prefab");
        }

        // --- ZONA 5: SALA ELÉCTRICA (Núcleo de seguridad, cuchillas y almacenamiento) ---
        private static GameObject BuildSalaElectrica(RuntimeAnimatorController npcCtrl)
        {
            var root = new GameObject("PF_SalaElectrica_Vestida");
            var estructura = CreateGroup(root.transform, "Arquitectura_Base");
            var interactivos = CreateGroup(root.transform, "Equipos_Interactivos");
            var decor = CreateGroup(root.transform, "Decoracion_NoBloqueante");

            Place("Assets/ResourcePack/SalaElectrica/Prefabs/SalaElectricaAmplia.prefab", estructura, Vector3.zero, 0f, Vector3.one, isStatic: true, disableColliders: false);
            Place("Assets/ResourcePack/Puerta/Prefabs/PuertaCortafuego.prefab", interactivos, new Vector3(0f, 0f, -2.6f), 0f, Vector3.one, isStatic: false, disableColliders: false);

            // Pared Norte (fondo) con pasillo frontal libre de 2.5m para cámara 1ra persona y multímetro
            Place("Assets/ResourcePack/TablerosElectrico/Prefabs/TableroGeneralDistrib_Puerta.prefab", interactivos, new Vector3(-0.9f, 1.15f, 2.35f), 180f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/Interruptor/Prefabs/InterruptorSeccionador.prefab", interactivos, new Vector3(0.45f, 1.35f, 2.35f), 180f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/ControladorCarga/Prefabs/ControladorMPPT.prefab", interactivos, new Vector3(1.45f, 1.45f, 2.35f), 180f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/Inversor/Prefabs/InversorHibrido.prefab", interactivos, new Vector3(1.45f, 0.85f, 2.35f), 180f, Vector3.one, isStatic: false, disableColliders: false);
            Place("Assets/ResourcePack/Bateria/Prefabs/Bateria1.prefab", interactivos, new Vector3(2.1f, 0f, 1.7f), -90f, Vector3.one, isStatic: false, disableColliders: false);

            // Canalización y técnico supervisor
            Place("Assets/ResourcePack/Canaleta/Prefabs/CanaletaIndustrial.prefab", decor, new Vector3(0.2f, 2.15f, 2.38f), 180f, new Vector3(2.2f, 1f, 1f), isStatic: true, disableColliders: true);
            Place("Assets/ResourcePack/CajaConexion/Prefabs/CajaDerivacion.prefab", decor, new Vector3(-1.85f, 1.6f, 2.35f), 180f, Vector3.one, isStatic: true, disableColliders: true);
            PlaceNpc("Assets/ResourcePack/PersonalEscuela/Prefabs/PersonalMantenimiento.prefab", decor, new Vector3(-1.6f, 0f, 0.4f), 140f, npcCtrl);

            return SaveZonePrefab(root, ElectricalDir + "/PF_SalaElectrica_Vestida.prefab");
        }

        private static void AssembleMasterCampus(
            GameObject aulaA, GameObject aulaB, GameObject laboratorio,
            GameObject patio, GameObject salaElectrica)
        {
            string masterPath = StructuresDir + "/PF_CampusDefinitivo_LC15.prefab";
            var root = new GameObject("PF_CampusDefinitivo_LC15");

            // Integrar entorno exterior LC-10 si ya existe en el proyecto
            string lc10Path = EnvDir + "/PF_EntornoCampus_LC10.prefab";
            var lc10Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(lc10Path);
            if (lc10Prefab != null)
            {
                var envInst = (GameObject)PrefabUtility.InstantiatePrefab(lc10Prefab);
                envInst.name = "00_EntornoExterior_LC10";
                envInst.transform.SetParent(root.transform, false);
            }

            InstantiateZone(aulaA, root.transform, "01_Zona_AulaA", new Vector3(-7.5f, 0f, 5.5f), 0f);
            InstantiateZone(aulaB, root.transform, "02_Zona_AulaB", new Vector3(0.5f, 0f, 5.5f), 0f);
            InstantiateZone(laboratorio, root.transform, "03_Zona_Laboratorio", new Vector3(8.5f, 0f, 5.0f), 0f);
            InstantiateZone(patio, root.transform, "04_Zona_PatioYCaptacion", new Vector3(0f, 0f, -3.5f), 0f);
            InstantiateZone(salaElectrica, root.transform, "05_Zona_SalaElectrica", new Vector3(-8.5f, 0f, -3.5f), 0f);

            PrefabUtility.SaveAsPrefabAsset(root, masterPath);
            Object.DestroyImmediate(root);
        }

        private static Transform CreateGroup(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void InstantiateZone(GameObject zonePrefab, Transform parent, string name, Vector3 pos, float rotY)
        {
            if (zonePrefab == null) return;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(zonePrefab);
            inst.name = name;
            inst.transform.SetParent(parent, false);
            inst.transform.localPosition = pos;
            inst.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);
        }

        private static void Place(
            string assetPath, Transform parent, Vector3 pos, float rotY, Vector3 scale,
            bool isStatic, bool disableColliders)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[LC-15] Asset no encontrado: {assetPath}. Ejecuta 'git lfs pull'.");
                return;
            }

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(parent, false);
            inst.transform.localPosition = pos;
            inst.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);
            inst.transform.localScale = scale;

            if (disableColliders)
            {
                foreach (Collider col in inst.GetComponentsInChildren<Collider>(true))
                    col.enabled = false;
            }

            if (isStatic)
            {
                SetStaticRecursive(inst,
                    StaticEditorFlags.BatchingStatic |
                    StaticEditorFlags.OccludeeStatic |
                    StaticEditorFlags.ContributeGI);
            }
        }

        private static void PlaceNpc(string assetPath, Transform parent, Vector3 pos, float rotY, RuntimeAnimatorController ctrl)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) return;

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(parent, false);
            inst.transform.localPosition = pos;
            inst.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);

            foreach (Collider col in inst.GetComponentsInChildren<Collider>(true))
                col.enabled = false;

            if (ctrl != null)
            {
                var anim = inst.GetComponent<Animator>() ?? inst.AddComponent<Animator>();
                anim.runtimeAnimatorController = ctrl;
                anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }
        }

        private static GameObject SaveZonePrefab(GameObject root, string path)
        {
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return saved;
        }

        private static void SetStaticRecursive(GameObject go, StaticEditorFlags flags)
        {
            GameObjectUtility.SetStaticEditorFlags(go, flags);
            foreach (Transform child in go.transform)
                SetStaticRecursive(child.gameObject, flags);
        }
    }
}
