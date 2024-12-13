// WaveTextManager.cs는 웨이브 정보 관리를 담당하는 스크립트
// 주요 기능
// 1. 웨이브 정보 업데이트: 현재 웨이브에 포함된 몬스터와 수량 표시.
// 2. 패널 표시/숨김 관리: 웨이브 대기 중 또는 진행 중 상태에 따라 패널 활성화 상태를 조정.

using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveTextManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI waveInfoText; // 웨이브 정보를 표시할 TextMeshPro
    [SerializeField]
    private GameObject displayPanel;     // 정보를 표시할 패널

    private List<Wave> waves;            // 웨이브 데이터를 참조할 리스트
    private int currentWaveIndex = 0;    // 현재 웨이브 인덱스

    // 웨이브 데이터를 설정
    public void Initialize(List<Wave> waveData)
    {
        waves = waveData;
        UpdateWaveInfo(); // 초기 상태 업데이트
    }

    // 현재 웨이브 인덱스 설정
    public void SetCurrentWaveIndex(int index)
    {
        currentWaveIndex = index;
        UpdateWaveInfo();
    }

    private void UpdateWaveInfo()
    {
        // 웨이브 데이터 유효성 및 UI 상태 확인
        if (currentWaveIndex >= waves.Count || waveInfoText == null || displayPanel == null) return;

        // 현재 웨이브에 대한 몬스터 정보를 가져와 UI 업데이트
        Wave nextWave = waves[currentWaveIndex];
        string info = ""; // 웨이브 번호 제외, 몬스터 정보만 표시

        foreach (var monster in nextWave.monsterSpawnData)
        {
            info += $"{monster.monsterPrefab.name} x {monster.spawnCount}\n";
        }

        // 마지막 개행 제거 후 텍스트 적용
        waveInfoText.text = info.Trim();
    }

    // 패널을 활성화 (웨이브 대기 중)
    public void ShowWaveInfo()
    {
        displayPanel.SetActive(true);
    }

    // 패널을 비활성화 (웨이브 진행 중)
    public void HideWaveInfo()
    {
        displayPanel.SetActive(false);
    }
}
