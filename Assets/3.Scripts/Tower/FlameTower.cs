using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameTower: Tower
{
    [SerializeField] private List<GameObject> flameProjeciles;
    private GameObject currentFlameProjectile;

    public bool IsTargeting;
    private int originalDamage;
    private float originalRange;

    public FlameTower()
    {
        Name = "Flame Tower";
        Element = "Fire";
        Damage = 2;
        Range = 3f;
        FireRate = 3f;
        DamageDealt = 0;
        TotalKilled = 0;
        UpgradePrice = 30;
        SellPrice = 15;
        TargetPriority = "Most Progress";
        Info = "Fire!";
    }

    protected override void Start()
    {
        originalDamage = Damage;
        originalRange = Range;
        currentFlameProjectile = flameProjeciles[Level - 1];
        base.Start();
    }

    public override void ApplyBuff(BuffField buff)
    {
        base.ApplyBuff(buff);
        
        if (currentFlameProjectile != null)
        {
            FlameProjectile proj = currentFlameProjectile.GetComponent<FlameProjectile>();
            if (proj != null)
            {
                proj.Damage = Mathf.RoundToInt(originalDamage * buff.damageMultiplier);
            }
        }
    }

    public override void RemoveBuff(BuffField buff)
    {
        base.RemoveBuff(buff);
        
        if (currentFlameProjectile != null)
        {
            FlameProjectile proj = currentFlameProjectile.GetComponent<FlameProjectile>();
            if (proj != null)
            {
                proj.Damage = originalDamage;
            }
        }
    }

    protected override IEnumerator Attack()
    {
        while (true)
        {
            if (CurrentTarget != null)
            {
                currentFlameProjectile.SetActive(true);
                FlameProjectile proj = currentFlameProjectile.GetComponent<FlameProjectile>();
                proj.Damage = this.Damage; 
                proj.IsTargeting = this.IsTargeting;
                proj.Target = this.CurrentTarget;
                yield return new WaitForSeconds(FireRate);
            }
            else
            {
                currentFlameProjectile.SetActive(false);
                yield return null;
            }
        }
    }

    protected override void OnLevelUp()
    {
        if (Level == 2)
        {
            StopCoroutine(Attack());
            currentFlameProjectile.SetActive(false);
            currentFlameProjectile = flameProjeciles[Level - 1];
            StartCoroutine(Attack());
            originalDamage += 3;  
            Damage += 3;
        }
        else if (Level == 3)
        {
            StopCoroutine(Attack());
            currentFlameProjectile.SetActive(false);
            currentFlameProjectile = flameProjeciles[Level - 1];
            StartCoroutine(Attack());
            originalDamage += 1; 
            Damage += 1;
            originalRange += 2f; 
            Range += 2f;
        }
    }

    protected override void Update()
    {
        Detect();
        FollowTarget();
        TooltipPopupCheck();
    }
}
