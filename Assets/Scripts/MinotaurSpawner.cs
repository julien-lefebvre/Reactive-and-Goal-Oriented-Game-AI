using UnityEngine;

public class MinotaurSpawner : MonoBehaviour {

    public Transform minotaurPrefab;

    private void Awake() {
        Vector3 spawnPosition = GetRandomSpawnPoint();
        Instantiate(minotaurPrefab, spawnPosition, Quaternion.identity);
    }

    private Vector3 GetRandomSpawnPoint() {
        Vector3[] SpawnPoints = {new Vector3(8.95f, 0.9f, 0.91f), new Vector3(8.9f, 0.9f, 12.71f), new Vector3(-9.45f, 0.9f, 4.18f), new Vector3(3.41f, 0.9f, -2.22f), new Vector3(-11.91f, 0.9f, -10.61f), new Vector3(-2.78f, 0.9f, 0.65f), new Vector3(0.24f, 0.9f, -5.61f), new Vector3(0.84f, 0.9f, 8f), new Vector3(-2.78f, 0.9f, -4.52f)};
        return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
    }
}
