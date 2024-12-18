using UnityEngine;

public class LightningProjectile : Projectile
{
    public int ChainCount = 3;
    public float ChainRange = 5f;
    private GameObject lightningEffectPrefab;

    public void Initialize(GameObject effectPrefab)
    {
        lightningEffectPrefab = effectPrefab;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(Damage);
                CreateLightningEffect(transform.position, other.transform.position);
                ChainToNextTarget(other.transform, Damage, ChainCount - 1);
            }
            
            Destroy(gameObject);
        }
    }

    private void ChainToNextTarget(Transform currentTarget, int damage, int remainingChains)
    {
        if (remainingChains <= 0) return;

        Transform nextTarget = FindNextTarget(currentTarget.position, currentTarget);
        if (nextTarget != null)
        {
            Monster monster = nextTarget.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(Mathf.RoundToInt(damage * 0.7f));
                CreateLightningEffect(currentTarget.position, nextTarget.position);
                
                ChainToNextTarget(nextTarget, Mathf.RoundToInt(damage * 0.7f), remainingChains - 1);
            }
        }
    }

    private Transform FindNextTarget(Vector3 position, Transform currentTarget)
    {
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;

        Collider[] hitColliders = Physics.OverlapSphere(position, ChainRange);
        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Monster") && collider.transform != currentTarget)
            {
                float distance = Vector3.Distance(position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = collider.transform;
                }
            }
        }
        return closestTarget;
    }

    private void CreateLightningEffect(Vector3 start, Vector3 end)
    {
        GameObject effect = Instantiate(lightningEffectPrefab, transform.position, Quaternion.identity);
        LightningEffect lightning = effect.GetComponent<LightningEffect>();
        lightning.CreateLightning(start, end);
    }
} 