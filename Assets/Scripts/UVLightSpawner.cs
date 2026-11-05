
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVLightSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject uvLightPrefab;
    public Transform player;

    [Header("Rare Spawn Settings")]
    public float laneDistance = 3f;
    public float firstSpawnAhead = 35f;
    public float minSpawnDistance = 250f;
    public float maxSpawnDistance = 500f;
    public float spawnHeight = 1.5f;

    [Range(0f, 1f)]
    public float spawnChance = 0.4f;

    [Header("Obstacle Avoidance")]
    [Tooltip("Extra distance kept between UV lights and obstacles.")]
    public float obstacleClearance = 8f;

    [Tooltip("How far ahead to search for a clear spawn position.")]
    public float maxSearchAhead = 45f;

    [Tooltip("Spacing between alternative spawn positions.")]
    public float searchStep = 5f;

    [Header("Respawn After Use")]
    public float respawnDelay = 5f;

    private float nextSpawnZ;
    private bool initialized;
    private bool respawnRequested;
    private float respawnTime;
    private bool waitingForUseRespawn;
    private bool firstSpawn = true;

    private readonly List<GameObject> spawnedLights =
        new List<GameObject>();

    void Start()
    {
        if (uvLightPrefab == null || player == null)
        {
            Debug.LogError(
                "UVLightSpawner: Assign UV Light Prefab and Player!"
            );
            enabled = false;
            return;
        }

        if (minSpawnDistance <= 0f ||
            maxSpawnDistance < minSpawnDistance)
        {
            Debug.LogError(
                "UVLightSpawner: Check minimum and maximum spawn distances!"
            );
            enabled = false;
            return;
        }

        nextSpawnZ = player.position.z + firstSpawnAhead;
        initialized = true;
    }

    void Update()
    {
        if (!initialized || player == null)
            return;

        if (GameManager.Instance == null ||
            !GameManager.Instance.gameStarted ||
            GameManager.Instance.gameOver)
            return;

        // Remove references to lights that were collected or destroyed.
        spawnedLights.RemoveAll(item => item == null);

        if (respawnRequested && Time.time >= respawnTime)
        {
            respawnRequested = false;
            waitingForUseRespawn = false;

            nextSpawnZ = player.position.z + firstSpawnAhead;

            if (SpawnUVLight())
                ScheduleNextSpawn();

            return;
        }

        if (!waitingForUseRespawn &&
            player.position.z >=
            nextSpawnZ - (firstSpawn ? firstSpawnAhead : 0f))
        {
            if (firstSpawn)
            {
                if (SpawnUVLight())
                {
                    firstSpawn = false;
                    Debug.Log("First UV light spawned in a clear spot.");
                }
            }
            else
            {
                if (Random.value < spawnChance)
                {
                    if (SpawnUVLight())
                        Debug.Log("Rare UV light spawned in a clear spot.");
                }
                else
                {
                    Debug.Log("No UV light at this spawn opportunity.");
                }
            }

            ScheduleNextSpawn();
        }
    }

    void ScheduleNextSpawn()
    {
        float distance = Random.Range(
            minSpawnDistance,
            maxSpawnDistance
        );

        nextSpawnZ = player.position.z + distance;
    }

    bool SpawnUVLight()
    {
        // Try all three lanes at several positions ahead.
        List<int> lanes = new List<int> { 0, 1, 2 };

        // Randomize which lane is checked first.
        for (int i = 0; i < lanes.Count; i++)
        {
            int randomIndex = Random.Range(i, lanes.Count);
            int temp = lanes[i];
            lanes[i] = lanes[randomIndex];
            lanes[randomIndex] = temp;
        }

        float startZ = Mathf.Max(
            nextSpawnZ,
            player.position.z + firstSpawnAhead
        );

        float step = Mathf.Max(2f, searchStep);
        float searchDistance = Mathf.Max(step, maxSearchAhead);

        for (float z = startZ; z <= startZ + searchDistance; z += step)
        {
            foreach (int lane in lanes)
            {
                float x = (lane - 1) * laneDistance;

                Vector3 candidate = new Vector3(
                    x,
                    spawnHeight,
                    z
                );

                if (IsPositionClear(candidate))
                {
                    GameObject lightObject = Instantiate(
                        uvLightPrefab,
                        candidate,
                        Quaternion.identity
                    );

                    spawnedLights.Add(lightObject);

                    Debug.Log(
                        "UV light spawned in lane X=" + x +
                        ", Z=" + z
                    );

                    return true;
                }
            }
        }

        Debug.LogWarning(
            "UVLightSpawner: No clear position found. UV light skipped."
        );

        return false;
    }

    bool IsPositionClear(Vector3 candidate)
    {
        // Check nearby obstacle colliders in the scene.
        Collider[] nearby = Physics.OverlapSphere(
            new Vector3(candidate.x, 0.5f, candidate.z),
            obstacleClearance
        );

        foreach (Collider hit in nearby)
        {
            if (hit == null || !hit.enabled)
                continue;

            // Ignore the player and the UV light itself.
            if (player != null &&
                hit.transform.root == player.root)
                continue;

            if (hit.GetComponentInParent<UVLightSpawner>() != null)
                continue;

            // Obstacles are spawned by ObstacleSpawner and get
            // an ObstacleCleanup component on their root.
            if (hit.GetComponentInParent<ObstacleCleanup>() != null)
                return false;
        }

        return true;
    }

    public void RequestRespawnAfterUse()
    {
        if (waitingForUseRespawn)
            return;

        waitingForUseRespawn = true;
        respawnRequested = true;
        respawnTime = Time.time + respawnDelay;
    }
}
