namespace Soulbreak.Core
{
    // Anything that can take damage (reactor, enemies). Keeps systems decoupled (Dependency Inversion).
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }
}
