using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private void OnDestroy()
    {
        // 진행 중인 모든 코루틴 중지
        StopAllCoroutines();
        
        // 경로 관련 데이터 정리
        if (PathManager.Instance != null)
        {
            // 이벤트 구독 해제 제거 (이벤트를 사용하지 않으므로)
            // PathManager.Instance.OnPathCalculated -= HandlePathCalculation;
        }
    }

    private void OnDisable()
    {
        // 비활성화될 때도 코루틴 중지
        StopAllCoroutines();
    }

    // 기존 코드에서 코루틴을 시작하기 전에 이전 코루틴 중지
    protected void StartNewCoroutine(IEnumerator coroutine)
    {
        StopAllCoroutines();
        StartCoroutine(coroutine);
    }
} 