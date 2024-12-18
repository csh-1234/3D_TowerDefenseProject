using UnityEngine;
using UnityEngine.EventSystems;

public class UI_TowerLoadoutSlot : MonoBehaviour, IDropHandler
{
    string subStringValue = "(Clone)";
    public enum SlotType
    {
        Inventory,
        Equipment
    }

    public SlotType slotType;
    private RectTransform rectTransform; 
    public int SlotNum;  

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (!GetComponent<UnityEngine.UI.Image>())
        {
            gameObject.AddComponent<UnityEngine.UI.Image>().raycastTarget = true;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {

        if (eventData.pointerDrag == null)
        {
            return;
        }

        UI_Draggable draggedTower = eventData.pointerDrag.GetComponent<UI_Draggable>();
        if (draggedTower == null)
        {
            return;
        }

        SoundManager.Instance.Play("DropSound", SoundManager.Sound.Effect);

        SwapTowers(draggedTower);

    }
    private void SwapTowers(UI_Draggable draggedTower)
    {
        if (transform.childCount > 0)
        {
            Transform existingTower = transform.GetChild(0);
            UI_Draggable existingDraggable = existingTower.GetComponent<UI_Draggable>();

            if (existingDraggable != null)
            {
                Vector3 targetPosition = existingTower.position;
                Transform targetParent = transform;

                existingTower.SetParent(draggedTower.OriginalParent);
                existingDraggable.SetPosition(draggedTower.OriginalPosition);

                draggedTower.transform.SetParent(targetParent);
                draggedTower.SetPosition(targetPosition);

                draggedTower.SetSwapped(true);
                existingDraggable.SetSwapped(true);

                UpdateGameManagerLists(draggedTower, existingDraggable);
            }
        }
        else
        {
            draggedTower.transform.SetParent(transform);
            draggedTower.SetPosition(rectTransform.position);
            draggedTower.SetSwapped(true);

            UI_TowerLoadoutSlot originalSlot = draggedTower.OriginalParent.GetComponent<UI_TowerLoadoutSlot>();

            if (originalSlot.slotType == SlotType.Inventory)
            {
                GameManager.Instance.UnEquipTowerList[originalSlot.SlotNum] = null;
            }
            else if (originalSlot.slotType == SlotType.Equipment)
            {
                GameManager.Instance.EquipTowerList[originalSlot.SlotNum] = null;
            }

            if (slotType == SlotType.Inventory)
            {
                int firstFindIndex = draggedTower.name.IndexOf(subStringValue);
                print(firstFindIndex);
                string RemoveResult = draggedTower.name.Remove(firstFindIndex, subStringValue.Length);
                GameManager.Instance.UnEquipTowerList[SlotNum] = RemoveResult;
            }
            else
            {
                int firstFindIndex = draggedTower.name.IndexOf(subStringValue);
                print(firstFindIndex);
                string RemoveResult = draggedTower.name.Remove(firstFindIndex, subStringValue.Length);
                GameManager.Instance.EquipTowerList[SlotNum] = RemoveResult;
            }
        }
    }

    private void UpdateGameManagerLists(UI_Draggable draggedTower, UI_Draggable existingTower)
    {
        UI_TowerLoadoutSlot originalSlot = draggedTower.OriginalParent.GetComponent<UI_TowerLoadoutSlot>();

        if (slotType == SlotType.Inventory)
        {
            int firstFindIndex = draggedTower.name.IndexOf(subStringValue);
            print(firstFindIndex);
            string RemoveResult = draggedTower.name.Remove(firstFindIndex, subStringValue.Length);
            GameManager.Instance.UnEquipTowerList[SlotNum] = RemoveResult;
            if (originalSlot.slotType == SlotType.Inventory)
            {
                firstFindIndex = draggedTower.name.IndexOf(subStringValue);
                RemoveResult = draggedTower.name.Remove(firstFindIndex, subStringValue.Length);
                GameManager.Instance.UnEquipTowerList[originalSlot.SlotNum] = RemoveResult;
            }
            else
            {
                firstFindIndex = draggedTower.name.IndexOf(subStringValue);
                RemoveResult = draggedTower.name.Remove(firstFindIndex, subStringValue.Length);
                GameManager.Instance.EquipTowerList[originalSlot.SlotNum] = RemoveResult;
            }
        }
        else if (slotType == SlotType.Equipment)
        {
            int firstFindIndex = draggedTower.name.IndexOf(subStringValue);
            print(firstFindIndex);
            string RemoveResult = draggedTower.name.Remove(firstFindIndex, subStringValue.Length);
            GameManager.Instance.EquipTowerList[SlotNum] = RemoveResult;
            if (originalSlot.slotType == SlotType.Inventory)
            {
                firstFindIndex = draggedTower.name.IndexOf(subStringValue);
                RemoveResult = draggedTower.name.Remove(firstFindIndex, subStringValue.Length);
                GameManager.Instance.UnEquipTowerList[originalSlot.SlotNum] = RemoveResult;
            }
            else
            {
                firstFindIndex = draggedTower.name.IndexOf(subStringValue);
                RemoveResult = draggedTower.name.Remove(firstFindIndex, subStringValue.Length);
                GameManager.Instance.EquipTowerList[originalSlot.SlotNum] = RemoveResult;
            }
        }
    }
} 