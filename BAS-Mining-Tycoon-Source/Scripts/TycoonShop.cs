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

        private Transform categoryAnchor;
        private Transform itemAnchor;
        private Transform spawnPoint;

        private string currentCategory;
        private Text currentCategoryText;

        private readonly Dictionary<string, List<Item>> categories = new Dictionary<string, List<Item>>();

        private void Awake()
        {
            categoryAnchor = transform.GetChild(1).GetChild(1);
            itemAnchor = transform.GetChild(1).GetChild(3);
            spawnPoint = transform.GetChild(1).GetChild(4);

            // Clear any UI.
            Refresh();

            // Add categories.
            AddCategory("Items", "Items");
            currentCategory = "Items";
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

        /// <summary>
        /// Add an item to the shop.
        /// </summary>
        public void AddItem(Item item, string category)
        {
            if (!categories.TryGetValue(category, out _))
            { return; }

            categories[category].Add(item);
            Debug.Log($"[Mining Tycoon] {item.id} registered to category '{category}'!");
        }

        public void AddCategory(string id, string name)
        {
            if (categories.ContainsKey(id))
            { return; }

            categories.Add(id, new List<Item>());
            Debug.Log($"[Mining Tycoon] {id} category added!");
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

                    Entry.SpawnObject<Texture2D>(items[index].iconAddress, false, icon => go.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero, 10, 0, SpriteMeshType.FullRect));

                    go.transform.GetChild(2).GetComponent<Text>().text = items[index].id;
                    go.GetComponent<Button>().onClick.AddListener(() => Entry.SpawnItem(items[index].id, spawnPoint.position, Quaternion.identity));
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
        /// Refresh the UI.
        /// </summary>
        private void Refresh()
        {
            // Re-render categories.
            DisplayCategories();

            // Open the current category.
            if (!string.IsNullOrEmpty(currentCategory))
            { DisplayCategory(currentCategory); }
        }
    }
}