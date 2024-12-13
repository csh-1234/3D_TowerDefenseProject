using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Threading;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;


public class ObjectManager
{
    private static ObjectManager _instance;
    public static ObjectManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ObjectManager();
            return _instance;
        }
    }
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
    public HashSet<Projectile> Projectiles { get; } = new HashSet<Projectile>();


    public GameObject Instantiate(GameObject prefab, Vector3? position = null, Quaternion? rotation = null, Transform parent = null, bool pooling = false) 
    {
        GameObject go = null;
        
        if (pooling)
        {
            if (prefab.GetComponent<Monster>() != null)
                go = PoolManager.Instance.Pop(prefab.GetComponent<Monster>()).gameObject;
            else if (prefab.GetComponent<Projectile>() != null)
                go = PoolManager.Instance.Pop(prefab.GetComponent<Projectile>()).gameObject;
            else
                go = PoolManager.Instance.Pop(prefab.GetComponent<MonoBehaviour>()).gameObject;
        }
        else
            go = UnityEngine.Object.Instantiate(prefab, parent);

        go.name = prefab.name;

        if (position != null)
            go.transform.position = position.Value;
        if (rotation != null)
            go.transform.rotation = rotation.Value;
        if (parent != null && !pooling)
            go.transform.SetParent(parent);

        return go;
    }



    public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion? rotation = null) where T : Component
    {
        Type type = typeof(T);

        if(type == typeof(Monster))
        {
            Monster monster = PoolManager.Instance.Pop(prefab.GetComponent<Monster>());
            monster.transform.position = position;
            if (rotation.HasValue)
                monster.transform.rotation = rotation.Value;
            Monsters.Add(monster);
            return monster as T;
        }
        else if (type == typeof(Projectile))
        {
            Projectile projectile = PoolManager.Instance.Pop(prefab.GetComponent<Projectile>());
            projectile.transform.position = position;
            if (rotation.HasValue)
                projectile.transform.rotation = rotation.Value;
            Projectiles.Add(projectile);
            return projectile as T;
        }
        else if (type == typeof(PooledParticle))
        {
            PooledParticle particle = PoolManager.Instance.Pop(prefab.GetComponent<PooledParticle>());
            particle.transform.position = position;
            if (rotation.HasValue)
                particle.transform.rotation = rotation.Value;
            return particle as T;
        }
        else if (type == typeof(LightningEffect))
        {
            LightningEffect line = PoolManager.Instance.Pop(prefab.GetComponent<LightningEffect>());
            line.transform.position = position;
            if (rotation.HasValue)
                line.transform.rotation = rotation.Value;
            return line as T;
        }
        else if (type == typeof(DamageFontEffect))
        {
            DamageFontEffect damageFont = PoolManager.Instance.Pop(prefab.GetComponent<DamageFontEffect>());
            damageFont.transform.position = position;
            if (rotation.HasValue)
                damageFont.transform.rotation = rotation.Value;
            return damageFont as T;
        }

        return null;
    }

    public void Despawn<T>(T obj) where T : UnityEngine.Object
    {
        if (obj == null)
        {
            return;
        }

        System.Type type = typeof(T);

        if (type == typeof(Projectile))
        {
            Projectiles.Remove(obj as Projectile);
            PoolManager.Instance.Push(obj as Projectile);
        }
        else if (type == typeof(Monster))
        {
            Monsters.Remove(obj as Monster);
            PoolManager.Instance.Push(obj as Monster);
        }
        else if (type == typeof(PooledParticle))
        {
            PoolManager.Instance.Push(obj as PooledParticle);
        }
        else if (type == typeof(LightningEffect))
        {
            PoolManager.Instance.Push(obj as LightningEffect);
        }
        else if (type == typeof(DamageFontEffect))
        {
            PoolManager.Instance.Push(obj as DamageFontEffect);
        }
    }
    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        if (go.GetComponent<Monster>() != null)
            PoolManager.Instance.Push(go.GetComponent<Monster>());
        else if (go.GetComponent<Projectile>() != null)
            PoolManager.Instance.Push(go.GetComponent<Projectile>());
        else if (go.GetComponent<PooledParticle>() != null)
            PoolManager.Instance.Push(go.GetComponent<PooledParticle>());
        else if (go.GetComponent<LightningEffect>() != null)
            PoolManager.Instance.Push(go.GetComponent<LightningEffect>());
        else
            UnityEngine.Object.Destroy(go);
    }

    public void Clear()
    {
        foreach (var monster in Monsters.ToList())
        {
            if (monster != null)
                Despawn(monster);
        }
        Monsters.Clear();

        foreach (var projectile in Projectiles.ToList())
        {
            if (projectile != null)
                Despawn(projectile);
        }
        Projectiles.Clear();
    }
}
