using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UI_Draw : MonoBehaviour
{
    public int DrawPrice = 5;
    public GameObject Dot;
    public GameObject O;
    public GameObject T;
    public GameObject Z;
    public GameObject I; 
    public GameObject L;
    public Button DrawButton;
    public TextMeshProUGUI DrawPriceText;
    public RectTransform HorizontalLayout;
    public GameObject DialogBox;
    public GameObject DrawPriceInfo;
    public GameObject FullHandText;

    public static Action draw;

    private void Awake()
    {
        DrawButton.onClick.AddListener(PayDrawCard);
        draw = () => { DrawCard(); };
    }
    private void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            DrawCard();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.HandTetrisList.Count >= 10)
        {
            DialogBox.SetActive(true);
            DrawPriceInfo.SetActive(false);
            FullHandText.SetActive(true);
        }
        
        else if (GameManager.Instance.HandTetrisList.Count <7)
        {
            DialogBox.SetActive(true);
            DrawPriceInfo.SetActive(true);
            FullHandText.SetActive(false);
            DrawPriceText.text = $"{DrawPrice}";
        }

        else
        {
            DialogBox.SetActive(false);
        }

    }

    public void DrawCard()
    {
        int drawCard = Random.Range(0, GameManager.Instance.TotalTetrisList.Count);
        for (int i = 0; i<GameManager.Instance.TotalTetrisList.Count; i++)
        {
            if (i == drawCard)
            {
                GameObject type;
                switch (GameManager.Instance.TotalTetrisList[i])
                {
                    case 0:
                        type = Instantiate(Dot, HorizontalLayout);
                        GameManager.Instance.HandTetrisList.Add(type);
                        break;
                    case 1:
                        type = Instantiate(O, HorizontalLayout);
                        GameManager.Instance.HandTetrisList.Add(type);
                        break;
                    case 2:
                        type = Instantiate(T, HorizontalLayout);
                        GameManager.Instance.HandTetrisList.Add(type);
                        break;
                    case 3:
                        type = Instantiate(Z, HorizontalLayout);
                        GameManager.Instance.HandTetrisList.Add(type);
                        break;
                    case 4:
                        type = Instantiate(I, HorizontalLayout);
                        GameManager.Instance.HandTetrisList.Add(type);
                        break;
                    case 5:
                        type = Instantiate(L, HorizontalLayout);
                        GameManager.Instance.HandTetrisList.Add(type);
                        break;
                }
                break;
            }
        }
    }

    public void PayDrawCard()
    {
        if(GameManager.Instance.HandTetrisList.Count < 7 && GameManager.Instance.CurrentMoney >= DrawPrice)
        {
            SoundManager.Instance.Play("DrawCard", SoundManager.Sound.Effect);
            GameManager.Instance.CurrentMoney -= DrawPrice;
            int drawCard = Random.Range(0, GameManager.Instance.TotalTetrisList.Count);
            DrawCard();
            if (GameManager.Instance.clearStage != 6)
            { 
                DrawPrice += 5;
            }
        }
    }
}
