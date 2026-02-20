using Assets.Scripts.ObjectPool;
using Common;
using System;
using System.Collections.Generic;

using UnityEngine;

public class PoolManager : MonoSingleton<PoolManager>
{
    private Dictionary<string, object> pools = new Dictionary<string, object>();

    public void CreatePool<T>(string key, T prefab, int size, Transform transform) where T : Component
    {
        if (pools.ContainsKey(key)) return;

        ObjectPool<T> pool = new ObjectPool<T>(prefab, size, transform);
        pools.Add(key, pool);
    }

    public T Spawn<T>(string key) where T : Component
    {
        if (pools.TryGetValue(key, out var Pool))
        {
            ObjectPool<T> pool = Pool as ObjectPool<T>;
            return pool.Get();
        }

        Debug.LogError($"未找到对象池: {key}");
        return null;
    }

    public void Despawn<T>(string key, T obj) where T : Component
    {
        if (pools.TryGetValue(key, out var Pool))
        {
            ObjectPool<T> pool = Pool as ObjectPool<T>;
            pool.Release(obj);
        }
    }
    public void ReturnAllToPool<T>(string key) where T : Component
    {
        T[] actives = FindObjectsOfType<T>();
        foreach (var obj in actives)
        {
            Despawn(key, obj);
        }
    }
}
