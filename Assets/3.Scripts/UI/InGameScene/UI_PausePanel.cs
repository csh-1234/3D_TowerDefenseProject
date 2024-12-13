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

    // 엔드 패널과 게임 오버 패널 참조 추가
    [Tooltip("엔드 패널 GameObject")]
    public GameObject EndPanel;

    [Tooltip("게임 오버 패널 GameObject")]
    public GameObject GameOverPanel;

    private void Awake()
    {
        Continue.onClick.AddListener(EnterContinue);
        Options.onClick.AddListener(EnterOptions);
        Title.onClick.AddListener(BackToTitle);
    }

    private void Update()
    {
        // 엔드 패널 또는 게임 오버 패널이 활성화된 경우 ESC 키 입력 무시
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
        // 버튼 소리
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        Time.timeScale = GameManager.Instance.InGameSpeed;
        gameObject.SetActive(false);
    }

    private void EnterOptions()
    {
        // 버튼 소리
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        OptionsPanel.SetActive(true);
    }

    private void BackToTitle()
    {
        // 버튼 소리
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        BackTitlePopup.SetActive(true);
    }
}
