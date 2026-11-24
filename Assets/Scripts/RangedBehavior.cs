using UnityEngine;

public class RangedBehavior : MonoBehaviour, IDamageable
{
    public int CurrentHealth => Health;
    public float MoveSpeed = 3f;
    public float Acceleration = 8f;
    public float PreferredDistance = 6f;
    public float DistanceTolerance = 1.5f;

    public float StrafeSpeed = 1.5f;
    public float StrafeMin = 1.5f;
    public float StrafeMax = 3.5f;

    public float TurnSpeed = 360f;

    public GameObject ProjectilePrefab;
    public Transform FirePoint;

    public float ShootRange = 10f;
    public float FireInterval = 3f;
    public float ProjectileSpeed = 10f;
    public float SpreadAngle = 4f;
    public int ProjectileDamage = 20;
    public int Health = 100;
    public EnemyEventChannel EnemyDamagedChannel;
    public EnemyEventChannel EnemyDamageRequestChannel;
    public EnemyEventChannel EnemyDiedChannel;
    public EnemyEventChannel PlayerHitChannel;

    private Rigidbody2D _rigidbody;
    private GameObject _player;
    private float _fireTimer;
    private float _strafeTimer;
    private float _strafeDirection = 1f;

    void Start()
    {
        EnemyEventListener eventListener = GetComponent<EnemyEventListener>();
        if (eventListener == null) eventListener = gameObject.AddComponent<EnemyEventListener>();
        eventListener.ConfigureAll(EnemyDamageRequestChannel, EnemyDamagedChannel, null, EnemyDiedChannel);
        _rigidbody = GetComponent<Rigidbody2D>();
        _player = GameObject.Find("Player");
        DetectHit.IgnoreEnemyCollisions(gameObject);

        Collider2D[] enemyColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] playerColliders = _player.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (enemyCollider.isTrigger)
            {
                continue;
            }

            foreach (Collider2D playerCollider in playerColliders)
            {
                Physics2D.IgnoreCollision(enemyCollider, playerCollider);
            }
        }

        _fireTimer = Random.Range(0f, FireInterval);
        PickStrafeDirection();
    }

    void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        Vector2 toPlayer = (Vector2)_player.transform.position - _rigidbody.position;
        float distance = toPlayer.magnitude;
        Vector2 direction = distance > 0.01f ? toPlayer.normalized : Vector2.right;
        Vector2 movement = Vector2.zero;

        if (distance > PreferredDistance + DistanceTolerance)
        {
            movement = direction * MoveSpeed;
        }
        else if (distance < PreferredDistance - DistanceTolerance)
        {
            movement = -direction * MoveSpeed;
        }

        _strafeTimer -= deltaTime;
        if (_strafeTimer <= 0f)
        {
            PickStrafeDirection();
        }

        Vector2 sideways = new Vector2(-direction.y, direction.x);
        movement += sideways * StrafeSpeed * _strafeDirection;
        _rigidbody.linearVelocity = Vector2.MoveTowards(
            _rigidbody.linearVelocity,
            movement,
            Acceleration * deltaTime
        );

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(_rigidbody.rotation, targetAngle, TurnSpeed * deltaTime);
        _rigidbody.MoveRotation(angle);

        _fireTimer -= deltaTime;
        if (_fireTimer <= 0f && distance <= ShootRange)
        {
            Shoot(direction);
            _fireTimer = FireInterval;
        }
    }

    void PickStrafeDirection()
    {
        _strafeDirection = Random.value < 0.5f ? -1f : 1f;
        _strafeTimer = Random.Range(StrafeMin, StrafeMax);
    }

    public void TakeDamage(int damageAmount)
    {
        if (damageAmount <= 0 || Health <= 0)
        {
            return;
        }

        Health = Mathf.Max(0, Health - damageAmount);
        if (EnemyDamagedChannel != null) EnemyDamagedChannel.Raise(gameObject, damageAmount);
        EnemyVisualEffects.FlashOnHit(gameObject);
        if (Health == 0)
        {
            if (EnemyDiedChannel != null) EnemyDiedChannel.Raise(gameObject);
        }
    }

    void Shoot(Vector2 direction)
    {
        EnemyVisualEffects.FlashAggression(gameObject);
        SfxPlayer.Instance?.PlayEnemyShoot();
        float angle = Random.Range(-SpreadAngle, SpreadAngle);
        Vector2 shotDirection = Quaternion.Euler(0f, 0f, angle) * direction;
        Vector3 spawnPosition = FirePoint != null ? FirePoint.position : transform.position;
        GameObject shot = Instantiate(ProjectilePrefab, spawnPosition, Quaternion.identity, transform);

        Projectile projectile = shot.GetComponent<Projectile>();
        projectile.Init(shotDirection, ProjectileSpeed, ProjectileDamage);
    }
}
