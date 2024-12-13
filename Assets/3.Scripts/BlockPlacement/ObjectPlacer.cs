using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    private static ObjectPlacer instance;

    public static ObjectPlacer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ObjectPlacer>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ObjectPlacer");
                    instance = go.AddComponent<ObjectPlacer>();
                    DontDestroyOnLoad(go);
                }
                instance.Initialize();
            }
            return instance;
        }
    }

    [SerializeField]
    public List<GameObject> placedGameObjects = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        if (placedGameObjects == null)
        {
            placedGameObjects = new List<GameObject>();
        }
    }

    public int PlaceObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject newObject = Instantiate(prefab, position, rotation);
        
        // null 항목 제거
        placedGameObjects.RemoveAll(item => item == null);
        
        placedGameObjects.Add(newObject);
        return placedGameObjects.Count - 1;
    }

    public void RemoveObjectAt(int gameObjectIndex)
    {
        if (gameObjectIndex < 0 || gameObjectIndex >= placedGameObjects.Count)
        {
            Debug.LogWarning($"Invalid index for object removal: {gameObjectIndex}");
            return;
        }

        if (placedGameObjects[gameObjectIndex] != null)
        {
            Destroy(placedGameObjects[gameObjectIndex]);
            placedGameObjects[gameObjectIndex] = null;
            Debug.Log($"Removed object at index: {gameObjectIndex}");
        }
    }

    public void Clear()
    {
        foreach (var obj in placedGameObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        placedGameObjects.Clear();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}