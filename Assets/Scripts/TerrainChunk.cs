using static System.Buffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TerrainChunk : MonoBehaviour
{
    float[] densities;
    Vector3[] colors;
    public TerrainShape shape;
    Vector3 offset;
    public ComputeShader computeShader;
    public RunShader generator;

    public Vector3 toPlayer;

    public static int SIZE = 40;

    public static Dictionary<Vector3, TerrainDeformation> pastData = new Dictionary<Vector3, TerrainDeformation>();
    public static GameObject chunkPrefab;
    private TerrainDeformation currentDeform;
    private ChunkSaveSection chunkSaveSection;
    private SaveManager saveManager;

    private void Awake()
    {
        saveManager = GameObject.FindGameObjectWithTag("SaveManager").GetComponent<SaveManager>();
        chunkSaveSection = saveManager.GetSaveSection(ChunkSaveSection.chunkIdentifier) as ChunkSaveSection;
        chunkSaveSection.deformations = pastData;
        offset = transform.position;
        generator = new RunShader(computeShader);
    }

    private void Update()
    {
        if(saveManager.justLoaded)
        {
            GenTerrain();
        }
    }

    public void GenTerrain()
    {
        GenDensities();
        ComputeMesh();
    }

    public void UpdateFromSaveData(Player player)
    {
        Vector3 chunkPos = CalculateChunkPos();
        Vector3 displacement = chunkPos - player.getChunkPosition();

        if (Mathf.Abs(displacement.x) >= 2 || Mathf.Abs(displacement.y) >= 2 || Mathf.Abs(displacement.z) >= 2)
        {
            GenDensities();
            ComputeMesh();
        }
    }

    int ToIndex(Vector3 coord)
    {
        return (int)coord.x * 40 * 40 + (int)coord.y * 40 + (int)coord.z;
    }

    float NearestDensity(Vector3 point)
    {
        return densities[ToIndex(point)];
    }

    public bool InTerrain(Vector3 point, float tolerance)
    {
        Vector3 chunkCorner = offset - new Vector3(20, 20, 20);
        Vector3 relativePos = point - chunkCorner;
        return NearestDensity(relativePos) < tolerance;
    }

    public Vector3 RandomSurfacePoint()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        int numVerts = mesh.vertexCount;
        if (numVerts > 0)
        {
            int randIndex = Random.Range(0, numVerts);
            return mesh.vertices[randIndex] + transform.position;
        } else
        {
            return offset;
        }
    }

    public void GenDensities()
    {
        shape.GetDensities(offset, out densities, out colors);
        if (pastData.ContainsKey(offset))
        {
            pastData.TryGetValue(offset, out currentDeform);
            if(currentDeform != null)
            {
                densities = currentDeform.modify(densities);
            }
        } else
        {
            currentDeform = new TerrainDeformation();
        }
    }

    private Color ColorFromVec3(Vector3 color)
    {
        return new Color(color.x, color.y, color.z);
    }

    public void ComputeMesh()
    {
        
        RunShader.Triangle[] triangles = generator.run(densities, this.colors, transform.position);
        Vector3[] verts = new Vector3[triangles.Length*3];
        Color[] colors = new Color[triangles.Length*3];
        for(int i = 0; i < triangles.Length; i ++)
        {
            verts[i * 3] = triangles[i].pointA;
            verts[i * 3 + 1] = triangles[i].pointB;
            verts[i * 3 + 2] = triangles[i].pointC;
            Color color = ColorFromVec3(triangles[i].color);
            colors[i * 3] = color;
            colors[i * 3 + 1] = color;
            colors[i * 3 + 2] = color;
        }
        GenMesh(verts, colors);
    }

    private void GenMesh(Vector3[] vertices, Color[] colors)
    {
        int[] indices = new int[vertices.Length];
        for(int i = 0; i < indices.Length; i ++)
        {
            indices[i] = i;
        }
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = indices;

        mesh.RecalculateNormals();
        
        mesh.colors = colors;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void Reload()
    {
        offset = transform.position;
        GenDensities();
        ComputeMesh();
        gameObject.SetActive(true);
    }
    
    void SaveDeforms()
    {
        if (currentDeform.Modified())
        {
            if (!pastData.ContainsKey(offset))
            {
                pastData.Add(offset, currentDeform);
            }
        }
    }

    public void Deform(Vector3 deformCenter, float radius, int subtract)
    {
        int startX = (int)(deformCenter.x - radius);
        int startY = (int)(deformCenter.y - radius);
        int startZ = (int)(deformCenter.z - radius);

        int endX = (int)(deformCenter.x + radius);
        int endY = (int)(deformCenter.y + radius);
        int endZ = (int)(deformCenter.z + radius);

        bool updated = false;

        for (int x = startX; x < endX; x ++)
        {
            for(int y = startY; y < endY; y ++)
            {
                for(int z = startZ; z < endZ; z ++)
                {
                    Vector3 relativeToChunk = (new Vector3(x, y, z) - transform.position);
                    relativeToChunk += new Vector3(20, 20, 20);
                    if (relativeToChunk.x < 40 && relativeToChunk.x >= 0 && relativeToChunk.y < 40 && relativeToChunk.y >= 0 && relativeToChunk.z < 40 && relativeToChunk.z >= 0)
                    {
                        float dist = Mathf.Abs((new Vector3(x, y, z) - deformCenter).magnitude);
                        if (dist < 5)
                        {
                            int index = (int)relativeToChunk.x * 40 * 40 + (int)relativeToChunk.y * 40 + (int)relativeToChunk.z;
                            densities[index] -= (dist - 5) * 0.1f * subtract;
                            currentDeform.LogDensity(index, densities[index]);
                            updated = true;
                        }
                    }
                }
            }
        }

        if (updated)
        {
            ComputeMesh();
        }
        SaveDeforms();
    }

    public Vector3 CalculateChunkPos()
    {
        Vector3 chunkPos = GetChunkFromPos(transform.position);
        return chunkPos;
    }

    public static Vector3Int GetChunkFromPos(Vector3 position)
    {
        Vector3 rawPos = new Vector3(position.x, position.y, position.z);
        Vector3Int chunkPos = Round(rawPos / 39f);
        return chunkPos;
    }
    private static Vector3Int Round(Vector3 vec)
    {
        return new Vector3Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y), Mathf.RoundToInt(vec.z));
    }
}
