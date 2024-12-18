using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private float currentRange;

    [SerializeField]
    private float lineWidth = .7f; 
    [SerializeField]
    private float heightOffset = 0.1f; 

    public void Initialize(float range)
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        lineRenderer.useWorldSpace = true; 
        lineRenderer.loop = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
            
        transform.localPosition = Vector3.up * heightOffset; 
        UpdateRange(range);
    }

    public void UpdateRange(float newRange)
    {
        currentRange = newRange;
        DrawCircle(100, currentRange);
    }

    private void DrawCircle(int steps, float radius)
    {
        lineRenderer.positionCount = steps;
        Vector3 centerPosition = transform.parent.position;  

        float angleStep = 360f / steps;
        for (int i = 0; i < steps; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                centerPosition.x + Mathf.Cos(angle) * radius,
                centerPosition.y + heightOffset,
                centerPosition.z + Mathf.Sin(angle) * radius
            );
            lineRenderer.SetPosition(i, position);
        }
    }

    public void UpdatePosition()
    {
        if (lineRenderer != null && transform.parent != null)
        {
            DrawCircle(lineRenderer.positionCount, currentRange);
        }
    }
    private void LateUpdate()
    {
        if (transform.parent != null && transform.parent.GetComponent<Tower>()?.isPreview == true)
        {
            UpdatePosition();
        }
    }
}
