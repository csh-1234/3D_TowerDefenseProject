using System;
using TMPro;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField]
    public int hp = 100;
    [SerializeField]
    private float speed = 2f;
    [SerializeField]
    private int damage = 1;
    [SerializeField]
    private int gold = 1;
    [SerializeField]
    private float rotationSpeed = 5f;

    [SerializeField]
    public int maxHp = 100;
    [SerializeField]
    public float maxSpeed = 2f;
    [SerializeField]
    public int maxDamage = 1;
    [SerializeField]
    public int maxGold = 1;

    public bool IsDead { get; private set; }

    private Vector3[] path;
    private int currentWaypointIndex;
    protected bool isMoving = false;
    private Vector3 currentTargetPosition;
    private Vector3 moveDirection;
    private Transform spawnPoint;
    public bool IsSpawnDirect = false;
    // 직접 소환할 경우 인스펙터에서 할당
    public Transform spawnPos; 

    public event Action OnDead;

    //private void Awake()
    //{
    //    //프리팹 스탯을 적용하기 위한 체력스탯
    //    hp *= GameManager.Instance.Difficulty;
    //    maxHp = hp;
    //    maxSpeed = speed;
    //    maxDamage = damage;
    //    maxGold = gold;
    //}

    public void Initialize(Transform spawn)
    {
        spawnPoint = spawn;
        transform.position = PathManager.Instance.GetSpawnPosition(spawnPoint);
        Vector3[] newPath = PathManager.Instance.GetActualPath(spawnPoint);
        
        if (newPath != null && newPath.Length > 0)
        {
            path = newPath;
            currentWaypointIndex = 0;
            isMoving = true;
            
            if (PathManager.Instance != null)
            {
                PathManager.Instance.OnActualPathUpdated += OnActualPathUpdated;
            }
            
            UpdateCurrentTarget();
        }
    }

    private void OnActualPathUpdated(Transform updatedSpawnPoint, Vector3[] newPath)
    {
        if (spawnPoint == updatedSpawnPoint && newPath != null && newPath.Length > 0)
        {
            moveDirection = (currentTargetPosition - transform.position).normalized;
            
            path = newPath;
            float bestScore = float.MinValue;
            int bestIndex = 0;

            for (int i = 0; i < path.Length; i++)
            {
                Vector3 toWaypoint = (path[i] - transform.position);
                float distance = toWaypoint.magnitude;
                float dotProduct = Vector3.Dot(moveDirection, toWaypoint.normalized);
                
                float score = dotProduct / (distance + 0.1f);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            if (bestIndex > 0 && bestScore < -0.5f)
            {
                bestIndex--;
            }

            currentWaypointIndex = bestIndex;
            UpdateCurrentTarget();
        }
    }

    private void UpdateCurrentTarget()
    {
        if (path != null && currentWaypointIndex < path.Length)
        {
            currentTargetPosition = path[currentWaypointIndex];
            currentTargetPosition.y = transform.position.y;
        }
        else
        {
            currentTargetPosition = PathManager.Instance.GetTargetPosition();
            currentTargetPosition.y = transform.position.y;
        }
    }

    protected virtual void Update()
    {
        if (IsDead) return;
        
        if(IsSpawnDirect == true)
        {
            Initialize(spawnPoint);
            IsSpawnDirect = false;
        }

        if (!isMoving) return;

        Vector3 targetDirection = (currentTargetPosition - transform.position).normalized;
        moveDirection = Vector3.Lerp(moveDirection, targetDirection, Time.deltaTime * 10);

        // 이동
        transform.position += moveDirection * speed * Time.deltaTime;

        // 회전 설정
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if (Vector3.Distance(transform.position, currentTargetPosition) < 0.1f)
        {
            if (currentWaypointIndex < path.Length - 1)
            {
                currentWaypointIndex++;
                UpdateCurrentTarget();
            }
            else
            {
                Vector3 finalTarget = PathManager.Instance.GetTargetPosition();
                finalTarget.y = transform.position.y;

                if (Vector3.Distance(transform.position, finalTarget) < 0.1f)
                {
                    OnReachedTarget();
                }
            }
        }
    }

    private void OnReachedTarget()
    {
        GameManager.Instance.CurrentHp -= damage;
        if (PathManager.Instance != null)
        {
            PathManager.Instance.OnActualPathUpdated -= OnActualPathUpdated;
        }
        Die();
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsDead) return;

        GameObject damageFontPrefab = Resources.Load<GameObject>("Effects/DamageFont");
        GameObject damageCanvas = GameObject.Find("DamageCanvas");
        DamageFontEffect damageEffect = ObjectManager.Instance.Spawn<DamageFontEffect>(damageFontPrefab, Vector3.zero,Quaternion.identity);
        
        damageEffect.transform.SetParent(damageCanvas.transform, false);
        damageEffect.SetDamageText(damage.ToString(), transform.position);
        
        hp -= damage;
        
        if (hp <= 0)
        {
            hp = 0;
            GameManager.Instance.CurrentMoney += gold;
            Die();
        }
    }

    private void Die()
    {
        if (IsDead) return;
        
        IsDead = true;
        isMoving = false;
        SoundManager.Instance.Play("MonsterDeathSound", SoundManager.Sound.Effect);
        OnDead?.Invoke();
    }

    void OnDestroy()
    {
        if (PathManager.Instance != null)
        {
            PathManager.Instance.OnActualPathUpdated -= OnActualPathUpdated;
        }
    }
    private void OnEnable()
    {
        // 상태 초기화
        IsDead = false;
        isMoving = false;
        currentWaypointIndex = 0;
        spawnPoint = null;
        IsSpawnDirect = false;

        // 프리팹에 설정된 초기값 복원
        hp = maxHp;
        speed = maxSpeed;
        damage = maxDamage;
        gold = maxGold;

        // 난이도 적용 (체력만)
        hp *= GameManager.Instance.Difficulty;
        maxHp = hp;  // 난이도가 적용된 체력을 최대체력으로 설정
    }

    private void OnDisable()
    {
        // 이벤트 해제
        if (PathManager.Instance != null)
        {
            PathManager.Instance.OnActualPathUpdated -= OnActualPathUpdated;
        }
    }

    // Awake에서 최대값 초기화 추가
    private void Awake()
    {
        // 프리팹의 초기값을 최대값으로 저장
        maxHp = hp;
        maxSpeed = speed;
        maxDamage = damage;
        maxGold = gold;
    }
}
