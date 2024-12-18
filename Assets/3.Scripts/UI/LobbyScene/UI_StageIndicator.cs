using UnityEngine;

public class UI_StageIndicator : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 20f;  
    [SerializeField] private float moveSpeed = 2f;      
    
    private Vector3 startPosition;
    private float originalY;
    private float time;

    private void Start()
    {
        startPosition = transform.position;
        originalY = startPosition.y;
    }

    private void Update()
    {
        time += Time.deltaTime * moveSpeed;
        
        float newY = originalY + Mathf.Sin(time) * moveDistance;
        
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    public void SetPosition(Vector3 position)
    {
        startPosition = position;
        originalY = position.y;
        transform.position = position;
    }
} 