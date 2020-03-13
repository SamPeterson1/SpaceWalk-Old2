using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator
{
    public Dictionary<BiomeType, Biome> biomes;
    public BiomePoint[] biomePoints;
    public struct BiomePoint
    {
        public int biome;
        public float roughness;
        public float amplitude;
        public float persistence;
        public float baseRoughness;
        public float numLayers;
        public float minRadius;
        public Vector3 center;
        public Vector3 pos;
    }

    public BiomeGenerator(List<Biome> biomes)
    {
        SaveManager saveManager = GameObject.FindGameObjectWithTag("SaveManager").GetComponent<SaveManager>();
        BiomeSaveSection biomeSaveSection = saveManager.GetSaveSection(BiomeSaveSection.biomeIdentifier) as BiomeSaveSection;
        biomeSaveSection.biomeGenerator = this;
        this.biomes = new Dictionary<BiomeType, Biome>();
        foreach(Biome biome in biomes)
        {
            this.biomes.Add(biome.type, biome);
        }
    }

    public void GenerateBiomes(Vector4[] biomes)
    {
        biomePoints = new BiomePoint[biomes.Length];
        for(int i = 0; i < biomes.Length; i ++)
        {
            biomePoints[i] = GetBiomePoint(new Vector3(biomes[i].x, biomes[i].y, biomes[i].z), (BiomeType)(int)biomes[i].w);
        }
    }

    public BiomePoint GetBiomePoint(Vector3 pos, BiomeType biomeType)
    {
        BiomePoint biomePoint;
        biomePoint.pos = pos;
        biomes.TryGetValue(biomeType, out Biome biome);

        biomePoint.biome = (int)biome.type;
        NoiseSettings settings = biome.settings;
        biomePoint.amplitude = settings.amplitude;
        biomePoint.baseRoughness = settings.baseRoughness;
        biomePoint.center = settings.center;
        biomePoint.minRadius = settings.minRadius;
        biomePoint.numLayers = settings.numLayers;
        biomePoint.persistence = settings.persistence;
        biomePoint.roughness = settings.roughness;

        return biomePoint;
    }

    public void GenerateBiomes()
    {
        biomePoints = new BiomePoint[600];
        for(int i = 0; i < 600; i ++)
        {
            Vector3 randPoint = Random.insideUnitSphere * 1000.0f;

            if (i % 3 == 0)
            {
                //this.biomes.TryGetValue(BiomeType.MOUNTAINS, out biome);
                biomePoints[i] = GetBiomePoint(randPoint, BiomeType.MOUNTAINS);
            } else if(i % 3 == 1)
            {
                biomePoints[i] = GetBiomePoint(randPoint, BiomeType.PLAINS);
            } else
            {
                biomePoints[i] = GetBiomePoint(randPoint, BiomeType.ROCKY_HILLS);
            }

            /*
            if (biome == null) Debug.LogError("Biome not found!");
            biomePoint.biome = (int) biome.type;
            NoiseSettings settings = biome.settings;
            biomePoint.amplitude = settings.amplitude;
            biomePoint.baseRoughness = settings.baseRoughness;
            biomePoint.center = settings.center;
            biomePoint.minRadius = settings.minRadius;
            biomePoint.numLayers = settings.numLayers;
            biomePoint.persistence = settings.persistence;
            biomePoint.roughness = settings.roughness;

            
            biomePoint.pos = randPoint * 1000.0f;
            biomes[i] = biomePoint;
            */
        }
    }

}
