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
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            vehicle = GetComponentInParent<IntroductionTruckDrive>();
            image = new CorridorBrightness(GetComponent<Camera>(), null, BrightnessKey, GrainKey);
            ApplyCursor(UnityEngine.Application.isFocused);
        }

        private void OnDisable()
        {
            if (IsOpen) SetMenuOpen(false);
            image?.RestoreEnvironment();
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
        }

        private void OnApplicationFocus(bool focused) => ApplyCursor(focused);

        private void Update()
        {
            if (vehicle != null && vehicle.IsCinematic) return;
            Keyboard keys = Keyboard.current;
            if (IsOpen)
            {
                if (keys?.bKey.wasPressedThisFrame == true || keys?.escapeKey.wasPressedThisFrame == true)
                    SetMenuOpen(false);
            }
            else if (keys?.bKey.wasPressedThisFrame == true)
            {
                SetMenuOpen(true);
            }
        }

        private void SetMenuOpen(bool open)
        {
            if (open) image.Open();
            else image.Close();
            if (vehicle != null) vehicle.ImageMenuOpen = open;
            ApplyCursor(UnityEngine.Application.isFocused);
        }

        private void ApplyCursor(bool focused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = !focused || IsOpen;
        }

        private void OnGUI()
        {
            if (IsOpen) image.Draw("B");
        }
    }
}
