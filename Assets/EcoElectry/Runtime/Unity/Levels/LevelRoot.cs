using EcoElectry.Application.Levels;
using UnityEngine;

namespace EcoElectry.Unity.Levels
{
    [DisallowMultipleComponent]
    public sealed class LevelRoot : MonoBehaviour
    {
        [SerializeField] private LevelDefinition definition;

        public LevelSession Session { get; private set; }

        private void Awake()
        {
            if (definition == null)
            {
                Debug.LogError("Asigna la definición de nivel en LevelRoot.", this);
                return;
            }

            Session = new LevelSession(definition.CreateRules());
        }

        private void OnDestroy()
        {
            Session = null;
        }
    }
}
