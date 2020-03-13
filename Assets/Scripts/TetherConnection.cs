using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherConnection : MonoBehaviour
{
    public Tether tetherA;
    public Tether tetherB;
    private LineRenderer renderer;
    private Material material;
    public bool enabled;
    void Start()
    {
        renderer = GetComponent<LineRenderer>();
        material = renderer.material;
        renderer.positionCount = 2;
    }

    private void OnDestroy()
    {
        renderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (tetherA == null || tetherB == null) {
            Destroy(gameObject);
            return;
        }
        renderer.SetPosition(0, tetherA.transform.position);
        renderer.SetPosition(1, tetherB.transform.position);
        if (!tetherA.tetherNode.network.InRange(tetherA.tetherNode, tetherB.tetherNode))
        {
            renderer.enabled = false;
            enabled = false;
        } else
        {
            renderer.enabled = true;
            enabled = true;
        }

        if(tetherA.tetherNode.hasOxygen || tetherB.tetherNode.hasOxygen)
        {
            material.color = Color.cyan;
        } else
        {
            material.color = Color.red;
        }
    }
}
