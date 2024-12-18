using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneTower : Tower
{
    public DroneController drone;
    public Transform droneHomePosition;

    [SerializeField] private List<GameObject> projectiles;
    [SerializeField] private List<GameObject> muzzleEffects;
    [SerializeField] private List<GameObject> HitEffects;

    private int originalDamage;
    private float originalRange;
    private int originalDroneDamage;
    private float originalDroneRange;

    public DroneTower()
    {
        Name = "Drone Tower";
        Element = "Electric";
        Damage = 2;
        Range = 5f;
        FireRate = 2f;
        DamageDealt = 0;
        TotalKilled = 0;
        UpgradePrice = 15;
        SellPrice = 8;
        TargetPriority = "Most Progress";
        Info = "Automatically tracks and continuously attacks enemies within its range until they leave.";

        originalDamage = Damage;
        originalRange = Range;
    }

    public override void ApplyBuff(BuffField buff)
    {
        base.ApplyBuff(buff);
        
        if (drone != null)
        {
            drone.damage = Mathf.RoundToInt(originalDroneDamage * buff.damageMultiplier);
            drone.attackRange = originalDroneRange * buff.rangeMultiplier;
        }
    }

    public override void RemoveBuff(BuffField buff)
    {
        base.RemoveBuff(buff);
    }

    protected override void Start()
    {
        base.Start();
        originalDamage = Damage;
        originalRange = Range;

        if (drone != null && droneHomePosition != null)
        {
            drone.Initialize(droneHomePosition, transform, Range);
            originalDroneDamage = drone.damage;
            originalDroneRange = drone.attackRange;
        }
        else
        {
            Debug.LogError("?쒕줎 ?먮뒗 ???ъ??섏쓣 李얠쓣 ???놁뒿?덈떎!");
        }
        
        drone.projectilePrefab = projectiles[Level - 1];
        drone.projectileEffect = muzzleEffects[Level - 1];
        drone.projectileHitEffect = HitEffects[Level - 1];
    }

    protected override void Detect()
    {
        if (!drone.HasTarget())
        {
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, Range + 3f);
            Transform nearestTarget = null;
            float minDistance = float.MaxValue;

            foreach (Collider target in hitColliders)
            {
                if (target.CompareTag("Monster"))
                {
                    Monster monster = target.GetComponent<Monster>();
                    if (monster != null && target.gameObject.activeSelf && !monster.IsDead)
                    {
                        float distance = Vector3.Distance(drone.transform.position, target.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestTarget = target.transform;
                        }
                    }
                }
            }

            if (nearestTarget != null)
            {
                drone.SetTarget(nearestTarget);
            }
        }
        else
        {
            if (drone.CurrentTarget != null)
            {
                Monster monster = drone.CurrentTarget.GetComponent<Monster>();
                if (!drone.CurrentTarget.gameObject.activeSelf || 
                    monster == null || 
                    monster.IsDead || 
                    Vector3.Distance(drone.CurrentTarget.position, transform.position) > Range)
                {
                    drone.ClearTarget();
                    drone.isReturning = true;
                }
            }
            else
            {
                drone.ClearTarget();
                drone.isReturning = true;
            }
        }
    }

    protected override IEnumerator Attack()
    {
        yield break;
    }

    protected override void OnLevelUp()
    {
        if (Level == 2)
        {
            drone.moveSpeed *= 1.3f;
            drone.attackRange += 1.5f;
            drone.shootCooldown *= 0.5f;

            originalDroneRange += 1.5f; 
            Range += 1.5f;
            SetByLevel();
        }
        else if (Level == 3)
        {
            drone.damage += 1;
            drone.shootCooldown *= 0.2f;

            originalDroneDamage += 1;
            Damage +=1;
            SetByLevel();
        }
    }

    private void SetByLevel()
    {
        drone.projectilePrefab = projectiles[Level - 1];
        drone.projectileEffect = muzzleEffects[Level - 1];
        drone.projectileHitEffect = HitEffects[Level - 1];
    }
} 