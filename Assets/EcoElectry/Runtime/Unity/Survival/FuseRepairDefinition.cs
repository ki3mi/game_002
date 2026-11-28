using System;
using EcoElectry.Application.Survival;
using UnityEngine;

namespace EcoElectry.Unity.Survival
{
    [CreateAssetMenu(menuName = "EcoElectry/Caja de fusibles")]
    public sealed class FuseRepairDefinition : ScriptableObject
    {
        [Serializable]
        private struct Cartridge
        {
            public string id;
            public string reference;
            public bool conducts;
        }

        [SerializeField] private string repairId;
        [SerializeField] private string[] references;
        [SerializeField] private Cartridge[] cartridges;
        [SerializeField, TextArea] private string clue;
        [SerializeField] private FuseBoxModel box;
        [SerializeField] private GameObject fuse;
        [SerializeField] private GameObject screwdriver;
        [SerializeField] private GameObject spares;

        internal string Clue => clue;
        internal FuseBoxModel Box => box;
        internal GameObject Fuse => fuse;
        internal GameObject Screwdriver => screwdriver;
        internal GameObject Spares => spares;

        internal FuseRepair Enter(SurvivalSession session) => session.GetFuseRepair(repairId, references,
            Array.ConvertAll(cartridges, part => new FuseCartridge(part.id, part.reference, part.conducts)));
    }
}
