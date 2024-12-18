using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class ChainLightningTower : Tower
{
    public int ChainCount = 3;  
    public float ChainRange = 5f; 
    
    private int originalDamage;
    private float originalRange;
    private float originalChainRange;

    public List<GameObject> lightningEffectPrefabs;
    public GameObject currentEffect;
    public ChainLightningTower()
    {
        Name = "Lightning Tower";
        Element = "Electric";
        Damage = 4;
        Range = 4f;
        FireRate = 0.75f;
        DamageDealt = 0;
        TotalKilled = 0;
        UpgradePrice = 25;
        SellPrice = 13;
        TargetPriority = "Most Progress";
        Info = "Fires chain lightning that bounces between multiple targets.";

        originalDamage = Damage;
        originalRange = Range;
        originalChainRange = ChainRange;
    }
    protected override void Start()
    {
        currentEffect = lightningEffectPrefabs[Level - 1];
        base.Start();
    }


    public override void ApplyBuff(BuffField buff)
    {
        base.ApplyBuff(buff);
        ChainRange = originalChainRange * buff.rangeMultiplier;
    }

    public override void RemoveBuff(BuffField buff)
    {
        base.RemoveBuff(buff);
        ChainRange = originalChainRange;
    }

    protected override IEnumerator Attack()
    {
        while (true)
        {
            yield return new WaitForSeconds(FireRate);
            if (CurrentTarget != null)
            {
                StartCoroutine(ChainLightning(CurrentTarget, Damage, ChainCount));
            }
        }
    }

    private IEnumerator ChainLightning(Transform target, int damage, int remainingChains)
    {
        if (target == null || remainingChains <= 0)
        {
            yield break;
        }

        Monster monster = target.GetComponent<Monster>();
        if (monster != null)
        {
            monster.TakeDamage(damage);
            
            CreateLightningEffect(TowerMuzzle.position, target.position);

            Transform nextTarget = FindNextTarget(target.position, target);
            if (nextTarget != null && remainingChains > 0)
            {
                yield return new WaitForSeconds(0.1f);
                CreateLightningEffect(target.position, nextTarget.position);
                StartCoroutine(ChainLightning(nextTarget, Mathf.RoundToInt(damage * 0.7f), remainingChains - 1));
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
        LightningEffect lightning = ObjectManager.Instance.Spawn<LightningEffect>(currentEffect, transform.position);
        lightning.CreateLightning(start, end);
        SoundManager.Instance.Play("LightningTowerEffect", SoundManager.Sound.Effect);
    }

    protected override void OnLevelUp()
    {
        switch (Level)
        {
            case 1:
                currentEffect = lightningEffectPrefabs[Level - 1];
                break;
            case 2:
                currentEffect = lightningEffectPrefabs[Level - 1];
                ChainCount += 4;
                break;
            case 3:
                baseAttackDamage += 2;
                if (originalStats.Count == 0)
                {
                    Damage = baseAttackDamage;
                }
                FireRate *= 0.5f;
                currentEffect = lightningEffectPrefabs[Level - 1];  
                break;
        }

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
} 
