using System;
using System.Collections.Generic;
using AssetInteraction;
using UnityEngine;

namespace SensorAssets
{
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public sealed class SensorPresencia : PowerDeviceInteractable
    {
        [SerializeField] private SphereCollider detectionVolume;
        [SerializeField, Min(0.5f)] private float detectionRadius = 3.5f;
        private readonly HashSet<Collider> occupants = new HashSet<Collider>();

        public bool PresenceDetected => IsPowered && occupants.Count > 0;
        public event Action<bool> PresenceChanged;

        protected override void Awake()
        {
            base.Awake();
            if (detectionVolume == null) detectionVolume = GetComponent<SphereCollider>();
            if (detectionVolume != null) { detectionVolume.isTrigger = true; detectionVolume.radius = detectionRadius; }
            Rigidbody body = GetComponent<Rigidbody>();
            if (body != null) { body.isKinematic = true; body.useGravity = false; body.constraints = RigidbodyConstraints.FreezeAll; }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPowered || !IsActor(other)) return;
            bool wasOccupied = occupants.Count > 0;
            occupants.Add(other);
            if (!wasOccupied && occupants.Count > 0) PresenceChanged?.Invoke(true);
        }

        private void OnTriggerExit(Collider other)
        {
            bool wasOccupied = occupants.Count > 0;
            occupants.Remove(other);
            if (wasOccupied && occupants.Count == 0) PresenceChanged?.Invoke(false);
        }

        private static bool IsActor(Collider c)
        {
            return c != null && (c.GetComponentInParent<CharacterController>() != null ||
                c.CompareTag("Player") || c.transform.root.CompareTag("Player"));
        }

        public override void Interact()
        {
            SetPowered(!IsPowered);
            if (!IsPowered && occupants.Count > 0) { occupants.Clear(); PresenceChanged?.Invoke(false); }
        }
    }
}