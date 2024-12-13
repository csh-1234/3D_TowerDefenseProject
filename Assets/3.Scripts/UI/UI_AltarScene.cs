using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_AltarScene : MonoBehaviour
{
    public TextMeshProUGUI CurrentEmberText;
    public Button Recover;
    public Button Sacrifice;
    public Button Purify;
    public Button Strengthen;
    public Button Back;
    public Button TowerLoadout;
    public Button Backpack;
    public GameObject TowerLoadoutPanel;
    public GameObject BackpackPanel;

    void Start()
    {
        Recover.onClick.AddListener(ClickRecover);
        Sacrifice.onClick.AddListener(ClickSacrifice);
        Purify.onClick.AddListener(ClickPurify);
        Strengthen.onClick.AddListener(ClickStrengthen);
        Back.onClick.AddListener(GoToLobby);
        TowerLoadout.onClick.AddListener(PopupTowerLoadout);
        Backpack.onClick.AddListener(PopupBackpack);
        CurrentEmberText.text = $"{GameManager.Instance.CurrentEmber}";
    }

    private void ClickRecover()
    {
        if(GameManager.Instance.CurrentEmber>=25 && GameManager.Instance.CurrentHp < GameManager.Instance.MaxHp)
        {
            GameManager.Instance.CurrentEmber -= 25;
            if (GameManager.Instance.CurrentHp + 5 >= GameManager.Instance.MaxHp)
            {
                GameManager.Instance.CurrentHp = GameManager.Instance.MaxHp;
            }
            else
            {
                GameManager.Instance.CurrentHp += 5;
            }
            GameManager.Instance.clearStage += 1;
            FadeManager.Instance.LoadScene("LobbyScene");
        }
    }

    private void ClickSacrifice()
    {
        if (GameManager.Instance.CurrentHp > 5)
        {
            GameManager.Instance.bounusMoney += 30;
            GameManager.Instance.CurrentHp -= 5;
            //TODO:: Increase start gold by 30
            GameManager.Instance.clearStage += 1; 
            FadeManager.Instance.LoadScene("LobbyScene");
        }
    }

    private void ClickPurify()
    {
        if (GameManager.Instance.CurrentEmber >= 10)
        {
            GameManager.Instance.CurrentEmber -= 10;
            GameManager.Instance.MaxHp += 5;
            GameManager.Instance.clearStage += 1;
            FadeManager.Instance.LoadScene("LobbyScene");
        }
    }

    private void ClickStrengthen()
    {
        if (GameManager.Instance.CurrentEmber >= 60)
        {
            GameManager.Instance.CurrentEmber -= 60;
            GameManager.Instance.CurrentHp += 5;
            GameManager.Instance.MaxHp += 5;
            GameManager.Instance.clearStage += 1;
            FadeManager.Instance.LoadScene("LobbyScene");
        }
    }

    private void GoToLobby()
    {
        GameManager.Instance.clearStage += 1;
        FadeManager.Instance.LoadScene("LobbyScene");
    }

    private void PopupTowerLoadout()
    {
        TowerLoadoutPanel.SetActive(true);
    }

    private void PopupBackpack()
    {
        BackpackPanel.SetActive(true);
    }
}
