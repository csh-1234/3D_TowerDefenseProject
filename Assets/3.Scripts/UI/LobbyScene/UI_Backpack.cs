using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Backpack : MonoBehaviour
{
    public Button Exit;
    public GameObject NoCard;
    public Transform Cards1;
    public Transform Cards2;

    private void Awake()
    {
        Exit.onClick.AddListener(ExitBackpack);
    }
    private void Start()
    {
        if (GameManager.Instance.PlayerTetrisList.Count == 0)
        {
            NoCard.SetActive(true);
        }
        else
        {
            NoCard.SetActive(false);
            for (int i = 0; i < GameManager.Instance.PlayerTetrisList.Count; i++)
            {
                if (i<8)
                {
                    Instantiate(GameManager.Instance.PlayerTetrisList[i], Cards1);
                }
                else if(i<16)
                {
                    Instantiate(GameManager.Instance.PlayerTetrisList[i] , Cards2);
                }
            }
        }
    }

    private void Update()
    {
        
    }

    private void ExitBackpack()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        gameObject.SetActive(false);
    }
}


