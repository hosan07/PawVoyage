using System;
using System.Collections.Generic;

namespace PawVoyage.Data
{
    /// <summary>
    /// Persistent player progress data synchronized with backend storage.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        /// <summary>
        /// Soft currency earned through gameplay.
        /// </summary>
        public int Gold { get; set; }

        /// <summary>
        /// Premium currency used for paid unlocks or convenience purchases.
        /// </summary>
        public int Gems { get; set; }

        /// <summary>
        /// Current energy available for starting game sessions.
        /// </summary>
        public int Energy { get; set; }

        /// <summary>
        /// Maximum energy capacity.
        /// </summary>
        public int EnergyMax { get; set; } = 60;

        /// <summary>
        /// Last UTC time energy refill was calculated.
        /// </summary>
        public DateTime EnergyLastRefillTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Animal ids currently owned by the player.
        /// </summary>
        public List<string> OwnedAnimals { get; set; } = new List<string>();

        /// <summary>
        /// Animal id currently selected for gameplay.
        /// </summary>
        public string EquippedAnimal { get; set; } = string.Empty;

        /// <summary>
        /// Permanent upgrade levels purchased with gold.
        /// </summary>
        public PlayerUpgradeLevels UpgradeLevels { get; set; } = new PlayerUpgradeLevels();

        /// <summary>
        /// Whether VIP benefits are currently active.
        /// </summary>
        public bool VipActive { get; set; }

        /// <summary>
        /// UTC expiry time for VIP benefits, or null if inactive or permanent.
        /// </summary>
        public DateTime? VipExpiry { get; set; }

        /// <summary>
        /// Best survival time in seconds.
        /// </summary>
        public int BestSurvivalTime { get; set; }

        /// <summary>
        /// Permanent stat upgrade levels.
        /// </summary>
        [Serializable]
        public class PlayerUpgradeLevels
        {
            /// <summary>
            /// Attack power upgrade level.
            /// </summary>
            public int Attack { get; set; }

            /// <summary>
            /// Maximum health upgrade level.
            /// </summary>
            public int Hp { get; set; }

            /// <summary>
            /// Movement speed upgrade level.
            /// </summary>
            public int Speed { get; set; }
        }
    }
}
