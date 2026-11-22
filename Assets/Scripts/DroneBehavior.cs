using System.Collections;
using UnityEngine;

public class DroneBehavior : MonoBehaviour
{
    public GameObject BulletPrefab;
    public float BulletSpeed = 10f;
    public int BulletDamage = 8;
    public int BulletCount = 18;
    public int BulletsPerVolley = 3;
    public float BurstTime = 1f;
    public float SpreadAngle = 16f;
    public float Offset = 0.6f;

    public float FireDelay = 3f;
    public float AimTime = 0.6f;
    public float AimLockTime = 0.15f;
    public float FireRange = 14f;

    public float MoveSpeed = 3f;
    public float Acceleration = 8f;
    public float PreferredDistance = 7f;
    public float DistanceTolerance = 1.5f;
    public float StrafeSpeed = 1.5f;
    public float StrafeSwitchMin = 1.5f;
    public float StrafeSwitchMax = 3.5f;
    public float TurnSpeed = 360f;

    public int Health = 100;

    private Transform _player;
    private SpriteRenderer[] _sprites;
    private Vector2 _velocity;
    private Vector2 _lockedAim = Vector2.right;
    private float _fireTimer;
    private float _strafeDirection = 1f;
    private float _strafeTimer;
    private bool _attacking;
    private bool _dead;

    void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        _player = playerObject.transform;

        _sprites = GetComponentsInChildren<SpriteRenderer>();
        _fireTimer = FireDelay;
        _strafeDirection = Random.value < 0.5f ? -1f : 1f;
        _strafeTimer = Random.Range(StrafeSwitchMin, StrafeSwitchMax);
    }

    void Update()
    {
        if (_dead || _player == null) return;

        float dt = Time.deltaTime;
        Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
        float distance = toPlayer.magnitude;
        Vector2 direction = distance > 0.001f ? toPlayer / distance : Vector2.right;

        Vector2 desired = Vector2.zero;

        if (!_attacking)
        {
            float radial = Mathf.Clamp((distance - PreferredDistance) / Mathf.Max(DistanceTolerance, 0.01f), -1f, 1f);
            Vector2 sideways = new Vector2(-direction.y, direction.x);

            _strafeTimer -= dt;
            if (_strafeTimer <= 0f)
            {
                _strafeDirection = -_strafeDirection;
                _strafeTimer = Random.Range(StrafeSwitchMin, StrafeSwitchMax);
            }

            desired = direction * (MoveSpeed * radial) + sideways * (StrafeSpeed * _strafeDirection);

            _fireTimer -= dt;
            if (_fireTimer <= 0f && distance <= FireRange && BulletPrefab != null)
            {
                StartCoroutine(AttackRoutine());
            }
        }

        _velocity = Vector2.MoveTowards(_velocity, desired, Acceleration * dt);
        transform.position += (Vector3)(_velocity * dt);

        Vector2 facing = _attacking ? _lockedAim : direction;
        float targetAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, TurnSpeed * dt);
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    IEnumerator AttackRoutine()
    {
        _attacking = true;
        _lockedAim = AimAtPlayer();
        StartCoroutine(EnemyVisualEffects.WhiteFlash(_sprites, AimTime));

        float aimTimer = AimTime;
        while (aimTimer > 0f)
        {
            if (_dead || _player == null)
            {
                _attacking = false;
                yield break;
            }

            aimTimer -= Time.deltaTime;
            if (aimTimer > AimLockTime)
            {
                _lockedAim = AimAtPlayer();
            }
            yield return null;
        }

        int bullets = Mathf.Max(1, BulletsPerVolley);
        int volleys = Mathf.Max(1, Mathf.CeilToInt(BulletCount / (float)bullets));
        float interval = BurstTime / volleys;
        int fired = 0;

        for (int volley = 0; volley < volleys; volley++)
        {
            if (_dead) yield break;

            for (int i = 0; i < bullets && fired < BulletCount; i++)
            {
                FireBullet();
                fired++;
            }

            yield return new WaitForSeconds(interval);
        }

        _attacking = false;
        _fireTimer = FireDelay;
    }

    void FireBullet()
    {
        Vector2 shotDirection = (Vector2)(Quaternion.Euler(0f, 0f, ClusteredAngle()) * (Vector3)_lockedAim);
        Vector3 spawnPosition = transform.position + (Vector3)(_lockedAim * Offset);

        GameObject shot = Instantiate(BulletPrefab, spawnPosition, Quaternion.identity);
        Projectile projectile = shot.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Init(shotDirection, BulletSpeed, BulletDamage, true);
        }
    }

    float ClusteredAngle()
    {
        float bell = (Random.value + Random.value + Random.value) / 3f * 2f - 1f;
        return bell * SpreadAngle;
    }

    Vector2 AimAtPlayer()
    {
        if (_player == null) return transform.right;
        return ((Vector2)_player.position - (Vector2)transform.position).normalized;
    }

    public void TakeDamage(int amount)
    {
        if (_dead) return;

        Health -= amount;
        EnemyVisualEffects.FlashOnHit(gameObject);
        if (Health > 0) return;

        _dead = true;
        StopAllCoroutines();

        EnemyDeathFade fade = GetComponent<EnemyDeathFade>();
        if (fade == null) fade = gameObject.AddComponent<EnemyDeathFade>();
        fade.Begin();
    }
}
