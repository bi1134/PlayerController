using UnityEngine;

public class ChestSpawningManager : MonoBehaviour
{
    [Header("Chest Settings")]
    public GameObject[] chestPrefabs;
    public int chestCount = 20;

    [Tooltip("How much to sink the chest into the ground (e.g., 0.1f sinks it slightly)")]
    public float embedDepth = 0.1f;

    [Tooltip("Maximum terrain slope (in degrees) where a chest can be placed")]
    public float maxSlope = 30f;

    [Tooltip("Skip placing on terrain higher than this world Y value")]
    public float maxSpawnHeight = 50f;

    [Header("References")]
    public Terrain terrain;

    void Start()
    {
        SpawnChests();
    }

    void SpawnChests()
    {
        if (terrain == null || chestPrefabs.Length == 0)
        {
            Debug.LogWarning("Terrain or chest prefabs missing.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int attempts = 0;
        int spawned = 0;

        while (spawned < chestCount && attempts < chestCount * 10)
        {
            attempts++;

            float x = Random.Range(0f, terrainData.size.x);
            float z = Random.Range(0f, terrainData.size.z);
            Vector3 terrainLocalPos = new Vector3(x, 0, z);

            float y = terrain.SampleHeight(terrainLocalPos + terrainPos) + terrainPos.y;
            if (y > maxSpawnHeight) continue;

            Vector3 worldPos = new Vector3(x + terrainPos.x, y, z + terrainPos.z);

            // Reject if terrain hole
            if (IsTerrainHole(terrainData, x / terrainData.size.x, z / terrainData.size.z))
                continue;

            // Reject if slope is too high
            Vector3 normal = terrainData.GetInterpolatedNormal(x / terrainData.size.x, z / terrainData.size.z);
            float slope = Vector3.Angle(Vector3.up, normal);
            if (slope > maxSlope) continue;

            // Slightly embed into terrain
            Vector3 spawnPos = worldPos - Vector3.up * embedDepth;

            // Random rotation
            Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject chestPrefab = chestPrefabs[Random.Range(0, chestPrefabs.Length)];
            Instantiate(chestPrefab, spawnPos, randomRot);

            spawned++;
        }

        Debug.Log($"Spawned {spawned} chests after {attempts} attempts.");
    }

    bool IsTerrainHole(TerrainData terrainData, float normX, float normZ)
    {
        int holesResolution = terrainData.holesResolution;
        if (holesResolution <= 0) return false;

        int holeX = Mathf.Clamp(Mathf.FloorToInt(normX * holesResolution), 0, holesResolution - 1);
        int holeZ = Mathf.Clamp(Mathf.FloorToInt(normZ * holesResolution), 0, holesResolution - 1);

        // GetHoles returns a 2D bool array where false = hole
        bool[,] holes = terrainData.GetHoles(holeX, holeZ, 1, 1);
        return holes.Length > 0 && holes[0, 0] == false;
    }
}