using UnityEngine;

namespace TurboGaraj.Vehicle
{
    /// <summary>
    /// Controls the vehicle using WheelColliders for arcade-style physics.
    /// Implements automatic throttle and a stamina system that reduces max speed over time.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        [Header("Wheel Colliders")]
        public WheelCollider frontLeftWheel;
        public WheelCollider frontRightWheel;
        public WheelCollider rearLeftWheel;
        public WheelCollider rearRightWheel;

        [Header("Vehicle Settings")]
        [Tooltip("Base maximum speed of the vehicle when stamina is full (in meters per second)")]
        public float baseMaxSpeed = 20f;
        [Tooltip("How much the stamina affects the max speed (0 = no effect, 1 = stamina directly scales maxSpeed)")]
        [Range(0f, 1f)] public float staminaSpeedInfluence = 0.8f;
        [Tooltip("Initial stamina value (0-100)")]
        [Range(0f, 100f)] public float initialStamina = 100f;
        [Tooltip("Rate at which stamina drains per second when driving")]
        public float staminaDrainRate = 10f;

        [Header("Motor")]
        [Tooltip("Motor torque applied to the driven wheels (in Newton meters)")]
        public float baseMotorTorque = 1500f;

        [Header("Throttle")]
        [Tooltip("Constant throttle input (0-1) applied automatically")] [Range(0f, 1f)]
        public float throttleInput = 0.8f;

        private float currentStamina;
        private Rigidbody rb;

        /// <summary>
        /// Gets the current stamina as a normalized value (0-1)
        /// </summary>
        public float CurrentStaminaNormalized => initialStamina > 0 ? currentStamina / initialStamina : 0f;

        /// <summary>
        /// Gets the current speed in kilometers per hour.
        /// </summary>
        public float SpeedKmh => rb.linearVelocity.magnitude * 3.6f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = 1200f; // Set mass to a realistic car mass
            rb.centerOfMass = new Vector3(0f, -0.3f, 0f); // Lower the center of mass to prevent flipping

            currentStamina = initialStamina;

            // Configure WheelColliders for arcade feel
            ConfigureWheelCollider(frontLeftWheel);
            ConfigureWheelCollider(frontRightWheel);
            ConfigureWheelCollider(rearLeftWheel);
            ConfigureWheelCollider(rearRightWheel);
        }

        private void ConfigureWheelCollider(WheelCollider wc)
        {
            if (wc == null) return;

            // Set suspension spring
            var suspension = wc.suspensionSpring;
            suspension.spring = 35000f;
            suspension.damper = 4500f;
            suspension.targetPosition = 0.5f;
            wc.suspensionSpring = suspension;

            // Set friction curves for arcade grip
            var forwardFriction = wc.forwardFriction;
            var sidewaysFriction = wc.sidewaysFriction;

            // Extremum: where the tire starts to slip
            forwardFriction.extremumSlip = 0.4f;
            forwardFriction.extremumValue = 1.0f;
            // Asymptote: slipping further
            forwardFriction.asymptoteSlip = 0.8f;
            forwardFriction.asymptoteValue = 0.5f;
            // Stiffness: how quickly the friction builds (higher = more responsive)
            forwardFriction.stiffness = 2.0f;

            sidewaysFriction.extremumSlip = 0.4f;
            sidewaysFriction.extremumValue = 1.0f;
            sidewaysFriction.asymptoteSlip = 0.8f;
            sidewaysFriction.asymptoteValue = 0.5f;
            sidewaysFriction.stiffness = 2.0f;

            wc.forwardFriction = forwardFriction;
            wc.sidewaysFriction = sidewaysFriction;
        }

        private void FixedUpdate()
        {
            // Update stamina
            if (currentStamina > 0f)
            {
                currentStamina -= staminaDrainRate * Time.fixedDeltaTime;
                currentStamina = Mathf.Max(currentStamina, 0f);
            }

            // Calculate current max speed based on stamina
            float staminaNormalized = CurrentStaminaNormalized; // 0 to 1
            float speedMultiplier = 1f - (staminaSpeedInfluence * (1f - staminaNormalized));
            float currentMaxSpeed = baseMaxSpeed * speedMultiplier;

            // Apply automatic throttle to all drive wheels (assuming rear-wheel drive for simplicity)
            ApplyDrive(throttleInput);

            // Optional: limit speed to currentMaxSpeed (simple drag-based approach)
            LimitSpeed(currentMaxSpeed);
        }

        private void ApplyDrive(float throttle)
        {
            // Apply torque to the wheels (rear-wheel drive)
            rearLeftWheel.motorTorque = throttle * baseMotorTorque;
            rearRightWheel.motorTorque = throttle * baseMotorTorque;
            frontLeftWheel.motorTorque = 0f; // Front wheels not driven (RWD)
            frontRightWheel.motorTorque = 0f;
        }

        private void LimitSpeed(float maxSpeed)
        {
            // Simple speed limiter: if we exceed maxSpeed, add opposite drag
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                Vector3 vel = rb.linearVelocity;
                Vector3 overspeed = vel - (vel.normalized * maxSpeed);
                rb.AddForce(-overspeed * 10f, ForceMode.Acceleration); // Adjust strength as needed
            }
        }

        // Optional: Visualize wheel positions (for debugging)
        private void OnDrawGizmosSelected()
        {
            if (frontLeftWheel != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(frontLeftWheel.transform.position, 0.1f);
                Gizmos.DrawSphere(frontRightWheel.transform.position, 0.1f);
                Gizmos.DrawSphere(rearLeftWheel.transform.position, 0.1f);
                Gizmos.DrawSphere(rearRightWheel.transform.position, 0.1f);
            }
        }
    }
}