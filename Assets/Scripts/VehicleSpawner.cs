using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Modèles de véhicules")]
    [SerializeField] private Vehicle[] vehiclePrefabs;

    [Header("Positions de spawn")]
    [SerializeField] private float leftSpawnX = -12f;
    [SerializeField] private float rightSpawnX = 12f;

    [Header("Voies de circulation")]
    [SerializeField] private float leftToRightLaneZ = 2f;
    [SerializeField] private float rightToLeftLaneZ = -2f;

    [Header("Trafic")]
    [SerializeField] private float minimumSpawnDelay = 1f;
    [SerializeField] private float maximumSpawnDelay = 3f;

    [Header("Vitesse des véhicules")]
    [SerializeField] private float minimumSpeed = 5f;
    [SerializeField] private float maximumSpeed = 12f;

    [Header("Limite de disparition")]
    [SerializeField] private float destroyX = 15f;

    private float nextSpawnTime;

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnVehicle();
            nextSpawnTime = Time.time + Random.Range(minimumSpawnDelay, maximumSpawnDelay);
        }
    }

    private void SpawnVehicle()
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0)
        {
            Debug.LogError("Aucun prefab de véhicule n'est assigné dans VehicleSpawner.");
            return;
        }

        Vehicle vehiclePrefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];

        bool spawnFromLeft = Random.value < 0.5f;

        float spawnX = spawnFromLeft ? leftSpawnX : rightSpawnX;
        float laneZ = spawnFromLeft ? leftToRightLaneZ : rightToLeftLaneZ;
        int direction = spawnFromLeft ? 1 : -1;

        Vector3 spawnPosition = new Vector3(spawnX, 0.5f, laneZ);

        Vehicle vehicle = Instantiate(vehiclePrefab, spawnPosition, Quaternion.identity);
        vehicle.Initialize(direction, Random.Range(minimumSpeed, maximumSpeed), destroyX);
    }
}