using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_AcademyPanelEffect : MonoBehaviour
{
    private UI_EventHandler eventHandler;
    [SerializeField]private GameObject baseFrame;
    [SerializeField] private GameObject focusFrame;
    protected virtual void Awake()
    {
        eventHandler = GetComponent<UI_EventHandler>();
    }
    protected void OnEnable()
    {
        eventHandler.OnPointerEnterHandler += OnEnter;
        eventHandler.OnPointerExitHandler += OnExit;
    }

    protected void OnDisable()
    {
        eventHandler.OnPointerEnterHandler -= OnEnter;
        eventHandler.OnPointerExitHandler -= OnExit;
    }

    protected virtual void OnEnter()
    {
        SoundManager.Instance.Play("AcademyFocusSound", SoundManager.Sound.Effect);
        focusFrame.gameObject.SetActive(true);
        baseFrame.gameObject.SetActive(false);
    }

    protected virtual void OnExit()
    {
        focusFrame.gameObject.SetActive(false);
        baseFrame.gameObject.SetActive(true);
    }
}
