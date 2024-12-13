using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    #region ToolTip
    [Header("타워 정보")]
    public string Name;
    public string Element;
    public int Damage;
    public float FireRate;
    public int DamageDealt;
    public int TotalKilled;
    public int UpgradePrice;
    public int SellPrice;
    public string TargetPriority;
    public string Info;
    #endregion

    public GameObject rangeIndicatorPrefab;  // 라인 렌더러를 가진 프리팹
    private RangeIndicator rangeIndicator;

    public int Level { get; protected set; } = 1;
    public int MaxLevel = 3;

    [Header("타워 파츠")]
    public Transform TowerHead;
    public Transform TowerMuzzle;
    public GameObject TowerTooltip;

    [Header("공통 기능")]
    [Tooltip("타워가 프리뷰 상태에서 발사하지 않게함")]
    public bool isPreview = false;
    [Tooltip("타워가 타겟을 바라봄")]
    public bool isFollow = false;

    [Header("툴팁 캔버스")]
    public Canvas mainCanvas;

    protected Transform CurrentTarget = null;
    private GameObject currentTooltip;
    private Vector3 clickmousePointer;

    [SerializeField]
    private float _range;  // private 필드 추가
    public float Range  // 프로퍼티로 변경
    {
        get { return _range; }
        set  // protected를 제거하여 public으로 변경
        {
            _range = value;
            // Range가 변경될 때마다 RangeIndicator 업데이트
            if (rangeIndicator != null)
            {
                rangeIndicator.UpdateRange(_range);
            }
        }
    }

    private static Tower selectedTower = null;  // 현 선택된 타워를 추적하기 위한 static 변수 추가

    // 버프 관련 구조체와 딕셔너리 추가
    public struct TowerStats
    {
        public int damage;
        public float range;
    }

    // 각 버프 필드별 원래 스탯을 저장
    public Dictionary<BuffField, TowerStats> originalStats = new Dictionary<BuffField, TowerStats>();

    // 버프 적용 여부를 시각적으로 표시할 이펙트
    [SerializeField]
    protected GameObject buffEffect;  // Inspector에서 할당

    // 기본 스탯 저장용 변수 추가
    protected int baseAttackDamage;
    protected float baseRange;

    protected virtual void Start()
    {
        // 시작할 때 기본 스탯 저장
        baseAttackDamage = Damage;
        baseRange = Range;
        
        // 라인 렌더러 오브젝트를 자식으로 생성
        GameObject indicatorObj = Instantiate(rangeIndicatorPrefab, transform);
        rangeIndicator = indicatorObj.GetComponent<RangeIndicator>();
        rangeIndicator.Initialize(Range);  // 초기 Range 설정
        rangeIndicator.gameObject.SetActive(true);
        if (!isPreview)
        {
            GameManager.Instance.PlacedTowerList.Add(this);
            mainCanvas = UI_IngameScene.Instance.GetComponent<Canvas>();
            StartCoroutine(Attack());
            rangeIndicator.gameObject.SetActive(false);  // preview가 아닐 때만 숨김
        }
    }

    protected virtual void Update()
    {
        if (!isPreview)
        {
            Detect();
        }
        if(isFollow)
        {
            FollowTarget();
        }
        TooltipPopupCheck();

        // preview 상태일 때 indicator 위치 업데이트
        if (!isPreview && rangeIndicator != null)
        {
            rangeIndicator.UpdatePosition();
        }
    }

    protected virtual void OnLevelUp()
    {
        // 레벨업 후 현재 적용된 버프 재적용
        if (originalStats.Count > 0)
        {
            // 현재 적용된 모든 버프 효과 제거
            foreach (var buff in new List<BuffField>(originalStats.Keys))
            {
                RemoveBuff(buff);
            }

            // 버프 재적용
            foreach (var buff in new List<BuffField>(originalStats.Keys))
            {
                ApplyBuff(buff);
            }
        }
    }

    protected virtual void Detect()
    {
        if (CurrentTarget == null || !CurrentTarget.gameObject.activeSelf)
        {
            float closestDistance = float.MaxValue;
            Transform closestTarget = null;
            
            // Range 프로퍼티 사용
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, Range);
            foreach (Collider target in hitColliders)
            {
                if (target != null && target.CompareTag("Monster"))
                {
                    Monster monster = target.GetComponent<Monster>();
                    if (monster != null && monster.gameObject.activeSelf)  // 몬스터가 유효하고 활성화 상태인지 확인
                    {
                        float distance = Vector3.Distance(target.transform.position, transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestTarget = target.transform;
                        }
                    }
                }
            }
            CurrentTarget = closestTarget;
        }
        else
        {
            // 현재 타겟이 범위를 벗어났는지 확인할 때도 Range 프로퍼티 사용
            if (Vector3.Distance(CurrentTarget.transform.position, transform.position) > Range || 
                !CurrentTarget.gameObject.activeSelf)
            {
                CurrentTarget = null;
            }
        }
    }

    protected virtual void FollowTarget()
    {
        if(CurrentTarget != null)
        {
            Vector3 towerDir = CurrentTarget.transform.position - TowerHead.transform.position;
            towerDir.y = 0;
            TowerHead.forward = towerDir;
        }
    }

    protected void TooltipPopupCheck()
    {
        if (currentTooltip != null && Input.GetMouseButtonDown(0)) //툴팁이 있고, 마우스를 눌렀을 때
        {
            // 마우스 포인터가 UI 위에 없거나, 처음 클릭했던 마우스 위치와 누른 마우스 포지션이 같지 않으면
            if (!EventSystem.current.IsPointerOverGameObject() && clickmousePointer != Input.mousePosition)
            {
                if (rangeIndicator != null)
                {
                    rangeIndicator.gameObject.SetActive(false);
                }
                Destroy(currentTooltip);
                GameManager.Instance.tooltipCount = false;
                currentTooltip = null;
            }
            //EventSystem.current.IsPointerOverGameObject()
            //UI 요소와의 상호작용을 감지
            //터와 마우스 입력 모두 지원
        }
    }

    protected virtual IEnumerator Attack()
    {
        //각자 타워에서 구현
        yield return null;
    }

    private void OnDrawGizmos()
    {
        // Range 프로퍼티 사용
        Gizmos.DrawWireSphere(transform.position, Range);
    }

    public virtual void Upgrade()
    {
        if (Level >= MaxLevel)
        {
            Debug.Log("타워 최대레벨 초과");
            return;
        }

        Level++;
        OnLevelUp();
    }

    private void OnMouseDown()
    {
        // UI 요소 위에서 릭했다면 타워 선택을 무시
        if (isPreview || EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // 이전에 선택된 타워가 있고, 현재 타워와 다르다면
        if (selectedTower != null && selectedTower != this)
        {
            // 이전 타워의 위 표시와 툴팁을 제거
            selectedTower.HideTooltipAndRange();
        }

        clickmousePointer = Input.mousePosition;

        // 현재 툴팁이 있고 같은 포탑을 다시 클릭한 경우
        if (currentTooltip != null)
        {
            HideTooltipAndRange();
            selectedTower = null;
            return;
        }

        // 범위 표시 활성화
        if (rangeIndicator != null)
        {
            rangeIndicator.gameObject.SetActive(true);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!GameManager.Instance.tooltipCount)
        {
            if (Physics.Raycast(ray, out hit))
            {
                ShowTooltip(hit);
                selectedTower = this;  // 현재 타워를 선택된 타워로 설정
            }
        }
    }

    // 툴팁과 범위 표시를 숨기는 메서드
    private void HideTooltipAndRange()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.gameObject.SetActive(false);
        }
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
            currentTooltip = null;
        }
        GameManager.Instance.tooltipCount = false;
    }

    // 툴팁 표시 로을 분리한 메서드
    private void ShowTooltip(RaycastHit hit)
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(hit.point);
        currentTooltip = Instantiate(TowerTooltip, mainCanvas.transform);
        UI_TowerTooltip toolTip = currentTooltip.GetComponent<UI_TowerTooltip>();

        // 툴팁 정보 설정
        toolTip.Name = $"{this.Name}";
        toolTip.Element = $"{this.Element}";
        toolTip.Damage = $"{this.Damage}";
        toolTip.Range = $"{this.Range}";
        toolTip.FireRate = $"{this.FireRate}";
        toolTip.DamageDealt = $"{this.DamageDealt}";
        toolTip.UpgradePrice = $"{this.UpgradePrice}";
        toolTip.SellPrice = $"{this.SellPrice}";
        toolTip.TargetPriority = $"{this.TargetPriority}";
        toolTip.TotalKilled = $"{this.TotalKilled}";
        toolTip.TowerImage.sprite = Resources.Load<Sprite>($"Sprites/{this.Name}");

        toolTip.SetTower(this);

        RectTransform rect = currentTooltip.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvas.transform.GetComponent<RectTransform>(),screenPoint, mainCanvas.worldCamera, out localPoint);
        rect.anchoredPosition = localPoint;
        GameManager.Instance.tooltipCount = true;
    }

    // 버프 이펙트 표시/숨김
    protected void ShowBuffEffect(bool show)
    {
        if (buffEffect != null)
        {
            buffEffect.SetActive(show);
        }
    }

    // 타워가 제거될 때 버프 정리
    protected virtual void OnDestroy()
    {
        // 모든 버프 제거
        foreach (var buff in new List<BuffField>(originalStats.Keys))
        {
            if (buff != null)
            {
                RemoveBuff(buff);
            }
        }
        originalStats.Clear();
    }

    // 버프 적용
    public virtual void ApplyBuff(BuffField buff)
    {
        if (!originalStats.ContainsKey(buff))
        {
            // 현재 스탯 저장 (baseStats 기준으로)
            originalStats[buff] = new TowerStats
            {
                damage = baseAttackDamage,
                range = baseRange
            };

            // 버프 적용 (기본 스탯에 곱하기)
            Damage = Mathf.RoundToInt(baseAttackDamage * buff.damageMultiplier);
            Range = baseRange * buff.rangeMultiplier;

            ShowBuffEffect(true);
        }
    }

    // 버프 제거
    public virtual void RemoveBuff(BuffField buff)
    {
        if (originalStats.ContainsKey(buff))
        {
            // 기본 스탯으로 복구
            Damage = baseAttackDamage;
            Range = baseRange;
            originalStats.Remove(buff);

            // 다른 버프가 없으면 이펙트 숨김
            if (originalStats.Count == 0)
            {
                ShowBuffEffect(false);
            }
        }
    }
}