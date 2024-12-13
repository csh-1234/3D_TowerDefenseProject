using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AcademyTowerTooltip : MonoBehaviour
{
    [SerializeField] public TowerDataSo towerData;

    [SerializeField] private TextMeshProUGUI towerName;
    [SerializeField] private TextMeshProUGUI towerCost;
    [SerializeField] private TextMeshProUGUI towerElement;
    [SerializeField] private TextMeshProUGUI towerDamage;
    [SerializeField] private TextMeshProUGUI towerRange;
    [SerializeField] private TextMeshProUGUI towerFireRate;
    [SerializeField] private TextMeshProUGUI towerInfo;

    private void Start()
    {
        towerName.text = $"{towerData.Name}";
        towerCost.text = $"Gold Cost: {towerData.Price}";
        towerElement.text = $"Element: {towerData.Element}";
        towerDamage.text = $"Damage: {towerData.Damage}";
        towerRange.text = $"Range: {towerData.Range}";
        towerFireRate.text = $"Fire Rate: {towerData.FireRate}";
        towerInfo.text = $"{towerData.Info}";
    }
}
