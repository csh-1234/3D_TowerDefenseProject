using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_BackToTitlePanel : MonoBehaviour
{
    public Button Yes;
    public Button No;

    private void Awake()
    { 
        Yes.onClick.AddListener(ClickYes);
        No.onClick.AddListener(ClickNo);
    }
    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Time.timeScale = GameManager.Instance.InGameSpeed;
            gameObject.SetActive(false);
        }
    }
    private void ClickYes()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        Time.timeScale = 1f;
        GameManager.Instance.CurrentHp = GameManager.Instance.HpBeforeEnterStage;
        FadeManager.Instance.LoadScene("TitleScene");
    }

    private void ClickNo()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        gameObject.SetActive(false);
    }
}
