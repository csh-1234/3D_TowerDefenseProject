using UnityEngine;
using System.Collections;

public class LightningEffect : MonoBehaviour
{
    //일단은 shader graph material 쓰기 전까진 이걸로 처리함
    private LineRenderer lineRenderer;
    public int segments = 70;
    public float amplitude = 0.5f;
    public float duration = 0.2f;
    public Color lightningColor;
    public float startWidth = 0.7f;
    public float endWidth = 0.4f;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startColor = lightningColor;
        lineRenderer.endColor = lightningColor;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
    }   

    public void CreateLightning(Vector3 start, Vector3 end)
    {
        lineRenderer.positionCount = segments;
        Vector3[] positions = new Vector3[segments];
        
        for (int i = 0; i < segments; i++)
        {
            float progress = (float)i / (segments - 1);
            Vector3 position = Vector3.Lerp(start, end, progress);

            if (i != 0 && i != segments - 1)
            {
                position += Random.insideUnitSphere * amplitude;
            }
            
            positions[i] = position;
        }
        
        lineRenderer.SetPositions(positions);
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(duration);
        ObjectManager.Instance.Despawn(this);
    }

    private void Update()
    {
        if (lineRenderer != null)
        {
            Vector3[] positions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(positions);
            
            for (int i = 1; i < positions.Length - 1; i++)
            {
                positions[i] += Random.insideUnitSphere * (amplitude * 0.2f);
            }
            
            lineRenderer.SetPositions(positions);
        }
    }

    private void OnDisable()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }
} 