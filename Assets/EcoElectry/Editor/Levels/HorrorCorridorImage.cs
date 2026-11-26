using System;
using EcoElectry.Unity.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EcoElectry.Editor.Levels
{
    internal static class HorrorCorridorImage
    {
        private const string Folder = "Assets/EcoElectry/Content/Rendering";
        private const string ProfilePath = Folder + "/HorrorCorridorProfile.asset";

        internal static void Build(Camera camera, VolumeProfile profile = null)
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(Folder + "/CorridorSignal.shader");
            if (shader == null) throw new InvalidOperationException("Missing corridor signal shader.");

            AddVolume(camera, profile ?? LoadProfile());
            var signal = new SerializedObject(camera.gameObject.AddComponent<CorridorSignalGlitch>());
            signal.FindProperty("signalShader").objectReferenceValue = shader;
            signal.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void AddVolume(Camera camera, VolumeProfile profile)
        {
            var data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            int volumeLayer = LayerMask.NameToLayer("TransparentFX");
            data.volumeLayerMask = 1 << volumeLayer;

            var image = new GameObject("Perfil de imagen") { layer = volumeLayer };
            image.transform.SetParent(camera.transform, false);
            var volume = image.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        private static VolumeProfile LoadProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile != null) return profile;

            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            FilmGrain grain = Add<FilmGrain>(profile);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(0.29f);
            grain.response.Override(0.72f);
            Vignette vignette = Add<Vignette>(profile);
            vignette.intensity.Override(0.24f);
            vignette.smoothness.Override(0.48f);
            ColorAdjustments colors = Add<ColorAdjustments>(profile);
            colors.saturation.Override(-29f);
            colors.postExposure.Override(0f);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
            return profile;
        }

        private static T Add<T>(VolumeProfile profile) where T : VolumeComponent
        {
            T component = profile.Add<T>();
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }
    }
}
