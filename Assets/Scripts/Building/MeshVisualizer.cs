using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Managers the MeshManger in the gameObject.
public class MeshVisualizer : MonoBehaviour
{
    #region Variables
    //references to other scripts
    [Header("Script References")]
    public ModelManger Root;

    [SerializeField] private MeshManager mesh;
    [SerializeField] private HandleScript handle;

    [Header("Vertex Point Apparence")]
    public GameObject vertexPoint;
    public GameObject handles;
    public LayerMask VertexLayerMask = 8;
    public LayerMask HandleLayerMask = 9;

    public Material SelectedVertexMat;
    public Material DeselectedVertexMat;
    public Material VertexCreationMat;

    [HideInInspector]
    public LayerMask VertexLayerMask2 = 9;


    [Header("Script Settings")]
    public bool roundingEnabled = false;
    public bool disableGUI = false;
    public bool TrinagleAdding = false;
    public bool InplacmentMode = true;
    public bool VertexSelectionMode = true;
    public int roundingAccuracy = 1;

    [Header("Debug")]
    public List<Collider> Cols;
    public List<int> NewTrianglesSelection = new List<int>();
    public List<GameObject> NewTrianglePoints = new List<GameObject>();


    public List<int> Relatedvert = new List<int>();
    public List<int> AllFaces = new List<int>();

    public List<Vector3> vertexPosSelection = new List<Vector3>();

    public List<int> vertexSelection = new List<int>();

    public int Pass = -1;
    public int Runs = 0;

    [SerializeField] private Vector3 NewTriangleReferencePosition = Vector3.zero;
    private Transform handleTransform;

    // Per axis movement.
    private bool perAxisMovement = false;
    private GameObject PerAxisHandle = null;
    private readonly List<GameObject> Handles = new List<GameObject>();

    // Other mesh vertex alignment.
    private bool Snapped = false;

    // New Quads   
    private GameObject TempTrianglePoint = null;

    // General vertex point movement & selection.
    [HideInInspector]
    public int Selection = -10;

    //[HideInInspector]
    public int vertex = -1;

    private readonly List<List<int>> relatedVertsMatrix = new List<List<int>>();
    private List<int> relatedVertices;
    private Vector3 clampPos = Vector3.zero;
    private Vector3 colliderPos = Vector3.zero;
    private Vector3 mouseLastFrame = Vector3.zero;
    private bool colliderFound = false;
    private bool Stopmovement = false;
    #endregion

    //when enabled these things will happen
    public void Start()
    {
        // Get the handle script from the handle child.
        // Get the mesh manager component form the same game object.
        handle = GetComponentInChildren<HandleScript>();
        mesh = GetComponent<MeshManager>();
        Root = GetComponentInParent<ModelManger>();

        // Initialize the clone.
        mesh.InitMesh();
    }

    // Every frame these things will happen:
    void Update()
    {
        if (Root.MeshInEdit)
        {
            // Fire 1 (Mouse down functions)
            if (Input.GetButtonUp("Fire1"))
            {
                CheckLeftMouseUp();
            }
            if (Input.GetButtonDown("Fire1"))
            {
                CheckLeftMouseDown();
            }
            if (Input.GetButton("Fire1"))
            {
                CheckLeftMouse();
            }

            if (Input.GetButtonUp("Fire2"))
            {
                TriangleRightMouse();
            }

            // Selecting quad points, only works in triangle mode.
            if (Input.GetKeyDown(KeyCode.F) && TrinagleAdding)
            {
                if (Root.UIController.QuadSelection.GetComponent<Toggle>().isOn)
                    Root.UIController.QuadSelection.GetComponent<Toggle>().isOn = false;
                else if (!Root.UIController.QuadSelection.GetComponent<Toggle>().isOn)
                    Root.UIController.QuadSelection.GetComponent<Toggle>().isOn = true;
            }

            //Selection Box
            if (Input.GetKeyDown(KeyCode.F) && !TrinagleAdding)
            {
                SelectionModeToggler();
            }
            // Move
            if (Input.GetKeyDown(KeyCode.E) && !TrinagleAdding)
            {
                Root.UIController.LocationButton();
                if (VertexSelectionMode)
                {
                    SelectionModeToggler();
                }
            }

            // Toggle into triangle mode.
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (Root.UIController.AddQuadButton.GetComponent<Toggle>().isOn)
                {
                    Root.UIController.AddQuadButton.GetComponent<Toggle>().isOn = false;
                }
                else if (!Root.UIController.AddQuadButton.GetComponent<Toggle>().isOn)
                {
                    Root.UIController.AddQuadButton.GetComponent<Toggle>().isOn = true;
                }
            }

            // Remove triangle point only in triangle adding mode.
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (TrinagleAdding && !InplacmentMode)
                {
                    RemoveTrianglePoint();
                }
                else
                {
                    RemoveFace();
                }
            }

            // Create Quad shortcut. Only works with 4 tirangle points.
            if (Input.GetKeyDown(KeyCode.N))
            {
                if (NewTrianglePoints.Count == 4)
                {
                    List<Vector3> Points = new List<Vector3>();
                    for (int i = 0; i < NewTrianglePoints.Count; i++)
                    {
                        Points.Add(NewTrianglePoints[i].transform.localPosition);
                    }
                    mesh.CreateFace(Points);
                }
            }

            // Select a triangle and iterate through selection for deleting a triangle.
            if (Input.GetKeyDown(KeyCode.R) && vertexSelection.Count - 1 >= 0)
            {
                TriangleSelection();
            }

            // Resets the pass to -1.
            if (Input.GetKeyDown(KeyCode.R) && Input.GetButton("Left Control"))
            {
                Pass = -1;
            }
            // If mesh.moveVertexPoint is true and if LeftShift is not being pressed and if disableGUI is false:
            if (mesh.moveVertexPoint && !disableGUI && !TrinagleAdding)
            {
                // Update the positon of all vertex points.
                UpdateVertexPoints();
            }

            if (TrinagleAdding)
            {
                Root.UniSelection.Objects = NewTrianglePoints;
            }
        }
    }
    // Left mouse down function.
    void CheckLeftMouseDown()
    {
        // If this is the main visualizer.
        if (mesh.moveVertexPoint && !disableGUI)
        {
            // if the mouse pointer is over a UI element then end this left mouse down function
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            else
            {
                Root.UniSelection.SelectionMouseDown(DeselectedVertexMat,false);

                // Set stopmovement to false, then run the ray cast to see if we clicked on a valid item.
                Stopmovement = false;
                //GetVertexPoint();


                if (vertexSelection.Count - 1 >= 0 || NewTrianglesSelection.Count -1 >=0)
                {
                    GetVertexPoint();
                }
                // if we did not find anything deselect everything and end the function.s
                //if ((!TrinagleAdding && !colliderFound) || (TrinagleAdding && !InplacmentMode && !colliderFound))
                //{
                //    VertexDeselection();
                //    FaceDeselect(); //Deselection done in Mouse Up now.
                //    return;
                //}
            }


            // if we got a vertex or a triangle point has been selected.
            
            if ((vertex != -5 || TempTrianglePoint != null) && vertexSelection.Contains(vertex) || TempTrianglePoint != null)
            {
                // Distance to screen common.
                float distanceToScreen = float.NaN;

                // If vertexPosSelection is greater than or equal to 0:
                if (vertexPosSelection.Count - 1 >= 0)
                {
                    // Get the Z component of the distance to the primary vertexPoint. (item 0 in vertexPosSelection/vertexSelection).
                    distanceToScreen = Camera.main.WorldToScreenPoint(vertexPosSelection[0]).z;
                }
                // If NewTrianglesSelection is greater than or equal to 0:
                if (NewTrianglesSelection.Count - 1 >= 0)
                {
                    NewTriangleReferencePosition = NewTrianglePoints[NewTrianglesSelection[0]].transform.localPosition;
                    // Get the Z component of the distance to the primary vertexPoint. (item 0 in vertexPosSelection/vertexSelection).
                    distanceToScreen = Camera.main.WorldToScreenPoint(NewTriangleReferencePosition).z;
                }
                // If Per AxisHandle is NOT equal to null AND( NewTrianglesSelection OR vertexPosSelection length is greater than).
                if (PerAxisHandle != null && (NewTrianglesSelection.Count - 1 >= 0 || vertexPosSelection.Count - 1 >= 0))
                {
                    // distanceToScreen is equal to the z component of the distance from the handle clicked on.
                    distanceToScreen = Camera.main.WorldToScreenPoint(PerAxisHandle.transform.localPosition).z;
                }
                // Convert where the mouse is into a positon in the world, assign it to the Vector 3 mouseLastFrame.
                mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
            }
        }
    }

    // General left mouse is down/being held down.
    void CheckLeftMouse()
    {
        // if this is the main GUI.
        if (!disableGUI)
        {
            
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                //if (Input.GetButton("Left Control"))
                //{
                //    Root.UniSelection.SelectionMouse();
                //}
                // If mesh move vertex point is allowed and we arent in triangle adding mode.
                if (mesh.moveVertexPoint && !TrinagleAdding && !VertexSelectionMode)
                {
                    // If we have a collider from the mouse down function AND stop movement is false.
                    if (colliderFound && !Stopmovement)
                    {
                        // if the vertex index is valid.
                        if (vertex >= 0)
                        {
                            // if vertexPosSelection contains the current collider position
                            if (vertexPosSelection.Contains(colliderPos))
                            {
                                // Move the  selected vertices.
                                MoveVertexPoint();
                            }
                        }
                    }
                }
                // If we are in Triangle adding mode.
                else if (TrinagleAdding && !InplacmentMode && TempTrianglePoint!=null)
                {
                    // If NewTrianglesSelection is greater than or equal to 0:
                    if (NewTrianglesSelection.Count - 1 >= 0)
                    {
                        // Move the selected triangle points.
                        MoveVertexPoint();
                        // For some reason, triangle points need mouse last frame updated each frame after MoveVertexPoint has finished execution.
                        // Get the Z component of the distance to the primary vertexPoint. (item 0 in NewTrianglesSelection.
                        float distanceToScreen = Camera.main.WorldToScreenPoint(NewTrianglePoints[NewTrianglesSelection[0]].transform.localPosition).z;
                        
                        // If Per AxisHandle is NOT equal to null then:
                        if (PerAxisHandle != null)
                        {
                            // distanceToScreen is equal to the z component of the distance from the handle clicked on.
                            distanceToScreen = Camera.main.WorldToScreenPoint(PerAxisHandle.transform.localPosition).z;
                        }
                        // Convert where the mouse is into a positon in the world, assign it to the Vector 3 mouseLastFrame.
                        mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
                        
                    }
                    else
                    {
                        Root.UniSelection.SelectionMouse();
                    }
                }
                else
                {
                    Root.UniSelection.SelectionMouse();
                }
            }
        }
    }

    // Function tha runs when the left mouse button does up.
    void CheckLeftMouseUp()
    {
        // if this is the main MeshVisualizer.
        if (!disableGUI)
        {
            List<GameObject> TempList = Root.UniSelection.SelectionMouseUp();
            if (TempList.Count - 1 >= 0)
            {
                if (TempList.Count - 1 == 0)
                {
                    SelectTranslation(TempList);
                }
                else if (TempList.Count - 1 >= 0)
                {
                    //Debug.Log(TempList.Count - 1);
                    BoxSelectionTranslation(TempList);
                }

            }
            else
            {
                if ((!TrinagleAdding && !colliderFound) || (TrinagleAdding && !InplacmentMode))
                {
                    VertexDeselection();
                    FaceDeselect();
                    return;
                }
            }
            HaultPointMovement();
        }
    }

    void HaultPointMovement()
    {
        // IF moveVertexPoints is allowed and trinagle adding mode is disabled.
        if (mesh.moveVertexPoint && !TrinagleAdding)
        {
            // PerAxisHandle set to null, Snapped set to false, Stopmovement is set to true.
            PerAxisHandle = null;
            Snapped = false;
            Stopmovement = true;
            // For each item in vertexSelection:
            for (int i = 0; i < vertexSelection.Count; i++)
            {
                // Update the Position held in vertexPosSelection to the new position of the vertex Point.
                vertexPosSelection[i] = Cols[vertexSelection[i]].transform.localPosition;
            }
            // If vertexPosSelection contains items
            if (vertexPosSelection.Count - 1 >= 0)
            {
                //for each item in relatedVertices count, set the gameobject layer back to vertex point instead of ingore raycast.
                // for each item in relatedVertices.
                for (int j = 0; j < relatedVertsMatrix.Count; j++)
                {
                    for (int i = 0; i < relatedVertsMatrix[j].Count; i++)
                    {
                        // Set the gameObject layer to ingore ray cast.
                        Cols[relatedVertsMatrix[j][i]].gameObject.layer = 8;
                    }
                }
            }
        }
        else if (TrinagleAdding)
        {
            // If we are in triangle adding mode and we have at least 1 trinalge selected.
            if (NewTrianglesSelection.Count - 1 >= 0)
            {
                //Set PerAxisHandle to null and Update the NewTriangleReferencePosition.
                PerAxisHandle = null;
                NewTriangleReferencePosition = NewTrianglePoints[NewTrianglesSelection[0]].transform.localPosition;
                for (int i = 0; i < NewTrianglePoints.Count; i++)
                {
                    NewTrianglePoints[i].layer = 8;
                }
            }
        }
    }

    void TriangleRightMouse()
    {
        if (InplacmentMode && !EventSystem.current.IsPointerOverGameObject() && (InplacmentMode && TrinagleAdding))
        {
            // Instantiate a vertex point at index -10 (aka a triangle point at current camera focal point.
            InstantiateVertexPoint(-10);
        }
    }

    void SelectTranslation(List<GameObject> Selection)
    {
        if(TrinagleAdding && !InplacmentMode)
        {
            TriangleAddingMouseDown(Selection[0]);
        }
        else
        {
            TryGetVertPoint(Selection[0]);
            VertexMouseDown(false);
        }   
    }

    void BoxSelectionTranslation(List<GameObject> Selection)
    {
        if (TrinagleAdding && !InplacmentMode)
        {
            TriangleAddingMouseDown(Selection[0]);
            for (int i = 1; i < Selection.Count; i++)
            {
                TriangleAddingSecondaySelection(Selection[i]);
            }
        }
        else
        {
            TryGetVertPoint(Selection[0]);
            VertexMouseDown(false);
            for (int i = 1; i < Selection.Count; i++)
            {
                TryGetVertPoint(Selection[i]);
                Debug.Log(vertex);
                VertexMouseDown(true);
            }
        }
        
    }


    ///void TryGetHandle()
    ///{
    ///    // Set colliderFound to False.
    ///    // Run the RayCast helper, and assign its output to theCollider.
    ///    colliderFound = false;
    ///    Collider theCollider = Root.DoRaycast();
    ///    // If we didn't get a collider end function here
    ///    if (theCollider != null)
    ///    {
    ///        // If the Collider is on the handle layer mask:
    ///        int maskForThisHitObject = 1 << theCollider.gameObject.layer;
    ///        if ((maskForThisHitObject & HandleLayerMask) != 0)
    ///        {
    ///            // If we arent in triangle adding mode.
    ///            if (!TrinagleAdding)
    ///            {
    ///                // colliderPos is assigned theCollider's gameObject Positon.
    ///                colliderPos = theCollider.transform.parent.transform.parent.localPosition;
    ///                //for every vertex in vertices array
    ///                for (int i = 0; i < mesh.vertices.Length; i++)
    ///                {
    ///                    // If the colliderPos is equal to the Vertex's positon.
    ///                    if (colliderPos == mesh.vertices[i])
    ///                    {
    ///                        // vertex var is assigned the current vertex in mesh.vertice, vertex and position "i".
    ///                        // colliderPos is reassigned the vertex position at index "i".
    ///                        // colliderFound is set to true, we have found a valid collider.
    ///                        // PerAxisHandle is set to theCollider's gameObject.
    ///                        // Stop the for loop.
    ///                        vertex = i;
    ///                        colliderPos = mesh.vertices[i];
    ///                        colliderFound = true;
    ///                        PerAxisHandle = theCollider.gameObject;
    ///                        break;
    ///                    }
    ///                }
    ///            }
    ///            // If we are in triangle adding mode AND we aren't InplacementMode.
    ///            else if (TrinagleAdding && !InplacmentMode)
    ///            {
    ///                // Per Axis Handle is set to the gameobject we clicked on, collider found is set to true.
    ///                PerAxisHandle = theCollider.gameObject;
    ///                colliderFound = true;
    ///            }
    ///        }
    ///    }
    ///    // Else set the vertex to -5.
    ///    else
    ///    {
    ///        vertex = -5;
    ///    }
    ///}


    #region Adding and removing Triangles.
    // Selecting triangles for deletion.
    void TriangleSelection()
    {
        if (Pass == -1)
        {
            Relatedvert.Clear();
            AllFaces.Clear();
            for (int i = 0; i < Cols.Count; i++)
            {
                if (Cols[i].transform.localPosition == vertexPosSelection[0])
                {
                    Relatedvert.Add(i);
                    Pass = 0;
                }
            }

            for (int j = 0; j < Relatedvert.Count; j++)
            {
                for (int i = 0; i < mesh.triangles.Length; i++)
                {
                    if (Relatedvert[j] == mesh.triangles[i])
                    {
                        if (i % 3 == 0 || i == 0)
                        {
                            AllFaces.Add(mesh.triangles[i]);
                            AllFaces.Add(mesh.triangles[i + 1]);
                            AllFaces.Add(mesh.triangles[i + 2]);
                        }
                        else if ((i + 2) % 3 == 0)
                        {
                            AllFaces.Add(mesh.triangles[i - 1]);
                            AllFaces.Add(mesh.triangles[i]);
                            AllFaces.Add(mesh.triangles[i + 1]);
                        }
                        else if ((i + 1) % 3 == 0)
                        {
                            AllFaces.Add(mesh.triangles[i - 2]);
                            AllFaces.Add(mesh.triangles[i - 1]);
                            AllFaces.Add(mesh.triangles[i]);
                        }
                    }
                }
            }

            TriangleSelection();
        }
        else
        {
            VertexDeselection();

            if (Pass <= (AllFaces.Count - 1))
            {
                if (Relatedvert.Contains(AllFaces[Pass]))
                {
                    vertex = Relatedvert[0];
                    SelectPrimaryVert();
                    vertex = AllFaces[Pass + 1];
                    SelectSecondaryVert();
                    vertex = AllFaces[Pass + 2];
                    SelectSecondaryVert();
                    vertex = -1;
                }
                else if (Relatedvert.Contains(AllFaces[Pass + 1]))
                {
                    vertex = Relatedvert[0];
                    SelectPrimaryVert();
                    vertex = AllFaces[Pass];
                    SelectSecondaryVert();
                    vertex = AllFaces[Pass + 2];
                    SelectSecondaryVert();
                    vertex = -1;
                }
                else if (Relatedvert.Contains(AllFaces[Pass + 2]))
                {
                    vertex = Relatedvert[0];
                    SelectPrimaryVert();
                    vertex = AllFaces[Pass + 1];
                    SelectSecondaryVert();
                    vertex = AllFaces[Pass];
                    SelectSecondaryVert();
                    vertex = -1;
                }
                Pass += 3;
            }
            if (Pass > AllFaces.Count - 1)
            {
                Pass = 0;
            }
        }
    }

    
    /// //This actually finds a single Faced Quad, instad of a Double Faced Triangle. Useful but not what I meant ot make.
    ///(bool, int) CheckForSingleFacing()
    ///{
    ///    int PriTriStart = -1;
    ///    int SecTriStart = -1;
    ///
    ///    for (int i = 0; i < mesh.triangles.Length; i += 3)
    ///    {
    ///        if (mesh.triangles[i] == AllFaces[Pass - 3])
    ///        {
    ///            if (mesh.triangles[i + 1] == AllFaces[Pass - 2])
    ///            {
    ///                if (mesh.triangles[i + 2] == AllFaces[Pass - 1])
    ///                {
    ///                    PriTriStart = i;
    ///                    break;
    ///                }
    ///            }
    ///        }
    ///    }
    ///
    ///    if (mesh.triangles[PriTriStart + 3] == AllFaces[Pass - 2])
    ///    {
    ///        if (mesh.triangles[PriTriStart + 4] == AllFaces[Pass - 3])
    ///        {
    ///            if (mesh.triangles[PriTriStart + 5] != AllFaces[Pass - 1])
    ///            {
    ///                SecTriStart = PriTriStart + 3;
    ///                Debug.Log(mesh.triangles[PriTriStart].ToString() +
    ///                mesh.triangles[PriTriStart + 1].ToString() +
    ///                mesh.triangles[PriTriStart + 2].ToString());
    ///                Debug.Log(mesh.triangles[SecTriStart].ToString() +
    ///                mesh.triangles[SecTriStart + 1].ToString() +
    ///                mesh.triangles[SecTriStart + 2].ToString());
    ///                return (true, SecTriStart);
    ///            }
    ///        }
    ///    }
    ///    else if (mesh.triangles[PriTriStart - 3] == AllFaces[Pass - 2])
    ///    {
    ///        if (mesh.triangles[PriTriStart - 4] == AllFaces[Pass - 3])
    ///        {
    ///            if (mesh.triangles[PriTriStart - 5] != AllFaces[Pass - 1])
    ///            {
    ///                SecTriStart = PriTriStart - 3;
    ///                Debug.Log(mesh.triangles[PriTriStart].ToString() +
    ///                mesh.triangles[PriTriStart + 1].ToString() +
    ///                mesh.triangles[PriTriStart + 2].ToString());
    ///                Debug.Log(mesh.triangles[SecTriStart].ToString() +
    ///                mesh.triangles[SecTriStart + 1].ToString() +
    ///                mesh.triangles[SecTriStart + 2].ToString());
    ///                return (true, SecTriStart);
    ///            }
    ///        }
    ///    }
    ///    return (false, SecTriStart);
    ///
    ///}


    //check if the triangle is being backface culled
    (bool, int) CheckForDoubleTriangle()
    {
        int SecondTriangle = -1;
        int Vert1 = -1;
        int Vert2 = -1;
        int Vert3 = -1;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            if (i != AllFaces[Pass - 3] && i != AllFaces[Pass - 2] && i != AllFaces[Pass - 1])
            {
                if (mesh.vertices[i] == mesh.vertices[AllFaces[Pass - 3]])
                {
                    Vert2 = i;
                }
                if (mesh.vertices[i] == mesh.vertices[AllFaces[Pass - 2]])
                {
                    Vert1 = i;
                }
                if (mesh.vertices[i] == mesh.vertices[AllFaces[Pass - 1]])
                {
                    Vert3 = i;
                }
                if (Vert1 != -1 && Vert2 != -1 && Vert3 != -1)
                {
                    break;
                }
            }
        }

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            if (mesh.triangles[i] == Vert1)
            {
                if (mesh.triangles[i + 1] == Vert2)
                {
                    if (mesh.triangles[i + 2] == Vert3)
                    {
                        SecondTriangle = i;
                        Debug.Log(AllFaces[Pass - 3].ToString() +
                        AllFaces[Pass - 2].ToString() +
                        AllFaces[Pass - 1].ToString());
                        Debug.Log(mesh.triangles[SecondTriangle].ToString() +
                        mesh.triangles[SecondTriangle + 1].ToString() +
                        mesh.triangles[SecondTriangle + 2].ToString());
                        SecondTriangle = i;
                        return (true, SecondTriangle);
                    }
                }
            }
        }


        return (false, SecondTriangle);
    }
    void RemoveFace()
    {
        List<int> FaceToRemove = new List<int> { AllFaces[Pass - 3], AllFaces[Pass - 2], AllFaces[Pass - 1] };
        (bool DoubleFace, int SecTriStart) = CheckForDoubleTriangle();
        if (DoubleFace)
        {
            FaceToRemove.Add(mesh.triangles[SecTriStart]);
            FaceToRemove.Add(mesh.triangles[SecTriStart + 1]);
            FaceToRemove.Add(mesh.triangles[SecTriStart + 2]);
        }
        //        Debug.Log("FaceToRemove Length: " + (FaceToRemove.Count - 1).ToString());
        mesh.RemoveFace(FaceToRemove, DoubleFace);
    }

    public void FaceDeselect()
    {
        VertexDeselection();
        Relatedvert.Clear();
        AllFaces.Clear();
        Pass = -1;
    }

    // RemoveTrianglePoint
    public void RemoveTrianglePoint()
    {
        // If one or more Triangle Point is selected:
        if (NewTrianglesSelection.Count - 1 >= 0)
        {
            // Destroy all Handles currently spawned.
            DestroyerHandles();
            // For each item in NewTrianglesSelection:
            for (int i = 0; i < NewTrianglesSelection.Count; i++)
            {
                // Destroy the triangle point
                Destroy(NewTrianglePoints[NewTrianglesSelection[i]].gameObject);
                NewTrianglePoints.RemoveAt(NewTrianglesSelection[i]);
            }
            // Clear the selection list.
            NewTrianglesSelection.Clear();
        }
    }

    public void RemoveAllTrianglePoints()
    {
        // If one or more Triangle Point is selected:
        if (NewTrianglePoints.Count - 1 >= 0)
        {
            if (Handles.Count >= 0)
            {
                // Destroy all Handles currently spawned.
                DestroyerHandles();
            }
            // For each item in NewTrianglesSelection:
            for (int i = 0; i < NewTrianglePoints.Count; i++)
            {
                // Destroy the triangle point
                Destroy(NewTrianglePoints[i].gameObject);
            }
            // Clear the selection list.
            NewTrianglePoints.Clear();
            NewTrianglesSelection.Clear();
        }
    }

    // Triangle Adding mouse down function.
    void TriangleAddingMouseDown(GameObject Point) // triangle point selection
    {
        // if we are in Placement mode
        
        if (Point != null)
        {
            //Set the tag of the temp triangle point to "TrianglePoint".
            if (Point.CompareTag("TrianglePoint"))
            {
                // if NewTrianglesSelection contains nothing.
                if (NewTrianglesSelection.Count - 1 < 0)
                {
                    //Debug.Log("No Items selected");
                    // Add the current triangle Poin to the NewTrianglesSelection list, 
                    // set the material of it to indicate it is selected,  run InstantiateHandle(), as this is the first selection.
                    // colliderFound is set as true.
                    for (int i = 0; i < NewTrianglePoints.Count; i++)
                    {
                        if (Point == NewTrianglePoints[i])
                        {
                            NewTrianglesSelection.Add(i);
                            break;
                        }
                    }

                    //Point.GetComponent<MeshRenderer>().material = SelectedVertexMat;
                    Point.layer = 2;
                    InstantiateHandle();
                    colliderFound = true;
                    
                }
                // If Left Control is being pressed AND new NewTrianglesSelection contains at least 1 item.
                else if (Input.GetButton("Left Control") && NewTrianglesSelection.Count - 1 >= 0)
                {
                    Debug.Log("Seconday Selection cut in");
                    TriangleAddingSecondaySelection(Point);
                }
                else if (NewTrianglesSelection.Count - 1 >= 0 && Point != NewTrianglePoints[NewTrianglesSelection[0]])
                {
                    Debug.Log("replacing primary selection");
                    VertexDeselection();

                    // Add the current triangle Poin to the NewTrianglesSelection list, 
                    // set the material of it to indicate it is selected,  run InstantiateHandle(), as this is the first selection.
                    // colliderFound is set as true.
                    for (int i = 0; i < NewTrianglePoints.Count; i++)
                    {
                        if (Point == NewTrianglePoints[i])
                        {
                            NewTrianglesSelection.Add(i);
                            break;
                        }
                    }
                    //Point.GetComponent<MeshRenderer>().material = SelectedVertexMat;
                    Point.layer = 2;
                    InstantiateHandle();
                    colliderFound = true;
                }
            }
        }
    }

    void TriangleAddingSecondaySelection(GameObject Point)
    {
        // Add the current triangle Poin to the NewTrianglesSelection list, 
        // set the material of it to indicate it is selected.
        // colliderFound is set as true.
        for (int i = 0; i < NewTrianglePoints.Count; i++)
        {
            if (Point == NewTrianglePoints[i])
            {
                NewTrianglesSelection.Add(i);
                break;
            }
        }
        //TempTrianglePoint.GetComponent<MeshRenderer>().material = SelectedVertexMat;
        TempTrianglePoint.layer = 2;
        colliderFound = true;
    }

    #endregion

    #region Vertex Specific Stuff
    // Vertex manipulation Mouse Down.
    void VertexMouseDown(bool BoxSelect)
    {
        // If we have a valid vertex.
        if (vertex != -5)
        {
            // If no vertex is currently selected.
            if (vertexSelection.Count - 1 < 0)
            {
                SelectPrimaryVert();
            }            
            else if ((Input.GetButton("Left Control") || BoxSelect) && vertexPosSelection.Count - 1 >= 0) // If the Left Control Key is being pressed down:
            {
                SelectSecondaryVert();
            }
            else if(vertexPosSelection.Count - 1 >= 0 && vertex != vertexSelection[0])
            {
                int vertexTemp = vertex;
                VertexDeselection();
                vertex = vertexTemp;
                SelectPrimaryVert();
            }
        }
        // If vertexPosSelection contains one or more items
        if (vertexPosSelection.Count - 1 >= 0)
        {
            // for each item in relatedVertices.            
            for (int j = 0; j < relatedVertsMatrix.Count; j++)
            {
                for (int i = 0; i < relatedVertsMatrix[j].Count; i++)
                {
                    // Set the gameObject layer to ingore ray cast.
                    Cols[relatedVertsMatrix[j][i]].gameObject.layer = 2;
                }
            }
        }
    }

    void TryGetVertPoint(GameObject Object)
    {
        if (Object != null)
        {
            // If the Collider is on the Vertex Point layer mask and perAxisMovement is false:
            int maskForThisHitObject = 1 << Object.layer;
            if ((maskForThisHitObject & VertexLayerMask) != 0 && !perAxisMovement)
            {
                // If we arent in triangle adding mode.
                if (!TrinagleAdding)
                {
                    // colliderPos is assigned theCollider's gameObject Positon.
                    colliderPos = Object.transform.localPosition;

                    // For every vertex in vertices array:
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        // If the colliderPos is equal to the Vertex's positon.
                        if (colliderPos == mesh.vertices[i])
                        {
                            // vertex var is assigned the current vertex in mesh.vertice, vertex and position "i".
                            // colliderPos is reassigned the vertex position at index "i".
                            // colliderFound is set to true, we have found a valid collider.
                            // Stop the for loop.
                            vertex = i;
                            colliderPos = mesh.vertices[i];
                            colliderFound = true;
                            break;
                        }
                    }
                    // Stop the Function now.
                    return;
                }
                // If we are in triangle adding mode AND we aren't InplacementMode.
                else if (TrinagleAdding && !InplacmentMode)
                {
                    // Temp Triangle point is set to the game object we found and collider found is set to true.
                    // Stop the function.
                    TempTrianglePoint = Object;
                    colliderFound = true;
                    return;
                }
            }
            else
            {
                vertex = -5;
            }
        }
        else
        {
            vertex = -5;
        }
    }

    public void SelectPrimaryVert()
    {
        // If vertex is now greater than or equal to 0
        if (vertex >= 0)
        {
            // Add the value of vertex to VertexSelection List
            // Add the current value of colliderPos to the vertexPosSelection list.
            // Related Vertices List is assigned to the return of mesh.FindRelatedVertices.
            // Lastly Instantiate a handle.
            vertexSelection.Add(vertex);
            vertexPosSelection.Add(Cols[vertex].transform.localPosition);
            if (!VertexSelectionMode)
            {
                InstantiateHandle();
            }
            

            relatedVertices = mesh.FindRelatedVertices(Cols[vertex].transform.localPosition, false);
            //relatedVertices.Sort();
            relatedVertsMatrix.Add(new List<int>(relatedVertices));
            // Set the material of all related vertices to indicate they are selected.
            for (int i = 0; i < relatedVertices.Count; i++)
            {
                Cols[relatedVertices[i]].gameObject.GetComponent<MeshRenderer>().material = SelectedVertexMat;
            }
            relatedVertices.Clear();
        }
    }

    public void SelectSecondaryVert()
    {
        // If vertexSelection List does NOT contain the current value of Vertex:
        if (!vertexSelection.Contains(vertex))
        {
            // Add the value of vertex to VertexSelection List
            // Add the current value of colliderPos to the vertexPosSelection list.
            vertexSelection.Add(vertex);
            vertexPosSelection.Add(Cols[vertex].transform.localPosition);

            relatedVertices = mesh.FindRelatedVertices(Cols[vertex].transform.localPosition, false);
            //relatedVertices.Sort();
            relatedVertsMatrix.Add(new List<int>(relatedVertices));
            // Set the material of all related vertices to indicate they are selected.
            for (int i = 0; i < relatedVertices.Count; i++)
            {
                Cols[relatedVertices[i]].gameObject.GetComponent<MeshRenderer>().material = SelectedVertexMat;
            }
            relatedVertices.Clear();

        }
    }

    // Vertex Deselection.
    void VertexDeselection()
    {
        // Destroy handles is false.
        bool DestroyHandles = false;

        // If we aren't in triangle adding mode.
        if (!TrinagleAdding)
        {
            // ColliderPosition is set to 0.
            // Vertex is set to -1.
            // vertexSelection & vertexPosSelection Lists are both cleared.
            // Destroy handles flag is now true.
            colliderPos = Vector3.zero;
            vertex = -1;
            vertexSelection.Clear();
            vertexPosSelection.Clear();
            DestroyHandles = true;

            // for every item in Cols list.
            for (int i = 0; i < Cols.Count; i++)
            {
                // Set the material back to default.
                
                if (Cols[i].GetComponent<SelectionComponent>())
                {
                    Destroy(Cols[i].GetComponent<SelectionComponent>());
                }
                Cols[i].gameObject.GetComponent<MeshRenderer>().material = DeselectedVertexMat;
            }
        }
        // If we aren't in Placement mode AND we are in Triangle adding mode. 
        else if (!InplacmentMode && TrinagleAdding)
        {
            // for every item in NewTrianglesSelection
            for (int i = 0; i < NewTrianglesSelection.Count; i++)
            {
                // Set the material back to default.
                
                if (NewTrianglePoints[NewTrianglesSelection[i]].GetComponent<SelectionComponent>())
                {
                    Destroy(NewTrianglePoints[NewTrianglesSelection[i]].GetComponent<SelectionComponent>());
                }
                NewTrianglePoints[NewTrianglesSelection[i]].GetComponent<MeshRenderer>().material = VertexCreationMat;
            }
            for (int j = 0; j < relatedVertsMatrix.Count; j++)
            {
                for (int i = 0; i < relatedVertsMatrix[j].Count; i++)
                {
                    // Set the gameObject layer to ingore ray cast.
                    Cols[relatedVertsMatrix[j][i]].gameObject.layer = 8;
                }
            }
            // Clear the NewTrianglesSelection, set destroy handles flag to true.
            NewTrianglesSelection.Clear();
            relatedVertsMatrix.Clear();
            DestroyHandles = true;
        }

        // If destroy handles flag is set to true.
        if (DestroyHandles)
        {
            // Destroy all handles and set PerAxisHandle to false.
            DestroyerHandles();
            PerAxisHandle = null;
        }
    }


    ///void DeselectSecondary()
    ///{
    ///    List<int> Temp = new List<int>();
    ///    for (int i = 1; i < vertexSelection.Count; i++)
    ///    {
    ///        Temp.Add(vertexSelection[i]);
    ///
    ///    }
    ///    vertexSelection.RemoveRange(1, vertexSelection.Count - 1);
    ///    vertexPosSelection.RemoveRange(1, vertexPosSelection.Count - 1);
    ///    for (int i = 0; i < Temp.Count; i++)
    ///    {
    ///
    ///        Cols[Temp[i]].gameObject.GetComponent<MeshRenderer>().material = DeselectedVertexMat;
    ///    }
    ///
    ///}

    /// <summary>
    /// Snap to a vertex in another mesh
    /// </summary>
    /// <param name="pos">the currently calculated positon</param>
    /// <returns> a potentioanlly modifed positon, the modifcation would be setting it to a positon in another mesh if the mouse is over the vertexPoint in the other mesh</returns>
    private Vector3 SnapToVertexInOtherMesh(Vector3 pos)
    {
        // Get a collider, assign it to theCollider var.
        Collider theCollider = Root.DoRaycast();
        // If the collider is not null:
        if (theCollider != null)
        {
            // int maskForThisHitObject = 1 << theCollider.gameObject.layer;
            // See if the collider is the one we want to click ie, is on the default layer.
            int maskForThisHitObject = 1 << theCollider.gameObject.layer;
            if ((maskForThisHitObject & VertexLayerMask2) != 0)
            {
                // If the collider's parent gameObject is not the gameObject the Mesh Visualizer is on:
                if (theCollider.transform.parent.gameObject != gameObject)
                {
                    // Make pos equal to the positon of the vertexPoint in the otherMesh the mouse was over.
                    // Set Snapped to True.
                    pos = theCollider.transform.position - gameObject.transform.position - vertexPosSelection[0];
                    Snapped = true;
                }
            }
        }
        //return's either the position of the vertexPoint in another mesh or the pos the function was given when called.
        return pos;
    }

    private Vector3 SnapToVertexInSameMesh(Vector3 pos)
    {
        // Get a collider, assign it to theCollider var.
        Collider theCollider = Root.DoRaycast();
        // If the collider is not null:
        if (theCollider != null)
        {
            // See if the collider is the one we want to click ie, is on the default layer.
            int maskForThisHitObject = 1 << theCollider.gameObject.layer;
            if ((maskForThisHitObject & VertexLayerMask) != 0)
            {
                // Make pos equal to the positon of the vertexPoint in the otherMesh the mouse was over.
                // Set Snapped to True.
                if (vertexPosSelection.Count - 1 >= 0 && !TrinagleAdding)
                {
                    pos = theCollider.transform.localPosition - gameObject.transform.position - vertexPosSelection[0];
                    Snapped = true;
                }
                if (NewTrianglesSelection.Count - 1 >= 0 && TrinagleAdding)
                {
                    pos = theCollider.transform.localPosition - gameObject.transform.position - NewTrianglePoints[NewTrianglesSelection[0]].transform.localPosition;
                }
            }
        }
        //return's either the position of the vertexPoint in another mesh or the pos the function was given when called.
        return pos;
    }

    #endregion

    // Moving a vertex point
    void MoveVertexPoint()
    {
        // Create distanceToScreen.
        float distanceToScreen;

        // If we are not in triangle adding mode
        if (!TrinagleAdding)
        {
            // Get the Z component of the distance to the primary vertexPoint. (item 0 in vertexPosSelection/vertexSelection).
            distanceToScreen = Camera.main.WorldToScreenPoint(vertexPosSelection[0]).z;
        }
        else
        {
            // Get the Z component of the distance to the primary triangle point. (item 0 in NewTrianglesSelection).
            distanceToScreen = Camera.main.WorldToScreenPoint(NewTrianglePoints[NewTrianglesSelection[0]].transform.localPosition).z;
        }

        // If Per AxisHandle is NOT equal to null then:
        if (PerAxisHandle != null)
        {
            // distanceToScreen is equal to the z component of the distance from the handle clicked on.
            distanceToScreen = Camera.main.WorldToScreenPoint(PerAxisHandle.transform.localPosition).z;
        }

        //create a new vector3 which converts the mouse positon in the camrea to a world position using the distance from the camera to the object as z.
        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));

        // If Rounding is Enabled:
        if (roundingEnabled)
        {
            // Round every float in the Vector3 Pos to the spsified decmial places.
            pos = new Vector3((float)Math.Round(pos.x, roundingAccuracy), (float)Math.Round(pos.y, roundingAccuracy), (float)Math.Round(pos.z, roundingAccuracy));
        }

        // pos equals itself minus mouseLastFrame.
        pos -= mouseLastFrame;

        // If Selection contains more than 0 items AND we arent in TrinagleAdding mode then:
        if (Selection > 0 && !TrinagleAdding)
        {
            // Pos is updated to the return of the SnapToVertexInOtherMesh function.
            pos = SnapToVertexInOtherMesh(pos);
        }
        // If left Control is down
        if (Input.GetButton("Left Shift"))
        {
            // snap to vertex in same mesh
            pos = SnapToVertexInSameMesh(pos);
        }
        // If PerAxisHandle is null:
        if (PerAxisHandle == null)
        {
            // Clamp the Position, not sure if this really does anything?
            // If not can simplfy this IF statement to if PerAxis Handle is != null then do everything that is currently in the else.
            ClampMovement(pos);
        }
        else
        {
            // If the Name of the GameObject is "XAxis":
            if (PerAxisHandle.name == "XAxis")
            {
                // Set these two components to 0.
                pos.y = 0f;
                pos.z = 0f;
            }
            // If the Name of the GameObject is "YAxis":
            else if (PerAxisHandle.name == "YAxis")
            {
                // Set these two components to 0.
                pos.x = 0f;
                pos.z = 0f;
            }
            // If the Name of the GameObject is "ZAxis":
            else if (PerAxisHandle.name == "ZAxis")
            {
                // Set these two components to 0.
                pos.x = 0f;
                pos.y = 0f;
            }
        }
        // If we arent in triangle adding mode.
        if (!TrinagleAdding)
        {
            // Pos is componsenated incase the gameObject is rotated.
            pos = (Quaternion.Euler(Cols[vertexSelection[0]].transform.localRotation.eulerAngles) * pos);
            // For each item in vertexPosSelection, update the position to the current positon of the vertices stored in vertexSelection, 
            // to the position stored in vertexPosSelection + pos.
            for (int i = 0; i < vertexPosSelection.Count; i++)
            {
                mesh.DoAction(vertexSelection[i], vertexPosSelection[i] + pos);
            }
            // This stops the jumpyness of snapping to a vertex in another Mesh.
            if (Snapped)
            {
                HaultPointMovement();
            }
        }
        else
        {
            // Pos is componsenated incase the gameObject is rotated.
            pos = Quaternion.Euler(NewTrianglePoints[NewTrianglesSelection[0]].transform.localRotation.eulerAngles) * pos;

            // For each item in vertexPosSelection, update the position to the current positon of the vertices stored in vertexSelection, 
            // to the position stored in vertexPosSelection + pos.
            for (int i = 0; i < NewTrianglesSelection.Count; i++)
            {
                NewTrianglePoints[NewTrianglesSelection[i]].transform.localPosition += pos;
            }
        }
    }

    // Function that interpreset the typing input for moving a vertex point
    public void MoveVertexByTyping(Vector3 pos)
    {
        if (!TrinagleAdding)
        {
            // block makes sure pos contains 3 floats and not float.NaNs
            if (float.IsNaN(pos.x))
            {
                pos.x = vertexPosSelection[0].x;
            }
            if (float.IsNaN(pos.y))
            {
                pos.y = vertexPosSelection[0].y;
            }
            if (float.IsNaN(pos.z))
            {
                pos.z = vertexPosSelection[0].z;
            }
            // Position modifier relative to vertex stored at posiiton 0.
            // vertex at position 0 in the list is updated to the new position.
            Vector3 otherPos = pos - vertexPosSelection[0];
            mesh.DoAction(vertexSelection[0], pos);
            // for every item in vertexPosSelection (except item 1) has its position updated.
            for (int i = 1; i < vertexPosSelection.Count; i++)
            {
                mesh.DoAction(vertexSelection[i], vertexPosSelection[i] + otherPos);
            }
        }
        else
        {
            // block makes sure pos contains 3 floats and not float.NaNs
            if (float.IsNaN(pos.x))
            {
                pos.x = NewTrianglePoints[NewTrianglesSelection[0]].transform.localPosition.x;
            }
            if (float.IsNaN(pos.y))
            {
                pos.y = NewTrianglePoints[NewTrianglesSelection[0]].transform.localPosition.y;
            }
            if (float.IsNaN(pos.z))
            {
                pos.z = NewTrianglePoints[NewTrianglesSelection[0]].transform.localPosition.z;
            }
            // Position modifier relative to vertex stored at posiiton 0.
            // vertex at position 0 in the list is updated to the new position.
            Vector3 otherPos = pos - NewTrianglePoints[NewTrianglesSelection[0]].transform.localPosition;
            NewTrianglePoints[NewTrianglesSelection[0]].transform.localPosition = pos;
            // for every item in vertexPosSelection (except item 1) has its position updated.
            for (int i = 1; i < NewTrianglesSelection.Count; i++)
            {
                NewTrianglePoints[NewTrianglesSelection[i]].transform.localPosition += otherPos;
            }
        }
    }


    // Function used for seeing if we click on a vertex/ are over a vertex.
    void GetVertexPoint()
    {
        // Set colliderFound to False.
        // Run the RayCast helper, and assign its output to theCollider.
        colliderFound = false;
        Collider theCollider = Root.DoRaycast();
        // If we didn't get a collider end function here
        if (theCollider != null)
        {
            // If the Collider is on the Vertex Point layer mask and perAxisMovement is false:
            int maskForThisHitObject = 1 << theCollider.gameObject.layer;
            if ((maskForThisHitObject & VertexLayerMask) != 0 && !perAxisMovement)
            {
                // If we arent in triangle adding mode.
                if (!TrinagleAdding)
                {
                    // colliderPos is assigned theCollider's gameObject Positon.
                    colliderPos = theCollider.transform.localPosition;

                    // For every vertex in vertices array:
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        // If the colliderPos is equal to the Vertex's positon.
                        if (colliderPos == mesh.vertices[i])
                        {
                            // vertex var is assigned the current vertex in mesh.vertice, vertex and position "i".
                            // colliderPos is reassigned the vertex position at index "i".
                            // colliderFound is set to true, we have found a valid collider.
                            // Stop the for loop.
                            vertex = i;
                            colliderPos = mesh.vertices[i];
                            colliderFound = true;
                            break;
                        }
                    }
                    // Stop the Function now.
                    return;
                }
                // If we are in triangle adding mode AND we aren't InplacementMode.
                else if (TrinagleAdding && !InplacmentMode)
                {
                    // Temp Triangle point is set to the game object we found and collider found is set to true.
                    // Stop the function.
                    TempTrianglePoint = theCollider.gameObject;
                    colliderFound = true;
                    return;
                }
            }
            // If the Collider is on the handle layer mask:
            maskForThisHitObject = 1 << theCollider.gameObject.layer;
            if ((maskForThisHitObject & HandleLayerMask) != 0)
            {
                // If we arent in triangle adding mode.
                if (!TrinagleAdding)
                {
                    // colliderPos is assigned theCollider's gameObject Positon.
                    colliderPos = theCollider.transform.parent.transform.parent.localPosition;
                    //for every vertex in vertices array
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        // If the colliderPos is equal to the Vertex's positon.
                        if (colliderPos == mesh.vertices[i])
                        {
                            // vertex var is assigned the current vertex in mesh.vertice, vertex and position "i".
                            // colliderPos is reassigned the vertex position at index "i".
                            // colliderFound is set to true, we have found a valid collider.
                            // PerAxisHandle is set to theCollider's gameObject.
                            // Stop the for loop.
                            vertex = i;
                            colliderPos = mesh.vertices[i];
                            colliderFound = true;
                            PerAxisHandle = theCollider.gameObject;
                            break;
                        }
                    }
                }
                // If we are in triangle adding mode AND we aren't InplacementMode.
                else if (TrinagleAdding && !InplacmentMode)
                {
                    // Per Axis Handle is set to the gameobject we clicked on, collider found is set to true.
                    PerAxisHandle = theCollider.gameObject;
                    colliderFound = true;
                }
            }
        }
        // Else set the vertex to -5.
        else
        {
            vertex = -5;
        }
    }


    #region General Functionality

    // when this is called it runs the vertex point initializer or deinitilizer depending on mesh.moveVertexPoint
    public void EditMesh()
    {
        // Try get the HandleScript Component.
        handle = GetComponentInChildren<HandleScript>();

        // If mesh.moveVertexPoint is true:
        if (mesh.moveVertexPoint)
        {
            // If perAxisMovement is true, then Run PerAxisMovement Function.
            if (perAxisMovement)
                PerAxisMovement();

            // If handle is NOT null, Run the ShowHandles Function in the HandleScript.
            if (handle != null)
                handle.ShowHandles();
            if (TrinagleAdding)
            {
                Root.UIController.AddQuad();
                if (Root.UIController.AddQuadButton.GetComponent<Toggle>().isOn)
                    Root.UIController.AddQuadButton.GetComponent<Toggle>().isOn = false;
            }

            // Set moveVertexPoint to false.
            // Run the DeinitializeVertexPoints function.
            // Clear the Handlesm, vertexSelection &  vertexPosSelection Lists.
            // Run RemoveTrianglePoint, then clear the NewTrianglesSelection list.
            // TempTrianglePoint is set to null. NewTriangleReferencePosition set to 0.
            // Material of the mesh is set back to transparent. Lastly run CalculateMass().
            mesh.moveVertexPoint = false;
            DeinitializeVertexPoints();
            Handles.Clear();
            vertexPosSelection.Clear();
            vertexSelection.Clear();
            RemoveAllTrianglePoints();
            TempTrianglePoint = null;
            NewTriangleReferencePosition = Vector3.zero;
            TrinagleAdding = false;
            gameObject.GetComponent<MeshRenderer>().material = Root.TransparentMat;
            //CalculateMass();
        }
        else
        {
            // Material of the mesh is set back to opaque.
            Debug.Log(this.gameObject.name);
            if(gameObject.GetComponent<SelectionComponent>() != null)
            {
                gameObject.GetComponent<MeshRenderer>().material = gameObject.GetComponent<SelectionComponent>().Orignal;
            }
            
            // If handle is NOT null, Run the ShowHandles Function in the HandleScript.
            if (handle != null)
                handle.HideHandles();
            // Set moveVertexPoint to true.
            // Run InitiliseVertexPoints Function.
            mesh.moveVertexPoint = true;
            InitiliseVertexPoints();
        }

        // Set clonedMesh var in the meshManger script to the sharedMesh ofthe meshcollider on the gameObject this script is on.
        GetComponent<MeshCollider>().sharedMesh = mesh.clonedMesh;
        GetComponent<MeshFilter>().sharedMesh = mesh.clonedMesh;
    }

    // Change into adding a quad mode.
    public void AddQuadMode()
    {
        if (TrinagleAdding)
        {
            RemoveAllTrianglePoints();
            TempTrianglePoint = null;
            NewTriangleReferencePosition = Vector3.zero;
            TrinagleAdding = false;

        }
        else
        {
            TrinagleAdding = true;
        }
    }

    // toggle into placment mode.
    public void TogglePlacementMode()
    {
        if (InplacmentMode)
        {
            InplacmentMode = false;
        }
        else
        {
            InplacmentMode = true;
        }
    }

    // When is called and it destroys all the vertex points.
    public void DeinitializeVertexPoints()
    {
        VertexDeselection();
        // For every collider, destroy the gameObject it is attached to.
        // Then Clear the Cols List.
        for (int i = 0; i < Cols.Count; i++)
        {
            if (Cols[i] != null)
            {
                Destroy(Cols[i].gameObject);
            }
        }
        Cols.Clear();
        Cols.Capacity = 0;
    }

    // When called it creates all the vertex points.
    public void InitiliseVertexPoints()
    {
        DeinitializeVertexPoints();
        // the Transform, HandleTransform, is set to gameObject's transform that this Script is attached to. 
        // (should be the same gameObject as the meshmanager).
        handleTransform = gameObject.transform;
        // For every vertex in the vertices array.
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            // Run InstantiateVertexPoint Function.
            InstantiateVertexPoint(i);
        }
        if (!disableGUI)
        {
            Root.UniSelection.Objects.Clear();
            for (int i = 0; i < Cols.Count; i++)
            {
                Root.UniSelection.Objects.Add(Cols[i].gameObject);
            }
        }
        
        /// // Add every collider in the children of the this component's gameObject.
        /// //Cols.AddRange(this.transform.GetComponentsInChildren<BoxCollider>());
        /// 
        /// // For every collider in the Cols list
        /// //for (int i = 0; i < Cols.Count; i++)
        /// //{
        /// //    // If the gameObject the collider is attached to does not have the tag "VertexPoint":
        /// //    if (Cols[i].gameObject.tag != "VertexPoint")
        /// //    {
        /// //        // Remove collider at the positon i.
        /// //        Cols.Remove(Cols[i]);
        /// //    }
        /// //    if(Cols[i] is null)
        /// //    {
        /// //        Cols.Remove(Cols[i]);
        /// //    }
        /// //}
    }

    /// <summary>
    /// Place a vertex point at the vertice index.
    /// </summary>
    /// <param name="i"> Vertice index from mesh.vertice. </param>
    void InstantiateVertexPoint(int i)
    {
        // If mesh.moveVertexPoint is true
        if (mesh.moveVertexPoint && i != -10)
        {
            // Create a positon from the local postion of the vertice at i.
            Vector3 point = handleTransform.TransformPoint(mesh.vertices[i]);

            // Spawn a vertexPoint at the positon point we just created, setting the rotation to the rotation in the prefab vertexPoint.
            GameObject go = (GameObject)Instantiate(vertexPoint, point, Quaternion.identity);
            // Set the vertexPoint's parent to the gameObject the mesh component is on.
            go.transform.SetParent(mesh.transform);

            // Componsate for scale of the parent.
            go.transform.localScale *= transform.localScale.magnitude * 0.75f;
            if (disableGUI)
            {
                go.gameObject.layer = 0;
            }
            Cols.Add(go.GetComponent<Collider>());
        }
        // If i is equal to -10 then spawn a triangle point.
        if (i == -10 && NewTrianglePoints.Count < 4)
        {
            // Spawn a vertexPoint at the Camera's focal point, setting the rotation to the rotation in the prefab vertexPoint.
            GameObject go = (GameObject)Instantiate(vertexPoint, Camera.main.transform.parent.position, Quaternion.identity);
            // Set the vertexPoint's parent to the gameObject the mesh component is on.
            go.transform.SetParent(mesh.transform);
            // Componsate for scale of the parent.
            // Set the material to the vertex creation mat, and rename it to "TrianglePoint(Clone)" and set its tag to "TrianglePoint".
            go.transform.localScale *= transform.localScale.magnitude * 0.76f;
            go.GetComponent<MeshRenderer>().material = VertexCreationMat;
            go.tag = "TrianglePoint";
            go.name = "TrianglePoint(Clone)";
            NewTrianglePoints.Add(go);
        }
    }

    // Every time this is called, update the position of every vertex point to the actual position of the vertice in the mesh.
    void UpdateVertexPoints()
    {
        // handleTransform is set to the transform of the gameObject the meshManager component is on.
        handleTransform = mesh.transform;

        // For every collider in the List Cols:
        for (int i = 0; i < Cols.Count; i++)
        {
            // Set the positon of the gameObject that the collider "i" is on to
            // the vertice position it corrisponds to in mesh.vertices,
            // plus the position of the gameObject this script is on.
            Cols[i].transform.localPosition = mesh.vertices[i] + handleTransform.position - gameObject.transform.position;
        }
    }

    //this is supposed to stop things freaking out. idk if this actually is used/does anything
    private Vector3 ClampMovement(Vector3 pos)
    {
        Vector3 reference;
        if (TrinagleAdding)
        {
            reference = NewTriangleReferencePosition;
        }
        else
        {
            reference = vertexPosSelection[0];
        }

        clampPos = reference + (clampPos * 1.5f);

        if (reference.x + pos.x > reference.x)
        {
            pos.x = clampPos.x;
        }
        if (reference.x + pos.x > -clampPos.x)
        {
            pos.x = clampPos.x;
        }
        if (reference.y + pos.y > clampPos.y)
        {
            pos.y = clampPos.y;
        }
        if (reference.y + pos.y > -clampPos.y)
        {
            pos.y = clampPos.y;
        }
        if (reference.z + pos.z > clampPos.z)
        {
            pos.z = clampPos.z;
        }
        if (reference.z + pos.z > -clampPos.z)
        {
            pos.z = clampPos.z;
        }
        return pos;
    }

    #endregion

    #region PerAxisMovement

    // Per Axis Movement toggle
    void PerAxisMovement()
    {
        // If perAxisMovement is true:
        if (perAxisMovement)
        {
            // Run the DestroyHandles Function, 
            // Clear the Handles List.
            // Set perAxisMovement to false.
            DestroyerHandles();
            Handles.Clear();
            perAxisMovement = false;
        }
        else
        {
            // Set perAxisMovement to true.
            perAxisMovement = true;
        }
    }

    // Spawn a handle.
    void InstantiateHandle()
    {
        Vector3 point;
        // If we arent in Triangle Adding mode.
        if (!TrinagleAdding)
        {
            // Create a position from the position of the vertexPoint we need to spawn at.
            point = (mesh.gameObject.transform).TransformPoint(Cols[vertexSelection[0]].transform.localPosition);
        }
        else
        {
            // Create a position from the position of the NewTrianglesPoint we need to spawn at.
            point = (mesh.gameObject.transform).InverseTransformPoint(NewTrianglePoints[NewTrianglesSelection[0]].transform.localPosition);
        }

        // Spawn the handle.
        // Set the handle's parent to the Vertex Point.
        GameObject go = (GameObject)Instantiate(handles, point, Quaternion.identity);
        // If we arent in Triangle Adding mode.
        if (!TrinagleAdding)
        {
            // Set the parent to the first item in vertexSelection.
            go.transform.SetParent(Cols[vertexSelection[0]].transform);
        }
        else
        {
            // Set the parent to the first item in NewTrianglesSelection.
            go.transform.SetParent(NewTrianglePoints[NewTrianglesSelection[0]].transform);
        }

        // Componsate for scale of the parent.
        // Add the Handle toe the Handles List.
        go.transform.localScale *= transform.localScale.magnitude * 0.75f;
        Handles.Add(go);
    }

    // Remove every handle game object in the handles array.
    void DestroyerHandles()
    {
        // For every item in the Handles List:
        for (int i = 0; i < Handles.Count; i++)
        {
            // Destroy the gameObject the handle.
            Destroy(Handles[i].gameObject);
        }
        // Clear the Handles list.
        Handles.Clear();
    }

    #endregion

    #region Relay and Helper Functions

    // MergeMesh relay function.
    public void MergeMesh(List<ComponentStatsManager> SecondMesh)
    {
        mesh.MergeMesh(SecondMesh);
    }

    // Require Data relay function
    public void RequireData()
    {
        mesh.ReaquireData();
    }

    // ResetMesh relay function.
    public void ResetMesh()
    {
        mesh.Reset();
    }

    // Do action relay Function.
    public void DoActionRelay(int Col, Vector3 Pos)
    {
        mesh.DoAction(Col, Pos);
    }


    void SelectionModeToggler()
    {
        if (Root.UIController.SelectionModeToggle.GetComponent<Toggle>().isOn)
            Root.UIController.SelectionModeToggle.GetComponent<Toggle>().isOn = false;
        else if (!Root.UIController.SelectionModeToggle.GetComponent<Toggle>().isOn)
            Root.UIController.SelectionModeToggle.GetComponent<Toggle>().isOn = true;
    }
    public void SelectionMode()
    {
        if (VertexSelectionMode == false)
        {
            VertexSelectionMode = true;
        }
        else if (VertexSelectionMode == true)
        {
            VertexSelectionMode = false;
        }

        if (!VertexSelectionMode && (vertexSelection.Count - 1>=0 ||NewTrianglesSelection.Count -1 >=0))
        {
            InstantiateHandle();
        }
        else
        {
            DestroyerHandles();
        }
    }

    #endregion

    // This sets the default values for every variable in the function,
    // clears all the lists, and if there are any vertex Points spawned, despawns them.
    public void ResetVisualizer()
    {
        if (Cols.Count != 0)
        {
            EditMesh();
        }

        RemoveAllTrianglePoints();

        VertexLayerMask2 = 9;

        //per axis movement
        perAxisMovement = false;
        PerAxisHandle = null;

        //rounding coordinates
        roundingEnabled = false;
        roundingAccuracy = 1;

        //other mesh vertex alignment
        disableGUI = false;
        Snapped = false;

        //general vertex point movement
        clampPos = Vector3.zero;
        vertexPosSelection = new List<Vector3>();
        vertexSelection = new List<int>();
        vertex = -1;
        colliderPos = Vector3.zero;
        mouseLastFrame = Vector3.zero;
        colliderFound = false;
        Cols.Clear();
        relatedVertsMatrix.Clear();
        Selection = -10;
        gameObject.GetComponent<MeshCollider>().enabled = true;
        GetComponent<MeshVisualizer>().enabled = false;
        TrinagleAdding = false;
        InplacmentMode = true;
        if (Root.UIController.AddQuadButton.GetComponent<Toggle>().isOn)
            Root.UIController.AddQuadButton.GetComponent<Toggle>().isOn = false;
        if (Root.UIController.QuadSelection.GetComponent<Toggle>().isOn)
            Root.UIController.QuadSelection.GetComponent<Toggle>().isOn = false;
        NewTriangleReferencePosition = Vector3.zero;
        TempTrianglePoint = null;
    }
}
