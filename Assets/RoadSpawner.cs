using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class RoadSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> roadPrefabs;

    private float _index;
    private List<float> _spawnedIndexes;
    private List<GameObject> _spawnedRoads;

    private void Start()
    {
        _spawnedIndexes = new List<float>();
        _spawnedRoads = new List<GameObject>();
        SpawnInitialRoad();
    }

    private void FixedUpdate()
    {
        transform.position -= new Vector3(0, 0, 20 * Time.fixedDeltaTime);
        
        if (-transform.position.z >= _index)
        {
            SpawnRoad();
            _index += 40 - (3 * Time.fixedDeltaTime);
        }

        if (_spawnedRoads.Count <= 3) return;
        var firstSpawnedRoad = _spawnedRoads[0];
        Destroy(firstSpawnedRoad);
        _spawnedRoads.RemoveAt(0);
    }

    private void SpawnInitialRoad()
    {
        var road = Instantiate(GetRandomRoad(), new Vector3(0, 0, 1), transform.rotation, transform);
        _spawnedRoads.Add(road);
    }

    private void SpawnRoad()
    {
        var road = Instantiate(GetRandomRoad(), new Vector3(0, 0, 40), transform.rotation, transform);
        _spawnedRoads.Add(road);
    }

    private GameObject GetRandomRoad()
    {
        var index = Random.Range(0, roadPrefabs.Count);
        return roadPrefabs[index];
    }
}