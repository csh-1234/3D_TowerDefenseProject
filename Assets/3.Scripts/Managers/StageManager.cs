using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private string titleSceneName = "TitleScene";

    [SerializeField] private GameObject EndPanel;
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private Button gameOverButton;

    [SerializeField] private List<GameObject> uiElementsToDisable;

    private Button nextStageButton;
    private WaveManager waveManager;
    private bool allWavesCleared = false;
    private bool isGameOver = false; // 寃뚯엫 ?ㅻ쾭 ?곹깭瑜??섑??대뒗 ?뚮옒洹?異붽?

    public bool IsGameOver { get { return isGameOver; } } // ?몃??먯꽌 ?곹깭 ?뺤씤 媛??

    private void Start()
    {
        InitializeWaveManager();
        InitializePanels();
        InitializeUIElements();
        InitializeGameOverButton();
        InitializeNextStageButton();
    }

    private void Update()
    {
        CheckGameOverCondition();
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }

    private void InitializeWaveManager()
    {
        waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.OnAllWavesCleared += HandleAllWavesCleared;
        }
    }

    private void InitializePanels()
    {
        SetPanelActive(EndPanel, false);
        SetPanelActive(GameOverPanel, false);
    }

    private void InitializeUIElements()
    {
        SetUIElementsActive(uiElementsToDisable, true);
    }

    private void InitializeGameOverButton()
    {
        if (gameOverButton != null)
        {
            gameOverButton.onClick.AddListener(OnGameOverButtonClicked);
        }
    }

    private void InitializeNextStageButton()
    {
        if (EndPanel != null)
        {
            Button[] buttons = EndPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                if (btn.name == "NextStageButton")
                {
                    nextStageButton = btn;
                    nextStageButton.onClick.AddListener(LoadNextStage);
                    nextStageButton.gameObject.SetActive(false);
                    break;
                }
                else if(btn.name == "BackToTitle")
                {
                    Button backToTitle = btn;
                    backToTitle.onClick.AddListener(() => 
                    {
                        SceneManager.LoadScene("TitleScene");
                        GameManager.Instance.Clear(); 
                    });
                }
            }
        }
    }

    private void UnregisterEvents()
    {
        if (waveManager != null)
        {
            waveManager.OnAllWavesCleared -= HandleAllWavesCleared;
        }

        if (gameOverButton != null)
        {
            gameOverButton.onClick.RemoveListener(OnGameOverButtonClicked);
        }

        if (nextStageButton != null)
        {
            nextStageButton.onClick.RemoveListener(LoadNextStage);
        }
    }

    private void CheckGameOverCondition()
    {
        if (GameManager.Instance.CurrentHp <= 0 && !allWavesCleared && !isGameOver)
        {
            isGameOver = true;
            StartCoroutine(ShowGameOverPanelWithDelay(3f));
        }
    }

    private void HandleAllWavesCleared()
    {
        if (isGameOver) return; 

        allWavesCleared = true;
        StartCoroutine(ShowPreparationPanelWithDelay(3f));
    }

    private IEnumerator ShowPreparationPanelWithDelay(float delayTime)
    {
        yield return new WaitForSecondsRealtime(delayTime);

        if (isGameOver) yield break;

        ShowPreparationPanel();
    }

    private void ShowPreparationPanel()
    {
        if (isGameOver) return; 

        SoundManager.Instance.Play("Win_Bgm", SoundManager.Sound.Bgm, 1, false);
        SetPanelActive(EndPanel, true);
        EndPanel.GetComponent<UI_EndPanel>().init();
        SetUIElementsActive(uiElementsToDisable, false);

        if (nextStageButton != null)
        {
            nextStageButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator ShowGameOverPanelWithDelay(float delayTime)
    {
        yield return new WaitForSecondsRealtime(delayTime);
        ShowGameOverPanel();
    }

    private void ShowGameOverPanel()
    {
        SetPanelActive(GameOverPanel, true);
        if (gameOverButton != null)
        {
            SetUIElementsActive(uiElementsToDisable, false, gameOverButton.gameObject);
        }
        else
        {
            SetUIElementsActive(uiElementsToDisable, false);
        }
    }

    private void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }
    private void SetUIElementsActive(List<GameObject> elements, bool isActive, GameObject excludeElement = null)
    {
        foreach (var element in elements)
        {
            if (element != null && element != excludeElement)
            {
                element.SetActive(isActive);
            }
        }
    }

    public void OnGameOverButtonClicked()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        GameManager.Instance.Clear();
        LoadScene(titleSceneName);
    }

    private void LoadNextStage()
    {
        GameManager.Instance.ClearWin();

        if (allWavesCleared && !isGameOver) // 寃뚯엫 ?ㅻ쾭媛 ?꾨땺 ?뚮쭔 ?ㅼ쓬 ?ㅽ뀒?댁? 濡쒕뱶
        {
            GameManager.Instance.clearStage += 1;
            GameManager.Instance.CurrentEmber += 10;
            GameManager.Instance.CurrentExp += 10;
            LoadScene(lobbySceneName);
        }
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void OpenGameOverScene()
    {
        if (!string.IsNullOrEmpty(titleSceneName))
        {
            string sceneName = titleSceneName;
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}
