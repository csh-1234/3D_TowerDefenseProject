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

    private AudioSource fireEffectSource; 

    private void OnEnable()
    {
        NewGame.onClick.AddListener(LoadNewGame);
        Continue.onClick.AddListener(LoadContinue);
        Exit.onClick.AddListener(LoadExit);

        SoundManager.Instance.Play("Bonfire", SoundManager.Sound.Bgm);

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
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        StopSpecificEffect();
        CheckNewGame.SetActive(true);
    }

    private void LoadContinue()
    {
        StopSpecificEffect();
        GameManager.Instance.ClearWin();
        if (GameManager.Instance.IsSaved)
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }

    private void LoadExit()
    {
        StopSpecificEffect();
        Application.Quit();
    }

    private AudioSource PlayEffect(string effectName)
    {
        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>($"Sounds/{effectName}");
        audioSource.loop = true;
        audioSource.Play();

        Destroy(tempAudio, audioSource.clip.length);
        return audioSource;
    }

    private void StopSpecificEffect()
    {
        if (fireEffectSource != null && fireEffectSource.isPlaying)
        {
            fireEffectSource.Stop();
            Destroy(fireEffectSource.gameObject); 
        }
    }
}
