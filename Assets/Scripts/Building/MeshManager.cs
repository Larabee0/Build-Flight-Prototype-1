using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls the mesh in the gameObject
public class MeshManager : MonoBehaviour
{
    Mesh originalMesh;
    [HideInInspector]
    public Mesh clonedMesh;
    MeshFilter meshFilter;

    //[HideInInspector]
    public int[] triangles;
    //[HideInInspector]
    public Vector3[] vertices;

    [HideInInspector]
    public bool isCloned = false;

    [HideInInspector]
    public List<MeshFilter> meshFilterList = null;

    //for editor
    public float radius = 0.2f;
    public float pull = 0.3f;
    public float handleSize = 0.03f;
    public List<int>[] connectedVertices;
    public List<Vector3[]> allTriangleList;
    public bool moveVertexPoint = false;
    
    private MeshVisualizer MeshVisualizer;
    private ModelManger ModelManger;

    //Adding Quads/triangles
    public GameObject Quad;
    public List<GameObject> SpawnedQuads = new List<GameObject>();
    
    public void InitMesh()
    {
        MeshVisualizer = GetComponent<MeshVisualizer>();
        ModelManger = GetComponentInParent<ModelManger>();
        meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.sharedMesh;
        clonedMesh = new Mesh();
        clonedMesh.name = "clone";
        clonedMesh.vertices = originalMesh.vertices;
        clonedMesh.triangles = originalMesh.triangles;
        clonedMesh.normals = originalMesh.normals;
        clonedMesh.uv = originalMesh.uv;
        meshFilter.mesh = clonedMesh;

        vertices = clonedMesh.vertices;
        triangles = clonedMesh.triangles;
        isCloned = true;
        Debug.Log("Initilised MeshManager.cs & cloned mesh.");
    }

    public void ReaquireData()
    {
        if (clonedMesh != null && originalMesh != null)
        {
            vertices = clonedMesh.vertices;
            triangles = clonedMesh.triangles;
        }
    }

    public void Reset()
    {
        if (clonedMesh != null && originalMesh != null)
        {
            clonedMesh.vertices = originalMesh.vertices;
            clonedMesh.triangles = originalMesh.triangles;
            clonedMesh.normals = originalMesh.normals;
            clonedMesh.uv = originalMesh.uv;
            meshFilter.mesh = clonedMesh;

            vertices = clonedMesh.vertices;
            triangles = clonedMesh.triangles;
        }
    }

    public void GetConnectedVertices()
    {
        connectedVertices = new List<int>[vertices.Length];
    }

    public void DoAction(int index, Vector3 localpos)
    {
        PullSimilarVerts(index, localpos);
    }

    // returns list of ints that is related to the targetPt.
    public List<int> FindRelatedVertices(Vector3 targetpt, bool findConnected)
    {
        //List of ints
        List<int> relatedVertices = new List<int>();

        int idx = 0;
        Vector3 pos;

        //loop through triangles array of indices
        for (int t = 0; t < triangles.Length; t++)
        {
            //current idx return from tris
            idx = triangles[t];
            //current pos of the vertex
            pos = vertices[idx];
            //if current pos is the same as target.pt
            if (pos == targetpt)
            {
                //add to list
                relatedVertices.Add(idx);
                // if you find connected vertices
                if (findConnected)
                {
                    //min
                    // - prevent running out of count
                    if (t == 0)
                    {
                        relatedVertices.Add(triangles[t + 1]);
                    }
                    //max
                    //- prevent running out of count
                    if (t == triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                    }
                    //between 1 and max-1
                    // - add idx from triangles before t and after t
                    if (t > 0 && t < triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                        relatedVertices.Add(triangles[t + 1]);
                    }
                }
            }
        }
        //return complied list of ints
        return relatedVertices;
    }

    #region BROKEN find connected verts
    private Vector3 FindConnectedVerts(Vector3 targetVertexPos)
    {
        List<Vector3> connectedVerts = new List<Vector3>();
        connectedVerts.Add(Vector3.zero);
        connectedVerts.Add(Vector3.zero);
        connectedVerts.Add(Vector3.zero);

        int idx = 0;
        int xy = -1; //0
        int xz = -1; //1
        int zy = -1; //2
        Vector3 pos;
        for (int t = 0; t < triangles.Length; t++)
        {
            idx = triangles[t];
            pos = vertices[idx];
            if (pos.x == targetVertexPos.x && pos.y == targetVertexPos.y && xy == -1 && targetVertexPos.z != pos.z)
            {
                xy = t;
                connectedVerts.Insert(0, pos);
            }
            if (pos.x == targetVertexPos.x && pos.z == targetVertexPos.z && xz == -1 && targetVertexPos.y != pos.y)
            {
                xz = t;
                connectedVerts.Insert(1, pos);
            }
            if (pos.z == targetVertexPos.z && pos.y == targetVertexPos.y && zy == -1 && targetVertexPos.x != pos.x)
            {
                zy = t;
                connectedVerts.Insert(2, pos);
            }
            if (xy != -1 && zy != -1 && xz != -1)
            {
                Debug.Log("Broken out of loop");
                break;
            }

        }

        Vector3 newSnappedPos = Vector3.zero;

        if (connectedVerts[0].x == connectedVerts[1].x || connectedVerts[0].x == connectedVerts[2].x)
        {
            newSnappedPos.x = connectedVerts[0].x;
        }
        else if (connectedVerts[1].x == connectedVerts[2].x)
        {
            newSnappedPos.x = connectedVerts[1].x;
        }

        if (connectedVerts[0].y == connectedVerts[1].y || connectedVerts[0].y == connectedVerts[2].y)
        {
            newSnappedPos.y = connectedVerts[0].y;
        }
        else if (connectedVerts[1].y == connectedVerts[2].y)
        {
            newSnappedPos.y = connectedVerts[1].y;
        }

        if (connectedVerts[0].z == connectedVerts[1].z || connectedVerts[0].z == connectedVerts[2].z)
        {
            newSnappedPos.z = connectedVerts[0].z;
        }
        else if (connectedVerts[1].z == connectedVerts[2].z)
        {
            newSnappedPos.z = connectedVerts[1].z;
        }


        //Debug.Log(newSnappedPos);
        return newSnappedPos;
    }
    #endregion
    private void PullOneVert(int index, Vector3 newPos)
    {
        vertices[index] = newPos;
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }

    private void PullSimilarVerts(int index, Vector3 newPos)
    {
        Vector3 targetVertexPos = vertices[index];
        //get list of verts with the same position
        List<int> relatedVertices = FindRelatedVertices(targetVertexPos, false);

        ///for (int i = 0; i < MeshVisualizer.vertsToIngore.Count; i++)
        ///{
        ///    int checker = MeshVisualizer.vertsToIngore[i];
        ///    for (int t = 0; t < relatedVertices.Count; t++)
        ///    {
        ///        if(checker == relatedVertices[t])
        ///        {
        ///
        ///            relatedVertices.RemoveAt(t);
        ///            Debug.Log("Removed conflicting Vert");
        ///        }
        ///    }
        ///}
        ///update the position of each related vert to make sure the mesh isn't broken
        
        foreach (int i in relatedVertices)
        {
            vertices[i] = newPos;
        }
        // update the mesh and recalculate the normals
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }
    // Remember when this script ended here? Cos I don't.

    // function used to combine a two or more meshes
    public void MergeMesh(List<ComponentStatsManager> SecondMesh)
    {
        for (int i = SecondMesh.Count; i < 0; i--)
        {
            if (SecondMesh[i].NoVisualizer)
            {
                SecondMesh.RemoveAt(i);
            }
        }
        // Original transforms of this gameObject, split into 3 cos idk.
        Vector3 OldScale = gameObject.transform.localScale;
        Vector3 OldPos = gameObject.transform.position;
        Quaternion OldRot = gameObject.transform.rotation;
        
        // basically using this the thing form the wiki
        //https://docs.unity3d.com/ScriptReference/Mesh.CombineMeshes.html

        //add the current meshfilter to the list
        //then for each meshfilter in the secondmesh list, add it to the list.
        meshFilterList.Add(meshFilter);
        for (int i = 0; i < SecondMesh.Count; i++)
        {
            meshFilterList.Add(SecondMesh[i].GetComponent<MeshFilter>());
        }

        // For each gameobject in the meshfliter list, accept the first cos thats this.gameObject.
        // Set the parent to this.gameObject.transform aka this.transform. 
        // So this.gameObject is now the parent.
        for (int i = 1; i < meshFilterList.Count; i++)
        {
            meshFilterList[i].transform.SetParent(this.transform);
        }

        // Set this.transform to 0,0,0 or 1x scale for all transforms.
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);

        //create a new array of capacity meshfilter.length
        CombineInstance[] combine = new CombineInstance[meshFilterList.Count];

        // foreach item in meshfilter list do some stuff that idk really about
        // then set the gameobject to inactive
        for (int i = 0; i < meshFilterList.Count; i++)
        {
            combine[i].mesh = meshFilterList[i].sharedMesh;
            combine[i].transform = meshFilterList[i].transform.localToWorldMatrix;
            meshFilterList[i].gameObject.SetActive(false);
        }

        //assign the meshfilter on the current gameobject a new mesh
        //then assign that new mesh the result of the combination helper function
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, false, true);

        //foreach item (except the 1st) in meshfilter list, destroy the gameobject they were attached to
        for (int i = 1; i < SecondMesh.Count; i++)
        {
            Destroy(SecondMesh[i].gameObject);
        }
        ModelManger.SpawnedComponents.Clear();
        ModelManger.GetSpawnedCompoentns();
        // Reset the mesh collider, Then set transforms back to what they were.
        gameObject.GetComponent<MeshCollider>().sharedMesh = transform.GetComponent<MeshFilter>().mesh;
        
        gameObject.transform.localScale = OldScale;
        gameObject.transform.position = OldPos;
        gameObject.transform.rotation = OldRot;

        //set the origingal gameobject to active
        //run the deselect secondary function in the modelmanager to clear it of the gameobjects that no longer exist
        //clear the meshfilter list and then reinitialise the meshmanger script.
        this.gameObject.SetActive(true);
        ModelManger.UIController.Deselect();
        meshFilterList.Clear();
        InitMesh();
        ModelManger.GetSpawnedCompoentns();
    }

    // Spawns a quad. This contains a lot of fixed function.
    public void CreateFace(List<Vector3> NewTrianglesSelection)
    {
        Debug.Log("Creating Face...");
        MeshVisualizer.DeinitializeVertexPoints();
        // Spawn a vertexPoint at the positon point we just created, setting the rotation to the rotation in the prefab vertexPoint.
        GameObject go = (GameObject)Instantiate(Quad, Vector3.zero, Quaternion.identity);
        ModelManger.SpawnedComponents.Add(go.GetComponent<ComponentStatsManager>());
        meshFilterList.Add(go.transform.Find("QuadPrefab").GetComponent<MeshFilter>());
        meshFilterList[0].transform.SetParent(null);
        meshFilterList.Add(go.GetComponent<MeshFilter>());

        CombineInstance[] combine = new CombineInstance[meshFilterList.Count];

        for (int i = 0; i < meshFilterList.Count; i++)
        {
            combine[i].mesh = meshFilterList[i].sharedMesh;
            combine[i].transform = meshFilterList[i].transform.localToWorldMatrix;

            meshFilterList[i].gameObject.SetActive(false);
        }

        meshFilterList[1].mesh = new Mesh();
        meshFilterList[1].mesh.CombineMeshes(combine);

        Destroy(meshFilterList[0].gameObject);

        meshFilterList.Clear();

        go.SetActive(true);
        go.transform.SetParent(gameObject.transform.parent);

        MeshVisualizer GoMeshVisualizer = go.GetComponent<MeshVisualizer>();
        GoMeshVisualizer.disableGUI = true;
        GoMeshVisualizer.enabled = true;
        ModelManger.GetSpawnedCompoentns();
        GoMeshVisualizer.Start();
        GoMeshVisualizer.EditMesh();

        if (GoMeshVisualizer.Cols.Count - 1 >= 0)
        {
            List<Vector3> PointToVertexMap = new List<Vector3>();
            for (int i = 0; i < NewTrianglesSelection.Count; i++)
            {
                PointToVertexMap.Add(NewTrianglesSelection[i]);
            }

            Vector3 pos = new Vector3(3, 3, 3);

            PointToVertexMap.Sort((v1, v2) => (v1 - pos).sqrMagnitude.CompareTo((v2 - pos).sqrMagnitude));
            PointToVertexMap.Reverse();

            for (int i = 0; i < NewTrianglesSelection.Count; i++)
            {
                GoMeshVisualizer.DoActionRelay(i, PointToVertexMap[i]);
            }

            GoMeshVisualizer.EditMesh();
            ModelManger.UIController.EditMesh();

            List<ComponentStatsManager> Temp1 = new List<ComponentStatsManager> { GoMeshVisualizer.gameObject.GetComponent<ComponentStatsManager>() };
            for (int i = 0; i < Temp1.Count; i++)
            {
                Debug.Log(Temp1[i].gameObject.name);
            }
            MergeMesh(Temp1);

            ModelManger.SelectMain(gameObject);
            ModelManger.UIController.EditMesh();
        }
        else
        {
            Debug.LogError("Meshvisualizer failed to start or editMesh.");
        }
    }

    // This removes a single triangle.
    public void RemoveFace(List<int> FaceToRemove, bool DoubleTriangle)
    {
        Debug.Log("Vertices Length(Pre-Deletion): " + vertices.Length);

        MeshVisualizer.FaceDeselect();
        MeshVisualizer.DeinitializeVertexPoints();

        int cutPoint = -1;
        int cutsToMake = 3;

        List<int> TrianglelistTemp = new List<int>();
        for (int i = 0; i < triangles.Length; i++)
        {
            TrianglelistTemp.Add(triangles[i]);
        }
        // Searches through the triangle list to find the triangle to delete.
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (triangles[i] == FaceToRemove[0])
            {
                if (triangles[i + 1] == FaceToRemove[1])
                {
                    if (triangles[i + 2] == FaceToRemove[2])
                    {
                        // Once it finds it, set the cut point and break the loop.
                        cutPoint = i;
                        break;
                    }
                }
            }
        }
        // Remove the values from the list at position i, do this 3 timess as the vertices are in order,
        // and there are 3 vertices to a triangle.
        for (int i = 0; i < cutsToMake; i++)
        {
            TrianglelistTemp.RemoveAt(cutPoint);
        }

        // If we are checking for a double faced mesh.
        if (DoubleTriangle)
        {
            // Re
            for (int i = 0; i < TrianglelistTemp.Count; i += 3)
            {
                if (TrianglelistTemp[i] == FaceToRemove[3])
                {
                    if (TrianglelistTemp[i + 1] == FaceToRemove[4])
                    {
                        if (TrianglelistTemp[i + 2] == FaceToRemove[5])
                        {
                            cutPoint = i;
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < cutsToMake; i++)
            {
                TrianglelistTemp.RemoveAt(cutPoint);
            }
        }

        List<int> VerticesToRemove = new List<int>();
        List<Vector3> TempVertices = new List<Vector3>();

        for (int i = 0; i < vertices.Length; i++)
        {
            TempVertices.Add(vertices[i]);
            if (!TrianglelistTemp.Contains(i))
            {
                VerticesToRemove.Add(i);
            }
        }

        int tempVertex;
        int Offset = 0;

        for (int i = 0; i < VerticesToRemove.Count; i++)
        {
            tempVertex = VerticesToRemove[i] - Offset;
            for (int j = 0; j < TrianglelistTemp.Count; j++)
            {
                
                if (TrianglelistTemp[j] >= tempVertex)
                {
                    TrianglelistTemp[j] -= 1;
                }
            }
            
            Offset += 1;
        }

        VerticesToRemove.Reverse();

        for (int i = 0; i < VerticesToRemove.Count; i++)
        {
            TempVertices.RemoveAt(VerticesToRemove[i]);
        }

        Vector3[] newVertices = new Vector3[TempVertices.Count];

        for (int i = 0; i < TempVertices.Count; i++)
        {
            newVertices[i] = TempVertices[i];
        }

        int[] newTriangles = new int[TrianglelistTemp.Count];

        for (int i = 0; i < TrianglelistTemp.Count; i++)
        {
            newTriangles[i] = TrianglelistTemp[i];
        }
        
        TempVertices.Clear();
        TrianglelistTemp.Clear();

        vertices = newVertices;
        triangles = newTriangles;
        clonedMesh.triangles = triangles;
        clonedMesh.vertices = vertices;
        
        clonedMesh.RecalculateBounds();
        clonedMesh.RecalculateNormals();
        clonedMesh.RecalculateTangents();
        
        MeshVisualizer.RequireData();
        
        MeshVisualizer.InitiliseVertexPoints();
    }
}
