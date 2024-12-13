using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CheckNewGame : MonoBehaviour
{
    public Button Yes;
    public Button No;
    public GameObject Difficulty;

    private void Awake()
    {
        Yes.onClick.AddListener(NewGame);
        No.onClick.AddListener(Cancel);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
        }
    }

    private void NewGame()
    {

        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        GameManager.Instance.Clear();
        Difficulty.SetActive(true);
        gameObject.SetActive(false);
    }

    private void Cancel()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        gameObject.SetActive(false);
    }
}
