using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PausePanel : MonoBehaviour
{
    public Button Continue;
    public Button Options;
    public Button Title;
    public GameObject OptionsPanel;
    public GameObject BackTitlePopup;

    public GameObject EndPanel;

    public GameObject GameOverPanel;

    private void Awake()
    {
        Continue.onClick.AddListener(EnterContinue);
        Options.onClick.AddListener(EnterOptions);
        Title.onClick.AddListener(BackToTitle);
    }

    private void Update()
    {
        if ((EndPanel != null && EndPanel.activeSelf) ||
            (GameOverPanel != null && GameOverPanel.activeSelf))
        {
            return;
        }

        if (Input.GetButtonDown("Cancel"))
        {
            Time.timeScale = GameManager.Instance.InGameSpeed;
            gameObject.SetActive(false);
        }
    }

    private void EnterContinue()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        Time.timeScale = GameManager.Instance.InGameSpeed;
        gameObject.SetActive(false);
    }

    private void EnterOptions()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        OptionsPanel.SetActive(true);
    }

    private void BackToTitle()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        BackTitlePopup.SetActive(true);
    }
}
