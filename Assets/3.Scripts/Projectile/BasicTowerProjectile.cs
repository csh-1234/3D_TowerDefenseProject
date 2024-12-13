using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class BasicTowerProjectile : Projectile
{
    protected override void Update()
    {
        base.Update();
    }

    protected override void NonBomb(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            other.gameObject.GetComponent<Monster>().TakeDamage(Damage);
            GameObject effect =  Resources.Load<GameObject>("Effects/EnergyExplosionYellow");
            GameObject particle = Instantiate(effect, this.transform.position, Quaternion.identity);

            Destroy(particle, particle.GetComponent<ParticleSystem>().main.duration);
            Destroy(gameObject);
        }
    }
}
