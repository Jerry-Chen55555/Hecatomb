using UnityEngine;

public class DetectHit : MonoBehaviour
{
    public int Damage = 25;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.root == transform.root)
        {
            return;
        }

        LungeBehavior lungeEnemy = other.GetComponentInParent<LungeBehavior>();
        if (lungeEnemy != null)
        {
            lungeEnemy.TakeDamage(Damage);
            return;
        }

        RangedBehavior rangedEnemy = other.GetComponentInParent<RangedBehavior>();
        if (rangedEnemy != null)
        {
            rangedEnemy.TakeDamage(Damage);
        }
    }
}
