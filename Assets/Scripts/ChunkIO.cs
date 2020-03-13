using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChunkIO
{
    public static string savePath = "/Save/chunkData.dat";
    private static float packetSeparator = float.MaxValue;

    public static void CreateFile()
    {
        if(!File.Exists(savePath))
        {
            File.Create(savePath);
        }
    }

    public static void LoadData(out Dictionary<Vector3, TerrainDeformation> deformations, out BiomeGenerator.BiomePoint[] biomePoints, Dictionary<BiomeType, Biome> biomes, out TetherNetwork.TetherData[] tetherDatas)
    {
        deformations = new Dictionary<Vector3, TerrainDeformation>();
        using (BinaryReader reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
        {
            int numChunks = reader.ReadInt32();
            for (int i = 0; i < numChunks; i++)
            {
                float f;
                int index = 0;
                float x = 0;
                float y = 0;
                float z = 0;
                float density = 0;
                int densityIndex;

                Dictionary<int, float> points = new Dictionary<int, float>();

                while ((f = reader.ReadSingle()) != packetSeparator)
                {
                    if (index == 0) x = f;
                    else if (index == 1) y = f;
                    else if (index == 2) z = f;
                    else if (index % 2 == 1)
                    {
                        density = f;
                    }
                    else if (index % 2 == 0)
                    {
                        densityIndex = (int)f;
                        if (points.ContainsKey(densityIndex))
                        {
                            points.Remove(densityIndex);
                        }
                        points.Add(densityIndex, density);
                    }
                    index++;
                }

                Vector3 offset = new Vector3(x, y, z);
                deformations.Add(offset, new TerrainDeformation(points));
            }

            int numBiomes = reader.ReadInt32();
            biomePoints = new BiomeGenerator.BiomePoint[numBiomes];
            for (int i = 0; i < numBiomes; i++)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                int biome = reader.ReadInt32();

                BiomeGenerator.BiomePoint biomePoint;
                biomePoint.pos = new Vector3(x, y, z);
                biomePoint.biome = biome;

                biomes.TryGetValue((BiomeType)biome, out Biome biomeData);

                if (biomeData != null)
                {
                    biomePoint.biome = (int)biomeData.type;
                    NoiseSettings settings = biomeData.settings;
                    biomePoint.amplitude = settings.amplitude;
                    biomePoint.baseRoughness = settings.baseRoughness;
                    biomePoint.center = settings.center;
                    biomePoint.minRadius = settings.minRadius;
                    biomePoint.numLayers = settings.numLayers;
                    biomePoint.persistence = settings.persistence;
                    biomePoint.roughness = settings.roughness;
                    biomePoints[i] = biomePoint;
                }
                else
                {
                    Debug.LogError("Error loading data: biome not found");
                }
            }

            tetherDatas = GetTetherData(reader);
        }
    }

    public static void WriteTetherData(List<TetherNetwork.TetherData> tethers)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Append)))
        {
            writer.Write(tethers.Count);
            Debug.Log(tethers.Count);
            foreach (TetherNetwork.TetherData tether in tethers)
            {
                if (tether.isSupplier)
                {
                    writer.Write(packetSeparator);
                }
                writer.Write(tether.pos.x);
                writer.Write(tether.pos.y);
                writer.Write(tether.pos.z);
            }
        }
    }

    public static TetherNetwork.TetherData[] GetTetherData(BinaryReader reader)
    {
        int numTethers = reader.ReadInt32();
        Debug.Log("TETHERS" + numTethers);
        TetherNetwork.TetherData[] tetherData = new TetherNetwork.TetherData[numTethers];

        for (int i = 0; i < numTethers; i++)
        {
            TetherNetwork.TetherData tether;
            float x = reader.ReadSingle();
            if (x == packetSeparator)
            {
                tether.isSupplier = true;
                x = reader.ReadSingle();
            }
            else
            {
                tether.isSupplier = false;
            }
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            tether.pos = new Vector3(x, y, z);
            tether.tempSupplier = false; // not relevant to tether loading, but I didnt want to create a different struct
            tetherData[i] = tether;
        }
        return tetherData;
    }
    public static void WriteData(Dictionary<Vector3, TerrainDeformation> deformations, BiomeGenerator.BiomePoint[] biomes)
    {

        using (BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Append)))
        {
            writer.Write(deformations.Count);
            foreach (Vector3 key in deformations.Keys)
            {
                deformations.TryGetValue(key, out TerrainDeformation deformation);
                Dictionary<int, float> points = deformation.points;


                writer.Write(key.x);
                writer.Write(key.y);
                writer.Write(key.z);
                foreach (int i in points.Keys)
                {
                    points.TryGetValue(i, out float f);
                    writer.Write(f);
                    //make it a float to make reading the file easier
                    writer.Write((float)i);
                }
                writer.Write(packetSeparator);
            }

            writer.Write(biomes.Length);
            foreach (BiomeGenerator.BiomePoint biome in biomes)
            {
                Vector3 position = biome.pos;
                writer.Write(position.x);
                writer.Write(position.y);
                writer.Write(position.z);
                writer.Write(biome.biome);
            }
        }
    }
}
