using System;
using System.Collections.Generic;
using System.Linq;
using EcoElectry.Unity.Levels;
using EcoElectry.Unity.Player;
using EcoElectry.Unity.Survival;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace EcoElectry.Editor.Levels
{
    internal static partial class Level00IntroductionBuilder
    {
        private const string TimelineFolder = "Assets/EcoElectry/Content/Timeline/Level00";
        private const string SoundFolder = "Assets/EcoElectry/Content/Audio/Level00";
        private const string CallFolder = SoundFolder + "/SoundTracks";
        private const string CinematicsName = "Cinemáticas";
        private const string OpeningViewName = "Seguimiento de la camioneta";

        [MenuItem("Tools/EcoElectry/Nivel 00/Actualizar cinemáticas")]
        internal static void RefreshIntroductionCinematics()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || scene.isDirty || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Abre el Nivel 0 guardado en modo edición.");

            GameObject levelRoot = Array.Find(scene.GetRootGameObjects(), root => root.name == RootName);
            if (levelRoot == null) throw new InvalidOperationException("No se encontró el mapa del Nivel 0.");
            Transform level = levelRoot.transform;
            Transform rig = RequiredChild(level, CabRigName);
            Transform truck = RequiredChild(rig, DriveTruckName);
            var arrival = RequiredChild(rig, ArrivalZoneName).GetComponent<VehicleArrivalTransition>();
            var sky = RequiredChild(level, "Lighting/Luna").GetComponent<IntroductionSkyCycle>();
            ConfigureIntroductionCinematics(level, rig, truck.GetComponent<IntroductionTruckDrive>(),
                RequiredChild(truck, CabinCameraName).GetComponent<Camera>(), arrival, sky,
                RequiredChild(level, "Markers/VehicleParking"), GetIntroductionNightProfile());
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("No se pudieron guardar las cinemáticas.");
        }

        private static void ConfigureIntroductionCinematics(Transform level, Transform rig, IntroductionTruckDrive truck,
            Camera cockpit, VehicleArrivalTransition arrival, IntroductionSkyCycle sky, Transform parking, VolumeProfile profile)
        {
            EnsureFolder(TimelineFolder);
            Transform school = RequiredChild(level, "SchoolExterior");
            Transform guard = RequiredChild(school, "CasetaVigilancia");
            Transform path = RequiredChild(school, "CaminoMantenimiento");
            Transform workshop = RequiredChild(level, "Maintenance/SalaDeMantenimiento");
            RemoveChild(rig, CinematicsName);
            RemoveChild(truck.transform, OpeningViewName);
            Transform root = Group(level.gameObject.scene, rig, CinematicsName);
            root.gameObject.isStatic = false;
            var sequence = root.gameObject.AddComponent<IntroductionCinematics>();

            Camera openingView = CreateCinematicView(truck.transform, OpeningViewName, profile);
            Camera arrivalView = CreateCinematicView(root, "Vista exterior del colegio", profile);
            PlayableDirector opening = BuildSequence(root, "01-Salida", openingView, 7f, OpeningShots(),
                (5.6f, "Corte a cabina", sequence.CutToCabin), (6.95f, "Entrega del volante", sequence.DeliverDriving));

            Vector3 truckSpot = parking.position;
            Vector3 faculty = new Vector3(SchoolX, truckSpot.y + 5.5f, 3476f);
            Vector3 door = workshop.TransformPoint(0f, 2.08f, -3.90f);
            Vector3 trailStart = path.position + new Vector3(-23f, 2.5f, -4.25f);
            Vector3 trailEnd = door + new Vector3(-0.8f, 0.45f, -6.0f);
            Pose doorFrame = LocalLook(root, trailEnd, door);
            float gateCut = IntroductionCinematics.ArrivalCabinDuration;
            float parkedCut = IntroductionCinematics.ArrivalParkedTime;
            PlayableDirector arrivalDirector = BuildSequence(root, "02-Llegada", arrivalView, 21f, new[]
            {
                new Shot("Frontis del colegio", gateCut, parkedCut,
                    LocalLook(root, new Vector3(SchoolX - 1.1f, truckSpot.y + 14.2f, 3440f), faculty),
                    LocalLook(root, new Vector3(SchoolX + 1.15f, truckSpot.y + 12.3f, 3446.5f), faculty)),
                new Shot("Camioneta estacionada", parkedCut, IntroductionCinematics.GuardShotStart,
                    LocalLook(root, truckSpot + new Vector3(-6.6f, 7.4f, -4.5f), truckSpot + Vector3.up),
                    LocalLook(root, truckSpot + new Vector3(-4.9f, 6.7f, -3.4f), truckSpot + Vector3.up)),
                new Shot("Ventanilla de vigilancia", IntroductionCinematics.GuardShotStart, IntroductionCinematics.GuardShotEnd,
                    LocalPose(root, IntroductionCinematics.GuardShotPose(guard, 0f)),
                    LocalPose(root, IntroductionCinematics.GuardShotPose(guard, 1f))),
                new Shot("Camino y puerta de mantenimiento", IntroductionCinematics.GuardShotEnd, 19.85f,
                    LocalLook(root, trailStart, door + Vector3.left * 2.7f), doorFrame),
                new Shot("Entrega junto a la camioneta", 19.85f, 21f, doorFrame, doorFrame)
            }, (gateCut, "Corte al frontis del colegio", sequence.CutToFacade),
                (19.85f, "Corte junto a la puerta del conductor", sequence.CutToExit),
                (20.95f, "Entrega a pie", sequence.DeliverWalking));

            Vector3 workLight = workshop.TransformPoint(2.65f, 3.16f, -4.65f);
            Light signLight = AddIntroductionLight(root, "Luz del acceso de mantenimiento", workLight,
                Quaternion.LookRotation(door - workLight));
            TuneIntroductionSpot(signLight, new Color32(255, 217, 172, 255), 29f, 7f, 46f, 94f, false, 0);

            Material housing = AssetDatabase.LoadAssetAtPath<Material>(TerrainFolder + "/M_Level00_Steel.mat");
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 fixture = new Vector3(SchoolX + side * 20f, ParkingSurface + 0.21f, 3462.4f);
                Vector3 wall = new Vector3(SchoolX + side * 21f, faculty.y - 1f, 3468.5f);
                Block(root, "Proyector de fachada", root.InverseTransformPoint(fixture - Vector3.up * 0.11f),
                    new Vector3(0.32f, 0.18f, 0.28f), housing, false);
                Light wash = AddIntroductionLight(root, "Luz baja de fachada", fixture, Quaternion.LookRotation(wall - fixture));
                TuneIntroductionSpot(wash, new Color32(215, 222, 237, 255), 145f, 15f, 68f, 104f, false, 0);
            }

            AudioSource motor = CreateLoop(root, "Motor eléctrico", "MotorElectrico", 0.20f);
            AudioSource forest = CreateLoop(root, "Ambiente del bosque", "BosqueNocturno", 0.085f);
            var setup = new SerializedObject(sequence);
            setup.FindProperty("truck").objectReferenceValue = truck;
            setup.FindProperty("sky").objectReferenceValue = sky;
            setup.FindProperty("arrival").objectReferenceValue = arrival;
            setup.FindProperty("opening").objectReferenceValue = opening;
            setup.FindProperty("arrivalSequence").objectReferenceValue = arrivalDirector;
            setup.FindProperty("openingView").objectReferenceValue = openingView;
            setup.FindProperty("arrivalView").objectReferenceValue = arrivalView;
            setup.FindProperty("cockpit").objectReferenceValue = cockpit.gameObject;
            setup.FindProperty("motor").objectReferenceValue = motor;
            setup.FindProperty("forest").objectReferenceValue = forest;
            setup.FindProperty("guardWaypoint").objectReferenceValue = guard;
            setup.FindProperty("maintenanceWaypoint").objectReferenceValue = RequiredChild(workshop, "Acceso/Placa de sala");
            setup.FindProperty("tutorialSettings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SurvivalSettings>(
                "Assets/EcoElectry/Content/Definitions/Survival/Level01Survival.asset");
            setup.FindProperty("tutorialModels").objectReferenceValue = AssetDatabase.LoadAssetAtPath<InspectionModels>(
                "Assets/EcoElectry/Content/Definitions/Survival/InspectionModels.asset");
            setup.FindProperty("tutorialPreviewMaterial").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/EcoElectry/Content/Models/HorrorCorridor/Iron.mat");
            ConfigureDrivingCall(setup.FindProperty("guilleCallClips"));
            setup.ApplyModifiedPropertiesWithoutUndo();

            var arrivalSetup = new SerializedObject(arrival);
            arrivalSetup.FindProperty("cinematics").objectReferenceValue = sequence;
            ConfigureHighwayArrival(root, parking, arrivalSetup);
            arrivalSetup.ApplyModifiedPropertiesWithoutUndo();
            openingView.gameObject.SetActive(false);
            arrivalView.gameObject.SetActive(false);
        }

        private static void ConfigureDrivingCall(SerializedProperty clips)
        {
            string[] names =
            {
                "Asignacion", "Colegio_Aislado", "Contrato_y_Estancia", "Turno_Nocturno", "Advertencia_y_Llegada",
                "Despues_de_Estacionar", "Indicaciones_Caseta"
            };
            clips.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++)
            {
                string path = $"{CallFolder}/L00_Llamada_{i + 1:00}_{names[i]}.wav";
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) throw new InvalidOperationException("Falta el audio " + path);
                clips.GetArrayElementAtIndex(i).objectReferenceValue = clip;
            }
        }

        private static void ConfigureHighwayArrival(Transform root, Transform parking, SerializedObject setup)
        {
            PathData road = BuildPath(RoadControls, 4f);
            var points = new List<Vector3>();
            for (float z = FourthApproachLightZ; z < GateZ; z += 0.75f)
                points.Add(RoadAtZ(road, z));
            Vector3 gate = road.Points[^1];
            points.Add(gate);
            int gateSample = points.Count - 1;

            float y = parking.position.y;
            PathData manoeuvre = BuildPath(new[]
            {
                gate,
                new Vector3(GateX - 0.5f, y, GateZ + 8f),
                new Vector3(GateX - 4.6f, y, GateZ + 15.5f),
                parking.position + new Vector3(-0.1f, 0f, -9.1f),
                parking.position + new Vector3(-7.3f, 0f, -5.8f),
                parking.position + new Vector3(-7.5f, 0f, -1.8f),
                parking.position + new Vector3(-4.25f, 0f, -0.1f),
                parking.position
            }, 0.55f);
            points.AddRange(manoeuvre.Points.Skip(1));

            Transform marker = Group(root.gameObject.scene, root, "Inicio de llegada · cuarto poste");
            marker.SetPositionAndRotation(points[0], Quaternion.LookRotation(FlatDirection(points[0], points[1])));
            setup.FindProperty("highwayArrivalStart").objectReferenceValue = marker;
            setup.FindProperty("highwayGateSample").intValue = gateSample;
            SerializedProperty route = setup.FindProperty("highwayArrivalRoute");
            route.arraySize = points.Count;
            for (int i = 0; i < points.Count; i++)
                route.GetArrayElementAtIndex(i).vector3Value = points[i];
        }

        private static Camera CreateCinematicView(Transform parent, string name, VolumeProfile profile)
        {
            Camera view = CreateIntroductionCamera(parent, name, Vector3.zero, 60f);
            view.gameObject.isStatic = false;
            view.gameObject.AddComponent<Animator>().cullingMode = AnimatorCullingMode.AlwaysAnimate;
            HorrorCorridorImage.AddVolume(view, profile);
            return view;
        }

        private static AudioSource CreateLoop(Transform parent, string name, string assetName, float volume)
        {
            var source = Group(parent.gameObject.scene, parent, name).gameObject.AddComponent<AudioSource>();
            source.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(SoundFolder + "/" + assetName + ".wav");
            if (source.clip == null) throw new InvalidOperationException("Falta el audio " + assetName);
            source.loop = true;
            source.playOnAwake = false;
            source.volume = volume;
            return source;
        }

        private static Pose Look(Vector3 position, Vector3 target) =>
            new(position, Quaternion.LookRotation(target - position));

        private static Pose LocalLook(Transform parent, Vector3 position, Vector3 target) =>
            Look(parent.InverseTransformPoint(position), parent.InverseTransformPoint(target));

        private static Pose LocalPose(Transform parent, Pose pose) =>
            new(parent.InverseTransformPoint(pose.position), Quaternion.Inverse(parent.rotation) * pose.rotation);

        private static Shot[] OpeningShots()
        {
            Pose wide = Look(new(0f, 23f, -2.6f), new(0f, 0.7f, 2.25f));
            Pose follow = Look(new(0f, 19f, -1.45f), new(0f, 0.7f, 1.8f));
            Pose close = Look(new(0f, 15.5f, -0.60f), new(0f, 0.7f, 0.95f));
            return new[]
            {
                new Shot("Vista aérea de la carretera", 0f, 3.35f, wide, follow),
                new Shot("Seguimiento aéreo", 3.35f, 5.65f, follow, close)
            };
        }

        private readonly struct Shot
        {
            public readonly string Name;
            public readonly float Start, End;
            public readonly Pose From, To;

            public Shot(string name, float start, float end, Pose from, Pose to)
            {
                Name = name;
                Start = start;
                End = end;
                From = from;
                To = to;
            }
        }

        private static PlayableDirector BuildSequence(Transform root, string name, Camera view, float duration,
            Shot[] shots, params (float time, string label, UnityAction action)[] signals)
        {
            string path = TimelineFolder + "/" + name + ".playable";
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
            if (timeline == null)
            {
                timeline = ScriptableObject.CreateInstance<TimelineAsset>();
                AssetDatabase.CreateAsset(timeline, path);
            }

            timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
            timeline.fixedDuration = duration;
            AnimationTrack track = UpdateCameraShots(timeline, view, shots);

            var director = Group(root.gameObject.scene, root, name).gameObject.AddComponent<PlayableDirector>();
            director.playOnAwake = false;
            director.extrapolationMode = DirectorWrapMode.None;
            director.playableAsset = timeline;
            director.SetGenericBinding(track, view.GetComponent<Animator>());

            timeline.CreateMarkerTrack();
            var receiver = director.gameObject.AddComponent<SignalReceiver>();
            var markers = timeline.markerTrack.GetMarkers().OfType<SignalEmitter>().ToArray();
            foreach (var (time, label, action) in signals)
            {
                SignalEmitter marker = markers.FirstOrDefault(item => item.asset != null && item.asset.name == label);
                if (marker == null)
                {
                    marker = timeline.markerTrack.CreateMarker<SignalEmitter>(time);
                    marker.asset = ScriptableObject.CreateInstance<SignalAsset>();
                    marker.asset.name = label;
                    AssetDatabase.AddObjectToAsset(marker.asset, timeline);
                }
                marker.time = time;
                marker.retroactive = true;
                marker.emitOnce = true;
                var reaction = new UnityEvent();
                UnityEventTools.AddPersistentListener(reaction, action);
                receiver.AddReaction(marker.asset, reaction);
                EditorUtility.SetDirty(marker);
            }
            EditorUtility.SetDirty(timeline.markerTrack);
            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssetIfDirty(timeline);
            return director;
        }

        private static AnimationTrack UpdateCameraShots(TimelineAsset timeline, Camera view, Shot[] shots)
        {
            var track = timeline.GetRootTracks().OfType<AnimationTrack>().FirstOrDefault() ??
                         timeline.CreateTrack<AnimationTrack>(null, "Cámara");
            track.trackOffset = TrackOffset.ApplyTransformOffsets;
            var clips = track.GetClips().OrderBy(clip => clip.start).ToArray();
            for (int i = 0; i < shots.Length; i++)
            {
                Shot shot = shots[i];
                TimelineClip clip = i < clips.Length ? clips[i] : track.CreateClip<AnimationPlayableAsset>();
                var playable = (AnimationPlayableAsset)clip.asset;
                if (playable.clip == null)
                {
                    playable.clip = new AnimationClip();
                    AssetDatabase.AddObjectToAsset(playable.clip, timeline);
                }
                AnimateShot(playable.clip, shot);
                playable.removeStartOffset = false;
                playable.applyFootIK = false;
                clip.displayName = shot.Name;
                clip.start = shot.Start;
                clip.duration = shot.End - shot.Start;
                EditorUtility.SetDirty(playable);
            }
            foreach (TimelineClip obsolete in clips.Skip(shots.Length)) track.DeleteClip(obsolete);
            view.transform.SetLocalPositionAndRotation(shots[0].From.position, shots[0].From.rotation);
            EditorUtility.SetDirty(track);
            return track;
        }

        private static void AnimateShot(AnimationClip clip, Shot shot)
        {
            clip.name = shot.Name;
            float time = shot.End - shot.Start;
            Vector3 from = shot.From.position;
            Vector3 to = shot.To.position;
            Quaternion a = shot.From.rotation;
            Quaternion b = shot.To.rotation;
            if (Quaternion.Dot(a, b) < 0f) b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
            void Curve(string name, float start, float end) =>
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), name),
                    AnimationCurve.EaseInOut(0f, start, time, end));
            Curve("m_LocalPosition.x", from.x, to.x);
            Curve("m_LocalPosition.y", from.y, to.y);
            Curve("m_LocalPosition.z", from.z, to.z);
            Curve("m_LocalRotation.x", a.x, b.x);
            Curve("m_LocalRotation.y", a.y, b.y);
            Curve("m_LocalRotation.z", a.z, b.z);
            Curve("m_LocalRotation.w", a.w, b.w);
            clip.EnsureQuaternionContinuity();
            EditorUtility.SetDirty(clip);
        }
    }
}
