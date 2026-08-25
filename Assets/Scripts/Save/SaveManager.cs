using System;
using System.Collections.Generic;
using UnityEngine;

namespace TurboGaraj.Save
{
    /// <summary>
    /// Handles saving and loading of game data using JSON serialization to PlayerPrefs.
    /// Provides a static instance for easy access from other managers.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance of SaveManager.
        /// </summary>
        public static SaveManager Instance { get; private set; }

        /// <summary>
        /// Data structure that holds all persistable game state.
        /// </summary>
        [Serializable]
        public class GameData
        {
            public double softCurrency = 0;
            public double hardCurrency = 0;
            public int staminaLevel = 0;
            public int powerLevel = 0;
            public int incomeLevel = 0;
            public int projectParts = 0;
            public List<string> unlockedVehicleIds = new List<string>();
        }

        /// <summary>
        /// Current game data loaded from or to be saved to persistence.
        /// </summary>
        public GameData currentData = new GameData();

        /// <summary>
        /// Key used to store the JSON string in PlayerPrefs.
        /// </summary>
        private const string SAVE_KEY = "TurboGaraj_SaveData";

        private void Awake()
        {
            // Singleton pattern: ensure only one instance exists
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadGame(); // Load data when the manager initializes
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Loads game data from PlayerPrefs. If no save exists, initializes with default values.
        /// </summary>
        public void LoadGame()
        {
            string json = PlayerPrefs.GetString(SAVE_KEY, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    currentData = JsonUtility.FromJson<GameData>(json);
                    Debug.Log("[SaveManager] Game data loaded successfully.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to load save data: {e.Message}\nInitializing new game.");
                    currentData = new GameData(); // Reset to defaults on corruption
                }
            }
            else
            {
                Debug.Log("[SaveManager] No save data found. Starting new game.");
                currentData = new GameData(); // Ensure we have a clean slate
            }
        }

        /// <summary>
        /// Saves the current game data to PlayerPrefs as a JSON string.
        /// </summary>
        public void SaveGame()
        {
            string json = JsonUtility.ToJson(currentData, true); // true for pretty-print (optional)
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save(); // Ensure it's written to disk immediately
            Debug.Log("[SaveManager] Game data saved.");
        }

        /// <summary>
        /// Resets all save data to default values and saves.
        /// Useful for debugging or testing.
        /// </summary>
        public void ResetSaveData()
        {
            currentData = new GameData();
            SaveGame();
            Debug.Log("[SaveManager] Save data has been reset to defaults.");
        }

        #region Convenience Properties for Direct Access (Optional)

        // These properties allow direct access to fields if desired, but remember to call SaveGame() after modifying.
        public double SoftCurrency
        {
            get => currentData.softCurrency;
            set { currentData.softCurrency = value; }
        }

        public double HardCurrency
        {
            get => currentData.hardCurrency;
            set { currentData.hardCurrency = value; }
        }

        public int StaminaLevel
        {
            get => currentData.staminaLevel;
            set { currentData.staminaLevel = value; }
        }

        public int PowerLevel
        {
            get => currentData.powerLevel;
            set { currentData.powerLevel = value; }
        }

        public int IncomeLevel
        {
            get => currentData.incomeLevel;
            set { currentData.incomeLevel = value; }
        }

        public int ProjectParts
        {
            get => currentData.projectParts;
            set { currentData.projectParts = value; }
        }

        public List<string> UnlockedVehicleIds
        {
            get => currentData.unlockedVehicleIds;
            set => currentData.unlockedVehicleIds = value;
        }

        #endregion
    }
}