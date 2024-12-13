using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Rendering;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance; // 싱글톤 패턴
    private Image fadeImage; // Canvas 하위 Image

    public float fadeDuration = 1.0f; // 페이드 지속 시간
    public int canvasSortOrder = 100; // 페이드 캔버스의 Sort Order

    private Coroutine CoFade;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 삭제되지 않도록 설정
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 하위 Canvas 및 Image 자동 설정
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("FadeManager 하위에 Canvas가 필요합니다!");
            return;
        }

        // Canvas의 Sort Order 설정
        canvas.sortingOrder = canvasSortOrder;

        fadeImage = canvas.GetComponentInChildren<Image>();
        if (fadeImage == null)
        {
            Debug.LogError("Canvas 하위에 Image가 필요합니다!");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드 이벤트 등록
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 씬 로드 이벤트 해제
    }

    private void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 1);
            StartCoroutine(FadeIn());
        }
    }

    public void LoadScene(string sceneName)
    {
        if(CoFade == null)
        {
            Debug.Log("코루틴 실행됨");
            CoFade = StartCoroutine(FadeOutBeforeSceneChange(sceneName));
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 1);
            StartCoroutine(FadeIn()); // 새로운 씬이 로드된 후 페이드 인 실행
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0.0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            fadeImage.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 0;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }

    public IEnumerator FadeOutBeforeSceneChange(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0.0f;
        Color color = fadeImage.color;

        // 페이드 아웃
        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            fadeImage.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 1;
        fadeImage.color = color;

        // 씬 전환
        CoFade = null;
        SceneManager.LoadScene(sceneName);
    }
}
