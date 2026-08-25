using System;
using System.Collections.Generic;
using UnityEngine;

namespace TurboGaraj.Economy
{
    /// <summary>
    /// Manages the game's economy: currencies, upgrade systems, and income generation.
    /// Handles both online and offline earnings (offline calculation deferred to SaveManager in M3).
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        [Header("Currencies")]
        [Tooltip("Soft currency earned through races and upgrades")]
        public double softCurrency = 0; // Using double for large values
        [Tooltip("Hard/premium currency (earned through special events or IAP)")]
        public double hardCurrency = 0;

        [Header("Upgrade Settings")]
        [Tooltip("Base cost for level 1 upgrade")]
        public double baseUpgradeCost = 100;
        [Tooltip("Exponential multiplier for upgrade costs (1.15 = 15% increase per level)")]
        public float upgradeCostMultiplier = 1.15f;
        [Tooltip("Base income per second at level 0")]
        public float baseIncomePerSecond = 1f;
        [Tooltip("Income multiplier per income level")]
        public float incomeLevelMultiplier = 0.2f; // 20% increase per level

        [Header("Current Levels (0 = base, no upgrades)")]
        [Tooltip("Current stamina upgrade level")]
        public int staminaLevel = 0;
        [Tooltip("Current power upgrade level")]
        public int powerLevel = 0;
        [Tooltip("Current income upgrade level")]
        public int incomeLevel = 0;

        // Internal tracking for earnings
        private float _lastEarnTimestamp;
        private bool _hasEarnedThisSession;

        private void Awake()
        {
            // Initialize timestamp for earnings tracking
            _lastEarnTimestamp = Time.time;
            _hasEarnedThisSession = false;

            // Try to load saved data (will be enhanced in M3 with SaveManager)
            LoadBasicData();
        }

        private void Update()
        {
            // Generate passive income over time (idle mechanic)
            GeneratePassiveIncome();
        }

        /// <summary>
        /// Calculates the cost for upgrading a specific attribute to the next level.
        /// Formula: baseCost * (multiplier ^ currentLevel)
        /// </summary>
        public double GetUpgradeCost(int currentLevel)
        {
            return baseUpgradeCost * Math.Pow(upgradeCostMultiplier, currentLevel);
        }

        /// <summary>
        /// Attempts to purchase an upgrade for the specified attribute.
        /// Returns true if successful, false if insufficient funds.
        /// </summary>
        public bool PurchaseUpgrade(UpgradeType type)
        {
            double cost = GetUpgradeCost(GetCurrentLevel(type));

            if (softCurrency >= cost)
            {
                softCurrency -= cost;
                IncreaseLevel(type);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the current level for a specific upgrade type.
        /// </summary>
        public int GetCurrentLevel(UpgradeType type)
        {
            return type switch
            {
                UpgradeType.Stamina => staminaLevel,
                UpgradeType.Power => powerLevel,
                UpgradeType.Income => incomeLevel,
                _ => 0
            };
        }

        /// <summary>
        /// Increases the level of a specific upgrade type.
        /// </summary>
        public void IncreaseLevel(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Stamina:
                    staminaLevel++;
                    break;
                case UpgradeType.Power:
                    powerLevel++;
                    break;
                case UpgradeType.Income:
                    incomeLevel++;
                    break;
            }

            SaveBasicData();
        }

        /// <summary>
        /// Gets the current income per second based on income level.
        /// Formula: baseIncomePerSecond * (1 + incomeLevel * incomeLevelMultiplier)
        /// </summary>
        public float GetIncomePerSecond()
        {
            return baseIncomePerSecond * (1f + incomeLevel * incomeLevelMultiplier);
        }

        /// <summary>
        /// Adds soft currency to the player's balance.
        /// </summary>
        public void AddSoftCurrency(double amount)
        {
            softCurrency += amount;
            SaveBasicData();
        }

        /// <summary>
        /// Adds hard currency to the player's balance.
        /// </summary>
        public void AddHardCurrency(double amount)
        {
            hardCurrency += amount;
            SaveBasicData();
        }

        /// <summary>
        /// Generates passive income over time (idle mechanic).
        /// Called every frame, but only actually adds currency at set intervals.
        /// </summary>
        private void GeneratePassiveIncome()
        {
            // Only generate income if we've earned at least once this session
            // (prevents income before first race)
            if (!_hasEarnedThisSession) return;

            float incomePerSecond = GetIncomePerSecond();
            float incomeToAdd = incomePerSecond * Time.deltaTime;

            if (incomeToAdd >= 0.01f) // Only add if meaningful amount
            {
                AddSoftCurrency(incomeToAdd);
                _lastEarnTimestamp = Time.time;
            }
        }

        /// <summary>
        /// Marks that the player has earned currency from an active race.
        /// Enables passive income generation.
        /// </summary>
        public void OnRaceEarned()
        {
            _hasEarnedThisSession = true;
            _lastEarnTimestamp = Time.time;
        }

        #region Save/Load (Basic - to be enhanced with SaveManager in M3)

        private void LoadBasicData()
        {
            // In M3, this will be replaced with SaveManager integration
            // For now, using PlayerPrefs for persistence

            softCurrency = (double)PlayerPrefs.GetFloat("SoftCurrency", 0);
            hardCurrency = (double)PlayerPrefs.GetFloat("HardCurrency", 0);
            staminaLevel = PlayerPrefs.GetInt("StaminaLevel", 0);
            powerLevel = PlayerPrefs.GetInt("PowerLevel", 0);
            incomeLevel = PlayerPrefs.GetInt("IncomeLevel", 0);
        }

        private void SaveBasicData()
        {
            // In M3, this will be replaced with SaveManager integration
            PlayerPrefs.SetFloat("SoftCurrency", (float)softCurrency);
            PlayerPrefs.SetFloat("HardCurrency", (float)hardCurrency);
            PlayerPrefs.SetInt("StaminaLevel", staminaLevel);
            PlayerPrefs.SetInt("PowerLevel", powerLevel);
            PlayerPrefs.SetInt("IncomeLevel", incomeLevel);
            PlayerPrefs.Save();
        }

        #endregion
    }

    /// <summary>
    /// Enum representing the three upgrade axes in the game.
    /// </summary>
    public enum UpgradeType
    {
        Stamina,
        Power,
        Income
    }
}