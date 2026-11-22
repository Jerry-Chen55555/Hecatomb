using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Lifetime = 4f;
    public float HomingTurnSpeed = 720f;

    private Rigidbody2D _projectileRigidbody;
    private int _damage;
    private Transform _target;

    public int Damage => _damage;

    void Awake()
    {
        _projectileRigidbody = GetComponent<Rigidbody2D>();

        _target = GameObject.Find("Player").transform;
    }

    public void Init(Vector2 direction, float speed, int projectileDamage)
    {
        _projectileRigidbody.linearVelocity = direction.normalized * speed;
        _damage = projectileDamage;
        Destroy(gameObject, Lifetime);
    }

    void FixedUpdate()
    {
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
}
