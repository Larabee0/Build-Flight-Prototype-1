using System.Collections.Generic;
using UnityEngine;

public class ComponentStatsManager : MonoBehaviour
{
    private bool Touching = true;

    [Header("Reguired Script References")]
    public ModelManger ModelManger;
    public ShipManager ShipManager;

    [Header("Possible Script References")]
    public MeshVisualizer MeshVisualizer;
    public ThrusterTest ThrusterComp;

    [Header("Component References")]
    public Plane Plane;
    private MeshFilter MeshFilter;


    [Header("Ship Component Stats")]
    public int ComponentType = 1;
    public float Mass = 0;
    public float Density = 1;
    public float Volume = 0;
    public float Health = 0;

    [Header("Debug Info")]
    public bool NoVisualizer = false;
    public bool FlagForRemoval = false;

    
    private void Awake()
    {
        MeshVisualizer = gameObject.GetComponent<MeshVisualizer>();
        ModelManger = gameObject.GetComponentInParent<ModelManger>();
        ThrusterComp = gameObject.GetComponent<ThrusterTest>();


        if (MeshVisualizer == null)
        {
            NoVisualizer = true;
        }
        else
        {
            NoVisualizer = false;
        }
    }

    void Start()
    {

        ThrusterComp = gameObject.GetComponent<ThrusterTest>();
        if(ThrusterComp != null)
        {
            ComponentType = 2;
        }
        MeshVisualizer = gameObject.GetComponent<MeshVisualizer>();
        if(MeshVisualizer == null)
        {
            NoVisualizer = true;
        }
        else
        {
            NoVisualizer = false;
        }
        MeshFilter = this.GetComponent<MeshFilter>();
        if (MeshFilter == null)
        {
            Debug.Log(gameObject.name + " has no MeshFilter.");
        }
        ShipManager = gameObject.GetComponentInParent<ShipManager>();

        CalculateStats();
    }

    public void CalculateStats()
    {
        CalculateMass();        
        if (ComponentType == 1)
        {
            Health = Volume * Mass * 100;
        }
        if (ComponentType == 2)
        {
            Health = Volume * Mass * 50;
        }
    }

    
    // Calculate Mesh and volume of the mesh.
    public void CalculateMass()
    {
        if (MeshFilter == null)
        {
            MeshFilter = this.gameObject.GetComponent<MeshFilter>();
        }
        /// if(ComponentType != 1 || ComponentType != 2)
        /// {
        ///     List<MeshFilter> SubMeshes = new List<MeshFilter>();
        ///     SubMeshes.AddRange(GetComponentsInChildren<MeshFilter>());
        ///     float TempVolume = 0;
        ///     for (int i = 0; i < SubMeshes.Count; i++)
        ///     {
        ///         TempVolume += VolumeOfMesh(SubMeshes[i].mesh)*SubMeshes[i].transform.lossyScale.sqrMagnitude;
        ///     }
        ///     Volume = TempVolume;            
        /// }
        /// else
        /// {
        /// Volume = VolumeOfMesh(GetComponent<MeshFilter>().mesh) * GetComponent<Transform>().lossyScale.sqrMagnitude;
        /// }
        if(ComponentType == 3)
        {
            Mass = 5;
        }
        else
        {

            Volume = VolumeOfMesh(MeshFilter.mesh) * GetComponent<Transform>().lossyScale.sqrMagnitude;
            Mass = Volume * Density;
        }
    }
    #region Volume calculations.
    // Calcualtes the volume of hte mesh
    float VolumeOfMesh(Mesh mesh)
    {
        float volume = 0;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        return Mathf.Abs(volume);
    }

    // Idk the maths behind this all I know is it works soo.
    float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;

        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    #endregion

    #region Work out if we are totally inside another mesh.
    public bool AmIInside(List<MeshCollider>AllMeshes)
    {
        if(MeshFilter == null)
        {
            MeshFilter = this.GetComponent<MeshFilter>();
        }
        bool IsInisde = false;
        AllMeshes.Remove(MeshFilter.GetComponent<MeshCollider>());
        //Debug.Log(MeshFilter.name);
        //Debug.Log(this.transform.position);
        List<Vector3> Points = ShipManager.ConvertVertsToGlobal(MeshFilter, this.transform);
        
        for (int ii = 0; ii < AllMeshes.Count; ii++)
        {
            int pointsNotInSide = 0;
            for (int i = 0; i < Points.Count; i++)
            {
                if(!ShipManager.IsInCollider(AllMeshes[ii].GetComponent<MeshCollider>(), Points[i]))
                {
                    pointsNotInSide += 1;
                }
                if(pointsNotInSide >= 2)
                {
                    break;
                }
            }
            if(pointsNotInSide == 0)
            {
                IsInisde = true;
                break;
            }
        }
        return IsInisde;
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        if (Touching == false && ShipManager.OkToDestroyer)
        {
            Debug.Log("Im not touching anything, ohno time to explode");
            ShipManager.CaclulateShipStats();
            Destroy(gameObject);
            
        }
        if(Health <= 0 && ShipManager.OkToDestroyer)
        {
            Debug.Log("I no health, ohno time to explode");
            Destroy(gameObject);
        }
        
        
    }

    // Fixed Update is called once every physics pluse.
    void FixedUpdate()
    {
        Touching = false;
        
    }

    // Called every Fixed Update IF the collider on this gameObject is toucning another collider.
    void OnTriggerStay()
    {
        Touching = true;
    }

}
