public interface IDamageable
{
    void TakeDamage(int amount);
}

public interface IEnemyPlayerHit
{
    bool TryRegisterPlayerHit();
}

public interface IEnemyDeath
{
    void Die();
}
