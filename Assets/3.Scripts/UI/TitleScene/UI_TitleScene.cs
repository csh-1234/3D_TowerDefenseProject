using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_TitleScene : MonoBehaviour
{
    public Button NewGame;
    public Button Continue;
    public TextMeshProUGUI ContiText;
    public Button Exit;
    public GameObject CheckNewGame; 

    private AudioSource fireEffectSource; // "Fire" 효과음을 재생하는 AudioSource

    private void OnEnable()
    {
        NewGame.onClick.AddListener(LoadNewGame);
        Continue.onClick.AddListener(LoadContinue);
        Exit.onClick.AddListener(LoadExit);

        // 메인 테마곡
        SoundManager.Instance.Play("Bonfire", SoundManager.Sound.Bgm);

        // "Fire" 효과음을 재생하고 AudioSource를 저장
        fireEffectSource = PlayEffect("Fire");
    }

    private void Start()
    {
        if (!GameManager.Instance.IsSaved)
        {
            ContiText.color = Color.gray;
        }
    }

    private void LoadNewGame()
    {
        // 버튼 소리
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);

        // "Fire" 효과음 정지
        StopSpecificEffect();
        CheckNewGame.SetActive(true);
    }

    private void LoadContinue()
    {
        // "Fire" 효과음 정지 
        StopSpecificEffect();
        GameManager.Instance.ClearWin();
        if (GameManager.Instance.IsSaved)
        {
            SceneManager.LoadScene("LobbyScene");
        }

        // TODO : 불러오기
    }

    private void LoadExit()
    {
        // "Fire" 효과음 정지
        StopSpecificEffect();

        Application.Quit();
    }

    private AudioSource PlayEffect(string effectName)
    {
        // 임시 오브젝트에 AudioSource를 생성하고 효과음 재생
        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>($"Sounds/{effectName}");
        audioSource.loop = true; // 루프 설정
        audioSource.Play();

        // AudioSource가 끝난 후 오브젝트 제거
        Destroy(tempAudio, audioSource.clip.length);
        return audioSource;
    }

    private void StopSpecificEffect()
    {
        // 저장된 AudioSource가 있으면 정지
        if (fireEffectSource != null && fireEffectSource.isPlaying)
        {
            fireEffectSource.Stop();
            Destroy(fireEffectSource.gameObject); // AudioSource가 붙은 오브젝트 삭제
        }
    }
}
