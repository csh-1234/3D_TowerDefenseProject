using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Button 사용을 위해 추가

public class Tower_Placement : Block
{
    [SerializeField]
    protected int towerID = 101;  // 기본값 설정, 자식 클래스에서 접근 가능하도록 protected로 변경
    public int TowerPrice;

    protected Button placementButton; // 더 명확한 변수명 사용

    protected override void Awake()
    {
        blockType = towerID;  // towerID 사용
        base.Awake();

        // 버튼 컴포넌트 가져오기 (Block 클래스에 button 변수가 없을 경우)
        if (button == null)
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError($"Button 컴포넌트가 {gameObject.name}에 없습니다.");
            }
        }
    }

    protected override void Start()
    {
        base.Start();  // 부모 클래스의 Start 메서드 호출

        if (button != null)
        {
            button.onClick.RemoveAllListeners();  // 기존 리스너 제거
            button.onClick.AddListener(OnPlaceTowerClicked);  // 새로운 리스너 추가
        }
        else
        {
            Debug.LogError($"Button 컴포넌트가 {gameObject.name}에 없습니다.");
        }
    }

    // 버튼 클릭 시 호출되는 메서드
    private void OnPlaceTowerClicked()
    {
        // 버튼 클릭 시 사운드 재생
        SoundManager.Instance.Play("TowerClickSound", SoundManager.Sound.Effect);

        // 타워 배치 시도
        PlaceTower();
    }

    protected virtual void PlaceTower()
    {
        if (TowerPrice <= GameManager.Instance.CurrentMoney)
        {
            // 타워 배치 로직 호출
            PlacementSystem.Instance.StartTowerPlacement(blockType);
        }
        else
        {
            Debug.Log("타워를 배치할 돈이 부족합니다.");
            // 돈이 부족할 때 경고 사운드를 재생하려면 다음과 같이 추가할 수 있습니다.
            // SoundManager.Instance.Play("InsufficientFunds", SoundManager.Sound.Effect);
        }
    }

    // 타워 배치가 성공했을 때 호출되는 메서드
    protected override void HandlePlacementSuccess(int placedBlockID, string selectedCardID)
    {
        base.HandlePlacementSuccess(placedBlockID, selectedCardID);
    }
}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Tower_Placement : Block
//{
//    [SerializeField]
//    protected int towerID = 101;  // 기본값 설정, 자식 클래스에서 접근 가능하도록 protected로 변경
//    public int TowerPrice;

//    protected override void Awake()
//    {
//        blockType = towerID;  // towerID 사용
//        base.Awake();
//    }

//    protected override void Start()
//    {
//        base.Start();  // 부모 클래스의 Start 메서드 호출

//        if (button != null)
//        {
//            button.onClick.RemoveAllListeners();  // 기존 리스너 제거
//            button.onClick.AddListener(PlaceTower);  // 새로운 리스너 추가
//        }
//        else
//        {
//            Debug.LogError($"Button component is missing on {gameObject.name}");
//        }
//    }

//    private void PlaceTower()
//    {
//        if (TowerPrice <= GameManager.Instance.CurrentMoney)
//        {
//            PlacementSystem.Instance.StartTowerPlacement(blockType);  // blockType 사용
//        }
//        else
//        {
//            Debug.Log("Not enough money to place tower");
//        }
//    }

//    // 타워는 설치 후에도 제거되지 않도록 HandlePlacementSuccess를 오버라이드
//    protected override void HandlePlacementSuccess(int placedBlockID, string selectedCardID)
//    {
//        // 아무 동작도 하지 않음 (카드가 제거되지 않음)
//    }
//}
