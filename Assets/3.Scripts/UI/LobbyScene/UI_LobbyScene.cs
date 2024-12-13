using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class UI_LobbyScene : MonoBehaviour
{
    public Button Setting; 
    public Button Title;
    public Button TowerLoadout;
    public Button Backpack;
    public Button Talents;
    public GameObject SettingPopup;
    public GameObject TowerLoadoutPopup;
    public GameObject BackpackPopup;
    public TextMeshProUGUI EmberAmountText;
    public TextMeshProUGUI ExpAmountText;
    public TextMeshProUGUI HpAmountText;
    public GameObject EmptyCard;
    public Transform TowerCardLocation;
    public List<GameObject> towerCards = new List<GameObject>();

    private void Awake()
    {
        Setting.onClick.AddListener(PopupSetting);
        Title.onClick.AddListener(BackToTitle);
        TowerLoadout.onClick.AddListener(PopupTowerLoadout);
        Backpack.onClick.AddListener(PopupBackpack);
    }

    private void Start()
    {
        SoundManager.Instance.Play("MBGM", SoundManager.Sound.Bgm);
        TowerCard();
        GameManager.Instance.IsSaved = true;
        GameManager.Instance.HpBeforeEnterStage = GameManager.Instance.CurrentHp;
    }

    private void Update()
    {
        EmberAmountText.text = $"{GameManager.Instance.CurrentEmber}";
        ExpAmountText.text = $"{GameManager.Instance.CurrentExp}";
        HpAmountText.text = $"{GameManager.Instance.CurrentHp}/{GameManager.Instance.MaxHp}";
    }

    private void PopupSetting()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        SettingPopup.SetActive(true);
    }
    private void BackToTitle()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        FadeManager.Instance.LoadScene("TitleScene");
        //SceneManager.LoadScene("TitleScene");
    }
    private void PopupTowerLoadout()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        TowerLoadoutPopup.SetActive(true);
    }
    private void PopupBackpack()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        BackpackPopup.SetActive(true);
    }

    public void TowerCard()
    {
        foreach (var item in towerCards)
        {
            Destroy(item.gameObject); 
        }
        towerCards = new List<GameObject>();
        for (int i = 0; i<8; i++)
        {

            if (GameManager.Instance.EquipTowerList[i] == ""||GameManager.Instance.EquipTowerList[i]==null)
            {
                towerCards.Add(Instantiate(EmptyCard, TowerCardLocation));
            }
            else
            {
                towerCards.Add((GameObject)Instantiate(Resources.Load($"AcademyTowerCard/{GameManager.Instance.EquipTowerList[i]}"), TowerCardLocation));
            }
            
        }
    }
} 
