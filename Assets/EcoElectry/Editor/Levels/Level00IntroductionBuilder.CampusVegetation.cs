using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level00IntroductionBuilder
    {
        [MenuItem("Tools/EcoElectry/Nivel 00/Actualizar vegetación del campus")]
        internal static void RefreshCampusVegetation()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Abre el nivel 00 en modo edición para actualizar su vegetación.");

            Transform decoration = FindCampusDecoration(scene);
            if (decoration == null)
                throw new InvalidOperationException("El campus necesita sus jardineras y su vegetación de perímetro.");

            var plants = new NatureCampusPlants();
            var replacements = new Dictionary<string, NaturePlantSet>(StringComparer.Ordinal)
            {
                ["Assets/ResourcePack/Arbusto/Prefabs/ArbustoFlorido.prefab"] = plants.Flowers,
                ["Assets/ResourcePack/Arbusto/Prefabs/ArbustoBajo.prefab"] = plants.Shrubs,
                ["Assets/ResourcePack/Arbusto/Prefabs/Seto.prefab"] = plants.Hedges,
                [ThirdPartyRoot + "/FantasyForest/Prefabs/grass01.prefab"] = plants.Grass
            };

            Undo.IncrementCurrentGroup();
            int undo = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Actualizar vegetación del campus");
            int count = 0;
            foreach (Transform previous in decoration.GetComponentsInChildren<Transform>(true))
            {
                if (previous == null || !PrefabUtility.IsAnyPrefabInstanceRoot(previous.gameObject)) continue;
                string source = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(previous.gameObject);
                if (string.IsNullOrEmpty(source) || !replacements.TryGetValue(source, out NaturePlantSet group)) continue;

                GameObject next = group.Place(previous.parent, previous.position, previous.rotation, previous.localScale.x);
                next.transform.SetSiblingIndex(previous.GetSiblingIndex());
                next.SetActive(previous.gameObject.activeSelf);
                PrefabUtility.RecordPrefabInstancePropertyModifications(next);
                Undo.RegisterCreatedObjectUndo(next, "Colocar planta de Nature");
                Undo.DestroyObjectImmediate(previous.gameObject);
                count++;
            }
            Undo.CollapseUndoOperations(undo);
            if (count == 0) return;
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("No se pudo guardar la vegetación del campus.");
            Debug.Log($"Nivel 00: {count} plantas del campus sustituidas por prefabs de Nature.");
        }

        [MenuItem("Tools/EcoElectry/Nivel 00/Estabilizar arbustos del campus")]
        internal static void StabilizeCampusBushes()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Abre el nivel 00 en modo edición para estabilizar los arbustos.");

            Transform decoration = FindCampusDecoration(scene);
            if (decoration == null)
                throw new InvalidOperationException("No se encontró la decoración del campus.");

            int count = 0;
            foreach (LODGroup group in decoration.GetComponentsInChildren<LODGroup>(true))
            {
                if (UseStillBushMaterials(group.gameObject))
                    count++;
            }
            if (count == 0) return;

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("No se pudieron guardar los materiales de los arbustos.");
            Debug.Log($"Nivel 00: se desactivó el viento y la deformación de {count} arbustos.");
        }

        private static Transform FindCampusDecoration(Scene scene)
        {
            GameObject level = Array.Find(scene.GetRootGameObjects(), root => root.name == RootName);
            return level != null ? level.transform.Find("SchoolExterior/" + CampusDecorationName) : null;
        }

        private static bool UseStillBushMaterials(GameObject plant)
        {
            const string sourceFolder = ThirdPartyRoot + "/Nature/Materials/Bushes/";
            bool updated = false;
            foreach (MeshRenderer renderer in plant.GetComponentsInChildren<MeshRenderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material original = materials[i];
                    if (original == null || !AssetDatabase.GetAssetPath(original).StartsWith(sourceFolder, StringComparison.Ordinal))
                        continue;

                    EnsureFolder(MaterialsFolder);
                    string path = $"{MaterialsFolder}/M_Level00_{original.name}_Still.mat";
                    Material stable = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (stable == null)
                    {
                        stable = new Material(original) { name = $"M_Level00_{original.name}_Still" };
                        stable.SetFloat("_Wind_Strength", 0f);
                        stable.SetFloat("_Wind_Speed", 0f);
                        stable.SetFloat("_Bend_Strength", 0f);
                        AssetDatabase.CreateAsset(stable, path);
                    }
                    materials[i] = stable;
                    changed = true;
                }
                if (!changed) continue;

                Undo.RecordObject(renderer, "Material estático del arbusto");
                renderer.sharedMaterials = materials;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                updated = true;
            }
            return updated;
        }

        private sealed class NatureCampusPlants
        {
            public readonly NaturePlantSet Flowers = new NaturePlantSet(Vector3.one * 1.20f,
                "Flower_Pink", "Flower_White", "Flower_Purple", "Flower_Blue_01");
            public readonly NaturePlantSet Shrubs = new NaturePlantSet(Vector3.one * 0.45f,
                "Bush_03_01", "Bush_03_02", "Bush_02_01");
            public readonly NaturePlantSet Hedges = new NaturePlantSet(new Vector3(0.60f, 0.52f, 0.41f),
                "Bush_01_01", "Bush_02_01");
            public readonly NaturePlantSet Grass = new NaturePlantSet(Vector3.one * 0.61f,
                "Grass_01", "Grass_02", "Grass_03");
        }

        private sealed class NaturePlantSet
        {
            private readonly GameObject[] prefabs;
            private readonly Vector3 scale;

            public NaturePlantSet(Vector3 scale, params string[] names)
            {
                this.scale = scale;
                prefabs = Array.ConvertAll(names, name => LoadPrefab(ThirdPartyRoot + "/Nature/Prefabs/" + name + ".prefab"));
            }

            public void Plant(Transform parent, float x, float z, float multiplier, float yaw)
            {
                Place(parent, parent.TransformPoint(x, 0f, z),
                    parent.rotation * Quaternion.Euler(0f, yaw, 0f), multiplier);
            }

            public GameObject Place(Transform parent, Vector3 position, Quaternion rotation, float multiplier)
            {
                int index = Mathf.Abs(Mathf.RoundToInt(position.x * 23f + position.z * 59f)) % prefabs.Length;
                GameObject prefab = prefabs[index];
                GameObject instance = PlacePrefab(prefab, parent, position, rotation.eulerAngles.y, scale * multiplier, prefab.name);
                instance.transform.rotation = rotation;
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
                DisablePlantColliders(instance);

                // Los arbustos del paquete incluyen un disparador y partículas para la caída de hojas.
                foreach (MonoBehaviour behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour == null || !behaviour.enabled) continue;
                    behaviour.enabled = false;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(behaviour);
                }
                foreach (ParticleSystem particles in instance.GetComponentsInChildren<ParticleSystem>(true))
                {
                    particles.gameObject.SetActive(false);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(particles.gameObject);
                }
                UseStillBushMaterials(instance);
                return instance;
            }
        }
    }
}
