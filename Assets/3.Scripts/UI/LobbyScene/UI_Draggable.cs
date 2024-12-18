using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Draggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Transform OriginalParent;
    public Vector3 OriginalPosition;
    private bool isSwapped = false;

    public void SetPosition(Vector3 position)
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.position = position;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OriginalParent = transform.parent;
        OriginalPosition = transform.position;
        transform.SetParent(transform.root);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"End drag: {gameObject.name}");
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (!isSwapped && (eventData.pointerEnter == null ||
            !(eventData.pointerEnter.GetComponent<UI_TowerLoadoutSlot>() &&
              eventData.pointerEnter.GetComponent<UI_Draggable>())))
        {
            transform.SetParent(OriginalParent);
            transform.position = OriginalPosition;
        }
        isSwapped = false;
    }

    public void SetSwapped(bool value)
    {
        isSwapped = value;
    }
}