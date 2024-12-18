using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class Wave
{
    public List<MonsterSpawnData> monsterSpawnData;
    public float spawnInterval = 1f;
    public List<Transform> spawnPoints = new List<Transform>();
}

[System.Serializable]
public class SpeedSetting
{
    public float speedMultiplier;

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
        public GameObject monsterPrefab;
        public string monsterName;
        public int spawnCount;
    }
}

public class WaveManager : MonoBehaviour
{
    [SerializeField]
    private List<Wave> waves = new List<Wave>();

    [SerializeField]
    private WaveTextManager waveTextManager;

    public int currentWaveIndex = 0;

    [SerializeField]
    private int waveClearMoney = 30;

    [SerializeField]
    private MonsterSpawner monsterSpawner;

    [SerializeField]
    private List<Transform> spawnPoints;

    [SerializeField]
    private Transform targetPoint;

    [SerializeField]
    private Button battleButton;

    [SerializeField]
    private TextMeshProUGUI waveStatusText;

    [SerializeField]
    private TextMeshProUGUI wavePrepareText;

    [SerializeField]
    private TextMeshProUGUI wavePrepareTimeText;

    [SerializeField]
    private TextMeshProUGUI wavePrepareTimeText_1;

    [SerializeField]
    private Slider timeBar;

    [SerializeField]
    private GameObject speedButtonGroup;

    [SerializeField]
    private GameObject waveStartText;

    [SerializeField]
    private TextMeshProUGUI waveStartStateText;

    [SerializeField]
    private GameObject waveEndText;

    [SerializeField]
    private TextMeshProUGUI waveEndStateText;

    [SerializeField]
    private List<SpeedSetting> speedSettings = new List<SpeedSetting>();

    [SerializeField]
    private Color activeColor = Color.green;

    [SerializeField]
    private Color inactiveColor = Color.gray;

    [SerializeField]
    private float prepareCooldown = 10f;

    [SerializeField]
    private int remainingMonsters = 0;

    private StageManager stageManager; 
    private int totalMonstersToSpawn = 0;
    private int spawnedMonsters = 0;     

    private const int WaveClearDrawCount = 3;

    private bool isWaveActive = false;         
    private bool isReadyForNextWave = false;   
    private bool isGameOver = false;           
    public bool isFirstBattleClicked = false;     
    private Coroutine prepareCooldownCoroutine;   

    
    public event System.Action OnAllWavesCleared; 


    private void Start()
    {
        stageManager = FindObjectOfType<StageManager>();

        if (PathManager.Instance != null)
        {
            PathManager.Instance.SetupPoints(spawnPoints, targetPoint);
        }

        InitializeBattleButton();
        InitializeSpeedButtonGroup();
        InitializeWaveTextManager();
        InitializeUIState();
        UpdateSpeedImageColors(1f);

    }

    private void Update()
    {
        GameObject pausePanel = GameObject.Find("UI_PausePanel");
        if (pausePanel != null && pausePanel.activeSelf)
        {
            return;
        }

        if (GameManager.Instance.CurrentHp <= 0 && !isGameOver)
        {
            HandleGameOver();
            return; 
        }

        if (GameManager.Instance.CurrentHp > 0 && !isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space) && battleButton != null && battleButton.interactable)
            {
                battleButton.onClick.Invoke();
            }
        }

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
                        SoundManager.Instance.Play("SpeedChangeSound", SoundManager.Sound.Effect);
                    }
                });
            }
        }
    }

    private void InitializeWaveTextManager()
    {
        if (waveTextManager != null)
        {
            waveTextManager.Initialize(waves);
            waveTextManager.ShowWaveInfo();
        }
    }

    private void InitializeUIState()
    {
        if (wavePrepareTimeText != null) wavePrepareTimeText.gameObject.SetActive(false);
        if (wavePrepareTimeText_1 != null) wavePrepareTimeText_1.gameObject.SetActive(false);
        if (timeBar != null) timeBar.gameObject.SetActive(false);

        UpdateWaveText();
        UpdateWavePrepareText();
    }

    private void HandleSpeedInput()
    {
        if (Input.GetKeyDown(KeyCode.Z)) 
        {
            ChangeGameSpeed(0.5f);
            SoundManager.Instance.Play("SpeedChangeSound", SoundManager.Sound.Effect);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            ChangeGameSpeed(1f);
            SoundManager.Instance.Play("SpeedChangeSound", SoundManager.Sound.Effect);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeGameSpeed(2f);
            SoundManager.Instance.Play("SpeedChangeSound", SoundManager.Sound.Effect);
        }
        else if (Input.GetKeyDown(KeyCode.V)) 
        {
            ChangeGameSpeed(3f);
            SoundManager.Instance.Play("SpeedChangeSound", SoundManager.Sound.Effect);
        }
    }

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
            if (isWaveActive) 
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

            yield return null;
            elapsed += Time.deltaTime;
        }

        HideTimeBar();

        if (wavePrepareTimeText_1 != null) wavePrepareTimeText_1.gameObject.SetActive(false);
        if (wavePrepareText != null) wavePrepareText.gameObject.SetActive(false);
        if (wavePrepareTimeText != null) wavePrepareTimeText.gameObject.SetActive(false);

        StartNextWave();
    }

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

        monsterSpawner.OnMonsterSpawned += OnMonsterSpawned;
        monsterSpawner.OnMonsterDestroyed += OnMonsterDestroyed;
        monsterSpawner.OnSpawnComplete += OnSpawnComplete; 

        totalMonstersToSpawn = 0;
        foreach (var spawnData in currentWave.monsterSpawnData)
        {
            totalMonstersToSpawn += spawnData.spawnCount;
        }
        spawnedMonsters = 0;
        remainingMonsters = totalMonstersToSpawn; 

        monsterSpawner.StartSpawning();
        UpdateWaveText();
    }

    private void OnMonsterSpawned(GameObject monster)
    {
        spawnedMonsters++;
        Debug.Log($"Monster Spawned: {spawnedMonsters}, Remaining: {remainingMonsters}");
    }

    private void OnMonsterDestroyed(GameObject monster)
    {
        remainingMonsters--;

        if (stageManager != null && stageManager.IsGameOver)
        {
            if (remainingMonsters < 1)
            {
                remainingMonsters = 1;
            }
        }

        if (remainingMonsters <= 0 && monsterSpawner.IsSpawningComplete() &&
            GameManager.Instance.BossCount == 0)
        {
            EndCurrentWave();
        }
    }

    private void OnSpawnComplete()
    {
        if (spawnedMonsters >= totalMonstersToSpawn && remainingMonsters <= 0)
        {
            EndCurrentWave();
        }
    }

    private void EndCurrentWave()
    {
        monsterSpawner.OnMonsterSpawned -= OnMonsterSpawned;
        monsterSpawner.OnMonsterDestroyed -= OnMonsterDestroyed;
        monsterSpawner.OnSpawnComplete -= OnSpawnComplete;

        SoundManager.Instance.Play("Battlefield", SoundManager.Sound.Bgm);

        if (GameManager.Instance.HandTetrisList.Count < 10)
        {
            for (int i = 0; i < WaveClearDrawCount; i++)
            {
                UI_Draw.draw(); 
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

            waveTextManager?.SetCurrentWaveIndex(currentWaveIndex);
            waveTextManager?.ShowWaveInfo();
        }
        else
        {
            OnAllWavesCleared?.Invoke();
            HandleAllWavesCleared();
        }
    }

    private void HandleAllWavesCleared()
    {
        Time.timeScale = 1;
    }

    private void ChangeGameSpeed(float speed)
    {
        if (speed <= 0)
        {
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
