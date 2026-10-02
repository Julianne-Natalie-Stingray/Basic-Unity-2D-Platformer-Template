using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using System;
using System.Linq;

public sealed class GameObjectPool<TPooled>
    where TPooled : Component
{
    private TPooled pooledGameObject;
    private Transform poolRoot;

    private readonly ObjectPool<TPooled> pool;
    private readonly List<TPooled> activeInPool;

    public int CountActive => pool.CountActive;
    public int CountInactive => pool.CountInactive;
    public int CountAll => pool.CountAll;


    public GameObjectPool(
        TPooled pooled,
        Transform root,
        Action<TPooled> onGet = null,
        Action<TPooled> onRelease = null,
        bool collectionCheck = true,
        int defaultCapacity = 10,
        int poolSize = 100)
    {
        pooledGameObject = pooled;
        poolRoot = root;
        
        pool = new(
            createFunc: Create,
            actionOnGet: item =>
            {
                item.gameObject.SetActive(true);
                onGet?.Invoke(item);
            },
            actionOnRelease: item =>
            {
                onRelease?.Invoke(item);
                item.gameObject.SetActive(false);
            },
            actionOnDestroy: item =>
            {
                UnityEngine.Object.Destroy(item.gameObject);
            },
            collectionCheck: collectionCheck,
            defaultCapacity: defaultCapacity,
            maxSize: poolSize
        );

        activeInPool = new();
    }

    public TPooled Get()
    {
        var instance = pool.Get();
        activeInPool.Add(instance);
        return instance;
    }

    public void Release(TPooled item)
    {
        pool.Release(item);
        activeInPool.Remove(item);
    }

    public void Clear()
    {
        pool.Clear();
        activeInPool.Clear();
    }

    public void Prewarm(int count)
    {
        List<TPooled> instances = new();
        
        for (int i = 0; i < count; i++)
            instances.Add(pool.Get());
        
        foreach (var instance in instances)
            pool.Release(instance);
    }

    public int CountInActive(Func<TPooled, bool> predicate)
        => activeInPool.Count(predicate);
    
    private TPooled Create()
    {
        var instance = UnityEngine.Object.Instantiate(
            pooledGameObject,
            poolRoot);
        instance.gameObject.SetActive(false);
        return instance;
    }
}
