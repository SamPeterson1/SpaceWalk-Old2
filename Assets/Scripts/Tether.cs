using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class Tether : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody body;
    /*
    public static readonly float RANGE = 10f;
    public Tether supplier = null;
    public List<Tether> supplies = new List<Tether>();
    public GameObject connectionPrefab;
    private TetherConnection tetherConnection;
    private TetherNetwork network;
    public bool hasOxygen = false;
    public bool isRoot = false;
    public bool firstTether = false;
    public bool tempSupplier = false;
    public bool suppliedByTempSupplier = false;
    */
    private TetherConnection tetherConnection;
    public TetherNode tetherNode;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    public TetherNode CreateNode()
    {
        tetherNode = new TetherNode(this);
        return tetherNode;
    }

    void Update()
    {
        RaycastHit hit;
        if (body.constraints == RigidbodyConstraints.FreezeAll && Physics.Raycast(transform.position, -transform.position, out hit, Mathf.Infinity))
        {
            Vector3 localUp = transform.up;
            Vector3 target = hit.normal;
            transform.rotation *= Quaternion.FromToRotation(localUp, target);
            if (hit.distance > 1.2f)
            {
                body.constraints = RigidbodyConstraints.None;
            }
        }

    }
    public void MakeConnection(Tether other, GameObject prefab)
    {
        if (tetherConnection == null)
        {
            tetherConnection = Instantiate(prefab).GetComponent<TetherConnection>();
        }
        tetherConnection.tetherA = this;
        tetherConnection.tetherB = other;
    }

    /*
    void Start()
    {
        body = GetComponent<Rigidbody>();
        network = GameObject.FindGameObjectWithTag("Planet").GetComponent<TetherNetwork>();
        body.constraints = RigidbodyConstraints.FreezeAll;
        network.AddTether(this, TerrainChunk.getChunkFromPos(transform.position));
    }

    public void MakeSupplier()
    {
        supplier = this;
        hasOxygen = true;
        firstTether = true;
    }

    bool NotSupplied()
    {
        if (supplier == null) return true;
        if (!supplier.inRange(transform)) return true;
        return !hasOxygen;
    }

    void updateHasOxygen()
    {
        if (tempSupplier)
        {
            if (!hasOxygen) network.updatingOxygen = true;
            hasOxygen = true;
        } else if (supplier == null)
        {
            if (hasOxygen) network.updatingOxygen = true;
            hasOxygen = false;
        } else if (!inRange(supplier.transform))
        {
            if (hasOxygen) network.updatingOxygen = true;
            hasOxygen = false;
        } else if (hasOxygen != supplier.hasOxygen)
        {
            hasOxygen = supplier.hasOxygen;
            network.updatingOxygen = true;
        }
        if (!network.updatingOxygen && (network.updated || NotSupplied()))
        {
            Tether closest = FindClosestTether(!hasOxygen);
            if (closest != null && !supplies.Contains(closest))
            {
                if (TerrainChunk.getChunkFromPos(closest.transform.position) != TerrainChunk.getChunkFromPos(transform.position))
                {
                    tempSupplier = false;
                    firstTether = false;
                }
                if (tetherConnection == null)
                {
                    tetherConnection = Instantiate(connectionPrefab).GetComponent<TetherConnection>();
                }
                tetherConnection.tetherA = this;
                tetherConnection.tetherB = closest;
                if (!firstTether)
                {
                    supplier = closest;
                    closest.supplies.Add(this);
                    suppliedByTempSupplier = supplier.tempSupplier || supplier.suppliedByTempSupplier;
                }
            }
        }

        
    }

    void Update()
    {
        RaycastHit hit;
        if (body.constraints == RigidbodyConstraints.FreezeAll && Physics.Raycast(transform.position, -transform.position, out hit, Mathf.Infinity))
        {
            Vector3 localUp = transform.up;
            Vector3 target = hit.normal;
            transform.rotation *= Quaternion.FromToRotation(localUp, target);
            if(hit.distance > 1.2f)
            {
                body.constraints = RigidbodyConstraints.None;
            }
        }
        
        updateHasOxygen();
    }

    public void MakeConnection(Tether other)
    {
        if (tetherConnection == null)
        {
            tetherConnection = Instantiate(connectionPrefab).GetComponent<TetherConnection>();
        }
        tetherConnection.tetherA = this;
        tetherConnection.tetherB = other;
    }

    public Tether FindClosestTether(bool oxygenated)
    {
        Tether closest = null;
        float minDist = -1;
        foreach (List<Tether> tethers in network.tethers.Values)
        {
            foreach (Tether tether in tethers)
            {
                if (tether != this && tether.inRange(transform) && tether.hasOxygen == oxygenated)
                {
                    float dist = (tether.transform.position - transform.position).magnitude;
                    if (minDist == -1 || dist < minDist)
                    {
                        minDist = dist;
                        closest = tether;
                    }
                }
            }
        }

        return closest;
    }

    public bool inRange(Transform other)
    {
        Vector3 pos = other.position;
        return (transform.position - pos).magnitude < RANGE;
    }
    */

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.tag.Equals("Chunk"))
        {
            body.constraints = RigidbodyConstraints.FreezeAll;
            
        }  
    }
}
