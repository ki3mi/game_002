using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace EcoElectry.Editor.Levels
{
    [InitializeOnLoad]
    internal static class Level00SceneViewClipping
    {
        private const float NearClip = 0.05f;
        private const float FarClip = 6500f;
        private static readonly Dictionary<SceneView, (float Near, float Far)> Previous = new();

        static Level00SceneViewClipping()
        {
            SceneView.beforeSceneGui += UpdateClipping;
            AssemblyReloadEvents.beforeAssemblyReload += RestoreAll;
            EditorApplication.quitting += RestoreAll;
        }

        private static void UpdateClipping(SceneView view)
        {
            if (SceneManager.GetActiveScene().path != Level00IntroductionBuilder.ScenePath)
            {
                if (Previous.Remove(view, out var prior)) Restore(view, prior);
                return;
            }

            var settings = view.cameraSettings;
            if (!settings.dynamicClip || Previous.ContainsKey(view)) return;

            // The automatic near plane grows with the view size and cuts away nearby surfaces on this large map.
            Previous.Add(view, (settings.nearClip, settings.farClip));
            settings.dynamicClip = false;
            settings.nearClip = NearClip;
            settings.farClip = FarClip;
        }

        private static void RestoreAll()
        {
            foreach (var (view, prior) in Previous)
                if (view != null) Restore(view, prior);
            Previous.Clear();
        }

        private static void Restore(SceneView view, (float Near, float Far) prior)
        {
            var settings = view.cameraSettings;
            // Preserve manual adjustments made while inspecting the level.
            if (settings.dynamicClip || settings.nearClip != NearClip || settings.farClip != FarClip) return;
            settings.nearClip = prior.Near;
            settings.farClip = prior.Far;
            settings.dynamicClip = true;
        }
    }
}
