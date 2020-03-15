using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Biome
{
    public BiomeType type;
    public NoiseSettings settings;
    public List<ResourceType> resources;

    public Biome(BiomeType type, NoiseSettings settings)
    {
        this.type = type;
        this.settings = settings;
    }
}
