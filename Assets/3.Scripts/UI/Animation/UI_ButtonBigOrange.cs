using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class UI_ButtonBigOrange : MonoBehaviour
{
    private UI_EventHandler eventHandler;

    [SerializeField] private float addScale = 1.2f;
    [SerializeField] private float enterDuration = .1f;
    [SerializeField] private TextMeshProUGUI text;


    private Vector3 originalScale;

    private Tweener currentTween;
    private Sequence currentSequence;
    private Sequence currentSequence2;

    private Color targetColor = new Color32(255, 235, 181, 255);
    private Color baseColor = new Color32(255, 255, 255, 255);

    protected virtual void Awake()
    {
        eventHandler = GetComponent<UI_EventHandler>();
        originalScale = transform.localScale;
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
        SoundManager.Instance.Play("Click03", SoundManager.Sound.Effect);
        currentSequence?.Kill();
        currentSequence = DOTween.Sequence();
        currentSequence
            .Append(transform.DOScale(new Vector3(originalScale.x * addScale, originalScale.y * addScale, originalScale.z * addScale), enterDuration)
            .SetEase(Ease.Linear))
            .Join(text.DOColor(targetColor, 0.01f).SetEase(Ease.Linear))
            .SetUpdate(true); 
    }

    protected virtual void OnExit()
    {
        currentSequence?.Kill();
        currentSequence = DOTween.Sequence();
        currentSequence.Append(transform.DOScale(originalScale, enterDuration)
            .SetEase(Ease.Linear, 0.5f, 0.3f))
            .Join(text.DOColor(baseColor, 0.01f).SetEase(Ease.Linear))
            .SetUpdate(true); 
    }
}
