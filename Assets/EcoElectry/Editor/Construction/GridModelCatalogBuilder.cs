using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EcoElectry.Unity.Construction;
using EcoElectry.Unity.Electrical;
using UnityEditor;
using UnityEngine;

namespace EcoElectry.Editor.Construction
{
    internal static class GridModelCatalogBuilder
    {
        private const string Definitions = "Assets/EcoElectry/Content/Definitions";
        private const string CatalogFolder = Definitions + "/Construction";
        private const string CatalogPath = CatalogFolder + "/GridModelCatalog.asset";
        private const string SandboxPath = Definitions + "/Levels/SandboxElectrical.asset";
        private const string ShaderPath = "Assets/EcoElectry/Runtime/Unity/Construction/GridOverlay.shader";
        private const float MaxBaseSize = 4f;
        private const float MaxHeight = 4.5f;

        private static readonly string[] SourceFolders =
        {
            "Assets/ResourcePack", "Assets/EcoElectry/Content/Prefabs", "Assets/EcoElectry/Content/Models"
        };

        private static readonly string[] Pinned =
        {
            "SillaEscolar", "PanelSolar1", "Bateria1", "ArbolPequeno", "PuertaInterior", "TuboLED"
        };

        [MenuItem("Tools/EcoElectry/Actualizar catálogo de modelos")]
        private static void Build()
        {
            var sandbox = AssetDatabase.LoadAssetAtPath<SandboxDefinition>(SandboxPath);
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            if (sandbox == null || shader == null)
                throw new InvalidOperationException("Falta la definición del sandbox o el shader de la grilla.");

            string[] folders = SourceFolders.Where(AssetDatabase.IsValidFolder).ToArray();
            string[] paths = AssetDatabase.FindAssets("", folders).Select(AssetDatabase.GUIDToAssetPath).ToArray();
            string[] prefabs = paths.Where(path => HasExtension(path, ".prefab")).ToArray();
            var coveredModels = new HashSet<string>(prefabs.Select(DesignKey), StringComparer.OrdinalIgnoreCase);
            string[] sources = prefabs.Concat(paths.Where(path => IsModelPath(path) && !coveredModels.Contains(DesignKey(path))))
                .OrderBy(StarterRank)
                .ThenBy(Family, StringComparer.OrdinalIgnoreCase)
                .ThenBy(path => Path.GetFileNameWithoutExtension(path), StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var usable = new List<(string Path, GameObject Source, Bounds Bounds)>();
            foreach (string path in sources)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model != null && GridModelVisual.TryGetBounds(model, out Bounds bounds))
                    usable.Add((path, model, bounds));
                else
                    Debug.LogWarning($"El catálogo requiere una malla visible en {path}.");
            }

            if (usable.Count == 0) throw new InvalidOperationException("No se encontraron modelos para la grilla.");
            var catalog = AssetDatabase.LoadAssetAtPath<GridModelCatalog>(CatalogPath);
            if (catalog == null)
            {
                if (!AssetDatabase.IsValidFolder(CatalogFolder)) AssetDatabase.CreateFolder(Definitions, "Construction");
                catalog = ScriptableObject.CreateInstance<GridModelCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var data = new SerializedObject(catalog);
            data.FindProperty("previewShader").objectReferenceValue = shader;
            SerializedProperty models = data.FindProperty("models");
            models.arraySize = usable.Count;
            for (int index = 0; index < usable.Count; index++)
            {
                var (path, source, bounds) = usable[index];
                SerializedProperty entry = models.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("id").stringValue = AssetDatabase.AssetPathToGUID(path);
                entry.FindPropertyRelative("label").stringValue = DisplayName(Path.GetFileNameWithoutExtension(path));
                entry.FindPropertyRelative("family").stringValue = DisplayName(Family(path));
                entry.FindPropertyRelative("source").objectReferenceValue = source;
                entry.FindPropertyRelative("visualBounds").boundsValue = bounds;

                Vector3 size = bounds.size;
                float horizontal = Mathf.Max(0.001f, size.x, size.z);
                entry.FindPropertyRelative("scale").floatValue =
                    Mathf.Min(1f, MaxBaseSize / horizontal, MaxHeight / Mathf.Max(0.001f, size.y));
            }

            data.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(catalog);

            var definition = new SerializedObject(sandbox);
            definition.FindProperty("placementCatalog").objectReferenceValue = catalog;
            definition.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(sandbox);
            Debug.Log($"Catálogo de grilla actualizado con {usable.Count} modelos.", catalog);
        }

        private static string DesignKey(string path)
        {
            return Family(path) + "/" + Path.GetFileNameWithoutExtension(path);
        }

        private static string Family(string path)
        {
            string[] parts = path.Split('/');
            if (parts[1] == "ResourcePack") return parts.Length > 3 ? parts[2] : "General";
            return parts.Length > 5 ? parts[4] : "General";
        }

        private static int StarterRank(string path)
        {
            int rank = Array.IndexOf(Pinned, Path.GetFileNameWithoutExtension(path));
            return rank < 0 ? Pinned.Length : rank;
        }

        private static bool HasExtension(string path, string extension)
        {
            return string.Equals(Path.GetExtension(path), extension, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsModelPath(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".fbx":
                case ".obj":
                case ".blend":
                case ".dae":
                    return true;
                default:
                    return false;
            }
        }

        private static string DisplayName(string value)
        {
            string separated = Regex.Replace(value.Replace('_', ' '),
                @"(?<=[a-záéíóú0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|(?<=[A-Za-z])(?=[0-9])", " ");
            return separated.Replace("Arbol", "Árbol").Replace("Pequeno", "Pequeño").Replace("Bateria", "Batería")
                .Replace("Electrica", "Eléctrica").Replace("Electrico", "Eléctrico").Replace("Montana", "Montaña")
                .Replace("Conexion", "Conexión").Replace("Proyeccion", "Proyección");
        }
    }
}
