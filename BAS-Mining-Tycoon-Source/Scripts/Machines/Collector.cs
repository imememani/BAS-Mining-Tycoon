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
            if (!(collider.GetComponentInParent<TycoonWorldObject>() is TycoonWorldObject worldObject) || !(worldObject.data is OreItem item))
            { return; }

            // Collect the ore.
            // TODO: Fancy vanish animation.
            Destroy(worldObject.gameObject);
            
            // Add currency.
            TycoonSaveHandler.Current.currency += item.value;
            AudioSource.PlayClipAtPoint(TycoonShop.local.shopSFX.sounds[1], Player.local.transform.position);

            // Display floaty.
            TycoonFloatyText.Create(Player.local.head.transform.position + (Player.local.head.transform.forward * 0.5f), Player.local.head.transform, $"+ {item.value}", 3.0f);

            Logging.Log($"Collected '{item.value}' doubloons from '{item.id}'!");
        }
    }
}