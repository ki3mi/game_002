using EcoElectry.Unity.Player;
using UnityEngine;
using UnityEngine.Playables;

namespace EcoElectry.Unity.Levels
{
    [DisallowMultipleComponent]
    public sealed class IntroductionCinematics : MonoBehaviour
    {
        public const float ParkingMoveDuration = 2.4f;
        public const float ArrivalCabinDuration = 5f;
        public const float ArrivalParkedTime = 11f;

        private enum Phase { Opening, Driving, Arrival, Walking }

        [SerializeField] private IntroductionTruckDrive truck;
        [SerializeField] private IntroductionSkyCycle sky;
        [SerializeField] private VehicleArrivalTransition arrival;
        [SerializeField] private PlayableDirector opening;
        [SerializeField] private PlayableDirector arrivalSequence;
        [SerializeField] private Camera openingView;
        [SerializeField] private Camera arrivalView;
        [SerializeField] private GameObject cockpit;
        [SerializeField] private AudioSource motor;
        [SerializeField] private AudioSource forest;
        [SerializeField, Min(1f)] private float openingSpeedKmh = 100f;

        private Phase phase;
        private Camera cabinCamera;
        private AudioListener cabinListener;
        private float laneOffset, roadHeightOffset;
        private bool exitShot;
        private bool settled;
        private bool highwayArrival;

        internal bool IsDriving => phase == Phase.Driving;
        private float OpeningSpeed => openingSpeedKmh / 3.6f;

        private void Start()
        {
            phase = Phase.Opening;
            cabinCamera = cockpit.GetComponent<Camera>();
            cabinListener = cockpit.GetComponent<AudioListener>();
            sky.SampleRoad(0f, out Vector3 center, out Vector3 tangent);
            Vector3 side = Vector3.Cross(Vector3.up, tangent).normalized;
            Vector3 offset = truck.transform.position - center;
            laneOffset = Vector3.Dot(offset, side);
            roadHeightOffset = offset.y;
            truck.BeginCinematicMotion();
            ShowView(openingView.gameObject);
            opening.stopped += OpeningStopped;
            arrivalSequence.stopped += ArrivalStopped;
            if (motor.clip != null) motor.Play();
            if (forest.clip != null) forest.Play();
            opening.Play();
        }

        private void OnDestroy()
        {
            if (opening != null) opening.stopped -= OpeningStopped;
            if (arrivalSequence != null) arrivalSequence.stopped -= ArrivalStopped;
        }

        private void LateUpdate()
        {
            if (phase == Phase.Opening) ApplyOpeningPose();
            else if (phase == Phase.Arrival)
            {
                if (!settled)
                {
                    float time = (float)arrivalSequence.time;
                    float progress = Mathf.Clamp01((time - ArrivalCabinDuration) / ParkingMoveDuration);
                    bool clear = highwayArrival
                        ? arrival.AdvanceHighwayArrival(time, ArrivalCabinDuration, ArrivalParkedTime)
                        : arrival.MoveIntoParking(progress, ParkingMoveDuration);
                    if (!clear)
                    {
                        ResumeAfterObstruction();
                        return;
                    }
                    settled = highwayArrival ? time >= ArrivalParkedTime : progress >= 1f;
                }
                if (exitShot && arrival.TryGetExitView(out Vector3 eyes, out Quaternion rotation))
                    arrivalView.transform.SetPositionAndRotation(eyes, rotation);
            }

            float speed = truck.Speed;
            float movingVolume = Mathf.Lerp(0.07f, 0.28f, Mathf.InverseLerp(0f, 25f, speed));
            float motorVolume = phase == Phase.Walking || (phase == Phase.Arrival && settled) ? 0f : movingVolume;
            motor.volume = Mathf.MoveTowards(motor.volume, motorVolume, Time.deltaTime * 0.5f);
            motor.pitch = Mathf.Lerp(0.60f, 1.5f, Mathf.InverseLerp(0f, 28f, speed));
            float outdoorVolume = phase == Phase.Opening || phase == Phase.Driving ? 0.085f : 0.22f;
            forest.volume = Mathf.MoveTowards(forest.volume, outdoorVolume, Time.deltaTime * 0.25f);
        }

        private Vector3 ApplyOpeningPose()
        {
            float distance = (float)opening.time * OpeningSpeed;
            sky.SampleRoad(distance, out Vector3 point, out Vector3 tangent);
            sky.SampleRoad(distance + 2f, out _, out Vector3 ahead);
            Vector3 forward = (tangent + ahead).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
            truck.SetCinematicPose(point + right * laneOffset + Vector3.up * roadHeightOffset,
                Quaternion.LookRotation(Vector3.ProjectOnPlane(forward, Vector3.up)), OpeningSpeed);
            return forward;
        }

        public void CutToCabin()
        {
            if (UnityEngine.Application.isPlaying && phase == Phase.Opening) ShowView(cockpit);
        }

        public void DeliverDriving()
        {
            if (!UnityEngine.Application.isPlaying || phase != Phase.Opening) return;
            Vector3 direction = ApplyOpeningPose();
            phase = Phase.Driving;
            truck.ResumeDriving(direction * OpeningSpeed);
            ShowView(cockpit);
        }

        internal void StartArrival(bool fromHighway)
        {
            if (!IsDriving) return;
            phase = Phase.Arrival;
            exitShot = false;
            settled = false;
            highwayArrival = fromHighway;
            truck.BeginCinematicMotion();
            ShowView(fromHighway ? cockpit : arrivalView.gameObject);
            arrivalSequence.initialTime = fromHighway ? 0f : ArrivalCabinDuration;
            arrivalSequence.time = arrivalSequence.initialTime;
            arrivalSequence.Play();
        }

        public void CutToFacade()
        {
            if (!UnityEngine.Application.isPlaying || phase != Phase.Arrival) return;
            sky.BeginNight();
            ShowView(arrivalView.gameObject);
        }

        public void CutToExit() => exitShot = UnityEngine.Application.isPlaying && phase == Phase.Arrival;

        public void DeliverWalking()
        {
            if (!UnityEngine.Application.isPlaying || phase != Phase.Arrival) return;
            ShowView(null);
            if (arrival.FinishArrival())
            {
                phase = Phase.Walking;
                motor.Stop();
            }
            else
                ResumeAfterObstruction();
        }

        private void ResumeAfterObstruction()
        {
            phase = Phase.Driving;
            truck.ResumeDriving(Vector3.zero);
            ShowView(cockpit);
            arrivalSequence.Stop();
        }

        private void ShowView(GameObject view)
        {
            if (openingView.gameObject != view) openingView.gameObject.SetActive(false);
            if (arrivalView.gameObject != view) arrivalView.gameObject.SetActive(false);
            bool useCabin = view == cockpit;
            cabinCamera.enabled = useCabin;
            cabinListener.enabled = useCabin;
            if (view != null && !useCabin) view.SetActive(true);
        }

        private void OpeningStopped(PlayableDirector director)
        {
            if (phase == Phase.Opening)
            {
                director.time = director.duration;
                DeliverDriving();
            }
        }

        private void ArrivalStopped(PlayableDirector director)
        {
            if (phase == Phase.Arrival) DeliverWalking();
        }
    }
}
