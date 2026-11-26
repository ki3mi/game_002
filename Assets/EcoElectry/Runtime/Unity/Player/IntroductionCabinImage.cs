using UnityEngine;
using UnityEngine.InputSystem;

namespace EcoElectry.Unity.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class IntroductionCabinImage : MonoBehaviour
    {
        public const string BrightnessKey = "EcoElectry.Introduction.Night.Brightness";
        public const string GrainKey = "EcoElectry.Introduction.Night.Grain";

        private CorridorBrightness image;
        private IntroductionTruckDrive vehicle;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;
        internal bool IsOpen => image?.IsOpen == true;

        private void OnEnable()
        {
            vehicle = GetComponentInParent<IntroductionTruckDrive>();
            image = new CorridorBrightness(GetComponent<Camera>(), null, BrightnessKey, GrainKey);
        }

        private void OnDisable()
        {
            if (IsOpen) CloseMenu();
            // The walking camera applies the same saved settings from the original light levels.
            image?.RestoreEnvironment();
        }

        private void Update()
        {
            Keyboard keys = Keyboard.current;
            if (IsOpen)
            {
                if (keys?.bKey.wasPressedThisFrame == true || keys?.escapeKey.wasPressedThisFrame == true)
                    CloseMenu();
            }
            else if (keys?.bKey.wasPressedThisFrame == true)
            {
                previousCursorLock = Cursor.lockState;
                previousCursorVisible = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                image.Open();
                if (vehicle != null) vehicle.ImageMenuOpen = true;
            }
        }

        private void CloseMenu()
        {
            image.Close();
            if (vehicle != null) vehicle.ImageMenuOpen = false;
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
        }

        private void OnGUI()
        {
            if (IsOpen) image.Draw("B");
        }
    }
}
