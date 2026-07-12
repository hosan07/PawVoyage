using System;
using System.Collections.Generic;

namespace PawVoyage.Data
{
    /// <summary>
    /// 현재 게임 세션에서만 사용하는 런타임 상태입니다.
    /// </summary>
    [Serializable]
    public class SessionState
    {
        /// <summary>
        /// 초 단위 세션 경과 시간입니다.
        /// </summary>
        public float ElapsedTime { get; set; }

        /// <summary>
        /// 세션 중 현재 플레이어 체력입니다.
        /// </summary>
        public int CurrentHp { get; set; }

        /// <summary>
        /// 세션 내 현재 레벨입니다.
        /// </summary>
        public int CurrentLevel { get; set; } = 1;

        /// <summary>
        /// 다음 레벨까지 누적된 현재 경험치입니다.
        /// </summary>
        public float CurrentExp { get; set; }

        /// <summary>
        /// 이번 세션에서 선택한 업그레이드 ID 목록입니다.
        /// </summary>
        public List<string> ActiveUpgrades { get; set; } = new List<string>();
    }
}
