using BASLogger;
using MiningTycoon.Scripts.Core;
using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
    /// <summary>
    /// A sell zone allows ore to be sold when entering the zone.
    /// </summary>
    public class SellZone : Machine
    {
        protected override void Setup()
        {
            listenForTriggerEvents = true;
            base.Setup();
        }

        protected override void TriggerEntered(Collider collider)
        {
            TycoonWorldObject worldObject = collider.GetComponentInParent<TycoonWorldObject>();

            if (worldObject == null || !(worldObject.data is OreItem))
            { return; }

            // Despawn the item.
            worldObject.Despawn();

            // Add currency.
            double value = worldObject.data.value;
            TycoonSaveHandler.Current.AddCurrency(value);
            TycoonSaveHandler.Current.AddOre(1);
            AudioSource.PlayClipAtPoint(TycoonShop.local.shopSFX.sounds[1], Player.local.transform.position);

            // Display floaty.
            TycoonFloatyText.CreateFloatyCurrency($"<color=white>{worldObject.data.id}</color>\n      <color=yellow>{value.FormatDoubloons()}</color>",
                                                TycoonUtilities.GetFloatyTextPlayerAnchor(),
                                                Player.local.head.transform,
                                                    3.0f);

            Logging.Log($"Collected '{value}' doubloons from '{worldObject.data.id}'!");
        }
    }
}