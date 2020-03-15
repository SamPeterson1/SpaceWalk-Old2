﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public TerrainShape shape;
    public ComputeShader compute;
    public ComputeShader densities;
    public TextAsset biomesJSON;
    public RunShader shader;

    private Dictionary<Vector3, TerrainChunk> chunks;

    public GameObject chunkPrefab;
    private Player player;

    private Queue<TerrainChunk> needUpdate;

    public NoiseSettings settings;
    public TetherNetwork network;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        network = GameObject.FindGameObjectWithTag("Planet").GetComponent<TetherNetwork>();
        TerrainChunk.chunkPrefab = chunkPrefab;

        float foo = Time.time;
        shader = new RunShader(compute);
        shape = new TerrainShape(biomesJSON)
        {
            settings = settings,
            shader = densities
        };
        chunks = new Dictionary<Vector3, TerrainChunk>();
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

    public void Deform(Vector3 pos, float radius, int subtract)
    {
        foreach (TerrainChunk chunk in chunks.Values)
        {
            Vector3 chunkCenter = chunk.gameObject.transform.position;
            Vector3 toCenter = chunkCenter - pos;
            
            if (toCenter.magnitude - radius < 40)
            {
                chunk.Deform(pos, radius, subtract);
            }
        }
    }

    void CreateChunk(Vector3 offset)
    {
        TerrainChunk chunk = Instantiate(chunkPrefab, offset, Quaternion.identity).GetComponent<TerrainChunk>();
        chunk.shape = shape;
        chunk.GenTerrain();
        chunks.Add(chunk.CalculateChunkPos(), chunk);
    }

    // Update is called once per frame
    void Update()
    {
        player.readChunkData();
        player.readPastChunk();
        List<Vector3> removePlease = new List<Vector3>();
        if (player.changedChunks())
        {
            foreach (TerrainChunk chunk in chunks.Values)
            {
                Vector3 displacement = chunk.CalculateChunkPos() - player.getChunkPosition();
                if (Mathf.Abs(displacement.x) >= 3)
                {
                    removePlease.Add(chunk.CalculateChunkPos());
                    chunk.gameObject.transform.Translate(new Vector3(-5 * 39 * Mathf.Sign(displacement.x), 0, 0));
                    needUpdate.Enqueue(chunk);
                    chunk.gameObject.SetActive(false);
                } else if (Mathf.Abs(displacement.y) >= 3)
                {
                    removePlease.Add(chunk.CalculateChunkPos());
                    chunk.gameObject.transform.Translate(new Vector3(0, -5 * 39 * Mathf.Sign(displacement.y), 0));
                    needUpdate.Enqueue(chunk);
                    chunk.gameObject.SetActive(false);
                } else if (Mathf.Abs(displacement.z) >= 3)
                {
                    removePlease.Add(chunk.CalculateChunkPos());
                    chunk.gameObject.transform.Translate(new Vector3(0, 0, -5 * 39 * Mathf.Sign(displacement.z)));
                    needUpdate.Enqueue(chunk);
                    chunk.gameObject.SetActive(false);
                }
            }
        }

        foreach(Vector3 remove in removePlease)
        {
            chunks.Remove(remove);
        }

        if (needUpdate.Count > 0)
        {
            TerrainChunk chunk = needUpdate.Dequeue();
            chunks.Add(chunk.CalculateChunkPos(), chunk);
            chunk.Reload();
        }
    }

    public Vector3 RandomSurfacePoint(Vector3 chunkPos)
    {
        chunks.TryGetValue(chunkPos, out TerrainChunk chunk);
        return chunk.RandomSurfacePoint();
    }

    public bool ChunkExistsAtChunkPos(Vector3 chunkPos)
    {
        return chunks.ContainsKey(chunkPos);
    }

    public bool ChunkExists(Vector3 pos)
    {
        return chunks.ContainsKey(TerrainChunk.GetChunkFromPos(pos));
    }

    public bool InTerrain(Vector3 point, float tolerance)
    {
        Vector3 chunkPos = TerrainChunk.GetChunkFromPos(point);
        chunks.TryGetValue(chunkPos, out TerrainChunk chunk);
        if (chunk != null)
        {
            return chunk.InTerrain(point, tolerance);
        }
        else
        {
            return false;
        }
    }

    public bool DoneLoading()
    {
        return needUpdate.Count == 0;
    }
}
