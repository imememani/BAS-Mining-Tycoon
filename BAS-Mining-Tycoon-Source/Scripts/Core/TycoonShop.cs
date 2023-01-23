using BASLogger;
using System;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using UnityEngine.UI;

namespace MiningTycoon
{
    public class TycoonShop : MonoBehaviour
    {
        private readonly string categoryAddress = "Tycoon.UI.Category";
        private readonly string shopItemAddress = "Tycoon.UI.ShopItem";

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

        private AudioContainer shopSFX;

        private readonly Dictionary<string, List<Item>> categories = new Dictionary<string, List<Item>>();

        private void Awake()
        {
            // Load shop sounds.
            Catalog.LoadAssetAsync<AudioContainer>("Tycoon.Audio.StoreSounds", container => shopSFX = container, "Shop->Awake");

            // Locate the anchors.
            categoryAnchor = transform.GetChild(2).GetChild(1);
            itemAnchor = transform.GetChild(2).GetChild(3);
            spawnPoint = transform.GetChild(2).GetChild(4);

            // Locate ui stats.
            currencyDisplay = transform.GetChild(4).GetChild(2).GetChild(4).GetComponent<Text>();
            oreCollectionDisplay = transform.GetChild(4).GetChild(2).GetChild(5).GetComponent<Text>();
            playtimeDisplay = transform.GetChild(4).GetChild(2).GetChild(6).GetComponent<Text>();

            // Item panel.
            itemUI = transform.GetChild(2).GetChild(5).gameObject;
            itemIcon = itemUI.transform.GetChild(2).GetComponent<Image>();
            itemName = itemUI.transform.GetChild(4).GetComponent<Text>();
            itemDescription = itemUI.transform.GetChild(5).GetComponent<Text>();
            itemCost = itemUI.transform.GetChild(6).GetComponent<Text>();
            purchase = itemUI.transform.GetChild(7).GetComponent<Button>();

            purchase.onClick.AddListener(() => Purchase(currentlyDisplaying));
            itemUI.transform.GetChild(8).GetComponent<Button>().onClick.AddListener(() => DisplayItem(null));

            // Clear any UI.
            Refresh();

            // Add categories.
            AddCategory("Items");
            currentCategory = "Items";

            Logging.Log("Shop initialized.");
        }

        private void Start()
        {
            // Add all the pickaxes.
            foreach (var entry in Entry.ItemDatabase)
            {
                if (entry.Value is ToolItem)
                {
                    AddItem(entry.Value, "Items");
                }
            }

            Refresh();
        }

        private void LateUpdate()
        {
            // Await init.
            if (currencyDisplay == null || TycoonSaveHandler.Current == null)
            { return; }

            currencyDisplay.text = TycoonSaveHandler.Current.currency.ToString();
            oreCollectionDisplay.text = "0";

            TimeSpan playTime = DateTime.Now.Subtract(TycoonSaveHandler.Current.profileCreationDate);
            playtimeDisplay.text = $"{playTime.Days}D {playTime.Hours}H {playTime.Minutes}M {playTime.Seconds}S";
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
            itemName.text = item.id;

            // Description.
            itemDescription.text = item.GetShopDescription();

            // Cost.
            itemCost.text = $"<color={(CanPurchase(item) ? "white" : "red")}>{item.value}</color>";

            // Icon.
            Entry.LoadObject<Texture2D>(item.iconAddress, icon => itemIcon.sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero, 10, 0, SpriteMeshType.FullRect));

            // Enable/Disable purchase.
            purchase.interactable = CanPurchase(item);
        }

        /// <summary>
        /// Display a category and its items.
        /// </summary>
        private void DisplayCategory(string id)
        {
            // Remove any current children.
            for (int i = itemAnchor.childCount - 1; i >= 0; i--)
            {
                Destroy(itemAnchor.GetChild(i).gameObject);
            }

            currentCategory = id;
            List<Item> items = categories[currentCategory];

            // Spawn new items.
            for (int i = 0; i < items.Count; i++)
            {
                int index = i;

                Catalog.InstantiateAsync(shopItemAddress, Vector3.zero, Quaternion.identity, null, go =>
                {
                    go.transform.SetParent(itemAnchor);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

                    Entry.LoadObject<Texture2D>(items[index].iconAddress, icon => go.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero, 10, 0, SpriteMeshType.FullRect));

                    go.transform.GetChild(2).GetComponent<Text>().text = items[index].id;
                    go.GetComponent<Button>().onClick.AddListener(() => DisplayItem(items[index]));
                }, "Shop->ShopItem");
            }
        }

        /// <summary>
        /// Display all categories.
        /// </summary>
        private void DisplayCategories()
        {
            // Remove any existing categories.
            for (int i = categoryAnchor.childCount - 1; i >= 0; i--)
            {
                Destroy(categoryAnchor.GetChild(i).gameObject);
            }

            foreach (var key in categories)
            {
                Catalog.InstantiateAsync(categoryAddress, Vector3.zero, Quaternion.identity, null, go =>
                {
                    go.transform.SetParent(categoryAnchor);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

                    Text text = go.transform.GetChild(0).GetChild(0).GetComponent<Text>();
                    text.text = key.Key;

                    if (currentCategory == key.Key)
                    { text.color = Color.green; }

                    go.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if (currentCategory != null) currentCategoryText.color = Color.white;

                        currentCategoryText = text;
                        text.color = Color.green;

                        DisplayCategory(key.Key);
                    });

                    go.name = key.Key;
                }, "Shop->Category");
            }
        }

        /// <summary>
        /// Can the target item be purchased?
        /// </summary>
        private bool CanPurchase(Item item)
        {
            return TycoonSaveHandler.Current.currency >= item.value;
        }

        /// <summary>
        /// Purchase an item.
        /// </summary>
        private void Purchase(Item item)
        {
            // Deduct doubloons.
            TycoonSaveHandler.Current.currency -= item.value;

            // Spawn the item.
            Entry.SpawnTycoonItem(item.id, spawnPoint.position, Quaternion.identity);

            // Refresh the shop.
            Refresh();

            // Play sound.
            // 0 - Purchase.
            AudioSource.PlayClipAtPoint(shopSFX.sounds[0], Player.local.transform.position);

            TycoonSaveHandler.Save();
        }

        /// <summary>
        /// Refresh the UI.
        /// </summary>
        private void Refresh()
        {
            // Re-render categories.
            DisplayCategories();

            // Open the current category.
            if (!string.IsNullOrEmpty(currentCategory))
            { DisplayCategory(currentCategory); }

            // Display the current item or close.
            DisplayItem(currentlyDisplaying);

            Logging.Log($"Shop refreshed (category: {currentCategory}).");
        }
    }
}