using BASLogger;
using System.Collections;
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

        private bool despawning = false;

        /// <summary>
        /// Create a new floaty!
        /// </summary>
        public static void CreateFloatyCurrency(string text, Vector3 position, Transform lookTarget, float time)
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

        /// <summary>
        /// Create a new floaty!
        /// </summary>
        public static void CreateFloatyText(string text, Vector3 position, Transform lookTarget, float time)
        {
            Catalog.InstantiateAsync("Tycoon.UI.FloatyText", position, Quaternion.identity, null, go =>
            {
                TycoonFloatyText floaty = go.AddComponent<TycoonFloatyText>();
                floaty.GetComponent<TextMesh>().text = text;
                floaty.transform.GetChild(0).gameObject.SetActive(false);

                floaty.transform.position = position + new Vector3(0, Random.Range(-0.1f, 0.1f), 0);
                floaty.lookTarget = lookTarget;
                floaty.timer = Time.time + time;

                Logging.Log("Floaty text created!");
            }, "FloatyText->Create");
        }

        private void Update()
        {
            if (Time.time > timer && !despawning)
            {
                despawning = true;
                StartCoroutine(Despawn());
            }

            // Move up.
            transform.position = Vector3.Slerp(transform.position, transform.position + new Vector3(0, 0.1f, 0), Time.deltaTime * 2);
            transform.LookAt(lookTarget);
        }

        /// <summary>
        /// Slowly fade out the text before destroying it.
        /// </summary>
        private IEnumerator Despawn()
        {
            TextMesh text = GetComponent<TextMesh>();
            Color col = text.color;

            for (float i = col.a; i >= 0; i -= Time.deltaTime)
            {
                col.a = i;
                text.color = col;
                yield return null;
            }

            yield return null;

            Destroy(gameObject);

            yield break;
        }
    }
}