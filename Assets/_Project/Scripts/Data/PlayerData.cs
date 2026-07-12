using System;
using System.Collections.Generic;

namespace PawVoyage.Data
{
    /// <summary>
    /// 백엔드 저장소와 동기화되는 영구 플레이어 진행 데이터입니다.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        /// <summary>
        /// 게임 플레이로 획득하는 일반 재화입니다.
        /// </summary>
        public int Gold { get; set; }

        /// <summary>
        /// 유료 해금 또는 편의 구매에 사용하는 프리미엄 재화입니다.
        /// </summary>
        public int Gems { get; set; }

        /// <summary>
        /// 게임 세션 시작에 사용할 수 있는 현재 에너지입니다.
        /// </summary>
        public int Energy { get; set; }

        /// <summary>
        /// 최대 에너지 보유량입니다.
        /// </summary>
        public int EnergyMax { get; set; } = 60;

        /// <summary>
        /// 에너지 회복이 마지막으로 계산된 UTC 시간입니다.
        /// </summary>
        public DateTime EnergyLastRefillTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 플레이어가 현재 보유한 동물 ID 목록입니다.
        /// </summary>
        public List<string> OwnedAnimals { get; set; } = new List<string>();

        /// <summary>
        /// 게임 플레이에 현재 선택된 동물 ID입니다.
        /// </summary>
        public string EquippedAnimal { get; set; } = string.Empty;

        /// <summary>
        /// 골드로 구매한 영구 업그레이드 레벨입니다.
        /// </summary>
        public PlayerUpgradeLevels UpgradeLevels { get; set; } = new PlayerUpgradeLevels();

        /// <summary>
        /// VIP 혜택이 현재 활성화되어 있는지 여부입니다.
        /// </summary>
        public bool VipActive { get; set; }

        /// <summary>
        /// VIP 혜택의 UTC 만료 시간입니다. 비활성 또는 영구 혜택이면 null입니다.
        /// </summary>
        public DateTime? VipExpiry { get; set; }

        /// <summary>
        /// 초 단위 최고 생존 시간입니다.
        /// </summary>
        public int BestSurvivalTime { get; set; }

        /// <summary>
        /// 영구 스탯 업그레이드 레벨입니다.
        /// </summary>
        [Serializable]
        public class PlayerUpgradeLevels
        {
            /// <summary>
            /// 공격력 업그레이드 레벨입니다.
            /// </summary>
            public int Attack { get; set; }

            /// <summary>
            /// 최대 체력 업그레이드 레벨입니다.
            /// </summary>
            public int Hp { get; set; }

            /// <summary>
            /// 이동 속도 업그레이드 레벨입니다.
            /// </summary>
            public int Speed { get; set; }
        }
    }
}
