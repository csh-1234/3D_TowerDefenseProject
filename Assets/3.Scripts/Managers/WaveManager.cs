using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class Wave
{
    [Tooltip("각 웨이브별 몬스터 스폰 데이터 리스트")]
    public List<MonsterSpawnData> monsterSpawnData;

    [Tooltip("몬스터 스폰 간격")]
    public float spawnInterval = 1f;

    [Tooltip("몬스터 스폰 위치 리스트")]
    public List<Transform> spawnPoints = new List<Transform>();
}

[System.Serializable]
public class SpeedSetting
{
    [Tooltip("배속 값 (예: 0.5x, 1x, 2x, 3x")]
    public float speedMultiplier;

    [Tooltip("해당 배속 상태일 색상을 적용할 아이콘들")]
    public List<Image> speedImages;

    private void Start()
    {
        speedMultiplier = GameManager.Instance.InGameSpeed;
    }
}

namespace MyGame
{
    [System.Serializable]
    public class MonsterSpawnData
    {
        [Tooltip("스폰할 몬스터의 프리팹")]
        public GameObject monsterPrefab;

        [Tooltip("몬스터의 이름")]
        public string monsterName;

        [Tooltip("해당 웨이브에서 스폰할 몬스터 수")]
        public int spawnCount;
    }
}

public class WaveManager : MonoBehaviour
{
    #region 웨이브 설정
    [Header("웨이브 설정")]
    [Tooltip("전체 웨이브 데이터를 저장하는 리스트")]
    [SerializeField]
    private List<Wave> waves = new List<Wave>();

    [Tooltip("웨이브 정보 표시를 관리하는 매니저")]
    [SerializeField]
    private WaveTextManager waveTextManager;

    [Tooltip("현재 웨이브 인덱스 (0부터 시작)")]
    public int currentWaveIndex = 0;

    [Tooltip("웨이브 클리어 시 지급할 머니")]
    [SerializeField]
    private int waveClearMoney = 30;
    #endregion

    #region 스포너 설정
    [Header("스포너 설정")]
    [Tooltip("몬스터 스포너")]
    [SerializeField]
    private MonsterSpawner monsterSpawner;

    [Tooltip("몬스터 스폰 위치 리스트")]
    [SerializeField]
    private List<Transform> spawnPoints;

    [Tooltip("몬스터의 목표 지점")]
    [SerializeField]
    private Transform targetPoint;
    #endregion

    #region UI 요소
    [Header("UI 요소")]
    [Tooltip("배틀 시작 버튼")]
    [SerializeField]
    private Button battleButton;

    [Space(10)]
    [Tooltip("현재 웨이브 상태 텍스트 (예: Wave 1/5)")]
    [SerializeField]
    private TextMeshProUGUI waveStatusText;

    [Tooltip("다음 웨이브 준비 텍스트")]
    [SerializeField]
    private TextMeshProUGUI wavePrepareText;

    [Tooltip("다음 웨이브까지 남은 시간 안내 텍스트")]
    [SerializeField]
    private TextMeshProUGUI wavePrepareTimeText;

    [Tooltip("남은 초 표시 텍스트")]
    [SerializeField]
    private TextMeshProUGUI wavePrepareTimeText_1;

    [Tooltip("웨이브 준비 시간 바")]
    [SerializeField]
    private Slider timeBar;

    [Tooltip("배속 버튼 그룹 오브젝트")]
    [SerializeField]
    private GameObject speedButtonGroup;

    [Space(10)]
    [Tooltip("웨이브 시작 텍스트")]
    [SerializeField]
    private GameObject waveStartText;

    [Tooltip("웨이브 시작 상세 텍스트")]
    [SerializeField]
    private TextMeshProUGUI waveStartStateText;

    [Tooltip("웨이브 클리어 텍스트")]
    [SerializeField]
    private GameObject waveEndText;

    [Tooltip("웨이브 클리어 상세 텍스트")]
    [SerializeField]
    private TextMeshProUGUI waveEndStateText;
    #endregion

    #region 배속 상태일 때 UI에서 색상을 변경
    [Header("현재 배속 상태에 따른 아이콘 색상 변경")]
    [SerializeField]
    private List<SpeedSetting> speedSettings = new List<SpeedSetting>();

    [Tooltip("진행중o 배속 아이콘 색상")]
    [SerializeField]
    private Color activeColor = Color.green;

    [Tooltip("진행중x 배속 아이콘 색상")]
    [SerializeField]
    private Color inactiveColor = Color.gray;
    #endregion

    #region 기타 설정
    [Header("기타 설정")]
    [Tooltip("웨이브 시작 전 대기 시간(쿨다운)")]
    [SerializeField]
    private float prepareCooldown = 10f;

    [Tooltip("남은 몬스터 수")]
    [SerializeField]
    private int remainingMonsters = 0;
    #endregion

    #region 내부 변수
    private StageManager stageManager; // StageManager를 참조하기 위한 변수
    // 내부적으로 사용되는 변수들은 인스펙터에 노출하지 않기 위해 SerializeField를 제거
    private int totalMonstersToSpawn = 0; // 현재 웨이브에서 총 스폰할 몬스터 수
    private int spawnedMonsters = 0;      // 현재까지 스폰된 몬스터 수

    private const int WaveClearDrawCount = 3; // 매직 넘버 상수화

    private bool isWaveActive = false;                // 현재 웨이브 진행 중 여부, 웨이브 활성화 상태인지 알기 위한 개념
    private bool isReadyForNextWave = false;          // 다음 웨이브 준비 상태 여부
    private bool isGameOver = false;                  // 게임 오버 여부
    public bool isFirstBattleClicked = false;        // 처음 배틀 시작을 눌렀는지 여부
    private Coroutine prepareCooldownCoroutine;        // 준비 시간 코루틴 참조
    #endregion

    #region 이벤트
    public event System.Action OnAllWavesCleared;     // 모든 웨이브 클리어 시 발생하는 이벤트
    #endregion


    private void Start()
    {
        // StageManager 인스턴스 찾기
        stageManager = FindObjectOfType<StageManager>();
        if (stageManager == null)
        {
            Debug.LogError("StageManager를 찾을 수 없습니다! 게임 오버 상태 확인 불가.");
        }

        // PathManager 초기화
        if (PathManager.Instance != null)
        {
            PathManager.Instance.SetupPoints(spawnPoints, targetPoint);
        }
        else
        {
            Debug.LogError("PathManager 인스턴스가 존재하지 않습니다!");
        }

        InitializeBattleButton();
        InitializeSpeedButtonGroup();
        InitializeWaveTextManager();
        InitializeUIState();
        UpdateSpeedImageColors(1f); // 초기 배속 1배속 설정

    }

    private void Update()
    {
        // UI_PausePanel 활성화 여부 확인
        GameObject pausePanel = GameObject.Find("UI_PausePanel");
        if (pausePanel != null && pausePanel.activeSelf)
        {
            // UI_PausePanel이 활성화된 경우 입력 처리 중단
            return;
        }

        // 게임 오버 체크
        if (GameManager.Instance.CurrentHp <= 0 && !isGameOver)
        {
            HandleGameOver();
            return; // 게임오버 처리 후 아래 로직 불필요
        }

        // HP가 0보다 크고 게임 오버 상태가 아닐 때 스페이스바 입력 처리
        if (GameManager.Instance.CurrentHp > 0 && !isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space) && battleButton != null && battleButton.interactable)
            {
                battleButton.onClick.Invoke();
            }
        }

        // 웨이브 진행 중일 때만 배속(Z, X, C, V) 키 입력 처리
        if (isWaveActive)
        {
            HandleSpeedInput();
        }
    }

    private void InitializeBattleButton()
    {
        if (battleButton != null)
        {
            battleButton.onClick.AddListener(StartNextWaveManually);
            SetBattleButtonState(true);
        }
        else
        {
            Debug.LogError("BattleButton이 할당되지 않았습니다!");
        }
    }

    private void InitializeSpeedButtonGroup()
    {
        if (speedButtonGroup != null)
        {
            speedButtonGroup.SetActive(false);
            foreach (var button in speedButtonGroup.GetComponentsInChildren<Button>())
            {
                button.onClick.AddListener(() =>
                {
                    if (float.TryParse(button.name, out float speed))
                    {
                        ChangeGameSpeed(speed);
                        // 버튼 클릭 시 소리 재생
                        SoundManager.Instance.Play("SpeedChangeSound", SoundManager.Sound.Effect);
                    }
                    else
                    {
                        Debug.LogError($"배속 값 파싱 실패: {button.name}");
                    }
                });
            }
        }
        else
        {
            Debug.LogError("SpeedButtonGroup이 할당되지 않았습니다!");
        }
    }

    private void InitializeWaveTextManager()
    {
        if (waveTextManager != null)
        {
            waveTextManager.Initialize(waves);
            waveTextManager.ShowWaveInfo();
        }
        else
        {
            Debug.LogError("WaveTextManager가 할당되지 않았습니다!");
        }
    }

    private void InitializeUIState()
    {
        // 웨이브 준비 시간 관련 UI 비활성화
        if (wavePrepareTimeText != null) wavePrepareTimeText.gameObject.SetActive(false);
        if (wavePrepareTimeText_1 != null) wavePrepareTimeText_1.gameObject.SetActive(false);
        if (timeBar != null) timeBar.gameObject.SetActive(false);

        UpdateWaveText();
        UpdateWavePrepareText();
    }

    // 키보드 입력(Z, X, C, V)에 따른 배속 변경 처리
    private void HandleSpeedInput()
    {
        if (Input.GetKeyDown(KeyCode.Z)) // Z키: 배속 0.5x
        {
            ChangeGameSpeed(0.5f);
            SoundManager.Instance.Play("SpeedChangeSound", SoundManager.Sound.Effect);
        }
        else if (Input.GetKeyDown(KeyCode.X)) // X키: 배속 1x
        {
            ChangeGameSpeed(1f);
            SoundManager.Instance.Play("SpeedChangeSound", SoundManager.Sound.Effect);
        }
        else if (Input.GetKeyDown(KeyCode.C)) // C키: 배속 2x
        {
            ChangeGameSpeed(2f);
            SoundManager.Instance.Play("SpeedChangeSound", SoundManager.Sound.Effect);
        }
        else if (Input.GetKeyDown(KeyCode.V)) // V키: 배속 3x
        {
            ChangeGameSpeed(3f);
            SoundManager.Instance.Play("SpeedChangeSound", SoundManager.Sound.Effect);
        }
    }

    // 준비 시간 카운트다운 코루틴
    private IEnumerator PrepareCooldownRoutine()
    {
        float elapsed = 0f;

        SetBattleButtonState(true);

        if (!isFirstBattleClicked && wavePrepareText != null)
        {
            wavePrepareText.gameObject.SetActive(true);
        }

        if (wavePrepareTimeText != null)
        {
            wavePrepareTimeText.gameObject.SetActive(true);
            wavePrepareTimeText.text = "NEXT WAVE IN";
        }

        if (timeBar != null)
        {
            timeBar.gameObject.SetActive(true);
            timeBar.value = 0f;
        }

        while (elapsed < prepareCooldown)
        {
            if (isWaveActive) // 중간에 웨이브 시작 시 즉시 종료
            {
                HideTimeBar();
                yield break;
            }

            if (wavePrepareTimeText_1 != null)
            {
                wavePrepareTimeText_1.text = $"{Mathf.CeilToInt(prepareCooldown - elapsed)}";
                wavePrepareTimeText_1.gameObject.SetActive(true);
            }

            if (timeBar != null)
            {
                timeBar.value = elapsed / prepareCooldown;
            }

            yield return null; // 매 프레임마다 업데이트
            elapsed += Time.deltaTime;
        }

        HideTimeBar();

        if (wavePrepareTimeText_1 != null) wavePrepareTimeText_1.gameObject.SetActive(false);
        if (wavePrepareText != null) wavePrepareText.gameObject.SetActive(false);
        if (wavePrepareTimeText != null) wavePrepareTimeText.gameObject.SetActive(false);

        StartNextWave();
    }

    // 배틀 버튼으로 다음 웨이브 강제 시작
    public void StartNextWaveManually()
    {
        if (isWaveActive || currentWaveIndex >= waves.Count) return;

        SoundManager.Instance.Play("BattleButton", SoundManager.Sound.Effect);
        StartCoroutine(ShowWaveStateText(waveStartText));

        if (!isFirstBattleClicked)
        {
            isFirstBattleClicked = true;
            if (wavePrepareText != null) wavePrepareText.gameObject.SetActive(false);
        }

        if (prepareCooldownCoroutine != null)
        {
            StopCoroutine(prepareCooldownCoroutine);
            HideTimeBar();
            prepareCooldownCoroutine = null;
        }

        if (wavePrepareTimeText_1 != null) wavePrepareTimeText_1.gameObject.SetActive(false);
        if (wavePrepareTimeText != null) wavePrepareTimeText.gameObject.SetActive(false);

        StartNextWave();
    }

    private void StartNextWave()
    {
        if (isWaveActive || currentWaveIndex >= waves.Count) return;
        if (monsterSpawner == null)
        {
            Debug.LogError("MonsterSpawner가 할당되지 않았습니다!");
            return;
        }

        SoundManager.Instance.Play("InWave", SoundManager.Sound.Bgm);
        isWaveActive = true;
        isReadyForNextWave = false;
        SetBattleButtonState(false);

        if (speedButtonGroup != null) speedButtonGroup.SetActive(true);

        waveTextManager?.HideWaveInfo();

        var currentWave = waves[currentWaveIndex];
        monsterSpawner.SetMonsterData(currentWave.monsterSpawnData, currentWave.spawnInterval, currentWave.spawnPoints);

        // 이벤트 등록
        monsterSpawner.OnMonsterSpawned += OnMonsterSpawned;
        monsterSpawner.OnMonsterDestroyed += OnMonsterDestroyed;
        monsterSpawner.OnSpawnComplete += OnSpawnComplete; // 추가된 이벤트

        // 총 몬스터 수 계산
        totalMonstersToSpawn = 0;
        foreach (var spawnData in currentWave.monsterSpawnData)
        {
            totalMonstersToSpawn += spawnData.spawnCount;
        }
        spawnedMonsters = 0;
        remainingMonsters = totalMonstersToSpawn; // 변경된 부분: 전체 몬스터 수로 초기화

        monsterSpawner.StartSpawning();
        UpdateWaveText();
    }

    private void OnMonsterSpawned(GameObject monster)
    {
        spawnedMonsters++;
        // remainingMonsters++; // 이 줄을 주석 처리하거나 제거합니다.
        Debug.Log($"Monster Spawned: {spawnedMonsters}, Remaining: {remainingMonsters}");
    }

    private void OnMonsterDestroyed(GameObject monster)
    {
        remainingMonsters--;

        // StageManager를 참조하고 있다고 가정 (stageManager 변수 필요)
        // 게임 오버 상태인 경우에만 1 미만으로 내려가지 않도록 처리
        if (stageManager != null && stageManager.IsGameOver)
        {
            if (remainingMonsters < 1)
            {
                remainingMonsters = 1;
            }
        }

        Debug.Log($"Monster Destroyed: Remaining: {remainingMonsters}");

        if (remainingMonsters <= 0 && monsterSpawner.IsSpawningComplete() &&
            GameManager.Instance.BossCount == 0)
        {
            EndCurrentWave();
        }
    }

    private void OnSpawnComplete()
    {
        // 모든 몬스터가 스폰된 상태를 표시
        if (spawnedMonsters >= totalMonstersToSpawn && remainingMonsters <= 0)
        {
            EndCurrentWave();
        }
    }

    private void EndCurrentWave()
    {
        // 이벤트 해제
        monsterSpawner.OnMonsterSpawned -= OnMonsterSpawned;
        monsterSpawner.OnMonsterDestroyed -= OnMonsterDestroyed;
        monsterSpawner.OnSpawnComplete -= OnSpawnComplete;

        SoundManager.Instance.Play("Battlefield", SoundManager.Sound.Bgm);

        // 웨이브 클리어 처리
        if (GameManager.Instance.HandTetrisList.Count < 10)
        {
            for (int i = 0; i < WaveClearDrawCount; i++)
            {
                UI_Draw.draw(); // WaveClearDrawCount 만큼 효과 실행
                if (GameManager.Instance.HandTetrisList.Count >= 10) break;
            }
        }

        GameManager.Instance.CurrentMoney += waveClearMoney;
        waveEndStateText.text = $"Bonus Reward : {waveClearMoney}";
        waveClearMoney += 10;

        isWaveActive = false;
        currentWaveIndex++;
        waveStartStateText.text = $"-WAVE {currentWaveIndex + 1}-";

        if (speedButtonGroup != null) speedButtonGroup.SetActive(false);

        if (currentWaveIndex < waves.Count)
        {
            SoundManager.Instance.Play("WaveClearEffect", SoundManager.Sound.Effect);
            StartCoroutine(ShowWaveStateText(waveEndText));
            isReadyForNextWave = true;
            UpdateWaveText();
            UpdateWavePrepareText();

            if (currentWaveIndex != 0)
            {
                prepareCooldownCoroutine = StartCoroutine(PrepareCooldownRoutine());
            }

            // 다음 웨이브 준비를 위한 UI 업데이트
            waveTextManager?.SetCurrentWaveIndex(currentWaveIndex);
            waveTextManager?.ShowWaveInfo();
        }
        else
        {
            // 모든 웨이브 완료 시 이벤트 호출
            OnAllWavesCleared?.Invoke();
            HandleAllWavesCleared();
        }
    }

    private void HandleAllWavesCleared()
    {
        // 게임 클리어 처리 UI 표시
        Debug.Log("All waves cleared! Game Cleared!");
        Time.timeScale = 1;
    }

    private void ChangeGameSpeed(float speed)
    {
        if (speed <= 0)
        {
            Debug.LogError("배속 값은 0보다 커야 합니다!");
            return;
        }
        GameManager.Instance.InGameSpeed = speed;
        Time.timeScale = GameManager.Instance.InGameSpeed;
        UpdateSpeedImageColors(speed);
    }

    private void UpdateSpeedImageColors(float activeSpeed)
    {
        foreach (var setting in speedSettings)
        {
            if (setting.speedImages != null)
            {
                foreach (var speedImage in setting.speedImages)
                {
                    speedImage.color = Mathf.Approximately(setting.speedMultiplier, activeSpeed)
                        ? activeColor
                        : inactiveColor;
                }
            }
        }
    }

    private void HideTimeBar()
    {
        if (timeBar != null) timeBar.gameObject.SetActive(false);
    }

    private void SetBattleButtonState(bool isActive)
    {
        if (battleButton != null) battleButton.gameObject.SetActive(isActive);
    }

    private void UpdateWaveText()
    {
        if (waveStatusText != null)
        {
            if (currentWaveIndex == waves.Count - 1)
            {
                waveStatusText.text = "Final Wave";
            }
            else
            {
                waveStatusText.text = $"Wave {currentWaveIndex + 1} / {waves.Count}";
            }
        }
    }

    private void UpdateWavePrepareText()
    {
        if (wavePrepareText != null)
        {
            wavePrepareText.text = $"Prepare for {currentWaveIndex + 1}st wave!";
        }
    }

    private void HandleGameOver()
    {
        isGameOver = true;
        isWaveActive = false;
        isReadyForNextWave = false;
        SetBattleButtonState(false);
        if (speedButtonGroup != null) speedButtonGroup.SetActive(false);
        Time.timeScale = 1; 
    }

    private void OnAllWavesClearedHandler()
    {
        // 추가적인 게임 클리어 로직 구현 가능
        HandleAllWavesCleared();
    }

    private void OnEnable()
    {
        OnAllWavesCleared += OnAllWavesClearedHandler;
    }

    private void OnDisable()
    {
        OnAllWavesCleared -= OnAllWavesClearedHandler;
    }

    private IEnumerator ShowWaveStateText(GameObject go)
    {
        // 게임 오버 상태라면 텍스트를 표시하지 않음
        if (GameManager.Instance.CurrentHp <= 0)
        {
            yield break;
        }

        go.SetActive(true);
        yield return new WaitForSeconds(2f);
        go.SetActive(false);
        yield return null;
    }
}
