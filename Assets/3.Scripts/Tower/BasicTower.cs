using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTower : Tower
{
    [SerializeField] private List<GameObject> projectiles;
    [SerializeField] private List<GameObject> muzzleEffects;
    [SerializeField] private List<GameObject> HitEffects;

    private GameObject currentProjectile;
    private GameObject currentMuzzleEffect;
    private GameObject currentHitEffect;

    public bool IsTargeting;
    public bool IsBomb;
    public BasicTower()
    {
        Name = "Basic Tower";
        Element = "Normal";
        Damage = 2;
        Range = 4f;
        FireRate = 1.5f;
        DamageDealt = 0;
        TotalKilled = 0;
        UpgradePrice = 10;
        SellPrice = 5;
        TargetPriority = "Most Progress";
        Info = "The basic defense tower.";
    }

    protected override void Start()
    {
        currentProjectile = projectiles[Level-1];
        currentMuzzleEffect = muzzleEffects[Level-1];
        currentHitEffect = HitEffects[Level-1];
        base.Start();
    }

    protected override IEnumerator Attack()
    {
        while (true)
        {
            yield return new WaitForSeconds(FireRate);
            if (CurrentTarget != null)
            {
                Projectile projectile = ObjectManager.Instance.Spawn<Projectile>(currentProjectile, TowerMuzzle.transform.position);
                projectile.Damage = this.Damage;
                projectile.IsTargeting = this.IsTargeting;
                projectile.IsBomb = this.IsBomb;
                projectile.Target = this.CurrentTarget;

                PooledParticle muzzleEffect = ObjectManager.Instance.Spawn<PooledParticle>(
                         currentMuzzleEffect, TowerMuzzle.transform.position, TowerHead.transform.rotation
                );
                muzzleEffect?.Initialize();
            }
        }
    }

    protected override void OnLevelUp()
    {
        if (Level == 2)
        {
            baseAttackDamage += 2;
            baseRange += 1f;
            
            if (originalStats.Count == 0)
            {
                Damage = baseAttackDamage;
                Range = baseRange;
            }
        }
        else if (Level == 3)
        {
            FireRate /= 2f;
            baseRange += 3f;
            
            if (originalStats.Count == 0)
            {
                Range = baseRange;
            }
        }

        SetByLevel();
        
        if (originalStats.Count > 0)
        {
            var currentBuffs = new List<BuffField>(originalStats.Keys);
            
            foreach (var buff in currentBuffs)
            {
                RemoveBuff(buff);
            }

            foreach (var buff in currentBuffs)
            {
                ApplyBuff(buff);
            }
        }
    }

    private void SetByLevel()
    {
        currentProjectile = projectiles[Level-1];
        currentMuzzleEffect = muzzleEffects[Level-1];
        currentHitEffect = HitEffects[Level-1];
    }
}
