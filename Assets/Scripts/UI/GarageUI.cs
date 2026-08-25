using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TurboGaraj.UI
{
    /// <summary>
    /// Manages the Garage UI: vehicle selection, upgrade system (Stamina/Power/Income),
    /// and project screen for collecting parts to build new vehicles.
    /// </summary>
    public class GarageUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the EconomyManager in the scene")]
        public EconomyManager economyManager;

        [Header("Vehicle Selection")]
        [Tooltip("List of vehicle buttons (for selection/unlocking)")]
        public List<Button> vehicleButtons;
        [Tooltip("Text to display the currently selected vehicle's name")]
        public Text selectedVehicleNameText;

        [Header("Upgrade UI")]
        [Tooltip("Panel containing the upgrade buttons and info")]
        public GameObject upgradePanel;
        [Tooltip("Text for Stamina level")]
        public Text staminaLevelText;
        [Tooltip("Text for Stamina upgrade cost")]
        public Text staminaCostText;
        [Tooltip("Button to upgrade Stamina")]
        public Button staminaUpgradeButton;
        [Tooltip("Text for Power level")]
        public Text powerLevelText;
        [Tooltip("Text for Power upgrade cost")]
        public Text powerCostText;
        [Tooltip("Button to upgrade Power")]
        public Button powerUpgradeButton;
        [Tooltip("Text for Income level")]
        public Text incomeLevelText;
        [Tooltip("Text for Income upgrade cost")]
        public Text incomeCostText;
        [Tooltip("Button to upgrade Income")]
        public Button incomeUpgradeButton;

        [Header("Project UI")]
        [Tooltip("Panel containing the project progress and build button")]
        public GameObject projectPanel;
        [Tooltip("Text showing collected parts / required parts")]
        public Text projectProgressText;
        [Tooltip("Button to build a new vehicle when enough parts are collected")]
        public Button buildProjectButton;
        [Tooltip("Number of parts required to build a new vehicle")]
        public int partsRequiredForNewVehicle = 10;
        [Tooltip("Key used to store collected parts in PlayerPrefs (temporary until SaveManager)")]
        public string projectPartsPrefsKey = "TurboGaraj_ProjectParts";

        [Header("General UI")]
        [Tooltip("Text to display the player's soft currency")]
        public Text softCurrencyText;
        [Tooltip("Text to display the player's hard currency")]
        public Text hardCurrencyText;
        [Tooltip("Button to toggle between Upgrade and Project panels")]
        public Button togglePanelButton;
        [Tooltip("Text on the toggle panel button (shows which panel we're switching to)")]
        public Text togglePanelButtonText;

        // Internal state
        private bool _showingUpgradePanel = true;

        private void Awake()
        {
            // Validate references
            if (economyManager == null)
            {
                economyManager = FindObjectOfType<EconomyManager>();
                if (economyManager == null)
                {
                    Debug.LogError("[GarageUI] No EconomyManager found in scene. Please assign one or ensure it exists.");
                }
            }

            // Initialize UI state
            TogglePanel(_showingUpgradePanel);
            RefreshCurrencyUI();
            RefreshUpgradeUI();
            RefreshProjectUI();

            // Add listeners
            if (staminaUpgradeButton != null) staminaUpgradeButton.onClick.AddListener(OnStaminaUpgradeClicked);
            if (powerUpgradeButton != null) powerUpgradeButton.onClick.AddListener(OnPowerUpgradeClicked);
            if (incomeUpgradeButton != null) incomeUpgradeButton.onClick.AddListener(OnIncomeUpgradeClicked);
            if (buildProjectButton != null) buildProjectButton.onClick.AddListener(OnBuildProjectClicked);
            if (togglePanelButton != null) togglePanelButton.onClick.AddListener(TogglePanelClicked);
        }

        private void OnEnable()
        {
            // Refresh data when the UI is shown (e.g., after returning from a race)
            RefreshCurrencyUI();
            RefreshUpgradeUI();
            RefreshProjectUI();
        }

        #region UI Refresh Methods

        private void RefreshCurrencyUI()
        {
            if (economyManager != null)
            {
                if (softCurrencyText != null)
                    softCurrencyText.text = FormatCurrency(economyManager.softCurrency);
                if (hardCurrencyText != null)
                    hardCurrencyText.text = FormatCurrency(economyManager.hardCurrency);
            }
        }

        private void RefreshUpgradeUI()
        {
            if (economyManager == null) return;

            // Stamina
            if (staminaLevelText != null)
                staminaLevelText.text = $"Stamina: {economyManager.staminaLevel}";
            if (staminaCostText != null)
                staminaCostText.text = FormatCurrency(economyManager.GetUpgradeCost(economyManager.staminaLevel));
            if (staminaUpgradeButton != null)
                staminaUpgradeButton.interactable = economyManager.softCurrency >= economyManager.GetUpgradeCost(economyManager.staminaLevel);

            // Power
            if (powerLevelText != null)
                powerLevelText.text = $"Power: {economyManager.powerLevel}";
            if (powerCostText != null)
                powerCostText.text = FormatCurrency(economyManager.GetUpgradeCost(economyManager.powerLevel));
            if (powerUpgradeButton != null)
                powerUpgradeButton.interactable = economyManager.softCurrency >= economyManager.GetUpgradeCost(economyManager.powerLevel);

            // Income
            if (incomeLevelText != null)
                incomeLevelText.text = $"Income: {economyManager.incomeLevel}";
            if (incomeCostText != null)
                incomeCostText.text = FormatCurrency(economyManager.GetUpgradeCost(economyManager.incomeLevel));
            if (incomeUpgradeButton != null)
                incomeUpgradeButton.interactable = economyManager.softCurrency >= economyManager.GetUpgradeCost(economyManager.incomeLevel);
        }

        private void RefreshProjectUI()
        {
            int partsCollected = PlayerPrefs.GetInt(projectPartsPrefsKey, 0);
            if (projectProgressText != null)
                projectProgressText.text = $"Parts: {partsCollected} / {partsRequiredForNewVehicle}";
            if (buildProjectButton != null)
                buildProjectButton.interactable = partsCollected >= partsRequiredForNewVehicle;
        }

        #endregion

        #region Button Click Handlers

        private void OnStaminaUpgradeClicked()
        {
            if (economyManager != null && economyManager.PurchaseUpgrade(EconomyManager.UpgradeType.Stamina))
            {
                RefreshCurrencyUI();
                RefreshUpgradeUI();
            }
        }

        private void OnPowerUpgradeClicked()
        {
            if (economyManager != null && economyManager.PurchaseUpgrade(EconomyManager.UpgradeType.Power))
            {
                RefreshCurrencyUI();
                RefreshUpgradeUI();
            }
        }

        private void OnIncomeUpgradeClicked()
        {
            if (economyManager != null && economyManager.PurchaseUpgrade(EconomyManager.UpgradeType.Income))
            {
                RefreshCurrencyUI();
                RefreshUpgradeUI();
            }
        }

        private void OnBuildProjectClicked()
        {
            // Placeholder: In a full implementation, this would unlock a new vehicle
            // For now, we'll just reset the parts and show a message
            PlayerPrefs.SetInt(projectPartsPrefsKey, 0);
            RefreshProjectUI();
            Debug.Log("[GarageUI] New vehicle built! (placeholder)");
            // TODO: Actually unlock a new vehicle and update vehicleButtons
        }

        private void TogglePanelClicked()
        {
            _showingUpgradePanel = !_showingUpgradePanel;
            TogglePanel(_showingUpgradePanel);
        }

        #endregion

        #region Helper Methods

        private void TogglePanel(bool showUpgradePanel)
        {
            if (upgradePanel != null) upgradePanel.SetActive(showUpgradePanel);
            if (projectPanel != null) projectPanel.SetActive(!showUpgradePanel);
            if (togglePanelButtonText != null)
                togglePanelButtonText.text = showUpgradePanel ? "Go to Project" : "Go to Upgrades";
        }

        /// <summary>
        /// Call this method from other systems (e.g., TrackManager) when the player collects a part.
        /// </summary>
        public void AddProjectPart(int amount = 1)
        {
            int current = PlayerPrefs.GetInt(projectPartsPrefsKey, 0);
            PlayerPrefs.SetInt(projectPartsPrefsKey, current + amount);
            RefreshProjectUI();
        }

        private string FormatCurrency(double amount)
        {
            // Simple formatting: show as integer if no decimal, otherwise show 2 decimal places
            if (amount >= 1000000)
                return $"{(amount / 1000000f):F1}M";
            if (amount >= 1000)
                return $"{(amount / 1000f):F1}K";
            return amount >= 1 ? ((long)amount).ToString() : amount.ToString("F2");
        }

        #endregion
    }
}