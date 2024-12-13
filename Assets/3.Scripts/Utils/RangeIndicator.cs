using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private float currentRange;

    [SerializeField]
    private float lineWidth = .7f;  // 선의 두께
    [SerializeField]
    private float heightOffset = 0.1f;  // 바닥으로부터의 높이

    public void Initialize(float range)
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        // LineRenderer 기본 설정
        lineRenderer.useWorldSpace = true;  // 월드 좌표계 사용
        lineRenderer.loop = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
            
        transform.localPosition = Vector3.up * heightOffset;  // 바닥보다 약간 위로
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
        Vector3 centerPosition = transform.parent.position;  // 부모(타워) 위치 기준

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

    // Preview 상태일 때 부드러운 업데이트를 위해 LateUpdate 추가
    private void LateUpdate()
    {
        if (transform.parent != null && transform.parent.GetComponent<Tower>()?.isPreview == true)
        {
            UpdatePosition();
        }
    }
}
