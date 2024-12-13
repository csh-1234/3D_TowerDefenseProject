using UnityEngine;

public class UI_StageIndicator : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 20f;  // 위아래로 움직일 거리
    [SerializeField] private float moveSpeed = 2f;      // 움직임 속도
    
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
        
        // Sin 함수를 사용하여 부드러운 위아래 움직임 구현
        float newY = originalY + Mathf.Sin(time) * moveDistance;
        
        // 새로운 위치 적용
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    // 인디케이터 위치 설정 (UI_Map에서 호출)
    public void SetPosition(Vector3 position)
    {
        startPosition = position;
        originalY = position.y;
        transform.position = position;
    }
} 