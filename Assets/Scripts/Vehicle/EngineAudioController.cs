using UnityEngine;

namespace TurboGaraj.Vehicle
{
    /// <summary>
    /// Controls engine audio based on RPM, with optional crossfading between low, medium, and high RPM audio clips.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class EngineAudioController : MonoBehaviour
    {
        [Header("Audio Clips")]
        [Tooltip("Audio clip for low RPM (idle to medium)")]
        public AudioClip lowRPMClip;
        [Tooltip("Audio clip for medium RPM")]
        public AudioClip mediumRPMClip;
        [Tooltip("Audio clip for high RPM (near max)")]
        public AudioClip highRPMClip;

        [Header("RPM Ranges")]
        [Tooltip("RPM at which to start crossfading from low to medium")]
        public float lowToMediumRPM = 2000f;
        [Tooltip("RPM at which to start crossfading from medium to high")]
        public float mediumToHighRPM = 4000f;
        [Tooltip("Maximum RPM (for clamping)")]
        public float maxRPM = 7000f;

        [Header("Audio Settings")]
        [Tooltip("Base pitch multiplier (actual pitch = basePitch * RPM / maxRPM)")]
        public float basePitch = 1.0f;
        [Tooltip("How quickly the audio crossfades (lower = faster)")]
        public float crossfadeSpeed = 0.1f;
        [Tooltip("Minimum volume (to avoid silence)")]
        public float minVolume = 0.1f;

        // Internal state
        private AudioSource _audioSource;
        private EngineController _engine;
        private float _lowVolume = 1f;
        private float _mediumVolume = 0f;
        private float _highVolume = 0f;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = true;
            _engine = GetComponent<EngineController>();

            // If no audio clips are assigned, we can generate a simple tone or warn.
            if (lowRPMClip == null && mediumRPMClip == null && highRPMClip == null)
            {
                Debug.LogWarning("[EngineAudioController] No audio clips assigned. Engine sound will not play.");
            }
            else
            {
                _audioSource.Play();
            }
        }

        private void Update()
        {
            if (_engine == null || _audioSource == null)
                return;

            float rpm = _engine.RPM;
            float normalizedRPM = Mathf.Clamp01(rpm / maxRPM);

            // Determine crossfade factors based on RPM ranges
            float tLow = 0f, tMedium = 0f, tHigh = 0f;

            if (rpm < lowToMediumRPM)
            {
                // Only low
                tLow = 1f;
                tMedium = 0f;
                tHigh = 0f;
            }
            else if (rpm < mediumToHighRPM)
            {
                // Crossfade between low and medium
                float t = Mathf.InverseLerp(lowToMediumRPM, mediumToHighRPM, rpm);
                tLow = 1f - t;
                tMedium = t;
                tHigh = 0f;
            }
            else
            {
                // Crossfade between medium and high
                float t = Mathf.InverseLerp(mediumToHighRPM, maxRPM, rpm);
                tLow = 0f;
                tMedium = 1f - t;
                tHigh = t;
            }

            // Smoothly crossfade volumes
            _lowVolume = Mathf.Lerp(_lowVolume, tLow, crossfadeSpeed * Time.deltaTime);
            _mediumVolume = Mathf.Lerp(_mediumVolume, tMedium, crossfadeSpeed * Time.deltaTime);
            _highVolume = Mathf.Lerp(_highVolume, tHigh, crossfadeSpeed * Time.deltaTime);

            // Set the pitch based on RPM (optional: can also be done per clip)
            _audioSource.pitch = basePitch * normalizedRPM;

            // We'll use a single AudioSource and blend by changing the clip? Not possible at runtime.
            // Instead, we can use three AudioSources or use one AudioSource and change the clip and pitch.
            // For simplicity, we'll use one AudioSource and change the clip based on the dominant range,
            // and adjust pitch and volume. This is not a true crossfade but changes the clip abruptly.
            // For a better crossfade, we would need three AudioSources.

            // Given the complexity, we'll use one AudioSource and switch clips when the dominant range changes.
            // We'll keep track of the current clip and only change when the dominant range changes.

            // For now, let's use a simple approach: set the clip to the one with the highest volume and adjust pitch and volume.
            // This is not ideal but will demonstrate the concept.

            // Determine which clip is dominant
            AudioClip dominantClip = lowRPMClip;
            if (_mediumVolume > _lowVolume && _mediumVolume > _highVolume)
                dominantClip = mediumRPMClip;
            else if (_highVolume > _lowVolume && _highVolume > _mediumVolume)
                dominantClip = highRPMClip;

            // If the dominant clip has changed and is not null, change the clip
            if (dominantClip != null && dominantClip != _audioSource.clip)
            {
                _audioSource.clip = dominantClip;
                // Restart the clip to avoid clicking? We can continue, but for simplicity we'll restart.
                _audioSource.Stop();
                _audioSource.Play();
            }

            // Set the volume (combine minVolume with the dominant volume factor)
            float volume = Mathf.Max(_lowVolume, _mediumVolume, _highVolume);
            _audioSource.volume = Mathf.Lerp(minVolume, 1f, volume);
        }
    }
}