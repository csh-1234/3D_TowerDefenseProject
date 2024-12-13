using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MissileProjectile : Projectile
{
    List<Monster> targetedMosters = new List<Monster>();

    protected override void Bomb(Collider other)
    {
        if (Target == null || other == null)
        {
            Debug.Log("Target or other is null");
            return;
        }

        if (Target.GetComponent<Monster>() == other.GetComponent<Monster>())
        {
            if (other.CompareTag("Monster"))
            {
                Debug.Log("Bomb Hit Monster: " + other.name);
                Collider[] hit = Physics.OverlapSphere(transform.position, BombRange);

                if(ExplosionParticle != null)
                {
                    PooledParticle explosion = ObjectManager.Instance.Spawn<PooledParticle>(
                        ExplosionParticle, 
                        transform.position, 
                        Quaternion.identity
                    );
                    explosion.Initialize();
                }

                foreach (Collider h in hit)
                {
                    if (h.CompareTag("Monster"))
                    {
                        h.gameObject.GetComponent<Monster>().TakeDamage(Damage);
                    }
                }
                Debug.Log("Despawning Bomb Projectile");
                ObjectManager.Instance.Despawn(this);
            }
        }
    }

    protected override void NonBomb(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            Debug.Log($"Attempting to despawn missile: {gameObject.name}");
            if(ExplosionParticle != null)
            {
                PooledParticle hitEffect = ObjectManager.Instance.Spawn<PooledParticle>(
                    ExplosionParticle, 
                    transform.position, 
                    Quaternion.identity
                );
                hitEffect.Initialize();
            }
            
            other.gameObject.GetComponent<Monster>().TakeDamage(Damage);
            
            Projectile projectile = this;
            Debug.Log($"Despawning projectile component: {projectile != null}");
            ObjectManager.Instance.Despawn(projectile);
        }
    }

    public void SetTargets(List<Monster> monsters)
    {
        targetedMosters = monsters;
    }
    public void ClearTargets()
    {
        targetedMosters?.Clear();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ClearTargets();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Missile OnTriggerEnter - Before base call");
        base.OnTriggerEnter(other);
        Debug.Log($"Missile OnTriggerEnter - After base call");
    }
}
