
using System.Collections.Generic;
using UnityEngine;

public class EndlessTrack : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject forestTilePrefab;

    [Header("Track Settings")]
    public float tileLength = 49f;
    public int tilesOnScreen = 3;
    public float spawnAheadDistance = 100f;

    [Header("Cleanup")]
    public float destroyBehindDistance = 55f;

    private readonly List<GameObject> activeTiles =
        new List<GameObject>();

    private float nextSpawnZ = 0f;

    void Start()
    {
        if (player == null || forestTilePrefab == null)
        {
            Debug.LogError("Assign Player and Forest Tile Prefab!");
            enabled = false;
            return;
        }

        // Create the starting forest tiles
        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile();
        }
    }

    void Update()
    {
        // Spawn new tiles ahead of the player
        while (nextSpawnZ < player.position.z + spawnAheadDistance)
        {
            SpawnTile();
        }

        // Remove tiles far behind the player
        for (int i = activeTiles.Count - 1; i >= 0; i--)
        {
            if (activeTiles[i].transform.position.z <
                player.position.z - destroyBehindDistance)
            {
                Destroy(activeTiles[i]);
                activeTiles.RemoveAt(i);
            }
        }
    }

    void SpawnTile()
    {
        Vector3 spawnPosition = new Vector3(
            0f,
            0f,
            nextSpawnZ
        );

        GameObject tile = Instantiate(
            forestTilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        activeTiles.Add(tile);

        nextSpawnZ += tileLength;
    }
}
