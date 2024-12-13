using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_ButtonTest : MonoBehaviour
{
    private UI_EventHandler eventHandler;

    [SerializeField] private float pressDuration = 0.3f;
    [SerializeField] private float releaseDuration = 2f;
    [SerializeField] private float squishYScale = 1.4f;
    [SerializeField] private float stretchXScale = 0.8f;

    private Vector3 originalScale;
    private Tweener currentTween;

    protected virtual void Awake()
    {
        eventHandler = GetComponent<UI_EventHandler>();
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        eventHandler.OnPointerDownHandler += OnPress;
        eventHandler.OnPointerUpHandler += OnRelease;
    }

    private void OnDisable()
    {
        eventHandler.OnPointerDownHandler -= OnPress;
        eventHandler.OnPointerUpHandler -= OnRelease;
    }

    private void OnPress()
    {
        currentTween?.Kill();
        currentTween = transform.DOScale(new Vector3(originalScale.x * stretchXScale, originalScale.y * squishYScale, originalScale.z), pressDuration)
            .SetEase(Ease.OutQuad);
    }

    private void OnRelease()
    {
        currentTween?.Kill();
        currentTween = transform.DOScale(originalScale, releaseDuration)
            .SetEase(Ease.OutElastic, 0.5f, 0.3f);
    }

}