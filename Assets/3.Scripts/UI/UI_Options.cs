using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Options : MonoBehaviour
{
    public Slider MasterVolume;
    public Slider SoundVolume;
    public Slider MusicVolume;
    public TextMeshProUGUI MasterVolumeRatio;
    public TextMeshProUGUI SoundVolumeRatio;
    public TextMeshProUGUI MusicVolumeRatio;
    public Button Check;

    private void Awake()
    {
        MasterVolume.value = SoundManager.Instance.getMasterVolume() * 100;
        SoundVolume.value = SoundManager.Instance.getEffectVolume() * 100;
        MusicVolume.value = SoundManager.Instance.getBgmVolume() * 100;
        Check.onClick.AddListener(Confirm);
    }

    private void Update()
    {
        MasterVolumeRatio.text = $"{(int)MasterVolume.value}%";
        SoundVolumeRatio.text = $"{(int)SoundVolume.value}%";
        MusicVolumeRatio.text = $"{(int)MusicVolume.value}%";

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }

        SoundManager.Instance.SetMasterVolume(MasterVolume.value);
        SoundManager.Instance.SetEffectVolume(SoundVolume.value);
        SoundManager.Instance.SetBgmVolume(MusicVolume.value);
    }

    private void Confirm()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        gameObject.SetActive(false);
    }
}
