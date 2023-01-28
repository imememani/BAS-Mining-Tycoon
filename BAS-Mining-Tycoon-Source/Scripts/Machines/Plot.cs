using BASLogger;
using System;
using ThunderRoad;
using UnityEngine;
using UnityEngine.UI;

namespace MiningTycoon.Scripts.Core
{
    /// <summary>
    /// A machine plot.
    /// </summary>
    public class Plot : Machine
    {
        public double plotCost = 5000.0d; // 5K Doubloons.
        public PlotData data;

        // Blocker UI to purchase the plot.
        private GameObject plotMesh;
        private GameObject plotStatsUI;
        private GameObject plotPurchaseUI;
        private GameObject plotUpgradePurchaseUI;
        private GameObject secondTerminal;

        // Cache.
        private Transform moduleAnchor;
        private Button purchaseModule;
        private Button purchasePlot;
        private Text purchaseModuleName;
        private Text purchaseCost;
        private Text plotPurchaseCost;

        // Which module to upgrade?
        private TycoonUpgradeData currentlyDisplaying;

        private bool isPlotLoaded = false;

        protected override void Awake()
        {
            // Event Hooks.
            TycoonSaveHandler.Current.currencyChanged += amount => RefreshPlotUI();

            Tycoon.Tick -= Tick;
            Tycoon.Tick += Tick;

            // Obtain UI cache.
            UICache();

            // Try purchase the plot.
            purchasePlot.onClick.AddListener(TryPurchasePlot);
        }

        /// <summary>
        /// Cache the UI.
        /// </summary>
        private void UICache()
        {
            plotMesh = transform.GetChild(0).gameObject;
            plotMesh.SetActive(false);

            Transform uiRoot = transform.GetChild(2);
            secondTerminal = transform.GetChild(1).GetChild(1).gameObject;
            secondTerminal.SetActive(false);
            plotStatsUI = uiRoot.GetChild(0).gameObject;
            moduleAnchor = plotStatsUI.transform;
            plotPurchaseUI = uiRoot.GetChild(1).gameObject;
            plotUpgradePurchaseUI = uiRoot.GetChild(2).gameObject;
            plotUpgradePurchaseUI.SetActive(false);
            purchasePlot = plotPurchaseUI.transform.GetChild(2).GetComponent<Button>();
            plotPurchaseCost = plotPurchaseUI.transform.GetChild(1).GetChild(1).GetComponent<Text>();
            purchaseModuleName = plotUpgradePurchaseUI.transform.GetChild(0).GetComponent<Text>();
            purchaseCost = plotUpgradePurchaseUI.transform.GetChild(1).GetChild(1).GetComponent<Text>();
            purchaseModule = plotUpgradePurchaseUI.transform.GetChild(2).GetComponent<Button>();
        }

        protected override void Setup()
        {
            // Only setup when loaded.
            if (isPlotLoaded)
            { return; }

            // Plot been purchased yet?
            if (data == null)
            {
                // Refresh the UI.
                RefreshPlotUI();
                return;
            }

            // Enable drill.
            plotMesh.SetActive(true);

            // Enable the stat UI.
            plotStatsUI.SetActive(true);
            plotPurchaseUI.SetActive(false);

            // Cancel upgrade purchase.
            plotUpgradePurchaseUI.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => DisplayUpgradePurchase(null));

            // Handle purchase event.
            purchaseModule.onClick.AddListener(TryPurchaseModule);

            // Setup modules.
            // {} - value
            AddModule("Doubloon Production", "{} /m", data.doubloonsPerTick, 25.0f, 1.0f, val => data.doubloonsPerTick = val);
            AddModule("Doubloon Multiplier", "x{} /m", data.doubloonMultiplier, 25.0f, 0.01f, val => data.doubloonMultiplier = val);

            // Set flag.
            isPlotLoaded = true;

            // Refresh the UI.
            RefreshPlotUI();
        }

        private void Tick()
        {
            if (data != null)
            { TycoonSaveHandler.Current.AddCurrency(data.doubloonsPerTick * data.doubloonMultiplier, true); }
        }

        /// <summary>
        /// Load this plot.
        /// </summary>
        public void Load(PlotData data)
        {
            this.data = data;

            // Setup this plot.
            Setup();
        }

        /// <summary>
        /// Add an upgrade module.
        /// </summary>
        public void AddModule(string moduleName, string display, double currentValue, double cost, double incrementBy, Action<double> onValueChanged)
        {
            Catalog.InstantiateAsync("Tycoon.UI.UpgradeModule", Vector3.zero, Quaternion.identity, null, go =>
            {
                TycoonUpgradeData module = new TycoonUpgradeData()
                {
                    moduleName = moduleName,
                    moduleDisplay = display,

                    currentValue = currentValue,
                    cost = cost,
                    incrementBy = incrementBy,
                };

                go.transform.SetParent(moduleAnchor);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                go.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => DisplayUpgradePurchase(module));
                Text displayText = go.transform.GetChild(2).GetComponent<Text>();

                // Refresh display.
                displayText.text = display.Replace("{}", module.currentValue.FormatDoubloons());

                // Handle purchase.
                module.onPurchase = () =>
                {
                    // Bump the values.
                    module.cost += cost / 3;
                    module.currentValue += module.incrementBy;

                    // Refresh display.
                    displayText.text = display.Replace("{}", module.currentValue.FormatDoubloons());

                    // Invoke event.
                    onValueChanged?.Invoke(module.currentValue);

                    // Refresh the UI.
                    RefreshPlotUI();
                };

                Logging.Log($"Module added '{moduleName}' to {name}[{MachineID}]!");
            }, "Plot->AddModule");
        }

        /// <summary>
        /// Refresh the terminal ui.
        /// </summary>
        public void RefreshPlotUI()
        {
            // This plot has not been purchased.
            if (data == null)
            {
                // Refresh the plot cost.
                plotPurchaseCost.text = $"<color={(TycoonShop.CanPurchase(plotCost) ? "white" : "red")}>{plotCost.FormatDoubloons()}</color>";
                purchasePlot.interactable = TycoonShop.CanPurchase(plotCost);

                plotStatsUI.SetActive(false);
                plotPurchaseUI.SetActive(true);
                return;
            }

            // Refresh the upgrade ui if open.
            DisplayUpgradePurchase(currentlyDisplaying);
        }

        /// <summary>
        /// Display an upgrade to purchase.
        /// </summary>
        private void DisplayUpgradePurchase(TycoonUpgradeData module)
        {
            currentlyDisplaying = module;
            plotUpgradePurchaseUI?.SetActive(module != null);
            secondTerminal?.SetActive(module != null);

            if (module == null)
            { return; }

            // Module Name.
            purchaseModuleName.text = $"Upgrade {module.moduleName}?";

            // Cost.
            purchaseCost.text = $"<color={(TycoonShop.CanPurchase(module.cost) ? "white" : "red")}>{module.cost.FormatDoubloons()}</color>";

            // Can the player purchase?
            purchaseModule.interactable = TycoonShop.CanPurchase(module.cost);

            Logging.Log($"Displaying module: {module.moduleName}");
        }

        /// <summary>
        /// Try purchase the current module.
        /// </summary>
        private void TryPurchaseModule()
        {
            // Stop the purchase.
            if (!TycoonShop.CanPurchase(currentlyDisplaying.cost))
            { return; }

            // Purchase.
            TycoonShop.Purchase(currentlyDisplaying.moduleName, currentlyDisplaying.cost);

            // Invoke events.
            currentlyDisplaying?.onPurchase?.Invoke();
        }

        /// <summary>
        /// Try purchase this plot.
        /// </summary>
        private void TryPurchasePlot()
        {
            // Stop the purchase.
            if (!TycoonShop.CanPurchase(plotCost))
            { return; }

            // Purchase and open the plot.
            TycoonShop.Purchase("Plot", plotCost);
            data = new PlotData() { machineID = MachineID };
            TycoonSaveHandler.Current.plots.Add(data);

            // Reload the plot.
            Setup();

            // Save the current session.
            TycoonSaveHandler.Save();
        }
    }
}