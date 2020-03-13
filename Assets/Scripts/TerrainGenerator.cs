using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    TerrainShape shape;
    public ComputeShader compute;
    public ComputeShader densities;
    public RunShader shader;
    public ColorGenerator colorGen;
    public GameObject chunkPrefab2;
    private List<TerrainChunk> chunks;

    public int dist;

    public GameObject chunkPrefab;
    private Player player;

    private Queue<TerrainChunk> needUpdate;
    private BinaryWriter chunkData;

    public List<Biome> biomes;
    public NoiseSettings settings;
    public TetherNetwork network;
    public SaveFile saveFile;

    void Awake()
    {
        saveFile = new SaveFile("/Save/chunkData.dat");
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        network = GameObject.FindGameObjectWithTag("Planet").GetComponent<TetherNetwork>();
        TerrainChunk.chunkPrefab = chunkPrefab;

        float foo = Time.time;
        shader = new RunShader(compute);
        shape = new TerrainShape(biomes);
        shape.settings = settings;
        shape.shader = densities;
        chunks = new List<TerrainChunk>();
        needUpdate = new Queue<TerrainChunk>();

        

        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    CreateChunk(new Vector3(x * 39, y * 39, z * 39) + new Vector3(0, 975, 0));
                }
            }
        }

        Debug.Log(Time.realtimeSinceStartup - foo + " TIME");
    }

    public void deform(Vector3 pos, float radius, int subtract)
    {
       // int i = 0;
        foreach (TerrainChunk chunk in chunks)
        {
            Vector3 chunkCenter = chunk.gameObject.transform.position;
            Vector3 toCenter = chunkCenter - pos;
            
            if (toCenter.magnitude - radius < 40)
            {
                //i++;
                chunk.deform(pos, radius, subtract);
            }
        }
        //Debug.Log(i);
    }

    /*
    void genChunk(Vector3 offset)
    {
        TerrainChunk chunk = new TerrainChunk(offset, shape, shader, chunkData, this);
        chunks.Add(chunk);
    }
    */

    void CreateChunk(Vector3 offset)
    {
        TerrainChunk chunk = Instantiate(chunkPrefab2, offset, Quaternion.identity).GetComponent<TerrainChunk>();
        chunk.shape = shape;
        chunk.GenTerrain();
        chunks.Add(chunk);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (TerrainChunk chunk in chunks)
            {
                chunk.genDensities();
                chunk.computeMesh();
            }
            TerrainChunk.SaveData(shape.biomePoints);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            foreach (TerrainChunk chunk in chunks)
            {
                chunk.SaveDeforms();
            }
            //TerrainChunk.WriteData(saveFile);
            saveFile.WriteData();
            
            /*
            TerrainChunk.SaveData(shape.biomePoints);
            //network.Save();
            
        } else if(Input.GetKeyDown(KeyCode.L))
        {
            saveFile.ReadData();
            //TerrainChunk.LoadData(saveFile.GetSaveSection(ChunkSaveSection.chunkIdentifier) as ChunkSaveSection);
            foreach (TerrainChunk chunk in chunks)
            {
                chunk.genDensities();
                chunk.computeMesh();
            }
            */
            /*
             
            shape.biomePoints = TerrainChunk.loadSaveData(shape.biomeGenerator, out TetherNetwork.TetherData[] tetherDatas);
            foreach (TerrainChunk chunk in chunks)
            {
                chunk.genDensities();
                chunk.computeMesh();
            }
            //network.Load(tetherDatas);
            */
        //}

        player.readChunkData();
        player.readPastChunk();
        if (player.changedChunks())
        {
            foreach (TerrainChunk chunk in chunks)
            {
                Vector3 displacement = chunk.CalculateChunkPos() - player.getChunkPosition();
                if (Mathf.Abs(displacement.x) >= 3)
                {
                    chunk.gameObject.transform.Translate(new Vector3(-5 * 39 * Mathf.Sign(displacement.x), 0, 0));
                    needUpdate.Enqueue(chunk);
                    chunk.gameObject.SetActive(false);
                } else if (Mathf.Abs(displacement.y) >= 3)
                {
                    chunk.gameObject.transform.Translate(new Vector3(0, -5 * 39 * Mathf.Sign(displacement.y), 0));
                    needUpdate.Enqueue(chunk);
                    chunk.gameObject.SetActive(false);
                } else if (Mathf.Abs(displacement.z) >= 3)
                {
                    chunk.gameObject.transform.Translate(new Vector3(0, 0, -5 * 39 * Mathf.Sign(displacement.z)));
                    needUpdate.Enqueue(chunk);
                    chunk.gameObject.SetActive(false);
                }
            }
        }
        if (needUpdate.Count > 0)
        {
            TerrainChunk chunk = needUpdate.Dequeue();
            chunk.Reload();
        }
    }

    public bool DoneLoading()
    {
        return needUpdate.Count == 0;
    }
}
