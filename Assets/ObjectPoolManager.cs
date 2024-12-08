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
            // var keyParent = new GameObject(entry.key.ToString()).transform; 
            // keyParent.SetParent(transform); 

            for (var i = 0; i < entry.poolSize; i++)
            {
                var obj = Instantiate(entry.prefab, transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            _objectPools[entry.key] = objectQueue;
        }
    }

    public GameObject Get(PoolKey key)
    {
        if (_objectPools.ContainsKey(key) && _objectPools[key].Count > 0)
        {
            var obj = _objectPools[key].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else if (_objectPools.ContainsKey(key))
        {
            Debug.LogWarning($"Pool for '{key}' is empty. Instantiating a new object.");
            var entry = poolEntries.Find(e => e.key == key);
            if (entry != null)
            {
                // var keyParent = new GameObject(entry.key.ToString()).transform; 
                // keyParent.SetParent(transform); 
                var obj = Instantiate(entry.prefab, transform);
                return obj;
            }
        }

        Debug.LogError($"No pool found for key: {key}");
        return null;
    }

    private void ReturnOrDestroy(PoolKey key, GameObject obj)
    {
        obj.SetActive(false);
        _objectPools[key].Enqueue(obj);
    }

    public void Return(PoolKey key, GameObject obj)
    {
        ReturnOrDestroy(key, obj);
    }

    public async UniTaskVoid ReturnWithDelay(PoolKey key, GameObject obj, float delay)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
        ReturnOrDestroy(key, obj);
    }
}