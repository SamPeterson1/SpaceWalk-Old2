using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject resourcePrefab;
    public Dictionary<ResourceType, ResourceSettings> resources;
    public List<ResourceSettings> settings;
    public List<Resource> spawnedResources;
    public TerrainGenerator terrainGenerator;

    void Start()
    {
        terrainGenerator = GameObject.FindGameObjectWithTag("Generator").GetComponent<TerrainGenerator>();
        resources = new Dictionary<ResourceType, ResourceSettings>();
        foreach (ResourceSettings setting in settings)
        {
            resources.Add(setting.type, setting);
        }
        
        GameObject testResource = new GameObject();
        testResource.transform.Translate(terrainGenerator.RandomSurfacePoint(TerrainChunk.GetChunkFromPos(new Vector3(0, 1000, 0))));
        Resource resource = testResource.AddComponent<Resource>();
        resources.TryGetValue(ResourceType.COMPOUND, out resource.settings);
        resource.Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
