using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class FlameProjectile : Projectile
{
    public float DamageInterval = 1f;
    private Dictionary<Collider, float> _lastDamageTime = new Dictionary<Collider, float>();

    protected override void Move()
    {
    }

    protected override void OnTriggerEnter(Collider other)
    {
        Monster monster = other.gameObject.GetComponent<Monster>();
        if (monster == null || monster.IsDead) return;

        monster.TakeDamage(this.Damage);
        _lastDamageTime[other] = Time.time;
    }

    protected void OnTriggerStay(Collider other)
    {
        Monster monster = other.gameObject.GetComponent<Monster>();
        if (monster == null || monster.IsDead)
        {
            if (_lastDamageTime.ContainsKey(other))
                _lastDamageTime.Remove(other);
            return;
        }

        if (_lastDamageTime.ContainsKey(other))
        {
            float timeSinceLastDamage = Time.time - _lastDamageTime[other];
            if (timeSinceLastDamage >= DamageInterval)
            {
                monster.TakeDamage(this.Damage);
                _lastDamageTime[other] = Time.time;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_lastDamageTime.ContainsKey(other))
        {
            _lastDamageTime.Remove(other);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _lastDamageTime.Clear();
    }
}
