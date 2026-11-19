using System;
using System.Collections.Generic;
using EcoElectry.Unity.Levels;
using UnityEditor;
using UnityEngine;

namespace EcoElectry.Editor.Levels
{
    public static class LevelDefinitionValidator
    {
        [MenuItem("Tools/EcoElectry/Validar definiciones de nivel")]
        private static void ValidateDefinitions()
        {
            string[] definitionGuids = AssetDatabase.FindAssets("t:LevelDefinition");
            var definitionPathsById = new Dictionary<string, string>(StringComparer.Ordinal);
            int errorCount = 0;

            foreach (string guid in definitionGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var definition = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
                if (definition == null)
                {
                    Debug.LogError($"No se pudo cargar la definición de nivel de {path}.");
                    errorCount++;
                    continue;
                }

                try
                {
                    string id = definition.CreateRules().Id;
                    if (definitionPathsById.TryGetValue(id, out string previousPath))
                    {
                        Debug.LogError($"El identificador «{id}» también se usa en {previousPath}.", definition);
                        errorCount++;
                    }
                    else
                    {
                        definitionPathsById.Add(id, path);
                    }
                }
                catch (ArgumentException exception)
                {
                    Debug.LogError(exception.Message, definition);
                    errorCount++;
                }
            }

            if (errorCount == 0)
                Debug.Log($"Definiciones de nivel revisadas: {definitionGuids.Length}.");
        }
    }
}
