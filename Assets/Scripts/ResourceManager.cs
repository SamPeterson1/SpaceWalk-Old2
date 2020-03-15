using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public Dictionary<ResourceType, ResourceSettings> resources;
    public TextAsset resourcesJSON;
    public TerrainGenerator terrainGenerator;
    public BiomeGenerator biomeGenerator;
    public Player player;
    public int numResources;
    private Dictionary<Vector3, List<ResourceData>> unloadedResources;
    private Dictionary<Vector3, List<ResourceUnit>> loadedResources;
    private List<Vector3> generatedChunks = new List<Vector3>();

    private struct ResourceData
    {
        public Vector3 pos;
        public ResourceType type;
    }

    void Start()
    {
        terrainGenerator = GameObject.FindGameObjectWithTag("Generator").GetComponent<TerrainGenerator>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        biomeGenerator = terrainGenerator.shape.biomeGenerator;
        resources = new Dictionary<ResourceType, ResourceSettings>();
        unloadedResources = new Dictionary<Vector3, List<ResourceData>>();
        loadedResources = new Dictionary<Vector3, List<ResourceUnit>>();
        foreach (ResourceSettings setting in LoadResourcesFromFile())
        {
            resources.Add(setting.type, setting);
        }
    }

    private void Update()
    {

        if (player.changedChunks())
        {
            LoadResources();
            UnloadResources();
            Vector3 off = TerrainChunk.GetChunkFromPos(player.transform.position);
            for (int x = -2 + (int)off.x; x <= 2 + off.x; x++)
            {
                for (int y = -2 + (int)off.y; y <= 2 + off.y; y++)
                {
                    for (int z = -2 + (int)off.z; z <= 2 + off.z; z++)
                    {
                        Vector3 pos = new Vector3(x, y, z);
                        if (!loadedResources.ContainsKey(pos) && !unloadedResources.ContainsKey(pos))
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                SpawnResourceInChunk(pos);
                            }
                        }
                    }
                }
            }
        }

    }

    void LoadResources()
    {
        List<Vector3> loaded = new List<Vector3>();
        foreach (Vector3 chunkPos in unloadedResources.Keys)
        {
            if (InRange(chunkPos))
            {
                unloadedResources.TryGetValue(chunkPos, out List<ResourceData> data);
                List<ResourceUnit> resourceUnits = new List<ResourceUnit>();
                foreach (ResourceData resourceData in data)
                {
                    resources.TryGetValue(resourceData.type, out ResourceSettings settings);
                    GameObject resourcePrefab = (GameObject)Resources.Load("Prefabs/" + settings.resourcePrefab);
                    ResourceUnit unit = Instantiate(resourcePrefab, resourceData.pos, Quaternion.identity).GetComponent<ResourceUnit>();
                    unit.type = settings.type;
                    resourceUnits.Add(unit);
                }

                loaded.Add(chunkPos);
                loadedResources.Add(chunkPos, resourceUnits);
            }
        }

        foreach (Vector3 remove in loaded)
        {
            unloadedResources.Remove(remove);
        }
    }

    void UnloadResources()
    {
        List<Vector3> removed = new List<Vector3>();
        foreach (Vector3 chunkPos in loadedResources.Keys)
        {
            if (!InRange(chunkPos))
            {
                loadedResources.TryGetValue(chunkPos, out List<ResourceUnit> units);
                List<ResourceData> resourceData = new List<ResourceData>();
                foreach (ResourceUnit unit in units)
                {
                    if (unit != null)
                    {
                        ResourceData data;
                        data.pos = unit.transform.position;
                        data.type = unit.type;
                        resourceData.Add(data);
                        Destroy(unit.gameObject);
                    }
                }

                removed.Add(chunkPos);
                unloadedResources.Add(chunkPos, resourceData);
            }
        }

        foreach (Vector3 remove in removed)
        {
            loadedResources.Remove(remove);
        }
    }

    bool InRange(Vector3 chunkPos)
    {
        Vector3 displacement = chunkPos - TerrainChunk.GetChunkFromPos(player.transform.position);
        return Mathf.Abs(displacement.x) < 3 && Mathf.Abs(displacement.y) < 3 && Mathf.Abs(displacement.z) < 3;
    }

    List<ResourceSettings> LoadResourcesFromFile()
    {
        string text = resourcesJSON.text;
        text = text.Replace("\n", "");
        string[] array = text.Split(new string[] { "RESOURCE" }, System.StringSplitOptions.RemoveEmptyEntries);
        List<ResourceSettings> resources = new List<ResourceSettings>();
        foreach (string str in array)
        {
            string toBiome = str.Replace(" ", "");
            resources.Add(JsonUtility.FromJson<ResourceSettings>(toBiome));
        }

        return resources;
    }

    void SpawnResourceInChunk(Vector3 chunkPos)
    {
        if (!generatedChunks.Contains(chunkPos) && terrainGenerator.ChunkExistsAtChunkPos(chunkPos))
        {
            SpawnResource(terrainGenerator.RandomSurfacePoint(chunkPos));
        }
    }

    void SpawnResource(Vector3 pos)
    {
        Biome biome = biomeGenerator.GetBiomeAt(pos);
        List<ResourceType> resources = biome.resources;
        ResourceType toSpawn = resources[Random.Range(0, resources.Count)];
        InitResource(pos, toSpawn);
    }

    void InitResource(Vector3 pos, ResourceType type)
    {
        resources.TryGetValue(type, out ResourceSettings settings);
        for (int i = 0; i < settings.count; i++)
        {
            Vector3 randPoint = Random.insideUnitSphere;
            randPoint *= settings.radius;
            /*
            if (terrainGenerator.InTerrain(randPoint + pos, 0f))
            {
                GameObject resourcePrefab = (GameObject)Resources.Load("Prefabs/" + settings.resourcePrefab);
                ResourceUnit unit = Instantiate(resourcePrefab, randPoint + pos, Quaternion.identity).GetComponent<ResourceUnit>();
                unit.type = settings.type;
                Vector3 chunkPos = TerrainChunk.GetChunkFromPos(unit.transform.position);
                if (loadedResources.ContainsKey(chunkPos))
                {
                    loadedResources.TryGetValue(chunkPos, out List<ResourceUnit> units);
                    units.Add(unit);
                }
                else
                {
                    List<ResourceUnit> units = new List<ResourceUnit>() { unit };
                    loadedResources.Add(chunkPos, units);
                }
            }
            else
            {
            */
                ResourceData data;
                data.pos = randPoint + pos;
                data.type = settings.type;
                Vector3 chunkPos = TerrainChunk.GetChunkFromPos(data.pos);
                if (unloadedResources.ContainsKey(chunkPos))
                {
                    unloadedResources.TryGetValue(chunkPos, out List<ResourceData> dataList);
                    dataList.Add(data);
                }
                else
                {
                    List<ResourceData> dataList = new List<ResourceData>() { data };
                    unloadedResources.Add(chunkPos, dataList);
                }
            //}
        }
    }

}
