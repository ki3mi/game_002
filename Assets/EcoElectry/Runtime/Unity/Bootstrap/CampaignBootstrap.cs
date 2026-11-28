using UnityEngine;
using UnityEngine.SceneManagement;
using EcoElectry.Unity.Survival;

namespace EcoElectry.Unity.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class CampaignBootstrap : MonoBehaviour
    {
        [SerializeField] private string firstLevelScenePath;

        private void Start()
        {
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(firstLevelScenePath);
            if (buildIndex < 0)
            {
                Debug.LogError("Registra la escena del primer nivel en la lista de escenas de compilación.", this);
                return;
            }

            SurvivalRun.StartNew();
            SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
        }
    }
}
