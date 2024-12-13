using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Draggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    // 드래그 시작 시의 원래 부모와 위치를 저장하기 위한 변수
    public Transform OriginalParent;
    public Vector3 OriginalPosition;
    // 타워가 다른 타워와 스왑되었는지 추적하는 플래그
    private bool isSwapped = false;

    // 타워의 위치를 설정하는 메서드
    public void SetPosition(Vector3 position)
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.position = position;
        }
    }

    // 드래그 시작 시 호출되는 메서드
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 현재 부모와 위치를 저장
        OriginalParent = transform.parent;
        OriginalPosition = transform.position;
        // 드래그 중인 오브젝트를 최상위 캔버스로 이동
        transform.SetParent(transform.root);
        // 레이캐스트 차단을 해제하여 다른 UI 요소와 상호작용 가능하게 함
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    // 드래그 중 지속적으로 호출되는 메서드
    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 커서를 따라 오브젝트 이동
        transform.position = eventData.position;
    }

    // 드래그가 끝날 때 호출되는 메서드
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"End drag: {gameObject.name}");
        // 레이캐스트 차단을 다시 활성화
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        // 스왑이 발생하지 않았고, 유효한 슬롯에 드롭되지 않은 경우
        if (!isSwapped && (eventData.pointerEnter == null ||
            !(eventData.pointerEnter.GetComponent<UI_TowerLoadoutSlot>() &&
              eventData.pointerEnter.GetComponent<UI_Draggable>())))
        {
            Debug.Log("Returning to original position");
            // 원래 위치로 되돌림
            transform.SetParent(OriginalParent);
            transform.position = OriginalPosition;
        }
        // 스왑 플래그 초기화
        isSwapped = false;
    }

    // 스왑 상태를 설정하는 메서드
    public void SetSwapped(bool value)
    {
        isSwapped = value;
    }
}