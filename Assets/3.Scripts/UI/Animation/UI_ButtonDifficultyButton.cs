using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Image = UnityEngine.UI.Image;

public class UI_ButtonDifficultyButton : UI_EventHandler
{
    private UI_EventHandler eventHandler;
    [SerializeField] private Image focusImage;
    [SerializeField] private float stretchDuration = 0.2f;
    [SerializeField] private float stretchSacle = 1.1f;
    [SerializeField] private float StartTime = 0.2f;

    private Vector3 originalScale;
    private Sequence currentSequence;
    

    protected virtual void Awake()
    {
        eventHandler = GetComponent<UI_EventHandler>();
        originalScale = transform.localScale;
        
    }

    private void OnEnable()
    {
        OnActive();
        eventHandler.OnPointerEnterHandler += OnEnter;
        eventHandler.OnPointerExitHandler += OnExit;
    }

    private void OnDisable()
    {
        OnDeActive();
        eventHandler.OnPointerEnterHandler -= OnEnter;
        eventHandler.OnPointerExitHandler -= OnExit;
    }

    private void OnActive()
    {
        transform.localScale *= 0.8f;
        currentSequence?.Kill();
        currentSequence = DOTween.Sequence();
        currentSequence
            .Append(transform.DOScale(new Vector3(originalScale.x * stretchSacle, originalScale.y * stretchSacle, originalScale.z * stretchSacle), stretchDuration)
            .SetEase(Ease.Linear)).SetDelay(StartTime)
            .Append(transform.DOScale(new Vector3(originalScale.x / stretchSacle, originalScale.y / stretchSacle, originalScale.z / stretchSacle), stretchDuration)
            .SetEase(Ease.Linear));
    }
    private void OnDeActive()
    {
        //SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        transform.localScale = originalScale;
    }

    private void OnEnter()
    {
        //포커스 소리
        SoundManager.Instance.Play("Click03", SoundManager.Sound.Effect);
        currentSequence?.Kill();
        currentSequence = DOTween.Sequence();
        currentSequence
            .Append(transform.DOScale(new Vector3(originalScale.x * stretchSacle, originalScale.y * stretchSacle, originalScale.z * stretchSacle), 0.01f)
            .SetEase(Ease.Linear));
        focusImage.gameObject.SetActive(true);
    }

    private void OnExit()
    {
        currentSequence?.Kill();
        currentSequence = DOTween.Sequence();
        currentSequence.Append(transform.DOScale(originalScale, 0.01f)
            .SetEase(Ease.Linear));
        focusImage.gameObject.SetActive(false);
    }

}
