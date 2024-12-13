using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Action OnClickHandler = null;        // 마우스 클릭
    public Action OnPressedHandler = null;      // 마우스 꾹 누름
    public Action OnPointerDownHandler = null;  // 마우스 꾹 누름 유지
    public Action OnPointerUpHandler = null;    // 마우스 누름 해제

    public Action<BaseEventData> OnDragHandler = null;      // 마우스 드래그
    public Action<BaseEventData> OnBeginDragHandler = null; // 마우스 드래그 중
    public Action<BaseEventData> OnEndDragHandler = null;   // 마우스 드래그 종료

    public Action OnPointerEnterHandler = null; // 마우스 오버 시작
    public Action OnPointerExitHandler = null;  // 마우스 오버 종료

    bool isPressed = false;

    private void Update()
    {
        if (isPressed)
        {
            OnPressedHandler?.Invoke();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("좌클릭");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("우클릭");
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            Debug.Log("중간 버튼 클릭");
        }

        OnClickHandler?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        OnPointerDownHandler?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        OnPointerUpHandler?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnDragHandler?.Invoke(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnBeginDragHandler?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragHandler?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterHandler?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitHandler?.Invoke();
    }
}