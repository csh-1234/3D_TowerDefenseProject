using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random; 

public class UI_AcademyPanel : MonoBehaviour, IPointerClickHandler
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

    public GameObject TetrisListLocation;
    public GameObject TowerListLocation;
    public List<GameObject> ImageTetrisList;
    public List<GameObject> TetrisList;
    public List<GameObject> ImageTowerList;
    public List<GameObject> TowerList;
    public List<string> TowerNameList;
    public List<string> RandomTowerList;

    private void Awake()
    {
        SoundManager.Instance.Play("Academy", SoundManager.Sound.Bgm);

        RandomTetrisCard();
        RandomTowerCard();
    }


    private void RandomTetrisCard()
    {

        for (int i = 0; i < 7; i++)
        {
            int cardnum;
            GameObject go;
            cardnum = Random.Range(0, GameManager.Instance.TotalTetrisList.Count);
            go = Instantiate(Resources.Load<GameObject>($"TetrisCardPrefab/{Enum.GetName(typeof(ImageTetrisEnum), cardnum)}"),
                TetrisListLocation.transform);
            ImageTetrisList.Add(go);
            TetrisList.Add(Resources.Load<GameObject>($"TetrisCardPrefab/{Enum.GetName(typeof(TetrisEnum), cardnum)}"));
        }
    }

    private void RandomTowerCard()
    {
        int[] tower = new int[(int)TowerEnum.Count];
        for(int i = 0; i<(int)TowerEnum.Count; i++)
        {
            tower[i] = i;
        }
        int random1, random2;
        int temp;
        for(int i= 0; i<=tower.Length; i++)
        {
            random1 = Random.Range(0, tower.Length);
            random2 = Random.Range(0, tower.Length);
            temp = tower[random1];
            tower[random1] = tower[random2];
            tower[random2] = temp;
        }

        
        for (int i = 0; i < (int)TowerEnum.Count; i++)
        {
            RandomTowerList.Add(Enum.GetName(typeof(TowerEnum), tower[i]));            
        }

        for (int i = 0; i < 3; i++)
        {
            print(Enum.GetName(typeof(TowerEnum), tower[i]));
            TowerList.Add(Instantiate(Resources.Load<GameObject>($"AcademyTowerCard/{Enum.GetName(typeof(TowerEnum), tower[i])}"),
                TowerListLocation.transform));
            TowerNameList.Add(Enum.GetName(typeof(TowerEnum), tower[i]));
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.Play("AcademySelect", SoundManager.Sound.Effect);
        for (int i = 0; i < TowerList.Count; i++)
        {
            GameManager.Instance.EquipTowerList[i]=TowerNameList[i];
        }
        GameManager.Instance.TotalTowerList = RandomTowerList;
        GameManager.Instance.PlayerTetrisList = TetrisList;
        FadeManager.Instance.LoadScene("LobbyScene");
        GameManager.Instance.clearStage = 1 ;
    }
}