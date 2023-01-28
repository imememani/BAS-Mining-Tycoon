using BASLogger;
using MiningTycoon.Scripts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;
using UnityEngine.UI;

namespace MiningTycoon
{
    public class TycoonShop : MonoBehaviour
    {
        public static TycoonShop local;

        private readonly string categoryAddress = "Tycoon.UI.Category";
        private readonly string shopItemAddress = "Tycoon.UI.ShopItem";

        public AudioContainer shopSFX;

        // Tick timer.
        public Image tickTimer;

        // Generic anchors.
        private Transform categoryAnchor;
        private Transform itemAnchor;
        private Transform spawnPoint;

        // Item display.
        private Item currentlyDisplaying;
        private GameObject itemUI;
        private Button purchase;
        private Image itemIcon;
        private Text itemCost;
        private Text itemName;
        private Text itemDescription;

        // Player stat panel.
        private Text currencyDisplay;
        private Text oreCollectionDisplay;
        private Text playtimeDisplay;

        private string currentCategory;
        private Text currentCategoryText;

        private readonly Dictionary<string, List<Item>> categories = new Dictionary<string, List<Item>>();

        private void Awake()
        {
            local = this;

            // Load shop sounds.
            Catalog.LoadAssetAsync<AudioContainer>("Tycoon.Audio.StoreSounds", container => shopSFX = container, "Shop->Awake");

            // Locate the anchors.
            categoryAnchor = transform.GetChild(2).GetChild(2);
            itemAnchor = transform.GetChild(2).GetChild(3);
            spawnPoint = transform.GetChild(2).GetChild(5);

            // Locate ui stats.
            currencyDisplay = transform.GetChild(3).GetChild(0).GetChild(1).GetComponent<Text>();
            oreCollectionDisplay = transform.GetChild(3).GetChild(1).GetChild(1).GetComponent<Text>();
            playtimeDisplay = transform.GetChild(3).GetChild(2).GetChild(1).GetComponent<Text>();
            tickTimer = transform.GetChild(3).GetChild(4).GetComponent<Image>();

            // Item panel.
            itemUI = transform.GetChild(2).GetChild(4).gameObject;
            itemIcon = itemUI.transform.GetChild(2).GetComponent<Image>();
            itemName = itemUI.transform.GetChild(3).GetComponent<Text>();
            itemDescription = itemUI.transform.GetChild(4).GetComponent<Text>();
            itemCost = itemUI.transform.GetChild(5).GetChild(1).GetComponent<Text>();
            purchase = itemUI.transform.GetChild(6).GetComponent<Button>();

            purchase.onClick.AddListener(() => Purchase(currentlyDisplaying.id, currentlyDisplaying.value));
            itemUI.transform.GetChild(7).GetComponent<Button>().onClick.AddListener(() => DisplayItem(null));

            // Collector machine.
            transform.GetChild(4).GetChild(0).gameObject.AddComponent<Collector>();

            // Clear any UI.
            Refresh();

            // Set category.
            currentCategory = "Items";

            // Add categories.
            AddCategory("Items");
            AddCategory("Misc");

            // Event hooks.
            TycoonSaveHandler.PlayerLoaded -= HandlePlayerLoad;
            TycoonSaveHandler.PlayerLoaded += HandlePlayerLoad;

            Logging.Log("Shop initialized.");
        }

        private void Start()
        {
            FillCategories();
            Refresh();
        }

        private void LateUpdate()
        {
            // Await init.
            if (currencyDisplay == null || TycoonSaveHandler.Current == null)
            { return; }

            TimeSpan time = TycoonSaveHandler.Current.GetRealtimePlaytime();
            playtimeDisplay.text = $"{(time.Days > 0 ? $"{time.Days}D " : "")}{(time.Hours > 0 ? $"{time.Hours}H " : "")}{(time.Minutes > 0 ? $"{time.Minutes}M " : "")}{(time.Seconds > 0 ? $"{time.Seconds}S " : "")}";
        }

        /// <summary>
        /// Can the target item be purchased?
        /// </summary>
        public static bool CanPurchase(double value)
        {
            return TycoonSaveHandler.Current != null && TycoonSaveHandler.Current.doubloons >= value;
        }

        /// <summary>
        /// Purchase an item.
        /// </summary>
        public static void Purchase(string id, double value)
        {
            // Deduct doubloons.
            TycoonSaveHandler.Current.AddCurrency(-value);

            // Spawn the item.
            if (Tycoon.IsTycoonItem(id))
            {
                if (Tycoon.ItemDatabase[id] is CreatureItem)
                { Tycoon.SpawnTycoonCreature(id, local.spawnPoint.position, 0); }
                else
                { Tycoon.SpawnTycoonItem(id, local.spawnPoint.position, Quaternion.identity); }
            }

            // Play sound.
            // 0 - Purchase.
            AudioSource.PlayClipAtPoint(local.shopSFX.sounds[0], Player.local.transform.position);

            // Display floaty.
            TycoonFloatyText.CreateFloatyCurrency($"<color=white>{id}</color>\n     <color=red>-{value.FormatDoubloons()}</color>",
                                        TycoonUtilities.GetFloatyTextPlayerAnchor(),
                                       Player.local.head.transform,
                                           3.0f);
        }

        /// <summary>
        /// Add an item to the shop.
        /// </summary>
        public void AddItem(Item item, string category)
        {
            if (!categories.TryGetValue(category, out _))
            { return; }

            categories[category].Add(item);
            Logging.Log($"{item.id} registered to category '{category}'!");
        }

        public void AddCategory(string id)
        {
            if (categories.ContainsKey(id))
            { return; }

            categories.Add(id, new List<Item>());
            Logging.Log($"{id} category added!");
        }

        /// <summary>
        /// Display an item for purchase.
        /// </summary>
        private void DisplayItem(Item item)
        {
            itemUI.SetActive(item != null);
            currentlyDisplaying = item;

            if (item == null)
            { return; }

            // Name.
            itemName.text = !string.IsNullOrEmpty(item.displayName) ? item.displayName : item.id;

            // Description.
            itemDescription.text = item.GetShopDescription();

            // Cost.
            itemCost.text = $"<color={(CanPurchase(item.value) ? "white" : "red")}>{item.value.FormatDoubloons()}</color>";

            // Icon.
            Tycoon.LoadObject<Texture2D>(item.iconAddress, icon => itemIcon.sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero, 10, 0, SpriteMeshType.FullRect));

            // Enable/Disable purchase.
            purchase.interactable = CanPurchase(item.value);
        }

        /// <summary>
        /// Clear all categories.
        /// </summary>
        private void ClearCategories()
        {
            // Clear categories.
            for (int i = categoryAnchor.childCount - 1; i >= 0; i--)
            {
                Destroy(categoryAnchor.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Clear all items.
        /// </summary>
        private void ClearItems()
        {
            for (int i = itemAnchor.childCount - 1; i >= 0; i--)
            {
                Destroy(itemAnchor.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Display a category and its items.
        /// </summary>
        private void OpenCategory(string id)
        {
            Logging.Log($"Opening category: {id}");
            ClearItems();

            currentCategory = id;

            // Order items by cost.
            Item[] items = categories[currentCategory].OrderBy(item => item.value).ToArray();

            // Spawn new items.
            for (int i = 0; i < items.Length; i++)
            {
                int index = i;
                Logging.Log($"Creating shop item: {items[i].id}");

                Catalog.InstantiateAsync(shopItemAddress, Vector3.zero, Quaternion.identity, null, go =>
                {
                    go.transform.SetParent(itemAnchor);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

                    // Icon.
                    Tycoon.LoadObject<Texture2D>(items[index].iconAddress, icon => go.transform.GetChild(2).GetComponent<Image>().sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero, 10, 0, SpriteMeshType.FullRect));

                    // Title.
                    go.transform.GetChild(4).GetComponent<Text>().text = !string.IsNullOrEmpty(items[index].displayName) ? items[index].displayName : items[index].id;

                    // Cost.
                    go.transform.GetChild(3).GetChild(1).GetComponent<Text>().text = items[index].value.FormatDoubloons();

                    // Display.
                    go.GetComponent<Button>().onClick.AddListener(() => DisplayItem(items[index]));
                }, "Shop->ShopItem");
            }
        }

        /// <summary>
        /// Display all categories.
        /// </summary>
        private void DisplayCategories()
        {
            ClearCategories();

            foreach (var key in categories)
            {
                Logging.Log($"Creating category: {key.Key}");

                Catalog.InstantiateAsync(categoryAddress, Vector3.zero, Quaternion.identity, null, go =>
                {
                    go.transform.SetParent(categoryAnchor);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

                    Text text = go.transform.GetChild(0).GetChild(0).GetComponent<Text>();
                    text.text = key.Key;

                    if (currentCategory == key.Key)
                    {
                        text.color = Color.green;
                        currentCategoryText = text;
                    }

                    go.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if (currentCategory != null) currentCategoryText.color = Color.white;

                        currentCategoryText = text;
                        text.color = Color.green;

                        OpenCategory(key.Key);
                    });

                    go.name = key.Key;
                }, "Shop->Category");
            }
        }

        /// <summary>
        /// Refresh the UI.
        /// </summary>
        private void Refresh()
        {
            // Clear UI.
            ClearCategories();
            ClearItems();

            // Re-render categories.
            DisplayCategories();

            // Open the current category.
            if (!string.IsNullOrEmpty(currentCategory))
            { OpenCategory(currentCategory); }

            // Display the current item or close.
            DisplayItem(currentlyDisplaying);

            Logging.Log($"Shop refreshed (category: {currentCategory}).");
        }

        /// <summary>
        /// Add items to categories.
        /// </summary>
        private void FillCategories()
        {
            // Add all the pickaxes.
            foreach (var entry in Tycoon.ItemDatabase)
            {
                if (entry.Value is ToolItem)
                {
                    AddItem(entry.Value, "Items");
                }
            }

            AddItem(Tycoon.ItemDatabase["Chicken"], "Misc");
        }

        /// <summary>
        /// Invoked when the player is loaded.
        /// </summary>
        private void HandlePlayerLoad(TycoonPlayer player)
        {
            // Update display.
            currencyDisplay.text = $"{TycoonSaveHandler.Current.doubloons.FormatDoubloons()}";
            oreCollectionDisplay.text = TycoonSaveHandler.Current.oresCollected.ToString();

            // Hook in to events.
            TycoonSaveHandler.Current.oreChanged += amount =>
            {
                // Update display.
                oreCollectionDisplay.text = TycoonSaveHandler.Current.oresCollected.ToString();
            };
            TycoonSaveHandler.Current.currencyChanged += amount =>
            {
                // Update display.
                currencyDisplay.text = $"{TycoonSaveHandler.Current.doubloons.FormatDoubloons()}";

                // Refresh shop if an item is on display.
                if (currentlyDisplaying != null)
                { Refresh(); }
            };
        }
    }
}