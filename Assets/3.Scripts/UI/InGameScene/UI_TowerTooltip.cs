using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TowerTooltip : MonoBehaviour
{
    public string Name = "";
    public string Element = "";
    public string Damage = "";
    public string Range = "";
    public string FireRate = "";

    public string DamageDealt = "";
    public string TotalKilled = "";

    public string UpgradePrice = "";
    public string SellPrice = "";
    public string TargetPriority = "";

    public Image TowerImage;

    public TextMeshProUGUI NameText;
    public TextMeshProUGUI UpgradeText;
    public TextMeshProUGUI InfoText;
    public TextMeshProUGUI SellPriceText;
    public TextMeshProUGUI TargetPriorityText;

    public Button UpgradeButton;
    public Button SellButton;

    private Tower selectedTower; // 현재 선택된 타워
    private GridData towerData; // 타워 데이터 참조
    private PlacementSystem placementSystem; // PlacementSystem 참조 추가

    private void Start()
    {
        // PlacementSystem 찾기
        placementSystem = FindObjectOfType<PlacementSystem>();
        if (placementSystem != null)
        {
            // TowerData 직접 가져오기
            towerData = placementSystem.GetTowerData();
            if (towerData == null)
            {
                Debug.LogError("Failed to get TowerData from PlacementSystem");
            }
        }
        else
        {
            Debug.LogError("PlacementSystem not found in scene");
        }
        
        UpdateUI();
    }

    public void SetTower(Tower tower)
    {
        selectedTower = tower;
        Name = tower.Name;
        Element = tower.Element;
        Damage = tower.Damage.ToString();
        Range = tower.Range.ToString();
        FireRate = tower.FireRate.ToString();
        DamageDealt = tower.DamageDealt.ToString();
        TotalKilled = tower.TotalKilled.ToString();
        UpgradePrice = $"{tower.UpgradePrice}";
        SellPrice = $"{tower.SellPrice}";
        TargetPriority = tower.TargetPriority;
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (NameText != null) NameText.text = Name;
        if (InfoText != null)
        {
            InfoText.text =
                $"Element: {Element}" + System.Environment.NewLine +
                $"Damage: {Damage}" + System.Environment.NewLine +
                $"Range: {Range}" + System.Environment.NewLine +
                $"Fire Rate: {FireRate}" + System.Environment.NewLine +
                System.Environment.NewLine + System.Environment.NewLine +
                $"Damage Dealt: {DamageDealt}" + System.Environment.NewLine +
                $"Total Killed: {TotalKilled}";
        }

        if (TargetPriorityText != null) TargetPriorityText.text = $"Target: {TargetPriority}";
        if (SellPriceText != null) SellPriceText.text = $"Sell       ${SellPrice}";
        
        if (selectedTower != null && UpgradeButton != null && UpgradeText != null)
        {
            // 최대 레벨이면 업그레이드 버튼 비활성화
            if (selectedTower.Level >= selectedTower.MaxLevel)
            {
                UpgradeText.text = "Max Level";
                UpgradeButton.interactable = false;
            }
            else
            {
                UpgradeText.text = $"Upgrade   ${UpgradePrice}";
                UpgradeButton.interactable = true;
            }
        }

        // 버튼 이벤트 연결
        if (UpgradeButton != null)
        {
            UpgradeButton.onClick.RemoveAllListeners();
            UpgradeButton.onClick.AddListener(OnUpgradeClick);
        }
        
        if (SellButton != null)
        {
            SellButton.onClick.RemoveAllListeners();
            SellButton.onClick.AddListener(OnSellClick);
        }
    }

    private void OnUpgradeClick()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);

        if (selectedTower != null)
        {
            if (GameManager.Instance.CurrentMoney >= int.Parse(UpgradePrice))
            {
                GameManager.Instance.CurrentMoney -= int.Parse(UpgradePrice);
                selectedTower.Upgrade();
                SetTower(selectedTower);
            }
            else
            {
                Debug.Log("Not enough gold!");
            }
        }
    }

    private void OnSellClick()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);

        if (selectedTower != null && towerData != null)
        {
            SoundManager.Instance.Play("TowerSell", SoundManager.Sound.Effect);
            // 판매 금액 추가
            GameManager.Instance.CurrentMoney += int.Parse(SellPrice);
            
            // 타워의 그리드 위치 계산 (floor 값 포함)
            Vector3 worldPos = selectedTower.transform.position;
            Vector3Int gridPosition = towerData.WorldToGridPosition(worldPos);
            
            Debug.Log($"Selling tower at world position {worldPos}, grid position {gridPosition}");
            
            // TowerData에서 타워 정보 제거
            PlacementData data = towerData.GetPlacementData(gridPosition);
            if (data != null)
            {
                Debug.Log($"Found tower data at {gridPosition} with ID: {data.ID}, Index: {data.PlacedObjectIndex}, OccupiedPositions: {data.occupiedPositions.Count}");
                
                // TowerData에서 타워 정보 제거
                towerData.RemoveObjectAt(gridPosition);
                
                // ObjectPlacer에서 타워 제거
                if (data.PlacedObjectIndex >= 0)
                {
                    Debug.Log($"Removing tower object at index: {data.PlacedObjectIndex}");
                    //ObjectPlacer.Instance?.RemoveObjectAt(ObjectPlacer.Instance.placedGameObjects.Count - data.PlacedObjectIndex);
                }
                
                Debug.Log($"Successfully removed tower data from grid position {gridPosition}");
            }

            // 타워 리스트에서 제거
            GameManager.Instance.PlacedTowerList.Remove(selectedTower);
            
            // 타워 게임 오브젝트 제거
            Debug.Log($"Destroying tower game object");
            Destroy(selectedTower.gameObject);

            GameManager.Instance.tooltipCount = false;
            // 툴팁 제거
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError($"Cannot sell tower: selectedTower is {(selectedTower == null ? "null" : "not null")}, towerData is {(towerData == null ? "null" : "not null")}");
        }
    }
}
