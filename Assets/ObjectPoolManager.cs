using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum PoolKey
{
    Bullet,
}

[System.Serializable]
public class PoolEntry
{
    public PoolKey key;
    public GameObject prefab;
    public int poolSize;
}

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    public List<PoolEntry> poolEntries;

    private readonly Dictionary<PoolKey, Queue<GameObject>> _objectPools = new Dictionary<PoolKey, Queue<GameObject>>();

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
            return;
        }

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var entry in poolEntries)
        {
            var objectQueue = new Queue<GameObject>();

            for (var i = 0; i < entry.poolSize; i++)
            {
                var obj = Instantiate(entry.prefab, transform);
                var a = entry.prefab.name;
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            _objectPools[entry.key] = objectQueue;
        }
    }

    public GameObject Get(PoolKey key)
    {
        GameObject objectToReturn;

        if (_objectPools[key].Count > 0)
        {
            objectToReturn = _objectPools[key].Dequeue();
            objectToReturn.GetComponent<TrailRenderer>().Clear();
            objectToReturn.SetActive(true);
            return objectToReturn;
        }

        var entry = poolEntries.Find(e => e.key == key);
        objectToReturn = Instantiate(entry.prefab, transform);
        return objectToReturn;
    }

    private void ReturnObjectToPool(PoolKey key, GameObject obj,Action<GameObject> beforeReturn = null)
    {
        obj.SetActive(false);
        beforeReturn?.Invoke(obj);
        _objectPools[key].Enqueue(obj);
    }

    public void Return(PoolKey key, GameObject obj)
    {
        ReturnObjectToPool(key, obj);
    }

    public async UniTaskVoid ReturnWithDelay(PoolKey key, GameObject obj, float delay,Action<GameObject> beforeReturn = null)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
        ReturnObjectToPool(key, obj,beforeReturn);
    }
}