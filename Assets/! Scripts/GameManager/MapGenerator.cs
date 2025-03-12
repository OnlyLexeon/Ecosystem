using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;

[System.Serializable]
public class Vegetation
{
    public int patchCount;
    public int minPerPatch;
    public int maxPerPatch;
    public int lonelyCount;
    public float heightOffset;
    public GameObject prefab;
}


public class MapGenerator : MonoBehaviour
{
    [Header("Size")]
    public int width = 64;
    public int length = 64;

    [Header("Water Bodies")]
    [Tooltip("Lower = More scattered, smaller | Higher = Fewer, larger ")] public float waterScale;
    [Tooltip("Lower = More Common | Higher = Less Common")] public float waterThreshold;

    [Header("Vegetation Settings")]
    public Vegetation bushes;
    public Vegetation trees;
    public Vegetation carrots;

    [Header("Prefabs")]
    public GameObject landPrefab;
    public GameObject waterPrefab;

    [Header("References(*)")]
    public Transform landHolder;
    public Transform waterHolder;
    public Transform vegetationHolder;
    public Transform burrowHolder;
    public NavMeshSurface navMeshSurface;
    public Transform map;

    private GameObject[,] terrainGrid;

    public static MapGenerator Instance;

    void Awake()
    {
        Instance = this;

        StartCoroutine(GenerateMapAndBakeNavMesh());
    }

    public void DoDailyVegetation()
    {
        GenerateVegetation(carrots);
    }

    IEnumerator GenerateMapAndBakeNavMesh()
    {
        terrainGrid = new GameObject[width, length];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                Vector3 position = new Vector3(x, 0, z);
                GameObject grassBlock = Instantiate(landPrefab, position, Quaternion.identity, landHolder);
                terrainGrid[x, z] = grassBlock; // Store grass block for later placement
            }
        }
        yield return null;

        GenerateWater();
        yield return null;

        GenerateVegetation(bushes);
        yield return null;

        GenerateVegetation(trees);
        yield return null;

        DoDailyVegetation();
        yield return null;

        GenerateOuterLandRing();
        yield return null;

        CenterMap();
        yield return null;

        BakeNavMesh();
    }
    void BakeNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh baked successfully!");
        }
        else
        {
            Debug.LogError("NavMeshSurface not assigned!");
        }
    }

    void CenterMap()
    {
        Vector3 offset = new Vector3(-width / 2f, 0, -length / 2f);

        map.transform.Translate(offset);

        Debug.Log("Map centered at (0,0,0)");
    }

    //GENERATIOM
    void GenerateOuterLandRing()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                // Check only outer edges
                if (x == 0 || x == width - 1 || z == 0 || z == length - 1)
                {
                    if (terrainGrid[x, z] != null && terrainGrid[x, z].tag == "Drink")
                    {
                        // Remove the water tile
                        Destroy(terrainGrid[x, z]);

                        // Replace with land
                        Vector3 position = new Vector3(x, 0, z);
                        terrainGrid[x, z] = Instantiate(landPrefab, position, Quaternion.identity, landHolder);
                    }
                }
            }
        }
    }
    void GenerateWater()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                float perlinValue = Mathf.PerlinNoise(x * waterScale, z * waterScale);

                if (perlinValue > waterThreshold) // Water if value is high
                {
                    Destroy(terrainGrid[x, z]); // Remove grass
                    terrainGrid[x, z] = Instantiate(waterPrefab, new Vector3(x, 0, z), Quaternion.identity, waterHolder);
                }
            }
        }
    }
    void GenerateVegetation(Vegetation vege)
    {
        List<Vector2Int> landPositions = new List<Vector2Int>();

        // Collect valid land positions
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if (terrainGrid[x, z] != null && terrainGrid[x, z].tag == "Land")
                {
                    landPositions.Add(new Vector2Int(x, z));
                }
            }
        }

        // Generate vegetation patches
        for (int i = 0; i < vege.patchCount; i++)
        {
            if (landPositions.Count == 0) break;

            Vector2Int patchCenter = landPositions[Random.Range(0, landPositions.Count)];
            int perPatch = Random.Range(vege.minPerPatch, vege.maxPerPatch);

            for (int j = 0; j < perPatch; j++)
            {
                int offsetX = Random.Range(-2, 3);
                int offsetZ = Random.Range(-2, 3);
                int newX = Mathf.Clamp(patchCenter.x + offsetX, 0, width - 1);
                int newZ = Mathf.Clamp(patchCenter.y + offsetZ, 0, length - 1);

                if (terrainGrid[newX, newZ] != null && terrainGrid[newX, newZ].tag == "Land")
                {
                    Vector3 position = terrainGrid[newX, newZ].transform.position + Vector3.up * vege.heightOffset;
                    Instantiate(vege.prefab, position, Quaternion.identity, vegetationHolder);
                }
            }
        }

        // Generate lonely vegetation
        for (int i = 0; i < vege.lonelyCount; i++)
        {
            if (landPositions.Count == 0) break;

            Vector2Int randomPos = landPositions[Random.Range(0, landPositions.Count)];
            Vector3 position = terrainGrid[randomPos.x, randomPos.y].transform.position + Vector3.up * vege.heightOffset;
            Instantiate(vege.prefab, position, Quaternion.identity, vegetationHolder);
        }
    }

    
}
