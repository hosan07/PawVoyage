using System;
using System.Collections.Generic;

namespace PawVoyage.Data
{
    /// <summary>
    /// Runtime-only state for the current gameplay session.
    /// </summary>
    [Serializable]
    public class SessionState
    {
        /// <summary>
        /// Elapsed session time in seconds.
        /// </summary>
        public float ElapsedTime { get; set; }

        /// <summary>
        /// Current player health during the session.
        /// </summary>
        public int CurrentHp { get; set; }

        /// <summary>
        /// Current in-session level.
        /// </summary>
        public int CurrentLevel { get; set; } = 1;

        /// <summary>
        /// Current experience amount toward the next level.
        /// </summary>
        public float CurrentExp { get; set; }

        /// <summary>
        /// Upgrade ids selected during this session.
        /// </summary>
        public List<string> ActiveUpgrades { get; set; } = new List<string>();
    }
}
