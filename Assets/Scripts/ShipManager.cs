using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipManager : MonoBehaviour
{
    [Header("Script References")]
    public ModelManger ModelManger;
    public Hud FlightHudScript;
    public MouseFlightController MouseFlight;
    public EditButtonHandler BuildHudScript;
    public Plane Plane;

    [Header("Ship Components")]
    public List<MeshFilter> ModelMeshes = new List<MeshFilter>();
    public List<ThrusterTest> Thrusters = new List<ThrusterTest>();
    public List<WeaponTag> Weapons = new List<WeaponTag>();
    public List<TurretAim> Turrets = new List<TurretAim>();
    public List<ComponentStatsManager> Components = new List<ComponentStatsManager>();

    public List<MeshCollider> MeshColliders = new List<MeshCollider>();

    [Header("Flight Control")]
    public GameObject CameraRig;
    public GameObject FlightRig;
    public Rigidbody ShipRigidbody;

    [Header("Settings")]
    public bool OkToDestroyer = false;

    [Header("Debug Info")]
    public float shipMass = 0;
    public float LocalThrust = 0;


    // Update is called once per frame
    void Update()
    {
        if(Thrusters.Count -1 >= 0)
        {
            LocalThrust = 0f;
            for (int i = 0; i < Thrusters.Count; i++)
            {
                LocalThrust += Thrusters[i].ThrusterOutput;
            }
        }
    }

    // Fixed Update is called once every physics pluse.
    void FixedUpdate()
    {
        Plane.thrust = LocalThrust;
    }

    // When flight mode is started;
    public void FlightMode()
    {
        // Shuts down the ModelManger and moves ModelManger.gameObject to a child of this.gameObject.
        ModelManger.BuildModeStopper();
        ModelManger.ShipManager = this;

        ModelManger.GetMeshColliders();
        
        ModelManger.gameObject.transform.SetParent(gameObject.transform);
        
        // Enable the flight rig, and make the camera rig a child of it.
        FlightRig.SetActive(true);
        CameraRig.transform.SetParent(FlightRig.transform);
        CameraRig.transform.Find("Sphere").gameObject.SetActive(false);        
        CameraRig.transform.localRotation = Quaternion.Euler(Vector3.zero);
        CameraRig.transform.localPosition = Vector3.zero;      
        
        // Set up the camera to default values. And switch UIs.
        Transform Camera = CameraRig.transform.Find("Main Camera").transform;
        Camera.GetComponent<CameraManger>().enabled = false;
        Camera.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Camera.transform.localPosition = new Vector3(0f, 9f, -30f);
        BuildHudScript.gameObject.SetActive(false);
        FlightHudScript.gameObject.SetActive(true);
        // Enable the plane script.
        Plane.enabled = true;
        // Calculate various stats of the ship, aquire MeshFilters and colliders.
        CaclulateShipStats();
        GetWeaponsAndEnable();
        GetTurrets();
        SetModelCollider();
        
        // Enable the rigibody and set its collision detectionmode to continuous.
        ShipRigidbody.isKinematic = false;
        ShipRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    // Calculates ship health stats.
    public void CaclulateShipStats()
    {
        // This block works out if any of the components are inside each other. and disables components that are.
        Components.Clear();
        Components.AddRange(gameObject.GetComponentsInChildren<ComponentStatsManager>());
        for (int i = 0; i < Components.Count; i++)
        {
            if(Components[i].ComponentType != 3)
            {
                Components[i].ShipManager = this;
                if (Components[i].AmIInside(MeshColliders))
                {
                    Components[i].gameObject.SetActive(false);
                    Components.RemoveAt(i);
                    i -= 1;
                }
            }
            
        }
        ModelMeshes.Clear();
        Components.AddRange(gameObject.GetComponentsInChildren<ComponentStatsManager>());
        ModelMeshes.AddRange(ModelManger.transform.GetComponentsInChildren<MeshFilter>());

        // This works out the mass.
        shipMass = 0;
        for (int i = 0; i < Components.Count; i++)
        {
            Components[i].CalculateStats();

            shipMass += Components[i].Mass;
            if (Components[i].ComponentType == 2)
            {
                Components[i].Plane = Plane;
                Thrusters.Add(Components[i].ThrusterComp);
                Components[i].ThrusterComp.enabled = true;
            }

            Components[i].enabled = true;
        }
        
        ShipRigidbody.mass = shipMass;
    }

    // When we go into build mode;
    public void BuildMode()
    {
        // Disable the component stat managers, then clear the components list, and then disable and clear weapons and turrets.
        for (int i = 0; i < Components.Count; i++)
        {
            Components[i].enabled = false;
        }
        Components.Clear();

        ClearWeaponsAndDisable();
        ClearTurrets();
        ClearThrusters();
        // Disable the plane script, and disable the rigidbody.
        Plane.enabled = false;
        ShipRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        ShipRigidbody.isKinematic = true;
        // Disable the and remove the mesh from the mesh collider on the modelmanager.
        ModelManger.gameObject.GetComponent<MeshCollider>().enabled = false;
        ModelManger.gameObject.GetComponent<MeshCollider>().sharedMesh = null;
        // De-parent the cameraRig, disable the flight rig, enable the focal point and set the cameras position to build mode defaults.
        CameraRig.transform.SetParent(null);
        FlightRig.SetActive(false);
        CameraRig.transform.Find("Sphere").gameObject.SetActive(true);
        
        CameraRig.transform.rotation = transform.rotation;
        CameraRig.transform.position = transform.position;

        Transform Camera = CameraRig.transform.Find("Main Camera").transform;
        Camera.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Camera.transform.localPosition = new Vector3(0f, 0f, -2.5f);
        Camera.GetComponent<CameraManger>().enabled = true;
        // De-parent the modelmanager.gameObject, and Switch UIs.
        ModelManger.gameObject.transform.SetParent(null);
        FlightHudScript.gameObject.SetActive(false);
        BuildHudScript.gameObject.SetActive(true);
        
        ModelManger.enabled = true;
    }

    // This sets the ship to Ship collider on the modelmanager.gameObject.
    // Basically it combines all the colliders of all the components into 1 mesh, it will be a convex mesh collider for collision reasons.
    void SetModelCollider()
    {
        for (int i = 0; i < ModelMeshes.Count; i++)
        {
            if (ModelMeshes[i].GetComponent<ComponentStatsManager>() == null)
            {
                ModelMeshes.Remove(ModelMeshes[i]);
                i -= 1;
            }
            else
            {
                if (ModelMeshes[i].GetComponent<ComponentStatsManager>().ComponentType != 1 || ModelMeshes[i].GetComponent<ComponentStatsManager>().ComponentType != 2)
                {
                    ModelMeshes.Remove(ModelMeshes[i]);
                    i -= 1;
                }
            }
        }
        CombineInstance[] combine = new CombineInstance[ModelMeshes.Count];

        for (int i = 0; i < ModelMeshes.Count; i++)
        {
            combine[i].mesh = ModelMeshes[i].sharedMesh;
            combine[i].transform = ModelMeshes[i].transform.localToWorldMatrix;
            ModelMeshes[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < ModelMeshes.Count; i++)
        {
            ModelMeshes[i].gameObject.SetActive(true);
            ModelMeshes[i].gameObject.GetComponent<ComponentStatsManager>().enabled = true;
        }
        ModelManger.gameObject.GetComponent<MeshCollider>().sharedMesh = new Mesh();
        ModelManger.gameObject.GetComponent<MeshCollider>().sharedMesh.CombineMeshes(combine);
        ModelManger.gameObject.GetComponent<MeshCollider>().enabled = true;
        
        ModelMeshes.Clear();
    }

    // Gets and enables the weapons.
    void GetWeaponsAndEnable()
    {
        Weapons.AddRange(GetComponentsInChildren<WeaponTag>());
        for (int i = 0; i < Weapons.Count; i++)
        {
            Weapons[i].EnableWeapon = true;
            Weapons[i].ShipRigidbody = ShipRigidbody;
        }
    }

    // Disables and clears weapon list.
    void ClearWeaponsAndDisable()
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            Weapons[i].EnableWeapon = false;
        }
        Weapons.Clear();
    }

    // Get turrets and enable them.
    void GetTurrets()
    {
        Turrets.AddRange(GetComponentsInChildren<TurretAim>());
        for (int i = 0; i < Turrets.Count; i++)
        {
            Turrets[i].enabled = true;
            Turrets[i].Root = this;
        }
    }

    // Disable turrets and clears them.
    void ClearTurrets()
    {
        for (int i = 0; i < Turrets.Count; i++)
        {
            Turrets[i].enabled = false;
            Turrets[i].Root = null;
        }
        Turrets.Clear();
    }

    void ClearThrusters()
    {
        for (int i = 0; i < Thrusters.Count; i++)
        {
            Thrusters[i].enabled = false;
        }
        Thrusters.Clear();
    }


    // These three functions are used to workout if a point is inside mesh.
    // They are used by component stat managers to work out if they are inside something.
    public bool IsInCollider(MeshCollider other, Vector3 point)
    {
        Vector3 from = (Vector3.up * 5000f);
        Vector3 dir = (from - point).normalized;
        float dist = Vector3.Distance(from, point);
        //fwd      
        int hit_count = Cast_Till(from, point, other);
        //back
        hit_count += Cast_Till(point, point + (dir * dist), other);

        if (hit_count % 2 == 1)
        {
            return (true);
        }
        return (false);
    }

    int Cast_Till(Vector3 from, Vector3 to, MeshCollider other)
    {
        int counter = 0;
        Vector3 dir = (to - from).normalized;
        float dist = Vector3.Distance(from, to);
        bool Break = false;
        while (!Break)
        {
            Break = true;
            RaycastHit[] hit = Physics.RaycastAll(from, dir, dist);
            for (int tt = 0; tt < hit.Length; tt++)
            {
                if (hit[tt].collider == other)
                {
                    counter++;
                    from = hit[tt].point + dir.normalized * .001f;
                    dist = Vector3.Distance(from, to);
                    Break = false;
                    break;
                }
            }
        }
        return (counter);
    }

    public List<Vector3> ConvertVertsToGlobal(MeshFilter mesh, Transform transform)
    {
        List<Vector3> GlobalVerts = new List<Vector3>();
        for (int j = 0; j < mesh.sharedMesh.vertexCount; j++)
        {
            if (!GlobalVerts.Contains(transform.TransformPoint(mesh.sharedMesh.vertices[j])))
            {
                GlobalVerts.Add(transform.TransformPoint(mesh.sharedMesh.vertices[j]));
            }
        }
        return GlobalVerts;
    }

}
