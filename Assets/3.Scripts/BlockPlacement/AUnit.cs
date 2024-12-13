using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AUnit : MonoBehaviour
{
    public Transform target;
    private Vector3[] path;
    private Vector3[] previewPath;
    private LineRenderer actualPathRenderer;
    private LineRenderer previewPathRenderer;
    
    [SerializeField]
    private Material actualPathMaterial;  // 실제 경로용 머티리얼
    [SerializeField]
    private Material previewPathMaterial; // 프리뷰 경로용 머티리얼
    [SerializeField]
    private Color actualPathColor = Color.black;
    [SerializeField]
    private Color previewPathColor = new Color(1f, 0.5f, 0f, 0.5f);

    public event Action<Vector3[], bool> OnPathUpdated;

    void Awake()
    {
        InitializeLineRenderers();
    }

    public void Clear()
    {
        path = null;
        previewPath = null;
        if (actualPathRenderer != null)
        {
            actualPathRenderer.positionCount = 0;
        }
        if (previewPathRenderer != null)
        {
            previewPathRenderer.positionCount = 0;
        }
        OnPathUpdated = null;
    }

    private void InitializeLineRenderers()
    {
        // 실제 경로용 LineRenderer
        if (actualPathRenderer == null)
        {
            GameObject actualPathObj = new GameObject("ActualPathRenderer");
            actualPathObj.transform.SetParent(transform);
            actualPathRenderer = actualPathObj.AddComponent<LineRenderer>();
            SetupLineRenderer(actualPathRenderer, actualPathColor, actualPathMaterial);
        }

        // 프리뷰 경로용 LineRenderer
        if (previewPathRenderer == null)
        {
            GameObject previewObj = new GameObject("PreviewPathRenderer");
            previewObj.transform.SetParent(transform);
            previewPathRenderer = previewObj.AddComponent<LineRenderer>();
            SetupLineRenderer(previewPathRenderer, previewPathColor, previewPathMaterial);
        }

        ShowPreviewPath(false);
        ShowActualPath(true);
    }

    private void SetupLineRenderer(LineRenderer renderer, Color color, Material material)
    {
        if (renderer != null)
        {
            renderer.startWidth = 0.15f;
            renderer.endWidth = 0.15f;
            
            // 머티리얼이 할당되지 않은 경우에만 기본 머티리얼 생성
            if (material == null)
            {
                material = new Material(Shader.Find("Sprites/Default"));
            }
            
            renderer.material = new Material(material);
            renderer.material.color = color;
            renderer.startColor = color;
            renderer.endColor = color;
            renderer.positionCount = 0;
            renderer.enabled = true;
            renderer.textureMode = LineTextureMode.Tile;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.allowOcclusionWhenDynamic = false;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
            UpdatePath();
    }

    public void UpdatePath()
    {
        if (target == null) return;
        ShowPreviewPath(false);
        ShowActualPath(true);
        PathRequestManager.RequestPath(transform.position, target.position, 
            (path, success) => HandleActualPathFound(path, success));
    }

    private void HandleActualPathFound(Vector3[] path, bool success)
    {
        if (success)
        {
            this.path = path;
            DrawPath(this.path, actualPathRenderer, actualPathColor);
            previewPathRenderer.positionCount = 0;
            OnPathUpdated?.Invoke(this.path, true);

            if (PathManager.Instance != null)
            {
                PathManager.Instance.OnPathCalculated(path, true, false);
            }
        }
        else
        {
            this.path = null;
            actualPathRenderer.positionCount = 0;

            if (PathManager.Instance != null)
            {
                PathManager.Instance.OnPathCalculated(null, false, false);
            }
            OnPathUpdated?.Invoke(null, false);
        }
    }

    public void CheckPreviewPath()
    {
        if (target == null) return;
        ShowPreviewPath(true);
        ShowActualPath(true);
        PathRequestManager.RequestPath(transform.position, target.position, 
            (path, success) => HandlePreviewPathFound(path, success));
    }

    public void HandlePreviewPathFound(Vector3[] path, bool success)
    {
        if (success)
        {
            previewPath = path;
            DrawPath(previewPath, previewPathRenderer, previewPathColor);
            OnPathUpdated?.Invoke(previewPath, true);

            if (PathManager.Instance != null)
            {
                PathManager.Instance.OnPathCalculated(path, true, true);
            }
        }
        else
        {
            previewPath = null;
            previewPathRenderer.positionCount = 0;

            if (PathManager.Instance != null)
            {
                PathManager.Instance.OnPathCalculated(null, false, true);
            }
            OnPathUpdated?.Invoke(null, false);
        }
    }

    private void DrawPath(Vector3[] pathToDraw, LineRenderer renderer, Color color)
    {
        if (pathToDraw == null || pathToDraw.Length == 0) 
        {
            renderer.positionCount = 0;
            return;
        }

        Vector3[] points = new Vector3[pathToDraw.Length + 2];
        points[0] = transform.position;
        for (int i = 0; i < pathToDraw.Length; i++)
        {
            points[i + 1] = pathToDraw[i];
        }
        points[points.Length - 1] = target.position;

        float pathHeight = transform.position.y;
        for (int i = 0; i < points.Length; i++)
        {
            points[i].y = pathHeight;
        }

        renderer.material.color = color;
        renderer.startColor = color;
        renderer.endColor = color;
        renderer.positionCount = points.Length;
        renderer.SetPositions(points);
    }

    public void ShowPreviewPath(bool show)
    {
        previewPathRenderer.enabled = show;
        if (!show)
        {
            previewPathRenderer.positionCount = 0;
        }
    }

    public void ShowActualPath(bool show)
    {
        actualPathRenderer.enabled = show;
        if (!show)
        {
            actualPathRenderer.positionCount = 0;
        }
    }

    public void ClearAllPaths()
    {
        actualPathRenderer.positionCount = 0;
        previewPathRenderer.positionCount = 0;
        path = null;
        previewPath = null;
    }

    public Vector3[] GetCurrentPath()
    {
        return path;
    }

    public void SetLineMaterials(Material actualMat, Material previewMat)
    {
        if (actualMat != null && actualPathRenderer != null)
        {
            actualPathMaterial = actualMat;
            actualPathRenderer.material = new Material(actualMat);
            actualPathRenderer.material.color = actualPathColor;
        }

        if (previewMat != null && previewPathRenderer != null)
        {
            previewPathMaterial = previewMat;
            previewPathRenderer.material = new Material(previewMat);
            previewPathRenderer.material.color = previewPathColor;
        }
    }

    public void SetLineMaterial(Material material)
    {
        SetLineMaterials(material, material);
    }
}
