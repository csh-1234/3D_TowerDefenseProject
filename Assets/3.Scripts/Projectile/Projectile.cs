using System.Collections;
using System.Data;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Projectile : MonoBehaviour
{
    public float MoveSpeed = 50f;
    public int Damage = 5;
    public float Duration = 3f;

    [Header("공통기능")]
    [Tooltip("투사체가 타겟을 따라감")]
    public bool IsTargeting = false;
    [Tooltip("투사체가 타겟 명중시 주변에 광역 데미지 발생")]
    public bool IsBomb = false;
    [Tooltip("광역 데미지 범위")]
    public float BombRange = 3f;
    [Tooltip("투사체 충돌이 없을시 스스로 파괴됨")]
    public bool isSelfDestroy = false;
    
    public Transform Target;
    public GameObject ExplosionParticle;

    private void OnEnable()
    {
        StartCoroutine(SelfDestroy(Duration));
    }

    public virtual void Initialize()
    {
        if(isSelfDestroy)
        {
            StartCoroutine(SelfDestroy(Duration));
        }
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        Target = null;
    }

    protected virtual void Update()
    {
        Move();
    }

    protected virtual void Move()
    {
        if (IsTargeting)
        {
            TargettingMove();
        }
        else
        {
            NonTargettingMove();
        }
    }

    protected virtual void TargettingMove()
    {
        if (Target == null || !Target.gameObject.activeSelf)
        {
            if (ExplosionParticle != null)
            {
                PooledParticle explosion = ObjectManager.Instance.Spawn<PooledParticle>(
                    ExplosionParticle, 
                    transform.position, 
                    Quaternion.identity
                );
            }
            ObjectManager.Instance.Despawn(this);
            return;
        }

        float time = Time.time + 1f;

        if (time > Time.time)
        {
            transform.Translate(Vector3.up * 30f * Time.deltaTime);
        }

        Vector3 dir = Target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(dir);
        transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
    }

    protected virtual void NonTargettingMove()
    {
        transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Base OnTriggerEnter - IsBomb: {IsBomb}");  // 디버그 로그 추가
        
        if (IsBomb)
        {
            Bomb(other);
        }
        else
        {
            NonBomb(other);
        }
    }

    protected virtual void Bomb(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            Collider[] hit = Physics.OverlapSphere(transform.position, BombRange);
            
            if(ExplosionParticle != null)
            {
                PooledParticle explosion = ObjectManager.Instance.Spawn<PooledParticle>(
                    ExplosionParticle, 
                    transform.position, 
                    Quaternion.identity
                );
            }

            foreach (Collider h in hit)
            {
                if (h.CompareTag("Monster"))
                {
                    other.gameObject.GetComponent<Monster>().TakeDamage(Damage);
                }
            }
            ObjectManager.Instance.Despawn(this);
        }
    }

    protected virtual void NonBomb(Collider other)
    {
        Debug.Log($"Base NonBomb - Target: {Target?.name}, Other: {other?.name}");  // 디버그 로그 추가
        
        if (other.CompareTag("Monster"))
        {
            if (ExplosionParticle != null)
            {
                PooledParticle hitEffect = ObjectManager.Instance.Spawn<PooledParticle>(
                    ExplosionParticle,
                    transform.position,
                    Quaternion.identity
                );
            }

            other.gameObject.GetComponent<Monster>().TakeDamage(Damage);
            Debug.Log("Base NonBomb - Despawning");  // 디버그 로그 추가
            ObjectManager.Instance.Despawn(this);

        }
    }

    protected IEnumerator SelfDestroy(float time)
    {
        yield return new WaitForSeconds(time);
        ObjectManager.Instance.Despawn(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, BombRange);
    }
}