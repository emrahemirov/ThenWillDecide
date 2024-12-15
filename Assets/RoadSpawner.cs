using UnityEngine;
using System.Collections.Generic;

public class RoadSpawner : MonoBehaviour
{
    [Header("Prefabs & Settings")] public List<GameObject> roadPrefabs;
    public float moveSpeed = 5f;

    [Header("Spawn Settings")] public float spawnInterval = 2f;

    private float _spawnTimer;
    private GameObject _lastSpawnedRoad;


    private List<GameObject> _activeRoads;
    private float _index;

    void Start()
    {
        _activeRoads = new List<GameObject>();
        SpawnInitialRoad();
    }

    private void FixedUpdate()
    {
        _index += 20 * Time.fixedDeltaTime;
        MoveRoads();
    }

    void HandleSpawning()
    {
    }

    private void SpawnInitialRoad()
    {
        var road = Instantiate(GetRandomRoad(), transform.position + new Vector3(0, 0, -40), transform.rotation);
        _activeRoads.Add(road);
    }

    private void SpawnRoad()
    {
        var road = Instantiate(GetRandomRoad(), transform.position, transform.rotation);
        _activeRoads.Add(road);
    }

    private void MoveRoads()
    {
        _activeRoads.ForEach(road => road.transform.position += Vector3.back * (Time.fixedDeltaTime * moveSpeed));
    }


    private GameObject GetRandomRoad()
    {
        return roadPrefabs[Random.Range(0, roadPrefabs.Count)];
    }
}