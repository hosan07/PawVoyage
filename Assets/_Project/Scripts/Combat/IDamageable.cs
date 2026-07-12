namespace PawVoyage.Combat
{
    /// <summary>
    /// Implemented by objects that can receive combat damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Whether this target can currently receive damage.
        /// </summary>
        bool CanReceiveDamage { get; }

        /// <summary>
        /// Applies a damage request to this target.
        /// </summary>
        /// <param name="request">Damage request data.</param>
        void ApplyDamage(DamageRequest request);
    }
}
