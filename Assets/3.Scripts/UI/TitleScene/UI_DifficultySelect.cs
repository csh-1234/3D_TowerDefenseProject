using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_DifficultySelect : MonoBehaviour
{
    public Button Casual;
    public Button Normal;
    public Button Heroic;
    public Button BackToTitle;

    private void OnEnable()
    {
        Casual.onClick.AddListener(EnterCasual);
        Normal.onClick.AddListener(EnterNormal);
        BackToTitle.onClick.AddListener(EnterBackToTitle);
    }

    private void EnterCasual()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        FadeManager.Instance.LoadScene("LobbyScene");
        GameManager.Instance.Difficulty = 1;
    }
    private void EnterNormal()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        FadeManager.Instance.LoadScene("LobbyScene");
        GameManager.Instance.Difficulty = 2;
    }
    private void EnterHeroic()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        FadeManager.Instance.LoadScene("LobbyScene");
        GameManager.Instance.Difficulty = 3;
    }
    private void EnterBackToTitle()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        gameObject.SetActive(false);
    }
}
