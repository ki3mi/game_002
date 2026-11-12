using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TablerosEnergia
{
    /// <summary>
    /// Runtime-only registry. It is created automatically, so prefabs need no scene setup object.
    /// Wattages represent editable game estimates, not electrical engineering calculations.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class EnergyGridRegistry : MonoBehaviour
    {
        private static EnergyGridRegistry instance;
        private readonly List<EnergyCircuit> circuits = new List<EnergyCircuit>();
        private readonly List<ModularBoard> boards = new List<ModularBoard>();
        private bool recalculating;
        private bool pending;

        public static EnergyGridRegistry Instance => instance;
        public event Action Changed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureRuntimeInstance();
        }

        public static void EnsureRuntimeInstance()
        {
            if (!Application.isPlaying || instance != null)
                return;

            var existing = FindObjectsByType<EnergyGridRegistry>();
            if (existing.Length > 0)
            {
                instance = existing[0];
                return;
            }

            var host = new GameObject("EnergyGridRegistry_Runtime");
            host.AddComponent<EnergyGridRegistry>();
            DontDestroyOnLoad(host);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public void Register(EnergyCircuit circuit)
        {
            if (circuit == null || circuits.Contains(circuit))
                return;
            circuits.Add(circuit);
            Recalculate();
        }

        public void Unregister(EnergyCircuit circuit)
        {
            if (circuit == null || !circuits.Remove(circuit))
                return;
            Recalculate();
        }

        public void RegisterBoard(ModularBoard board)
        {
            if (board == null || boards.Contains(board)) return;
            boards.Add(board);
            Recalculate();
        }

        public void UnregisterBoard(ModularBoard board)
        {
            if (board == null || !boards.Remove(board)) return;
            Recalculate();
        }

        public void Recalculate()
        {
            if (recalculating) { pending = true; return; }
            recalculating = true;
            do
            {
                pending = false;
                circuits.RemoveAll(c => c == null);
                boards.RemoveAll(b => b == null);
                foreach (var circuit in circuits)
                    circuit.IsEnergized = Evaluate(circuit, new HashSet<EnergyCircuit>());
                foreach (var board in boards.ToArray())
                    if (board != null && board.isActiveAndEnabled) board.RefreshState();
            } while (pending);
            recalculating = false;
            Changed?.Invoke();
        }

        private bool Evaluate(EnergyCircuit circuit, HashSet<EnergyCircuit> visiting)
        {
            if (circuit == null || !circuit.RequestedOn || !visiting.Add(circuit))
                return false;

            string parentId = circuit.ParentCircuitId;
            if (string.IsNullOrWhiteSpace(parentId))
                return true;

            var parents = circuits.Where(c => c != null && c != circuit && c.CircuitId == parentId).ToArray();
            if (parents.Length == 0)
                return false;

            foreach (var parent in parents)
            {
                var branch = new HashSet<EnergyCircuit>(visiting);
                if (Evaluate(parent, branch))
                    return true;
            }
            return false;
        }

        public float GetActiveLoadWatts()
        {
            return circuits.Where(c => c != null && c.Role == EnergyCircuitRole.Load && c.IsEnergized)
                .Sum(c => c.EstimatedWatts) + boards.Where(b => b != null && b.isActiveAndEnabled).Sum(b => b.ActiveWatts());
        }

        public int GetActiveLoadCount()
        {
            return circuits.Count(c => c != null && c.Role == EnergyCircuitRole.Load && c.IsEnergized) +
                boards.Where(b => b != null && b.isActiveAndEnabled).Sum(b => b.ActiveLoads());
        }

        public int GetConfiguredLoadCount()
        {
            return circuits.Count(c => c != null && c.Role == EnergyCircuitRole.Load) +
                boards.Where(b => b != null && b.isActiveAndEnabled).Sum(b => b.ConfiguredLoads());
        }

        public float GetActiveLoadWatts(ModularBoard scope)
        {
            if (scope == null) return GetActiveLoadWatts();
            ModularBoard root = scope.RootBoard();
            return boards.Where(b => b != null && b.isActiveAndEnabled && b.RootBoard() == root)
                .Sum(b => b.ActiveWatts());
        }

        public int GetActiveLoadCount(ModularBoard scope)
        {
            if (scope == null) return GetActiveLoadCount();
            ModularBoard root = scope.RootBoard();
            return boards.Where(b => b != null && b.isActiveAndEnabled && b.RootBoard() == root)
                .Sum(b => b.ActiveLoads());
        }

        public int GetConfiguredLoadCount(ModularBoard scope)
        {
            if (scope == null) return GetConfiguredLoadCount();
            ModularBoard root = scope.RootBoard();
            return boards.Where(b => b != null && b.isActiveAndEnabled && b.RootBoard() == root)
                .Sum(b => b.ConfiguredLoads());
        }

        public IReadOnlyList<EnergyCircuit> GetCircuits()
        {
            return circuits.Where(c => c != null).ToArray();
        }
    }
}
