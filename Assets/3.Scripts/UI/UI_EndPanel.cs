using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UI_EndPanel : MonoBehaviour
{
    //1. 테트리스 블럭을 랜덤으로 1개 골라서 gamemanager에 넣어준다.
    //2. 가지고 있는 타워를 제외한 블럭을 1개 골라서 gamemanager에 넣어준다.
    
    enum TetrisEnum
    {
        Block_L,
        Block_T,
        Block_O,
        Block_I,
        Block_Dot,
        Block_Z
    }

    enum ImageTetrisEnum
    {
        Image_L,
        Image_T,
        Image_O,
        Image_I,
        Image_Dot,
        Image_Z
    }

    enum TowerEnum
    {
        Tower_Basic,
        Tower_Missile,
        Tower_Lightning,
        Tower_Poison,
        Tower_Flame,
        Tower_Drone,
        Tower_Ice,
        Tower_Sniper,
        Count
    };


    public GameObject RewardLocation;

    public List<GameObject> ImageTetrisList; // 패널 출력용
    public GameObject TetrisList; // 실제 게임매니져
    public GameObject TowerList;  // 실제 게임매니져

    public void init()
    {
        GetRandomTetrisCard();
        GetRandomNotDuplicateTowerCard();
    }
    private void GetRandomTetrisCard()
    {
        int cardnum;
        cardnum = Random.Range(0, GameManager.Instance.TotalTetrisList.Count);
        GameObject go = Instantiate(Resources.Load<GameObject>($"TetrisRewardCard/{Enum.GetName(typeof(ImageTetrisEnum), cardnum)}"),RewardLocation.transform);
        ImageTetrisList.Add(go);

        GameManager.Instance.PlayerTetrisList.Add(Resources.Load<GameObject>($"TetrisCardPrefab/{Enum.GetName(typeof(TetrisEnum), cardnum)}"));
        Debug.Log($"테트리스 카드 추가됨 {Enum.GetName(typeof(TetrisEnum), cardnum)}");
    }

    private void GetRandomNotDuplicateTowerCard()
    {
        // 타워 리스트의 최대 인덱스를 체크
        int maxTowerIndex = GameManager.Instance.TotalTowerList.Count - 1;
        int currentIndex = 2 + GameManager.Instance.clearStage;
        
        // 인덱스가 범위를 벗어나지 않도록 체크
        if (currentIndex > maxTowerIndex)
        {
            Debug.LogWarning("모든 타워를 이미 획득했습니다!");
            return;
        }

        Debug.Log($"현재 타워 인덱스: {currentIndex}, 전체 타워 개수: {GameManager.Instance.TotalTowerList.Count}");
        
        // 타워 카드 생성 및 할당
        GameObject go = Instantiate(Resources.Load<GameObject>(
            $"TowerRewardCard/{GameManager.Instance.TotalTowerList[currentIndex]}"), 
            RewardLocation.transform);
        
        GameManager.Instance.EquipTowerList[currentIndex] = GameManager.Instance.TotalTowerList[currentIndex];
        Debug.Log($"타워 카드 추가됨: {GameManager.Instance.TotalTowerList[currentIndex]}");
    }
}
