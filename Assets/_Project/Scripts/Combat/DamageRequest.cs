using UnityEngine;

namespace PawVoyage.Combat
{
    /// <summary>
    /// 단일 피해 이벤트를 설명하는 불변 데이터입니다.
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
        /// 공격자 측 계산이 끝난 최종 피해량입니다.
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// 피해를 발생시킨 GameObject입니다. 알 수 없는 경우 null입니다.
        /// </summary>
        public GameObject Source { get; }

        /// <summary>
        /// 이 타격이 치명타인지 여부입니다.
        /// </summary>
        public bool IsCritical { get; }
    }
}
