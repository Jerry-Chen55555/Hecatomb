using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Lifetime = 4f;
    public float HomingTurnSpeed = 720f;

    private Rigidbody2D _projectileRigidbody;
    private int _damage;
    private Transform _target;

    void Awake()
    {
        _projectileRigidbody = GetComponent<Rigidbody2D>();

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            _target = player.transform;
        }
    }

    public void Init(Vector2 direction, float speed, int projectileDamage)
    {
        if (_projectileRigidbody != null)
        {
            _projectileRigidbody.linearVelocity = direction.normalized * speed;
        }

        _damage = projectileDamage;
        Destroy(gameObject, Lifetime);
    }

    void FixedUpdate()
    {
        if (_projectileRigidbody == null || _target == null)
        {
            return;
        }

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.root == transform.root)
        {
            return;
        }

        bool touchedEnemy =
            other.GetComponentInParent<LungeBehavior>() != null ||
            other.GetComponentInParent<RangedBehavior>() != null ||
            other.CompareTag("Enemy") ||
            other.transform.root.CompareTag("Enemy");
        if (touchedEnemy)
        {
            return;
        }

        PlayerController playerController = other.GetComponentInParent<PlayerController>();
        if (playerController != null)
        {
            playerController.Health = Mathf.Max(0, playerController.Health - _damage);
        }

        Destroy(gameObject);
    }
}
