using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    #region ToolTip
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

    public GameObject rangeIndicatorPrefab;
    private RangeIndicator rangeIndicator;

    public int Level { get; protected set; } = 1;
    public int MaxLevel = 3;

    public Transform TowerHead;
    public Transform TowerMuzzle;
    public GameObject TowerTooltip;

    public bool isPreview = false;
    public bool isFollow = false;

    public Canvas mainCanvas;

    protected Transform CurrentTarget = null;
    private GameObject currentTooltip;
    private Vector3 clickmousePointer;

    [SerializeField]
    private float _range;  
    public float Range 
    {
        get { return _range; }
        set  
        {
            _range = value;
            if (rangeIndicator != null)
            {
                rangeIndicator.UpdateRange(_range);
            }
        }
    }

    private static Tower selectedTower = null; 

    public struct TowerStats
    {
        public int damage;
        public float range;
    }

    public Dictionary<BuffField, TowerStats> originalStats = new Dictionary<BuffField, TowerStats>();

    [SerializeField]
    protected GameObject buffEffect;

    protected int baseAttackDamage;
    protected float baseRange;

    protected virtual void Start()
    {
        baseAttackDamage = Damage;
        baseRange = Range;
        
        GameObject indicatorObj = Instantiate(rangeIndicatorPrefab, transform);
        rangeIndicator = indicatorObj.GetComponent<RangeIndicator>();
        rangeIndicator.Initialize(Range);  
        rangeIndicator.gameObject.SetActive(true);
        if (!isPreview)
        {
            GameManager.Instance.PlacedTowerList.Add(this);
            mainCanvas = UI_IngameScene.Instance.GetComponent<Canvas>();
            StartCoroutine(Attack());
            rangeIndicator.gameObject.SetActive(false); 
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

        if (!isPreview && rangeIndicator != null)
        {
            rangeIndicator.UpdatePosition();
        }
    }

    protected virtual void OnLevelUp()
    {
        if (originalStats.Count > 0)
        {
            foreach (var buff in new List<BuffField>(originalStats.Keys))
            {
                RemoveBuff(buff);
            }

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
            
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, Range);
            foreach (Collider target in hitColliders)
            {
                if (target != null && target.CompareTag("Monster"))
                {
                    Monster monster = target.GetComponent<Monster>();
                    if (monster != null && monster.gameObject.activeSelf) 
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
        if (currentTooltip != null && Input.GetMouseButtonDown(0)) 
        {
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
        }
    }

    protected virtual IEnumerator Attack()
    {
        yield return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, Range);
    }

    public virtual void Upgrade()
    {
        if (Level >= MaxLevel)
        {
            return;
        }

        Level++;
        OnLevelUp();
    }

    private void OnMouseDown()
    {
        if (isPreview || EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (selectedTower != null && selectedTower != this)
        {
            selectedTower.HideTooltipAndRange();
        }

        clickmousePointer = Input.mousePosition;

        if (currentTooltip != null)
        {
            HideTooltipAndRange();
            selectedTower = null;
            return;
        }

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
                selectedTower = this; 
            }
        }
    }

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

    private void ShowTooltip(RaycastHit hit)
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(hit.point);
        currentTooltip = Instantiate(TowerTooltip, mainCanvas.transform);
        UI_TowerTooltip toolTip = currentTooltip.GetComponent<UI_TowerTooltip>();

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

    protected void ShowBuffEffect(bool show)
    {
        if (buffEffect != null)
        {
            buffEffect.SetActive(show);
        }
    }

    protected virtual void OnDestroy()
    {
        foreach (var buff in new List<BuffField>(originalStats.Keys))
        {
            if (buff != null)
            {
                RemoveBuff(buff);
            }
        }
        originalStats.Clear();
    }

    public virtual void ApplyBuff(BuffField buff)
    {
        if (!originalStats.ContainsKey(buff))
        {
            originalStats[buff] = new TowerStats
            {
                damage = baseAttackDamage,
                range = baseRange
            };

            Damage = Mathf.RoundToInt(baseAttackDamage * buff.damageMultiplier);
            Range = baseRange * buff.rangeMultiplier;

            ShowBuffEffect(true);
        }
    }

    public virtual void RemoveBuff(BuffField buff)
    {
        if (originalStats.ContainsKey(buff))
        {
            Damage = baseAttackDamage;
            Range = baseRange;
            originalStats.Remove(buff);

            if (originalStats.Count == 0)
            {
                ShowBuffEffect(false);
            }
        }
    }
}