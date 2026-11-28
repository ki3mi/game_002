using System;
using System.Collections.Generic;
using EcoElectry.Application.Survival;
using EcoElectry.Domain.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    [CreateAssetMenu(menuName = "EcoElectry/Linterna y miedo")]
    public sealed class SurvivalSettings : ScriptableObject
    {
        [SerializeField, Min(.1f)] private float secondsPerBattery = 135f;
        [SerializeField] private bool firstSlotOccupied = true;
        [SerializeField, Range(0, 1)] private float firstSlotCharge = 1f;
        [SerializeField] private bool secondSlotOccupied = false;
        [SerializeField, Range(0, 1)] private float secondSlotCharge = 1f;
        [SerializeField] private float[] initialSpareCharges = Array.Empty<float>();
        [SerializeField] private bool startsOn = true;
        [SerializeField, Range(.01f, 100)] private float alertThreshold = 12;
        [SerializeField, Range(0, 100)] private float safeThreshold = 6;
        [SerializeField, Min(0)] private float fearPerSecond = 3;
        [SerializeField, Min(0)] private float recoveryPerSecond = 5;

        public float SecondsPerBattery => Mathf.Max(.1f, secondsPerBattery);

        public FearRules CreateFearRules()
        {
            float alert = Mathf.Clamp(alertThreshold, .01f, 100);
            return new FearRules(alert, Mathf.Clamp(safeThreshold, 0, alert - .01f),
                Mathf.Max(0, fearPerSecond), Mathf.Max(0, recoveryPerSecond));
        }

        public SurvivalSession CreateSession()
        {
            var spares = new List<BatteryCell>();
            for (int i = 0; i < initialSpareCharges.Length; i++)
                spares.Add(new BatteryCell($"initial-spare-{i}", Mathf.Clamp01(initialSpareCharges[i])));
            return new SurvivalSession(
                firstSlotOccupied ? new BatteryCell("initial-slot-a", Mathf.Clamp01(firstSlotCharge)) : null,
                secondSlotOccupied ? new BatteryCell("initial-slot-b", Mathf.Clamp01(secondSlotCharge)) : null,
                startsOn, spares);
        }
    }
}
