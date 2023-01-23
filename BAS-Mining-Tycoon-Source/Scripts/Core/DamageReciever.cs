using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MiningTycoon
{
    public class DamageReciever: MonoBehaviour
    {
        /// <summary>
        /// Subtract health.
        /// </summary>
        public virtual void TakeHealth(ref float health, float amount)
        {
            health -= amount;

            // Break?
            if (health <= 0)
            {
                // Break item.
                Break();
                return;
            }
        }

        /// <summary>
        /// Break this reciever.
        /// </summary>
        public virtual void Break() 
        {
        }
    }
}