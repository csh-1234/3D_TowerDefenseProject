using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonTower : Tower
{
    [SerializeField] private List<GameObject> projectiles;
    [SerializeField] private List<GameObject> muzzleEffects;
    [SerializeField] private List<GameObject> HitEffects;

    private GameObject currentProjectile;
    private GameObject currentMuzzleEffect;
    private GameObject currentHitEffect;

    public bool IsTargeting;
    public bool IsBomb;
    public PoisonTower()
    {
        Name = "Poison Tower";
        Element = "Poison";
        Damage = 5;
        Range = 4f;
        FireRate = 1f;
        DamageDealt = 0;
        TotalKilled = 0;
        UpgradePrice = 10;
        SellPrice = 5;
        TargetPriority = "Most Progress";
        Info = "Shoots tiny poison darts!";
    }

    protected override void Start()
    {
        currentProjectile = projectiles[Level - 1];
        currentMuzzleEffect = muzzleEffects[Level - 1];
        currentHitEffect = HitEffects[Level - 1];
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
                SoundManager.Instance.Play("PoisonTowerEffect", SoundManager.Sound.Effect);
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
            baseAttackDamage += 4;

            if (originalStats.Count == 0)
            {
                Damage = baseAttackDamage;
            }
        }
        else if (Level == 3)
        {
            FireRate /= 1.5f;
            baseRange += 2f;

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
        currentProjectile = projectiles[Level - 1];
        currentMuzzleEffect = muzzleEffects[Level - 1];
        currentHitEffect = HitEffects[Level - 1];
    }
}
