using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UI_EndPanel : MonoBehaviour
{
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

    public List<GameObject> ImageTetrisList; 
    public GameObject TetrisList; 
    public GameObject TowerList;  

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
    }

    private void GetRandomNotDuplicateTowerCard()
    {
        int maxTowerIndex = GameManager.Instance.TotalTowerList.Count - 1;
        int currentIndex = 2 + GameManager.Instance.clearStage;
        
        if (currentIndex > maxTowerIndex)
        {
            return;
        }

        GameObject go = Instantiate(Resources.Load<GameObject>(
            $"TowerRewardCard/{GameManager.Instance.TotalTowerList[currentIndex]}"), 
            RewardLocation.transform);
        
        GameManager.Instance.EquipTowerList[currentIndex] = GameManager.Instance.TotalTowerList[currentIndex];
    }
}
