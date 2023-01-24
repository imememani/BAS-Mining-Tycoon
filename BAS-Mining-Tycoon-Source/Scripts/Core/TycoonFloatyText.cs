using BASLogger;
using ThunderRoad;
using UnityEngine;

namespace MiningTycoon.Scripts.Core
{
    /// <summary>
    /// Floaty text!
    /// </summary>
    public class TycoonFloatyText : MonoBehaviour
    {
        private Transform lookTarget;
        private float timer;

        /// <summary>
        /// Create a new floaty!
        /// </summary>
        public static void Create(string text, Vector3 position, Transform lookTarget, float time)
        {
            Catalog.InstantiateAsync("Tycoon.UI.FloatyText", position, Quaternion.identity, null, go =>
            {
                TycoonFloatyText floaty = go.AddComponent<TycoonFloatyText>();
                floaty.GetComponent<TextMesh>().text = text;

                floaty.transform.position = position + new Vector3(0, Random.Range(-0.1f, 0.1f), 0);
                floaty.lookTarget = lookTarget;
                floaty.timer = Time.time + time;

                Logging.Log("Floaty text created!");
            }, "FloatyText->Create");
        }

        private void Update()
        {
            if (Time.time > timer)
            {
                // TODO: Fancy dissolve maybe?
                Destroy(gameObject);
                return;
            }

            // Move up.
            transform.position = Vector3.Slerp(transform.position, transform.position + new Vector3(0, 0.1f, 0), Time.deltaTime * 2);
            transform.LookAt(lookTarget);
        }
    }
}