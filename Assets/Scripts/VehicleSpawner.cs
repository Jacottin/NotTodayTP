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

    [Header("Rembobinage après collision")]
    [SerializeField] private float rewindSeconds = 2f;

    private float nextSpawnTime;

    private void Start()
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0)
        {
            Debug.LogError("Aucun prefab de véhicule n'est assigné dans VehicleSpawner.");
        }

        if (RewindManager.Instance == null)
        {
            Debug.LogError("RewindManager est absent de la scène.");
        }

        nextSpawnTime = Time.time + 0.5f;
    }

    private void Update()
    {
        if (RewindManager.Instance != null && RewindManager.Instance.IsBeingRewinded)
        {
            nextSpawnTime += Time.deltaTime;
            return;
        }

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
            return;
        }

        Vehicle vehiclePrefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];

        if (vehiclePrefab == null)
        {
            Debug.LogError("Spawn annulé : le prefab choisi est vide.");
            return;
        }

        bool spawnFromLeft = Random.value < 0.5f;

        float spawnX = spawnFromLeft ? leftSpawnX : rightSpawnX;
        float laneZ = spawnFromLeft ? leftToRightLaneZ : rightToLeftLaneZ;
        int direction = spawnFromLeft ? 1 : -1;

        Vector3 spawnPosition = new Vector3(spawnX, 0.2f, laneZ);

        Vehicle vehicle = Instantiate(vehiclePrefab, spawnPosition, Quaternion.identity);

        vehicle.Initialize(
            direction,
            Random.Range(minimumSpeed, maximumSpeed),
            destroyX,
            rewindSeconds
        );

        RegisterVehicleForRewind(vehicle.gameObject);
    }

    private void RegisterVehicleForRewind(GameObject vehicle)
    {
        if (RewindManager.Instance == null)
        {
            Debug.LogError("Impossible d'enregistrer le véhicule : RewindManager est absent.");
            return;
        }

        GenericRewind[] rewindComponents =
            vehicle.GetComponentsInChildren<GenericRewind>(true);

        if (rewindComponents.Length == 0)
        {
            Debug.LogError($"Aucun GenericRewind sur {vehicle.name}.", vehicle);
            return;
        }

        foreach (GenericRewind rewindComponent in rewindComponents)
        {
            RewindManager.Instance.AddObjectForTracking(
                rewindComponent,
                RewindManager.OutOfBoundsBehaviour.Disable
            );
        }

        Debug.Log($"Véhicule enregistré pour le rembobinage : {vehicle.name}", vehicle);
    }
}