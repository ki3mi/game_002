using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level00IntroductionBuilder
    {
        private const string CobblestonePrefab = "Assets/ResourcePack/CaminoPatio/Prefabs/CaminoAdoquinado.prefab";
        private const string WalkwayRootName = "SenderoPeatonal";
        private const float WalkwayWidth = 1.65f;
        private const float WalkwaySurface = ParkingSurface + 0.065f;
        private const float FrontWalkwayZ = 3464.45f;

        [MenuItem("Tools/EcoElectry/Nivel 00/Actualizar caminos adoquinados")]
        internal static void RefreshCobblestonePaths()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Abre el nivel 00 en modo edición para actualizar los caminos.");

            GameObject level = Array.Find(scene.GetRootGameObjects(), root => root.name == RootName);
            Transform school = level != null ? level.transform.Find("SchoolExterior") : null;
            if (school == null) throw new InvalidOperationException("No se encontró el exterior del colegio.");

            BuildCobblestonePaths(school);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("No se pudieron guardar los caminos adoquinados.");
        }

        private static void BuildCobblestonePaths(Transform school)
        {
            GameObject prefab = LoadPrefab(CobblestonePrefab);
            MeshCollider collider = prefab.GetComponent<MeshCollider>();
            if (collider == null || collider.sharedMesh == null)
                throw new InvalidOperationException("El módulo de adoquines requiere su malla de colisión.");
            Bounds module = collider.sharedMesh.bounds;

            Transform existing = school.Find(WalkwayRootName);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
            Scene scene = school.gameObject.scene;
            Transform paths = Group(scene, school, WalkwayRootName);

            Transform entrance = Group(scene, paths, "Entrada del colegio");
            var gateLanding = new Vector3(PedestrianGateX, WalkwaySurface, 3393f);
            TileWalkway(prefab, module, entrance,
                new Vector3(PedestrianGateX, CampusGrade + 0.04f, GateZ - 2f), gateLanding);
            TileWalkway(prefab, module, entrance, gateLanding,
                new Vector3(PedestrianGateX, WalkwaySurface, 3465.04f));

            Transform guard = Group(scene, paths, "Puesto de vigilancia");
            TileWalkway(prefab, module, guard,
                new Vector3(GuardX - 0.85f, WalkwaySurface, GuardZ - 3f),
                new Vector3(PedestrianGateX - WalkwayWidth * 0.5f, WalkwaySurface, GuardZ - 3f));

            Transform workshop = Group(scene, paths, "Área de mantenimiento");
            TileWalkway(prefab, module, workshop,
                new Vector3(PedestrianGateX + WalkwayWidth * 0.5f, WalkwaySurface, FrontWalkwayZ),
                new Vector3(WorkshopX - WorkshopLandingWidth * 0.5f, WalkwaySurface, FrontWalkwayZ));
        }

        private static void TileWalkway(GameObject prefab, Bounds module, Transform route, Vector3 start, Vector3 end)
        {
            Vector3 span = end - start;
            float length = span.magnitude;
            int count = Mathf.Max(1, Mathf.RoundToInt(length / module.size.z));
            Quaternion rotation = Quaternion.LookRotation(span, Vector3.up);
            // Let the base reach below the campus grade while retaining the walking surface height.
            Vector3 scale = new Vector3(WalkwayWidth / module.size.x, 1.5f, length / (count * module.size.z));
            Vector3 top = new Vector3(module.center.x, module.max.y, module.center.z);
            Vector3 topOffset = rotation * Vector3.Scale(top, scale);

            for (int i = 0; i < count; i++)
            {
                Vector3 center = start + span * ((i + 0.5f) / count);
                GameObject tile = PlacePrefab(prefab, route, center - topOffset, rotation.eulerAngles.y, scale,
                    $"Adoquines {route.childCount + 1:00}");
                tile.transform.rotation = rotation;
                PrefabUtility.RecordPrefabInstancePropertyModifications(tile.transform);
            }
        }
    }
}
