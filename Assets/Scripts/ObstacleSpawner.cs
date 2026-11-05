
using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Obstacle
    {
        public GameObject prefab;
        public float groundY = 0f;
        public Vector3 rotationOffset = Vector3.zero;
        public bool middleLaneOnly = false;
    }

    [Header("Obstacle Prefabs")]
    public Obstacle[] obstacles = new Obstacle[4];

    [Header("Player")]
    public Transform player;

    [Header("Five Lane Settings")]
    public int totalLanes = 5;
    public float laneDistance = 3f;

    [Header("Spawn Distance")]
    public float spawnDistanceAhead = 45f;
    public float minimumGroupDistance = 42f;
    public float maximumGroupDistance = 50f;

    [Header("Pattern Chances")]
    [Range(0, 100)]
    public int singleObstacleChance = 25;

    [Range(0, 100)]
    public int tripleObstacleChance = 25;

    [Header("Cleanup")]
    public float destroyDistanceBehind = 65f;

    private float nextSpawnZ;
    private bool initialized;
    private bool stopped;

    private readonly List<Obstacle> validObstacles =
        new List<Obstacle>();

    void Start()
    {
        totalLanes = 5;
        laneDistance = 3f;

        if (player == null)
        {
            Debug.LogError("ObstacleSpawner: Assign Player!");
            stopped = true;
            return;
        }

        validObstacles.Clear();

        if (obstacles != null)
        {
            foreach (Obstacle obstacleItem in obstacles)
            {
                if (obstacleItem != null &&
                    obstacleItem.prefab != null)
                {
                    validObstacles.Add(obstacleItem);
                }
            }
        }

        if (validObstacles.Count == 0)
        {
            Debug.LogError(
                "ObstacleSpawner: Assign obstacle prefabs!"
            );

            stopped = true;
            return;
        }

        Debug.Log("Advanced Subway-style ObstacleSpawner ready!");
    }

    void Update()
    {
        if (stopped || player == null)
            return;

        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.gameStarted ||
                GameManager.Instance.gameOver)
                return;
        }

        if (!initialized)
        {
            nextSpawnZ = player.position.z +
                         Mathf.Max(5f, spawnDistanceAhead);

            initialized = true;
        }

        float spawnLimit = player.position.z +
                           Mathf.Max(5f, spawnDistanceAhead);

        if (nextSpawnZ <= spawnLimit)
        {
            SpawnRandomPattern(nextSpawnZ);

            float distance = Random.Range(
                minimumGroupDistance,
                maximumGroupDistance
            );

            nextSpawnZ += Mathf.Max(15f, distance);
        }
    }

    // ==================================================
    // PATTERN GENERATION
    // ==================================================

    void SpawnRandomPattern(float zPosition)
    {
        int roll = Random.Range(0, 100);

        int obstacleCount;

        if (roll < singleObstacleChance)
        {
            obstacleCount = 1;
        }
        else if (roll < singleObstacleChance + tripleObstacleChance)
        {
            obstacleCount = 3;
        }
        else
        {
            obstacleCount = 2;
        }

        List<int[]> patterns = GetLanePatterns(obstacleCount);

        ShufflePatterns(patterns);

        foreach (int[] lanes in patterns)
        {
            List<Obstacle> selectedObstacles =
                new List<Obstacle>();

            if (TrySelectUniqueObstacles(lanes, selectedObstacles))
            {
                for (int i = 0; i < lanes.Length; i++)
                {
                    SpawnObstacle(
                        selectedObstacles[i],
                        lanes[i],
                        zPosition
                    );
                }

                Debug.Log(
                    "Pattern spawned: " + lanes.Length +
                    " different obstacles at Z: " + zPosition
                );

                return;
            }
        }

        // Safe fallback: spawn one valid obstacle.
        SpawnSingleFallback(zPosition);
    }

    void SpawnSingleFallback(float zPosition)
    {
        if (validObstacles.Count == 0)
            return;

        List<Obstacle> candidates = new List<Obstacle>();

        foreach (Obstacle obstacleItem in validObstacles)
        {
            if (obstacleItem == null || obstacleItem.prefab == null)
                continue;

            candidates.Add(obstacleItem);
        }

        if (candidates.Count == 0)
            return;

        Obstacle selected = candidates[
            Random.Range(0, candidates.Count)
        ];

        int lane = selected.middleLaneOnly ? 2 :
            Random.Range(0, totalLanes);

        SpawnObstacle(selected, lane, zPosition);

        Debug.Log("Fallback: spawned one valid obstacle.");
    }

    // ==================================================
    // LANE PATTERNS
    // ==================================================

    List<int[]> GetLanePatterns(int count)
    {
        List<int[]> patterns = new List<int[]>();

        if (count == 1)
        {
            patterns.Add(new int[] { 0 });
            patterns.Add(new int[] { 1 });
            patterns.Add(new int[] { 2 });
            patterns.Add(new int[] { 3 });
            patterns.Add(new int[] { 4 });
        }
        else if (count == 2)
        {
            patterns.Add(new int[] { 0, 2 });
            patterns.Add(new int[] { 1, 3 });
            patterns.Add(new int[] { 2, 4 });
        }
        else
        {
            patterns.Add(new int[] { 0, 2, 4 });
        }

        return patterns;
    }

    // ==================================================
    // UNIQUE OBSTACLE SELECTION
    // ==================================================

    bool TrySelectUniqueObstacles(
        int[] lanes,
        List<Obstacle> selected)
    {
        selected.Clear();

        List<GameObject> usedPrefabs =
            new List<GameObject>();

        return SelectObstacleForLane(
            lanes, 0, selected, usedPrefabs
        );
    }

    bool SelectObstacleForLane(
        int[] lanes,
        int laneIndex,
        List<Obstacle> selected,
        List<GameObject> usedPrefabs)
    {
        if (laneIndex >= lanes.Length)
            return true;

        int lane = lanes[laneIndex];

        List<Obstacle> candidates = new List<Obstacle>();

        foreach (Obstacle obstacleItem in validObstacles)
        {
            if (obstacleItem == null ||
                obstacleItem.prefab == null)
                continue;

            if (usedPrefabs.Contains(obstacleItem.prefab))
                continue;

            string obstacleName =
                obstacleItem.prefab.name.ToLower()
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "");

            bool middleOnly =
                obstacleItem.middleLaneOnly ||
                obstacleName.Contains("fallenlog") ||
                obstacleName.Contains("barricade");

            if (middleOnly && lane != 2)
                continue;

            candidates.Add(obstacleItem);
        }

        // Randomize candidates.
        for (int i = 0; i < candidates.Count; i++)
        {
            int randomIndex = Random.Range(i, candidates.Count);

            Obstacle temporary = candidates[i];
            candidates[i] = candidates[randomIndex];
            candidates[randomIndex] = temporary;
        }

        foreach (Obstacle candidate in candidates)
        {
            selected.Add(candidate);
            usedPrefabs.Add(candidate.prefab);

            if (SelectObstacleForLane(
                lanes,
                laneIndex + 1,
                selected,
                usedPrefabs))
            {
                return true;
            }

            selected.RemoveAt(selected.Count - 1);
            usedPrefabs.Remove(candidate.prefab);
        }

        return false;
    }

    // ==================================================
    // SPAWNING
    // ==================================================

    void SpawnObstacle(
        Obstacle selected,
        int lane,
        float zPosition)
    {
        if (selected == null || selected.prefab == null)
            return;

        float xPosition = (lane - 2) * laneDistance;

        Vector3 spawnPosition = new Vector3(
            xPosition,
            selected.groundY,
            zPosition
        );

        Quaternion spawnRotation =
            selected.prefab.transform.rotation *
            Quaternion.Euler(selected.rotationOffset);

        GameObject spawned = Instantiate(
            selected.prefab,
            spawnPosition,
            spawnRotation
        );

        spawned.name = selected.prefab.name;

        AlignToGround(spawned, selected.groundY);

        ObstacleCleanup cleanup =
            spawned.GetComponent<ObstacleCleanup>();

        if (cleanup == null)
        {
            cleanup = spawned.AddComponent<ObstacleCleanup>();
        }

        cleanup.player = player;
        cleanup.destroyDistanceBehind = destroyDistanceBehind;

        Debug.Log(
            "Spawned: " + selected.prefab.name +
            " | Lane X: " + xPosition +
            " | Z: " + zPosition
        );
    }

    // ==================================================
    // GROUND ALIGNMENT
    // ==================================================

    void AlignToGround(GameObject obstacle, float groundY)
    {
        Renderer[] renderers =
            obstacle.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        float correction = groundY - bounds.min.y;

        obstacle.transform.position +=
            Vector3.up * correction;
    }

    // ==================================================
    // SHUFFLE PATTERNS
    // ==================================================

    void ShufflePatterns(List<int[]> patterns)
    {
        for (int i = 0; i < patterns.Count; i++)
        {
            int randomIndex = Random.Range(
                i,
                patterns.Count
            );

            int[] temporaryPattern = patterns[i];

            patterns[i] = patterns[randomIndex];

            patterns[randomIndex] = temporaryPattern;
        }
    }

    // ==================================================
    // GAME CONTROL
    // ==================================================

    public void StopSpawning()
    {
        stopped = true;
    }

    public void ResumeSpawning()
    {
        stopped = false;
    }
}

// ======================================================
// OBSTACLE CLEANUP
// ======================================================

public class ObstacleCleanup : MonoBehaviour
{
    public Transform player;
    public float destroyDistanceBehind = 65f;

    void Update()
    {
        if (player == null)
            return;

        if (transform.position.z <
            player.position.z - destroyDistanceBehind)
        {
            Destroy(gameObject);
        }
    }
}
