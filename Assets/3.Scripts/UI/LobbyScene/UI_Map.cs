using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Map : MonoBehaviour
{
    public Button Academy;
    public Button Stage1;
    public Button Stage2;
    public Button Stage3;
    public Button Stage4;
    public Button Altar;
    public Button Stage6; // Boss

    public GameObject StageIndicator;
    private UI_StageIndicator indicatorComponent;
    private Button currentTargetButton;
    private Vector3 offset = new Vector3(0, 120f, 0);

    private void Awake()
    {
        Academy.onClick.AddListener(EnterAcademy);
        Stage1.onClick.AddListener(LoadStage1);
        Stage2.onClick.AddListener(LoadStage2);
        Stage3.onClick.AddListener(LoadStage3);
        Stage4.onClick.AddListener(LoadStage4);
        Altar.onClick.AddListener(EnterAltar);
        Stage6.onClick.AddListener(LoadStage6);

        // 인디케이터 컴포넌트 가져오기
        indicatorComponent = StageIndicator.GetComponent<UI_StageIndicator>();
        if (indicatorComponent == null)
        {
            indicatorComponent = StageIndicator.AddComponent<UI_StageIndicator>();
        }
    }

    private void Start()
    {
        int count = GameManager.Instance.clearStage;
        
        // 현재 타겟 버튼 설정
        switch (count)
        {
            case 0:
                currentTargetButton = Academy;
                break;
            case 1:
                currentTargetButton = Stage1;
                break;
            case 2:
                currentTargetButton = Stage2;
                break;
            case 3:
                currentTargetButton = Stage3;
                break;
            case 4:
                currentTargetButton = Stage4;
                break;
            case 5:
                currentTargetButton = Altar;
                break;
            case 6:
                currentTargetButton = Stage6;
                break;
        }

        // 초기 위치 설정
        if (currentTargetButton != null)
        {
            indicatorComponent.SetPosition(currentTargetButton.transform.position + offset);
        }
    }

    private void Update()
    {
        // 스크롤 시에도 인디케이터가 버튼을 따라다니도록 매 프레임 위치 업데이트
        if (currentTargetButton != null)
        {
            indicatorComponent.SetPosition(currentTargetButton.transform.position + offset);
        }
    }

    private void EnterAcademy()
    {
         SoundManager.Instance.Play("AcademyClickSound", SoundManager.Sound.Effect);
        if(GameManager.Instance.clearStage == 0)
        {
            FadeManager.Instance.LoadScene("AcademyScene");
            //SceneManager.LoadScene("AcademyScene");
        }
    }

    private void LoadStage1()
    {
        SoundManager.Instance.Play("StageClickSound", SoundManager.Sound.Effect);
        if (GameManager.Instance.clearStage == 1)
        {
            FadeManager.Instance.LoadScene("InGameScene1");
            //SceneManager.LoadScene("InGameScene3");
        }
    }
    private void LoadStage2()
    {
        SoundManager.Instance.Play("StageClickSound", SoundManager.Sound.Effect);
        if (GameManager.Instance.clearStage == 2)
        {
            FadeManager.Instance.LoadScene("InGameScene2");
            //SceneManager.LoadScene("InGameScene3");
        }
    }
    private void LoadStage3()
    {
        SoundManager.Instance.Play("StageClickSound", SoundManager.Sound.Effect);
        if (GameManager.Instance.clearStage == 3)
        {
            FadeManager.Instance.LoadScene("InGameScene3");
            //SceneManager.LoadScene("InGameScene3");
        }
    }
    private void LoadStage4()
    {
        SoundManager.Instance.Play("StageClickSound", SoundManager.Sound.Effect);
        if (GameManager.Instance.clearStage == 4)
        {
            FadeManager.Instance.LoadScene("InGameScene4");

            //SceneManager.LoadScene("InGameScene3");
        }
    }
    private void EnterAltar()
    {
        SoundManager.Instance.Play("StageClickSound", SoundManager.Sound.Effect);
        if (GameManager.Instance.clearStage == 5)
        {
            FadeManager.Instance.LoadScene("AltarScene");
            //SceneManager.LoadScene("InGameScene3");
        }
    }
    private void LoadStage6()
    {
        SoundManager.Instance.Play("BossStageClickSound", SoundManager.Sound.Effect);
        if (GameManager.Instance.clearStage == 6)
        {
            FadeManager.Instance.LoadScene("InGameScene5");
            //SceneManager.LoadScene("InGameScene3");
        }
    }
}
