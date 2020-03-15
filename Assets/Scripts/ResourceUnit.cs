﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceUnit : MonoBehaviour
{

    float animationSpeed = 10f;
    float initialDist;
    bool animating;
    TerrainGenerator terrainGenerator;
    public ResourceType type;
    Player target;

    // Start is called before the first frame update
    void Awake()
    {
        terrainGenerator = GameObject.FindGameObjectWithTag("Generator").GetComponent<TerrainGenerator>();
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (terrainGenerator.ChunkExists(transform.position))
        {
            if (!animating && !terrainGenerator.InTerrain(transform.position, 0f))
            {
                animating = true;
                GetComponent<Collider>().enabled = false;
                initialDist = (transform.position - target.transform.position).magnitude;
            }

            if (animating)
            {
                float dist = (transform.position - target.transform.position).magnitude;
                float scale = dist / initialDist;
                transform.localScale = new Vector3(scale, scale, scale);
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position - Vector3.up, animationSpeed * Time.deltaTime);

                if (dist < 1.5f) Destroy(gameObject);
            }
        }
    }

}
