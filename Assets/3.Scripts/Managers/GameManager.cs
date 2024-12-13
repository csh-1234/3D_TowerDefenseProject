using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    public bool IsSaved=false;
    public int CurrentExp = 10;
    public int CurrentEmber = 10;
    public int CurrentHp = 15;
    public int CurrentMoney = 50;
    public int MaxHp = 15;
    public int Difficulty;
    public int HpBeforeEnterStage;
    public float InGameSpeed = 1f;
    public List<int> TotalTetrisList = new List<int> { 0,1,2,3,4,5}; //0~100
    public List<string> TotalTowerList;  //101~200
    public List<GameObject> PlayerTetrisList;
    public List<GameObject> HandTetrisList;
    public List<string> EquipTowerList;
    public List<string> UnEquipTowerList;
    public List<Tower> PlacedTowerList;

    public int bounusMoney = 0;
    public int clearStage = 0;

    public int BossCount = 0;

    public bool tooltipCount = false;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    //재시작시 게임매니저 초기화 메서드
    public void Clear()
    {
        clearStage = 0;
        CurrentHp = 15;
        CurrentMoney = 50;
        MaxHp = 15;
        IsSaved = false;

        if (PlayerTetrisList != null)
        {
            foreach (var obj in PlayerTetrisList)
            {
                if (obj != null)
                    Destroy(obj);
            }
            PlayerTetrisList.Clear();
        }

        if (HandTetrisList != null)
        {
            foreach (var obj in HandTetrisList)
            {
                if (obj != null)
                    Destroy(obj);
            }
            HandTetrisList.Clear();
        }

        if (EquipTowerList != null)
        {
            for (int i = 0; i < EquipTowerList.Count; i++)
            {
                EquipTowerList[i] = null;
            }
            //EquipTowerList.Clear();
        }

        if (UnEquipTowerList != null)
        {
            for (int i = 0; i < UnEquipTowerList.Count; i++)
            {
                UnEquipTowerList[i] = null;
            }
            //UnEquipTowerList.Clear();
        }

        if (PlacedTowerList != null)
        {
            foreach (var tower in PlacedTowerList)
            {
                if (tower != null)
                    Destroy(tower.gameObject);
            }
            PlacedTowerList.Clear();
        }

        Difficulty = 0;
        PlayerTetrisList = new List<GameObject>();
        HandTetrisList = new List<GameObject>();
        //EquipTowerList = new List<string>();
        //UnEquipTowerList = new List<string>();
        PlacedTowerList = new List<Tower>();
        tooltipCount = false;

    //각 매니저들 초기화
    GridData.Instance.Clear();
        PathManager.Instance.Clear();
        ObjectPlacer.Instance.Clear();
        PathManager.Instance.Clear();
        if (PlacementSystem.Instance != null)
        {
            PlacementSystem.Instance.Clear();
        }
        PoolManager.Instance.Clear();
        ObjectManager.Instance.Clear();
    }

    public void ClearWin()
    {
        if (HandTetrisList != null)
        {
            foreach (var obj in HandTetrisList)
            {
                if (obj != null)
                    Destroy(obj);
            }
            HandTetrisList.Clear();
        }

        if (PlacedTowerList != null)
        {
            foreach (var tower in PlacedTowerList)
            {
                if (tower != null)
                    Destroy(tower.gameObject);
            }
            PlacedTowerList.Clear();
        }

        tooltipCount = false;
        HandTetrisList = new List<GameObject>();
        PlacedTowerList = new List<Tower>();

        //각 매니저들 초기화
        GridData.Instance.Clear();
        PathManager.Instance.Clear();
        ObjectPlacer.Instance.Clear();
        PathManager.Instance.Clear();
        PlacementSystem.Instance.Clear();
        PoolManager.Instance.Clear();
        ObjectManager.Instance.Clear();

    }
}
