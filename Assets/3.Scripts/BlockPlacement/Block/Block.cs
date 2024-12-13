using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Block : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    protected Button button;
    protected int blockType;
    protected string uniqueID;
    private bool isSelected = false;

    protected virtual void Awake()
    {
        button = GetComponent<Button>();
        uniqueID = System.Guid.NewGuid().ToString();
    }   

    protected virtual void Start()
    {
        button.onClick.AddListener(Placement);
        PlacementSystem.Instance.OnPlacementSuccess += HandlePlacementSuccess;
    }

    protected virtual void OnDestroy()
    {
        if (PlacementSystem.Instance != null)
        {
            PlacementSystem.Instance.OnPlacementSuccess -= HandlePlacementSuccess;
            GameManager.Instance.HandTetrisList.Remove(this.gameObject);
        }
    }

    protected virtual void Placement()
    {
        foreach (var block in FindObjectsOfType<Block>())
        {
            block.isSelected = false;
        }
        
        isSelected = true;
        SoundManager.Instance.Play("BlockClickSound", SoundManager.Sound.Effect);
        PlacementSystem.Instance.StartPlacement(blockType, uniqueID);
        
    }

    protected virtual void HandlePlacementSuccess(int placedBlockID, string selectedCardID)
    {
        if (placedBlockID == blockType && isSelected && uniqueID == selectedCardID)
        {
            SoundManager.Instance.Play("BlcokPlace", SoundManager.Sound.Effect);
            Destroy(gameObject);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        print("ondrag");
        PlacementSystem.Instance.StartPlacementForDrag(blockType);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        PlacementSystem.Instance.StopPlacementForDrag();
    }
}
