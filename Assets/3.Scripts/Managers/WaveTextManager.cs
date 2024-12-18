using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveTextManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI waveInfoText; 
    [SerializeField]
    private GameObject displayPanel;     

    private List<Wave> waves;            
    private int currentWaveIndex = 0;    

    public void Initialize(List<Wave> waveData)
    {
        waves = waveData;
        UpdateWaveInfo(); 
    }

    public void SetCurrentWaveIndex(int index)
    {
        currentWaveIndex = index;
        UpdateWaveInfo();
    }

    private void UpdateWaveInfo()
    {
        if (currentWaveIndex >= waves.Count || waveInfoText == null || displayPanel == null) return;

        Wave nextWave = waves[currentWaveIndex];
        string info = ""; 

        foreach (var monster in nextWave.monsterSpawnData)
        {
            info += $"{monster.monsterPrefab.name} x {monster.spawnCount}\n";
        }

        waveInfoText.text = info.Trim();
    }

    public void ShowWaveInfo()
    {
        displayPanel.SetActive(true);
    }

    public void HideWaveInfo()
    {
        displayPanel.SetActive(false);
    }
}
