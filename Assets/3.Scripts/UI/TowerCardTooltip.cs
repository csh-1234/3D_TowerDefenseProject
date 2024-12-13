using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TowerCardTooltip : MonoBehaviour
{
    private UI_EventHandler eventHandler;
    [SerializeField] private GameObject tooltip;
    private GameObject currentTooltip;
    [SerializeField] public TowerDataSo Data;

    protected virtual void Awake()
    {
        tooltip.GetComponent<AcademyTowerTooltip>().towerData = Data;
        eventHandler = GetComponent<UI_EventHandler>();
    }

    protected void OnEnable()
    {
        eventHandler.OnPointerEnterHandler += OnEnter;
        eventHandler.OnPointerExitHandler += OnExit;
    }

    protected void OnDisable()
    {
        eventHandler.OnPointerEnterHandler -= OnEnter;
        eventHandler.OnPointerExitHandler -= OnExit;
    }


    protected virtual void OnEnter()
    {
        tooltip.GetComponent<AcademyTowerTooltip>().towerData = Data;
        currentTooltip = Instantiate(tooltip, transform);
        RectTransform rectTransform = currentTooltip.GetComponent<RectTransform>();
        //rectTransform.localRotation = Quaternion.identity;
        rectTransform.rotation = Quaternion.identity;

        rectTransform.anchoredPosition = new Vector2(0, 50); // 원하는 위치로 조정
    }

    protected virtual void OnExit()
    {
        if (currentTooltip != null)
        {
            // 방법 1: 즉시 삭제
            Destroy(currentTooltip);
        }
    }
}
