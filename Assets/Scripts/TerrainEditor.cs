using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainEditor : Editor
{

    private SerializedProperty settings;

    private void Awake()
    {
        settings = serializedObject.FindProperty("settings");
    }

    private void Update()
    {
        
    }
}
