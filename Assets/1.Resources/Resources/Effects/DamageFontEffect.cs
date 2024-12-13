using System.Collections;
using TMPro;
using UnityEngine;

public class DamageFontEffect : MonoBehaviour
{
    public float moveSpeed = .1f;
    public float fadeSpeed = .1f;
    private TextMeshProUGUI text;
    private Transform originalParent;
    private bool isBeingDespawned = false;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        originalParent = transform.parent;
    }

    public void SetDamageText(string damageText, Vector3 worldPosition)
    {
        isBeingDespawned = false;
        transform.position = worldPosition;
        transform.rotation = Camera.main.transform.rotation;
        
        text.text = damageText;
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
        StartCoroutine(FadeOut());
    }

    void Update()
    {
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        transform.rotation = Camera.main.transform.rotation;
    }

    IEnumerator FadeOut()
    {
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }
        
        if (!isBeingDespawned)
        {
            isBeingDespawned = true;
            ObjectManager.Instance.Despawn(this);
        }
    }

    private void OnDisable()
    {
        if (!isBeingDespawned)
        {
            if (text != null)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
            }
            transform.SetParent(originalParent, false);
        }
    }
}