using UnityEngine;
using UnityEngine.EventSystems;

public class UI_TowerLoadoutSlot : MonoBehaviour, IDropHandler
{
    string subStringValue = "(Clone)";
    // 슬롯의 타입을 정의하는 열거형
    public enum SlotType
    {
        Inventory,
        Equipment
    }

    public SlotType slotType;  // 현재 슬롯의 타입
    private RectTransform rectTransform;  // 슬롯의 RectTransform
    public int SlotNum;  // 슬롯 번호

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // 레이캐스트를 위한 Image 컴포넌트 추가
        if (!GetComponent<UnityEngine.UI.Image>())
        {
            gameObject.AddComponent<UnityEngine.UI.Image>().raycastTarget = true;
        }
    }

    // 드래그된 아이템이 드롭될 때 호출되는 메서드
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"OnDrop called on slot: {gameObject.name}");

        // 드래그된 오브젝트 검증
        if (eventData.pointerDrag == null)
        {
            Debug.LogWarning("No dragged object found");
            return;
        }

        UI_Draggable draggedTower = eventData.pointerDrag.GetComponent<UI_Draggable>();
        if (draggedTower == null)
        {
            Debug.LogWarning("Dragged object is not a tower");
            return;
        }

        SoundManager.Instance.Play("DropSound", SoundManager.Sound.Effect);

        // 타워 교환 실행
        SwapTowers(draggedTower);

    }

    // 타워 교환을 처리하는 메서드
    private void SwapTowers(UI_Draggable draggedTower)
    {
        if (transform.childCount > 0)
        {
            // 현재 슬롯에 타워가 있는 경우
            Transform existingTower = transform.GetChild(0);
            UI_Draggable existingDraggable = existingTower.GetComponent<UI_Draggable>();

            if (existingDraggable != null)
            {
                // 현재 슬롯의 위치 정보 저장
                Vector3 targetPosition = existingTower.position;
                Transform targetParent = transform;

                // 기존 타워를 드래그된 타워의 원래 위치로 이동
                existingTower.SetParent(draggedTower.OriginalParent);
                existingDraggable.SetPosition(draggedTower.OriginalPosition);

                // 드래그된 타워를 현재 슬롯으로 이동
                draggedTower.transform.SetParent(targetParent);
                draggedTower.SetPosition(targetPosition);

                // 스왑 완료 플래그 설정
                draggedTower.SetSwapped(true);
                existingDraggable.SetSwapped(true);

                // GameManager의 타워 리스트 업데이트
                UpdateGameManagerLists(draggedTower, existingDraggable);
            }
        }
        else
        {
            // 빈 슬롯인 경우
            draggedTower.transform.SetParent(transform);
            draggedTower.SetPosition(rectTransform.position);
            draggedTower.SetSwapped(true);

            // 원래 위치의 슬롯 정보 가져오기
            UI_TowerLoadoutSlot originalSlot = draggedTower.OriginalParent.GetComponent<UI_TowerLoadoutSlot>();

            // 원래 위치의 리스트에서 타워 제거
            if (originalSlot.slotType == SlotType.Inventory)
            {
                GameManager.Instance.UnEquipTowerList[originalSlot.SlotNum] = null;
            }
            else if (originalSlot.slotType == SlotType.Equipment)
            {
                GameManager.Instance.EquipTowerList[originalSlot.SlotNum] = null;
            }

            // 새로운 위치의 리스트에 타워 추가
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

    // GameManager의 타워 리스트를 업데이트하는 메서드
    private void UpdateGameManagerLists(UI_Draggable draggedTower, UI_Draggable existingTower)
    {
        UI_TowerLoadoutSlot originalSlot = draggedTower.OriginalParent.GetComponent<UI_TowerLoadoutSlot>();

        // 현재 슬롯 타입에 따라 적절한 리스트 업데이트
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