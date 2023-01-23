using ThunderRoad;

namespace MiningTycoon
{
    /// <summary>
    /// Constructs a pickaxe.
    /// </summary>
    public class ConstructionModule : ItemModule
    {
        public string loadByID;

        public override void OnItemLoaded(ThunderRoad.Item item)
        {
            item.gameObject.AddComponent<Pickaxe>().Load(loadByID);
            base.OnItemLoaded(item);
        }
    }
}
