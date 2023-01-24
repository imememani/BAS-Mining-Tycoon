using BASLogger;
using MiningTycoon.Scripts.Core;
using ThunderRoad;
using UnityEngine;

namespace MiningTycoon
{
    public class Collector : Machine
    {
        protected override void Setup()
        {
            listenForTriggerEvents = true;

            Logging.Log("Collector initialized!");
        }

        protected override void TriggerEntered(Collider collider)
        {
            // Ensure it's ore.
            if (!(collider.GetComponentInParent<TycoonWorldObject>() is TycoonWorldObject worldObject) || !(worldObject.data is Item item))
            { return; }

            // Collect the ore.
            // TODO: Fancy vanish animation.
            Destroy(worldObject.gameObject);

            // Add currency.
            float value = item is OreItem ? item.value : item.value / 3;
            TycoonSaveHandler.Current.AddCurrency(value);
            TycoonSaveHandler.Current.AddOre(1);
            AudioSource.PlayClipAtPoint(TycoonShop.local.shopSFX.sounds[1], Player.local.transform.position);

            // Display floaty.
            TycoonFloatyText.Create($"<color=white>{item.id}</color>\n      <color=yellow>{value.FormatDoubloons()}</color>", Player.local.head.transform.position + (Player.local.head.transform.forward * 0.65f), Player.local.head.transform, 3.0f);

            Logging.Log($"Collected '{value}' doubloons from '{item.id}'!");
        }
    }
}