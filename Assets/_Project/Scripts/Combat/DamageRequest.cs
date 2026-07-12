using UnityEngine;

namespace PawVoyage.Combat
{
    /// <summary>
    /// Immutable data describing a single damage event.
    /// </summary>
    public readonly struct DamageRequest
    {
        public DamageRequest(int amount, GameObject source = null, bool isCritical = false)
        {
            Amount = Mathf.Max(0, amount);
            Source = source;
            IsCritical = isCritical;
        }

        /// <summary>
        /// Final damage amount after attacker-side calculations.
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// GameObject responsible for the damage, if known.
        /// </summary>
        public GameObject Source { get; }

        /// <summary>
        /// Whether this hit was a critical hit.
        /// </summary>
        public bool IsCritical { get; }
    }
}
