using System.Collections.Generic;
using UnityEngine;
using TurboGaraj.Vehicle;

namespace TurboGaraj.Track
{
    /// <summary>
    /// Manages the endless track system: spawning/despawning track segments,
    /// spawning simple opponent vehicles, and tracking race progress/tasks.
    /// </summary>
    public class TrackManager : MonoBehaviour
    {
        [Header("Track Settings")]
        [Tooltip("Prefab for a single track segment (straight piece)")]
        public GameObject trackSegmentPrefab;
        [Tooltip("Number of segments to spawn initially")]
        public int initialSegmentCount = 10;
        [Tooltip("Length of each track segment along the Z axis (in Unity units)")]
        public float segmentLength = 20f;
        [Tooltip("How far ahead of the vehicle to spawn new segments (in Unity units)")]
        public float spawnAheadDistance = 100f;
        [Tooltip("How far behind the vehicle to despawn segments (in Unity units)")]
        public float despawnBehindDistance = 50f;

        [Header("Opponent Settings")]
        [Tooltip("Prefab for a simple opponent vehicle (no AI, just moves forward)")]
        public GameObject opponentPrefab;
        [Tooltip("Time interval between opponent spawns (in seconds)")]
        public float opponentSpawnInterval = 5f;
        [Tooltip("Speed at which opponents move forward (in Unity units per second)")]
        public float opponentSpeed = 10f;
        [Tooltip("How far ahead of the vehicle to initially spawn opponents (in Unity units)")]
        public float opponentSpawnAheadOffset = 20f;

        [Header("Task / Race Progress")]
        [Tooltip("Distance required to complete one race (in Unity units along Z)")]
        public float raceLength = 500f;
        [Tooltip("Number of races required to complete the current task (e.g., 2)")]
        public int racesRequired = 2;

        // Internal state
        private VehicleController _vehicle;
        private readonly Queue<GameObject> _activeSegments = new Queue<GameObject>();
        private readonly Queue<GameObject> _activeOpponents = new Queue<GameObject>();
        private float _spawnZ; // Z position where the next segment will be spawned
        private float _nextOpponentSpawnTime;
        private int _racesCompleted;
        private float _nextRaceThreshold; // Z position at which the next race completes
        private bool _hasWarnedAboutTrackPrefab = false;

        private void Awake()
        {
            // Find the vehicle controller in the scene
            _vehicle = FindObjectOfType<VehicleController>();
            if (_vehicle == null)
            {
                Debug.LogError("[TrackManager] No VehicleController found in scene. Please ensure there is a vehicle with VehicleController configured.");
                enabled = false;
                return;
            }

            InitializeTrack();
            InitializeOpponents();
            InitializeTask();
        }

        private void InitializeTrack()
        {
            // Spawn initial segments starting at Z = 0
            for (int i = 0; i < initialSegmentCount; i++)
            {
                SpawnSegment(i * segmentLength);
            }
            // Set next spawn position after the last initial segment
            _spawnZ = initialSegmentCount * segmentLength;
        }

        private void InitializeOpponents()
        {
            _nextOpponentSpawnTime = Time.time + opponentSpawnInterval; // first spawn after delay
        }

        private void InitializeTask()
        {
            _racesCompleted = 0;
            _nextRaceThreshold = raceLength; // first race completes after traveling raceLength units
        }

        private void Update()
        {
            if (_vehicle == null) return;

            UpdateTrack();
            UpdateOpponents();
            UpdateTask();
        }

        private void UpdateTrack()
        {
            float vehicleZ = _vehicle.transform.position.z;

            // Spawn new segments ahead of the vehicle
            while (_spawnZ < vehicleZ + spawnAheadDistance)
            {
                SpawnSegment(_spawnZ);
                _spawnZ += segmentLength;
            }

            // Despawn segments far behind the vehicle
            float despawnZ = vehicleZ - despawnBehindDistance;
            while (_activeSegments.Count > 0 && _activeSegments.Peek().transform.position.z < despawnZ)
            {
                GameObject segment = _activeSegments.Dequeue();
                Destroy(segment);
            }
        }

        private void SpawnSegment(float zPos)
        {
            if (trackSegmentPrefab == null)
            {
                // Only warn once to avoid spam
                if (!_hasWarnedAboutTrackPrefab)
                {
                    Debug.LogWarning("[TrackManager] trackSegmentPrefab is not assigned. Assign a prefab in the inspector.");
                    _hasWarnedAboutTrackPrefab = true;
                }
                return;
            }

            GameObject segment = Instantiate(trackSegmentPrefab, new Vector3(0f, 0f, zPos), Quaternion.identity);
            segment.name = $"TrackSegment_{zPos}";
            _activeSegments.Enqueue(segment);
        }

        private void UpdateOpponents()
        {
            // Spawn opponents based on timer
            if (Time.time >= _nextOpponentSpawnTime && opponentPrefab != null)
            {
                SpawnOpponent();
                _nextOpponentSpawnTime += opponentSpawnInterval;
            }

            // Move existing opponents forward and despawn those far behind
            float vehicleZ = _vehicle.transform.position.z;
            float despawnZ = vehicleZ - despawnBehindDistance * 2f; // opponents despawn a bit further behind

            // We cannot modify a queue while iterating easily, so we'll process from front until we find one that's still ahead
            while (_activeOpponents.Count > 0 && _activeOpponents.Peek().transform.position.z < despawnZ)
            {
                GameObject opp = _activeOpponents.Dequeue();
                Destroy(opp);
            }

            // Move each opponent forward
            // Since Queue doesn't support direct iteration, we'll dequeue and re-enqueue
            int count = _activeOpponents.Count;
            for (int i = 0; i < count; i++)
            {
                GameObject opp = _activeOpponents.Dequeue();
                if (opp != null)
                {
                    opp.transform.position += Vector3.forward * (opponentSpeed * Time.deltaTime);
                    _activeOpponents.Enqueue(opp);
                }
            }
        }

        private void SpawnOpponent()
        {
            if (opponentPrefab == null)
            {
                Debug.LogWarning("[TrackManager] opponentPrefab is not assigned.");
                return;
            }

            // Spawn ahead of the vehicle, with some random lateral offset to avoid perfect lining
            float laneOffset = Random.Range(-2f, 2f); // simple lane variation
            Vector3 spawnPos = new Vector3(laneOffset, 0f, _vehicle.transform.position.z + opponentSpawnAheadOffset);
            GameObject opp = Instantiate(opponentPrefab, spawnPos, Quaternion.identity);
            opp.name = $"Opponent_{Time.time}";
            _activeOpponents.Enqueue(opp);
        }

        private void UpdateTask()
        {
            float vehicleZ = _vehicle.transform.position.z;

            // Check if the vehicle has passed the next race threshold
            if (vehicleZ >= _nextRaceThreshold)
            {
                _racesCompleted++;
                Debug.Log($"[TrackManager] Race completed! Total races: {_racesCompleted}/{racesRequired}");
                _nextRaceThreshold += raceLength; // set threshold for next race

                // Optionally, you could trigger a UI event or give rewards here
                if (_racesCompleted >= racesRequired)
                {
                    Debug.Log("[TrackManager] Task completed! All required races finished.");
                    // Here you could notify other systems (e.g., unlock a new car, grant currency)
                }
            }
        }

        /// <summary>
        /// Gets the number of races completed so far.
        /// </summary>
        public int RacesCompleted => _racesCompleted;

        /// <summary>
        /// Gets whether the current task (required races) is completed.
        /// </summary>
        public bool TaskCompleted => _racesCompleted >= racesRequired;
    }
}