using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace EcoElectry.Editor.Levels
{
    [InitializeOnLoad]
    internal static class LevelSceneViewClipping
    {
        private const string InteriorScenePath = "Assets/EcoElectry/Content/Scenes/Level01.unity";
        private static readonly Dictionary<SceneView, ((float Near, float Far) Prior, (float Near, float Far) Applied)> Previous = new();

        static LevelSceneViewClipping()
        {
            SceneView.beforeSceneGui += UpdateClipping;
            AssemblyReloadEvents.beforeAssemblyReload += RestoreAll;
            EditorApplication.quitting += RestoreAll;
        }

        private static void UpdateClipping(SceneView view)
        {
            (float Near, float Far)? clipping = SceneManager.GetActiveScene().path switch
            {
                Level00IntroductionBuilder.ScenePath => (0.05f, 6500f),
                InteriorScenePath => (0.025f, 250f),
                _ => null
            };

            if (Previous.TryGetValue(view, out var previous))
            {
                if (clipping == previous.Applied) return;

                Restore(view, previous.Prior, previous.Applied);
                Previous.Remove(view);
            }

            var settings = view.cameraSettings;
            if (clipping is not { } level || !settings.dynamicClip) return;

            // Dynamic clipping pushes the near plane away when the Scene view is framed on the whole level.
            Previous.Add(view, ((settings.nearClip, settings.farClip), level));
            settings.dynamicClip = false;
            settings.nearClip = level.Near;
            settings.farClip = level.Far;
        }

        private static void RestoreAll()
        {
            foreach (var (view, previous) in Previous)
                if (view != null) Restore(view, previous.Prior, previous.Applied);
            Previous.Clear();
        }

        private static void Restore(SceneView view, (float Near, float Far) prior, (float Near, float Far) applied)
        {
            var settings = view.cameraSettings;
            // Preserve manual adjustments made while inspecting the level.
            if (settings.dynamicClip || settings.nearClip != applied.Near || settings.farClip != applied.Far) return;
            settings.nearClip = prior.Near;
            settings.farClip = prior.Far;
            settings.dynamicClip = true;
        }
    }
}
