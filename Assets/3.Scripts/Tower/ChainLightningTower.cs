using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class ChainLightningTower : Tower
{
    public int ChainCount = 3;  //체인 횟수
    public float ChainRange = 5f; //다음 타겟 탐지거리
    
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
                //체인 후 데미지 70%, 체인카운트 -1
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

    //지금 맞은 몬스터와 타겟 사이에 라이트닝 생성
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
                //UnityEngine.Color color = new Color32(240, 255, 83, 255);
                //effect.lightningColor = color;
                currentEffect = lightningEffectPrefabs[Level - 1];
                break;
            case 2:
                //color = new Color32(84, 108, 255, 255);
                //effect.lightningColor = color;
                currentEffect = lightningEffectPrefabs[Level - 1];
                ChainCount += 4;
                break;
            case 3:
                baseAttackDamage += 2;
                if (originalStats.Count == 0)
                {
                    Damage = baseAttackDamage;
                }
                //Damage += 2;
                FireRate *= 0.5f;
                currentEffect = lightningEffectPrefabs[Level - 1];  
                //color = new Color32(255, 83, 243, 255);
                //effect.lightningColor = color;
                break;
        }
        // 버프가 있다면 재적용
        if (originalStats.Count > 0)
        {
            // 현재 적용된 모든 버프를 저장
            var currentBuffs = new List<BuffField>(originalStats.Keys);

            // 모든 버프 제거
            foreach (var buff in currentBuffs)
            {
                RemoveBuff(buff);
            }

            // 버프 재적용
            foreach (var buff in currentBuffs)
            {
                ApplyBuff(buff);
            }
        }
    }
} 
