using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public ResourceSettings settings;
    public TerrainGenerator terrainGenerator;
    private List<GameObject> resources;

    private void Awake()
    {
        resources = new List<GameObject>();
        terrainGenerator = GameObject.FindGameObjectWithTag("Generator").GetComponent<TerrainGenerator>();
    }

    private void Update()
    {
       foreach(GameObject resourceObject in resources)
        {
            if(!terrainGenerator.InTerrain(resourceObject.transform.position, 0f))
            {
                resourceObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
        }
    }

    public void Spawn()
    {
        for (int i = 0; i < settings.count; i++)
        {
            Vector3 randPoint = Random.insideUnitSphere;
            randPoint *= settings.radius;

            if (terrainGenerator.InTerrain(randPoint + transform.position, 0f))
            {
                GameObject resourceObject = Instantiate(settings.resourcePrefab, transform);
                resourceObject.transform.Translate(randPoint);
                resources.Add(resourceObject);
            }
        }
    }
}
