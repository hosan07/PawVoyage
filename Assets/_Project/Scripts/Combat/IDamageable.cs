namespace PawVoyage.Combat
{
    /// <summary>
    /// 전투 피해를 받을 수 있는 오브젝트가 구현합니다.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 이 대상이 현재 피해를 받을 수 있는지 여부입니다.
        /// </summary>
        bool CanReceiveDamage { get; }

        /// <summary>
        /// 이 대상에게 피해 요청을 적용합니다.
        /// </summary>
        /// <param name="request">피해 요청 데이터입니다.</param>
        void ApplyDamage(DamageRequest request);
    }
}
