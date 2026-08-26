using UnityEngine;
using UnityEngine.UI;
using TurboGaraj.Vehicle;

namespace TurboGaraj.UI
{
    /// <summary>
    /// Updates a UI Slider to reflect the vehicle's stamina level.
    /// Attach to a GameObject that has a Slider component (e.g., the Slider itself or its parent).
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class StaminaUI : MonoBehaviour
    {
        [Tooltip("Reference to the VehicleController to read stamina from")]
        public VehicleController vehicle;

        private Slider staminaSlider;

        private void Awake()
        {
            staminaSlider = GetComponent<Slider>();
            // Ensure slider is set up correctly (min 0, max 1)
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = 1f;
            staminaSlider.value = 1f; // Start full
        }

        private void Update()
        {
            if (vehicle != null)
            {
                // Normalize stamina to 0-1 range
                float staminaNormalized = vehicle.CurrentStaminaNormalized;
                staminaSlider.value = staminaNormalized;
            }
        }
    }
}