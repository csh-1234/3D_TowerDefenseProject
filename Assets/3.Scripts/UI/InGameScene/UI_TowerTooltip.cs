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

    private Tower selectedTower;
    private GridData towerData; 
    private PlacementSystem placementSystem;

    private void Start()
    {
        placementSystem = FindObjectOfType<PlacementSystem>();
        if (placementSystem != null)
        {
            towerData = placementSystem.GetTowerData();
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
        }
    }

    private void OnSellClick()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);

        if (selectedTower != null && towerData != null)
        {
            SoundManager.Instance.Play("TowerSell", SoundManager.Sound.Effect);
            GameManager.Instance.CurrentMoney += int.Parse(SellPrice);
            
            Vector3 worldPos = selectedTower.transform.position;
            Vector3Int gridPosition = towerData.WorldToGridPosition(worldPos);
            
            
            PlacementData data = towerData.GetPlacementData(gridPosition);
            if (data != null)
            {
                towerData.RemoveObjectAt(gridPosition);
            }

            GameManager.Instance.PlacedTowerList.Remove(selectedTower);
            
            Destroy(selectedTower.gameObject);
            GameManager.Instance.tooltipCount = false;
            Destroy(gameObject);
        }
    }
}
