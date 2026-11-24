using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Lifetime = 4f;
    public float HomingTurnSpeed = 720f;

    private Rigidbody2D _projectileRigidbody;
    private int _damage;
    private Transform _target;
    private bool _reflected;
    private bool _perfectParry;
    private bool _homesIn = true;
    public bool IsDroneShot { get; private set; }

    public int Damage => _damage;
    public bool IsReflected => _reflected;
    public bool IsPerfectParry => _perfectParry;

    void Awake()
    {
        _projectileRigidbody = GetComponent<Rigidbody2D>();

        _target = GameObject.Find("Player").transform;
    }

    public void Init(Vector2 direction, float speed, int projectileDamage, bool droneShot = false)
    {
        _projectileRigidbody.linearVelocity = direction.normalized * speed;
        _damage = projectileDamage;
        IsDroneShot = droneShot;
        _homesIn = !droneShot;
        Destroy(gameObject, Lifetime);
    }

    void FixedUpdate()
    {
        if (_reflected || !_homesIn) return;
        Vector2 velocity = _projectileRigidbody.linearVelocity;
        float speed = velocity.magnitude;
        Vector2 toTarget = (Vector2)_target.position - _projectileRigidbody.position;
        if (speed <= 0.001f || toTarget.sqrMagnitude <= 0.001f)
        {
            return;
        }

        float currentAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        float homedAngle = Mathf.MoveTowardsAngle(
            currentAngle,
            targetAngle,
            HomingTurnSpeed * Time.fixedDeltaTime
        );
        _projectileRigidbody.linearVelocity = Quaternion.Euler(0f, 0f, homedAngle) * Vector2.right * speed;
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }

    public void Reflect(Vector2 direction)
    {
        _reflected = true;
        _target = null;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
        _projectileRigidbody.linearVelocity = direction.normalized * _projectileRigidbody.linearVelocity.magnitude;
        transform.SetParent(null, true);
        Collider2D projectileCollider = GetComponent<Collider2D>();
        foreach (Collider2D enemyCollider in FindObjectsOfType<Collider2D>())
        {
            if (enemyCollider.transform.root.CompareTag("Enemy"))
                Physics2D.IgnoreCollision(projectileCollider, enemyCollider, false);
        }
    }

    public void MarkPerfectParry()
    {
        _perfectParry = true;
    }
}
