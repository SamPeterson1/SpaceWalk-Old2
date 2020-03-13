using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Biome
{

    public BiomeType type;
    public NoiseSettings settings;

    public Biome(BiomeType type, NoiseSettings settings)
    {
        this.type = type;
        this.settings = settings;
    }
}
