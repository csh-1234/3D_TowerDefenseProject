// StageManager.cs는 게임의 상태 전환 및 UI 관리를 담당하는 스크립트
// 주요 기능
// 1. 씬 전환 관리: 다음 스테이지 또는 게임 오버 씬으로 이동.
// 2. 웨이브 상태 관리: 웨이브 완료 시 패널 표시 및 상태 업데이트.
// 3. UI 상태 관리: 특정 상황에서 UI 요소를 활성화/비활성화.
// 4. 이벤트 처리: 웨이브 완료 또는 게임 오버 시 적절한 처리 수행.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [Header("씬 이름 설정")]
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private string titleSceneName = "TitleScene";

    [Header("패널 및 버튼 설정")]
    [SerializeField] private GameObject EndPanel;
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private Button gameOverButton;

    [Header("비활성화할 UI 요소들")]
    [SerializeField] private List<GameObject> uiElementsToDisable;

    private Button nextStageButton;
    private WaveManager waveManager;
    private bool allWavesCleared = false;
    private bool isGameOver = false; // 게임 오버 상태를 나타내는 플래그 추가

    public bool IsGameOver { get { return isGameOver; } } // 외부에서 상태 확인 가능

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
            Debug.Log("WaveManager 이벤트 등록 완료.");
        }
        else
        {
            Debug.LogError("WaveManager를 찾을 수 없습니다.");
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
            Debug.Log("GameOverButton 이벤트 등록 완료.");
        }
        else
        {
            Debug.LogError("GameOverButton이 할당되지 않았습니다.");
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
                    Debug.Log("'NextStageButton'을 찾았고, 리스너를 추가했습니다.");
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

            if (nextStageButton == null)
            {
                Debug.LogError("EndPanel의 자식에서 'NextStageButton' 이름을 가진 버튼을 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("EndPanel이 할당되지 않았습니다.");
        }
    }

    private void UnregisterEvents()
    {
        if (waveManager != null)
        {
            waveManager.OnAllWavesCleared -= HandleAllWavesCleared;
            Debug.Log("WaveManager 이벤트 해제 완료.");
        }

        if (gameOverButton != null)
        {
            gameOverButton.onClick.RemoveListener(OnGameOverButtonClicked);
            Debug.Log("GameOverButton 이벤트 해제 완료.");
        }

        if (nextStageButton != null)
        {
            nextStageButton.onClick.RemoveListener(LoadNextStage);
            Debug.Log("NextStageButton 이벤트 해제 완료.");
        }
    }

    private void CheckGameOverCondition()
    {
        if (GameManager.Instance.CurrentHp <= 0 && !allWavesCleared && !isGameOver)
        {
            // HP <= 0이고 아직 클리어하지 않았다면 게임 오버 상태 진입
            // 딜레이 후 패널 표시하지만, 여기서 isGameOver를 true로 설정하면
            // 웨이브 매니저 등이 이를 감지하여 웨이브를 진행하지 않음
            isGameOver = true;
            StartCoroutine(ShowGameOverPanelWithDelay(3f));
        }
    }

    private void HandleAllWavesCleared()
    {
        if (isGameOver) return; // 게임 오버 상태면 엔드 패널 표시 안 함

        allWavesCleared = true;
        StartCoroutine(ShowPreparationPanelWithDelay(3f));
    }

    private IEnumerator ShowPreparationPanelWithDelay(float delayTime)
    {
        yield return new WaitForSecondsRealtime(delayTime);

        if (isGameOver) yield break; // 게임 오버 상태라면 중단

        ShowPreparationPanel();
    }

    private void ShowPreparationPanel()
    {
        if (isGameOver) return; // 게임 오버 상태면 엔드 패널 표시하지 않음

        SoundManager.Instance.Play("Win_Bgm", SoundManager.Sound.Bgm, 1, false);
        SetPanelActive(EndPanel, true);
        EndPanel.GetComponent<UI_EndPanel>().init();
        SetUIElementsActive(uiElementsToDisable, false);

        if (nextStageButton != null)
        {
            nextStageButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("NextStageButton이 할당되지 않았습니다.");
        }

        Debug.Log("EndPanel 활성화 및 NextStageButton 표시 완료.");
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
            Debug.Log($"{panel.name} 활성화 상태를 {isActive}로 설정했습니다.");
        }
    }

    private bool IsPanelActive(GameObject panel)
    {
        return panel != null && panel.activeSelf;
    }

    private void SetUIElementsActive(List<GameObject> elements, bool isActive, GameObject excludeElement = null)
    {
        foreach (var element in elements)
        {
            if (element != null && element != excludeElement)
            {
                element.SetActive(isActive);
                Debug.Log($"{element.name} 활성화 상태를 {isActive}로 설정했습니다.");
            }
        }
    }

    public void OnGameOverButtonClicked()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        Debug.Log("게임 오버 버튼 클릭됨.");
        GameManager.Instance.Clear();
        LoadScene(titleSceneName);
    }

    private void LoadNextStage()
    {
        Debug.Log("다음 스테이지 로드 시작.");
        GameManager.Instance.ClearWin();

        if (allWavesCleared && !isGameOver) // 게임 오버가 아닐 때만 다음 스테이지 로드
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
            Debug.LogError("로드할 씬 이름이 비어 있습니다.");
            return;
        }

        Debug.Log($"씬 '{sceneName}' 로드 시작.");
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void OpenGameOverScene()
    {
        if (!string.IsNullOrEmpty(titleSceneName))
        {
            string sceneName = titleSceneName;
            Debug.Log($"게임 오버 씬 '{sceneName}' 로드 시작.");
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("게임 오버 씬 이름이 설정되지 않았습니다.");
        }
    }
}
