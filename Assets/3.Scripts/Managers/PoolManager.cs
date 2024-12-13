using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Pool<T> where T : Component
{
    T _prefab;
    IObjectPool<T> _pool;

    Transform _root;
    Transform Root
    {
        get
        {
            if (_root == null)
            {
                GameObject go = new GameObject() { name = $"@{_prefab.name}Pool" };
                _root = go.transform;
            }
            return _root;
        }
    }

    public Pool(T prefab)
    {
        _prefab = prefab;
        _pool = new ObjectPool<T>(OnCreate, OnGet, OnRelease, OnDestroy);
    }

    public void Push(T item)
    {
        if (item.gameObject.activeSelf)
            _pool.Release(item);
    }

    public T Pop()
    {
        return _pool.Get();
    }

    #region Funcs
    T OnCreate()
    {
        T item = GameObject.Instantiate(_prefab);
        item.transform.SetParent(Root);
        item.name = _prefab.name;
        return item;
    }

    void OnGet(T item)
    {
        item.gameObject.SetActive(true);
    }

    void OnRelease(T item)
    {
        Debug.Log($"Pool OnRelease: {item.name}");
        item.gameObject.SetActive(false);
    }

    void OnDestroy(T item)
    {
        GameObject.Destroy(item.gameObject);
    }
    #endregion
}

public class PoolManager
{
    private static PoolManager _instance;
    public static PoolManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PoolManager();
            return _instance;
        }
    }

    Dictionary<string, object> _pools = new Dictionary<string, object>();

    public T Pop<T>(T prefab) where T : Component
    {
        if (!_pools.ContainsKey(prefab.name))
            CreatePool(prefab);

        return (_pools[prefab.name] as Pool<T>).Pop();
    }

    public bool Push<T>(T item) where T : Component
    {
        if (!_pools.ContainsKey(item.name))
            return false;

        (_pools[item.name] as Pool<T>).Push(item);
        return true;
    }

    void CreatePool<T>(T original) where T : Component
    {
        if (_pools.ContainsKey(original.name))
            return;

        Pool<T> pool = new Pool<T>(original);
        _pools.Add(original.name, pool);
    }

    public void Clear()
    {
        _pools.Clear();
    }
}
