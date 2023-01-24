using UnityEngine;

namespace MiningTycoon
{
    /// <summary>
    /// A machine.
    /// </summary>
    public class Machine : MonoBehaviour
    {
        public virtual int MachineID { get; set; }

        protected bool listenForTriggerEvents = true;

        protected virtual void Awake() => Setup();

        protected virtual void Setup()
        { }

        protected virtual void TriggerEntered(Collider collider)
        { }

        private void OnTriggerEnter(Collider collider)
        {
            if (!listenForTriggerEvents)
            { return; }

            TriggerEntered(collider);
        }
    }
}