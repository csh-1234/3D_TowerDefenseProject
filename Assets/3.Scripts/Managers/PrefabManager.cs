// PrefabManager.cs는 게임 오브젝트의 자식 오브젝트를 특정 프리팹으로 교체하는 스크립트
// 주요 기능
// 1. Singleton 패턴 구현: PrefabManager의 인스턴스를 글로벌로 관리.
// 2. 자식 오브젝트 교체: 부모 오브젝트의 특정 자식을 프리팹으로 변경.
// 3. 랜덤 교체: 마지막으로 사용된 프리팹을 제외한 랜덤 프리팹 선택.

using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance { get; private set; }

    [SerializeField]
    private GameObject parentObject; // 부모 오브젝트

    [SerializeField]
    private Transform targetChildObject; // 교체 대상 자식 오브젝트

    [SerializeField]
    private List<GameObject> replacementPrefabs; // 교체할 프리팹 리스트

    private GameObject lastReplacedPrefab; // 마지막으로 교체된 프리팹
    private bool isReplaced = false;       // 프리팹 교체 여부 플래그

    private void Awake()
    {
        // Singleton 패턴으로 인스턴스 관리
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 자식 오브젝트를 프리팹으로 교체하는 메서드
    public void ReplaceChildWithPrefab()
    {
        // 교체 조건 검사
        if (isReplaced || parentObject == null || targetChildObject == null || replacementPrefabs == null || replacementPrefabs.Count == 0)
            return;

        // 마지막 사용된 프리팹을 제외한 프리팹 리스트 생성
        List<GameObject> availablePrefabs = replacementPrefabs.FindAll(prefab => prefab != lastReplacedPrefab);

        if (availablePrefabs.Count == 0)
            return;

        // 사용 가능한 프리팹 중 랜덤 선택
        GameObject selectedPrefab = availablePrefabs[Random.Range(0, availablePrefabs.Count)];

        if (selectedPrefab == null)
            return;

        // 기존 자식 오브젝트의 위치, 회전, 스케일 저장
        Vector3 originalPosition = targetChildObject.localPosition;
        Quaternion originalRotation = targetChildObject.localRotation;
        Vector3 originalScale = targetChildObject.localScale;

        // 기존 자식 오브젝트 제거
        Destroy(targetChildObject.gameObject);

        // 선택된 프리팹 인스턴스화 및 설정
        GameObject newChild = Instantiate(selectedPrefab, parentObject.transform);
        newChild.transform.localPosition = originalPosition;
        newChild.transform.localRotation = originalRotation;
        newChild.transform.localScale = originalScale;

        // 마지막으로 교체된 프리팹 및 교체 여부 업데이트
        lastReplacedPrefab = selectedPrefab;
        isReplaced = true;
    }
}
