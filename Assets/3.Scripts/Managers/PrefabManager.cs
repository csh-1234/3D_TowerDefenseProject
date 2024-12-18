using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance { get; private set; }

    [SerializeField]
    private GameObject parentObject; 

    [SerializeField]
    private Transform targetChildObject; 

    [SerializeField]
    private List<GameObject> replacementPrefabs;

    private GameObject lastReplacedPrefab; 
    private bool isReplaced = false;       

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ReplaceChildWithPrefab()
    {
        if (isReplaced || parentObject == null || targetChildObject == null || replacementPrefabs == null || replacementPrefabs.Count == 0)
            return;

        List<GameObject> availablePrefabs = replacementPrefabs.FindAll(prefab => prefab != lastReplacedPrefab);

        if (availablePrefabs.Count == 0)
            return;

        GameObject selectedPrefab = availablePrefabs[Random.Range(0, availablePrefabs.Count)];

        if (selectedPrefab == null)
            return;

        Vector3 originalPosition = targetChildObject.localPosition;
        Quaternion originalRotation = targetChildObject.localRotation;
        Vector3 originalScale = targetChildObject.localScale;

        Destroy(targetChildObject.gameObject);

        GameObject newChild = Instantiate(selectedPrefab, parentObject.transform);
        newChild.transform.localPosition = originalPosition;
        newChild.transform.localRotation = originalRotation;
        newChild.transform.localScale = originalScale;

        lastReplacedPrefab = selectedPrefab;
        isReplaced = true;
    }
}
