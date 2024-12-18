using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PreviewSystem : MonoBehaviour
{
    [SerializeField]
    private float previewYOffset = 0.06f;
    [SerializeField]
    private float previewAlpha = 0.5f;
    public Canvas MainCanvas;
    private GameObject MouseTooltip = null;
    public GameObject MouseTooltipPrefab;
    
    [SerializeField]
    private GameObject cellIndicator;
    private GameObject previewObject;

    private List<Material> previewMaterialsCopy = new List<Material>();
    private List<Material> originalMaterials = new List<Material>();
    private Renderer cellIndicatorRenderer;

    private GameObject cursorParent;
    private List<Vector2Int> currentOccupiedCells;
    private int currentRotation = 0;

    private void Start()
    {
        cellIndicator.SetActive(false);
        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        if (MouseTooltip != null) 
        {
            Vector2 mousePos = Input.mousePosition;
            mousePos.x += 100;
            mousePos.y -= 50;
            MouseTooltip.transform.position = mousePos;
        }
    }
    public void StartShowingPlacementPreview(GameObject prefab, List<Vector2Int> occupiedCells, int rotationIndex)
    {
        currentOccupiedCells = new List<Vector2Int>(occupiedCells);
        currentRotation = rotationIndex;
        previewObject = Instantiate(prefab);
        MouseTooltip = Instantiate(MouseTooltipPrefab,MainCanvas.transform);
        previewObject.name = prefab.name;

        Tower[] towers = previewObject.GetComponentsInChildren<Tower>();
        foreach (Tower tower in towers)
        {
            tower.isPreview = true;
        }

        PreparePreview(previewObject);
        PrepareCursor(currentOccupiedCells);
        cellIndicator.SetActive(true);
        UpdateRotation(currentRotation);
    }

    private void PrepareCursor(List<Vector2Int> occupiedCells)
    {
        if (occupiedCells.Count == 0)
            return;

        if (cursorParent == null)
        {
            cursorParent = new GameObject("Cursor Parent");
            cursorParent.transform.SetParent(transform);
        }

        foreach (Transform child in cursorParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var cell in occupiedCells)
        {
            GameObject newCell = Instantiate(cellIndicator, cursorParent.transform);
            newCell.transform.localPosition = new Vector3(cell.x, 0, cell.y);
            newCell.transform.localScale = Vector3.one; //  1x1 크
            newCell.SetActive(true);
        }
    }

    private void PreparePreview(GameObject previewObject)
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            originalMaterials.AddRange(renderer.materials);

            Material[] materialsCopy = new Material[renderer.materials.Length];
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                materialsCopy[i] = new Material(renderer.materials[i]);
                materialsCopy[i].EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                materialsCopy[i].SetFloat("_Surface", 1);
                materialsCopy[i].SetFloat("_Blend", 0);
                materialsCopy[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                materialsCopy[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                if (materialsCopy[i].color != null)
                {
                    Color color = materialsCopy[i].color;
                    color.a = previewAlpha;
                    materialsCopy[i].color = color;
                }
                else
                {
                    Color color = Color.white;
                    color.a = previewAlpha;
                    materialsCopy[i].color = color;
                }

                previewMaterialsCopy.Add(materialsCopy[i]);
            }
            renderer.materials = materialsCopy;
        }
    }

    public void StopShowingPreview()
    {
        if (cursorParent != null)
        {
            foreach (Transform child in cursorParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
        if (previewObject != null)
        {
            foreach (var material in previewMaterialsCopy)
            {
                Destroy(material);
            }
            previewMaterialsCopy.Clear();
            originalMaterials.Clear();

            Destroy(previewObject);
        }
        Destroy(MouseTooltip);
    }

    public void UpdatePosition(Vector3 position, bool validity, int floor = 0)
    {
        if (previewObject != null)
        {
            MovePreview(position, floor);
            ApplyFeedbackToPreview(validity);
        }

        MoveCursor(position, floor);
        ApplyFeedbackToCursor(validity);
    }

    private void ApplyFeedbackToPreview(bool validity)
    {
        foreach (var material in previewMaterialsCopy)
        {
            if(material.color != null)
            {
                Color color = validity ? originalMaterials[previewMaterialsCopy.IndexOf(material)].color : Color.red;
                color.a = previewAlpha;
                material.color = color;
            }
        }
    }

    private void ApplyFeedbackToCursor(bool validity)
    {
        Color c = validity ? Color.white : Color.red;
        c.a = 0.5f;
        cellIndicatorRenderer.material.color = c;
    }

    private void MoveCursor(Vector3 position, int floor)
    {
        if (cursorParent != null)
        {
            cursorParent.transform.position = new Vector3( position.x, position.y + floor,position.z);
        }
    }

    private void MovePreview(Vector3 position, int floor)
    {
        previewObject.transform.position = new Vector3(position.x + 0.5f, position.y + previewYOffset + floor, position.z + 0.5f);
    }

    internal void StartShowingRemovePreview()
    {
        cellIndicator.SetActive(true);
        PrepareCursor(new List<Vector2Int>() { Vector2Int.zero });
        ApplyFeedbackToCursor(false);
    }

    public void UpdateRotation(int rotationIndex)
    {
        SoundManager.Instance.Play("TowerPreviewRotationSound", SoundManager.Sound.Effect);
        currentRotation = rotationIndex;
        if (previewObject != null)
        {
            previewObject.transform.rotation = Quaternion.Euler(0, 90 * currentRotation, 0);
        }

        List<Vector2Int> rotatedCells = new List<Vector2Int>();
        foreach (var cell in currentOccupiedCells)
        {
            var rotated = cell;
            for (int i = 0; i < currentRotation; i++)
            {
                rotated = new Vector2Int(rotated.y, -rotated.x);
            }
            rotatedCells.Add(rotated);
        }

        if (cursorParent != null)
        {
            foreach (Transform child in cursorParent.transform)
            {
                Destroy(child.gameObject);
            }

            var currentPosition = cursorParent.transform.position;
            PrepareCursor(rotatedCells);
            cursorParent.transform.position = currentPosition;
        }
    }
}