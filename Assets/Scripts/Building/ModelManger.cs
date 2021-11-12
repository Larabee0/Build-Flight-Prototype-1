using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// manages all the parts in the root gameObject
public class ModelManger : MonoBehaviour
{
    [Header("Script References")]
    public EditButtonHandler UIController;
    public DecalManager DecalManager;
    public ShipManager ShipManager;
    public UniversalSelection UniSelection;


    [Header("Materials and Objects")]
    public GameObject Handles;
    public GameObject ObjectToPlace;

    // Materials
    public Material TransparentMat;
    public Material ParentMat;


    [Header("Modes")]
    public bool selectionMode = false;
    public bool MeshInEdit = false;
    public bool SubCompInEdit = false;


    [Header("Debug")]
    public bool objectSelected = false;
    public bool SecondObjSelected = false;

    public RectTransform SelectionBox;
    public Vector2 StartPos;

    public List<ComponentStatsManager> Selection = null;// of the Selection.
    public List<GameObject> ClipBoard = new List<GameObject>();// of the copy pasteClipBoard
    
    public List<ComponentStatsManager> SpawnedComponents = new List<ComponentStatsManager>();

    public Material originalMat;
    private Vector3 posLastFrame = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        GetSpawnedCompoentns();
        // Grab the UI script.
        UIController = GameObject.FindObjectOfType<EditButtonHandler>();
    }

    public void GetSpawnedCompoentns()
    {
        SpawnedComponents.Clear();
        SpawnedComponents.AddRange(GetComponentsInChildren<ComponentStatsManager>());
        UniSelection.Objects.Clear();
        for (int i = 0; i < SpawnedComponents.Count; i++)
        {
            UniSelection.Objects.Add(SpawnedComponents[i].gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If the mesh is not in edit, subcompinedit is not currently used.
        if (!SubCompInEdit && !MeshInEdit)
        {
            // If we have something selected;
            if (Selection.Count - 1 >= 0)
            {
                // this changes the material of a object if its not a child of this.gameobject.
                if (Selection[0].transform.parent.name != gameObject.name)
                {
                    if (Selection[0].transform.parent.gameObject.GetComponent<MeshRenderer>().material != ParentMat)
                    {
                        GameObject Temp = Selection[0].transform.parent.gameObject;
                        Temp.GetComponent<MeshRenderer>().material = ParentMat;
                    }
                }
            }

            // Run selections function.
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetButtonUp("Fire1"))
                {
                    if (selectionMode)
                    {
                        ConvertGlobalSelectionToLocal();
                    }
                }
                if (Input.GetButtonDown("Fire1"))
                {

                    if (selectionMode)
                    {
                        UniSelection.SelectionMouseDown(TransparentMat,true);
                    }
                    ModelManagerLeftMouseDown();
                }
                if (Input.GetButton("Fire1"))
                {
                    if (selectionMode)
                    {
                        UniSelection.SelectionMouse();
                    }
                }
            }
            
            
            // Sets the parent of all objects(other than the first item) in the selection to the 1st item in the list.
            if (Input.GetKeyDown(KeyCode.P) && SecondObjSelected)
            {
                for (int i = 1; i < Selection.Count; i++)
                {
                    Selection[i].gameObject.transform.parent = Selection[0].gameObject.transform;
                }
            }

            // Sets the parent of ALL objects in the selection to this.gameObject.transform.
            if (Input.GetKeyDown(KeyCode.O) && Input.GetKey(KeyCode.LeftShift))
            {
                for (int i = 0; i < Selection.Count; i++)
                {
                    Selection[i].gameObject.transform.SetParent(gameObject.transform);
                }
            }

            //???
            if (Input.GetKeyDown(KeyCode.O) && objectSelected)
            {
                if (Selection[0].transform.parent.transform.parent.name == gameObject.name)
                {
                    Selection[0].transform.SetParent(gameObject.transform);
                }
                else
                {
                    Selection[0].transform.SetParent(Selection[0].transform.parent.transform.parent);
                }
            }

            // Copy component(s).
            if (Input.GetButton("Left Control") && Input.GetKeyDown(KeyCode.C) && selectionMode)
            {
                ClipBoard.Clear();
                if (objectSelected && Selection.Count - 1 >= 0)
                {
                    for (int i = 0; i < Selection.Count; i++)
                    {
                        ClipBoard.Add(Selection[i].gameObject);
                    }
                }
                else if (objectSelected && Selection.Count - 1 <= 0)
                {
                    ClipBoard.Add(Selection[0].gameObject);
                }
            }

            // Paste component(s).
            if (ClipBoard.Count - 1 >= 0 && Input.GetButton("Left Control") && Input.GetKeyDown(KeyCode.V) && selectionMode)
            {
                for (int i = 0; i < ClipBoard.Count; i++)
                {
                    // Spawn the gameobject currently assigned to Object to Place at the position of hte Camera's Focal point.
                    // Then Set the Parent of the object we just spawned to the gameobject this script is on.
                    GameObject go = (GameObject)Instantiate(ClipBoard[i], Camera.main.transform.parent);
                    go.transform.SetParent(gameObject.transform);
                    if (go.GetComponentInChildren<HandleScript>() != null)
                    {
                        Destroy(GetComponentInChildren<HandleScript>().PosAxisSpawned);
                        Destroy(go.GetComponentInChildren<HandleScript>().gameObject);
                    }

                    go.GetComponent<MeshRenderer>().material = originalMat;
                    go.GetComponent<MeshVisualizer>().enabled = false;
                    go.GetComponent<MeshCollider>().enabled = true;
                }
            }

            #region Secondary Selection sys Backened.
            // if the Selection array is not null and its count is greater than 0:
            if (Selection != null && Selection.Count - 1 >= 0)
            {
                if (!Selection[0].NoVisualizer)
                {
                    // Then:
                    // If the meshVisualizer at position 0's Cols List is Count is equal to 0 AND
                    // if the Cols List count in the activeMainVisualizer is NOT equal to 0 OR
                    // if the meshVisualizer at position 0's Cols List is Count is not equal to 0 AND
                    // if the Cols List count in the activeMainVisualizer is equal to 0.
                    if (Selection[0].MeshVisualizer.Cols.Count == 0 && Selection[0].MeshVisualizer.Cols.Count != 0 || Selection[0].MeshVisualizer.Cols.Count != 0 && Selection[0].MeshVisualizer.Cols.Count == 0)
                    {
                        // Then:
                        // For each item in the Selection List.
                        for (int i = 0; i < Selection.Count; i++)
                        {
                            // Run the Edit Mesh function on the current MeshVisualizer.
                            // Then Run the local function, Get Colliders, giving it the current meshVisualizer.
                            if (!Selection[0].NoVisualizer)
                            {
                                Selection[i].MeshVisualizer.EditMesh();
                                GetColliders(Selection[i].MeshVisualizer);
                            }
                        }
                    }

                    // IF the activeMainVisualizer's Cols List count is equal to 0 AND the MeshVisualizer Cols List count is also 0.
                    if (Selection[0].MeshVisualizer.Cols.Count == 0 && Selection[0].MeshVisualizer.Cols.Count == 0)
                    {
                        // If the M key went down this frame:
                        if (Input.GetKeyDown(KeyCode.M))
                        {
                            if(!Selection[0].NoVisualizer)
                                //Run the merge selection helper function in the ActiveMainVisualizer Script,
                                // passing in all the mesh visualizers in the selection list.
                                Selection[0].MeshVisualizer.MergeMesh(Selection);
                        }
                    }

                    // For each MeshVisualizer in the Selection list:
                    // This is so the secondary objects move with the main, but stay offset.
                    for (int i = 1; i < Selection.Count; i++)
                    {
                        // Move the position of the gameobject the current MeshVisualizer is attached to by the distance the mainVisualizer moved.
                        Selection[i].transform.position -= (posLastFrame - Selection[0].transform.position);
                    }
                }
            }
            #endregion

            // If the Delete Key went down this frame and an Object is selected:
            if (Input.GetKeyDown(KeyCode.Delete) && objectSelected)
            {
                // Destroy the activeMainVisualizer's gameobject.
                // Set the acttiveMainVisualizer var to null, and selected object selected to false.
                Destroy(GetComponentInChildren<HandleScript>().PosAxisSpawned);
                for (int i = 0; i < Selection.Count; i++)
                {
                    SpawnedComponents.Remove(Selection[i]);
                    Destroy(Selection[i].gameObject);
                }
                Selection.Clear();
                if (SecondObjSelected)
                {
                    SecondObjSelected = false;
                }

                UIController.handlesOn = null;
                UIController.Main = null;
                Selection.Clear();
                objectSelected = false;
                UniSelection.Objects.Clear();
                for (int i = 0; i < SpawnedComponents.Count; i++)
                {
                    UniSelection.Objects.Add(SpawnedComponents[i].gameObject);
                }
            }

            // If the F Key went down this frame and an Object isn't selected:
            if (Input.GetKeyDown(KeyCode.F) && !objectSelected)
            {
                //Flip the selectionMode bool value.
                SelectionModeToggler();
            }

            // If there is an activeMainVisualizer isn't null:
            if (Selection.Count - 1 >= 0)
            {
                // Get the activeMainVisualize's position this frame.
                posLastFrame = Selection[0].transform.position;
            }
        }
    }

    void ConvertGlobalSelectionToLocal()
    {
        List<GameObject> Results = UniSelection.SelectionMouseUp();
        if(Results.Count - 1 >= 0)
        {
            bool BoxSelect = false;
            if(Results.Count -1 > 0)
            {
                BoxSelect = true;
            }
            //SelectMain(Results[0]);
            for (int i = 0; i < Results.Count; i++)
            {
                SelectionSys(Results[i], BoxSelect);
            }
            posLastFrame = Selection[0].transform.position;
        }
        else
        {
            Deselect();
        }
    }

    // Toggle into and out of selection/placement mode.
    void SelectionModeToggler()
    {
        if (UIController.SelectionModeToggle.GetComponent<Toggle>().isOn)
            UIController.SelectionModeToggle.GetComponent<Toggle>().isOn = false;
        else if (!UIController.SelectionModeToggle.GetComponent<Toggle>().isOn)
            UIController.SelectionModeToggle.GetComponent<Toggle>().isOn = true;
    }

    // Sets up the modelManager so its ok to shut it down.
    public void BuildModeStopper()
    {
        UniSelection.RemoveSelectionComponents();
        Deselect();

        this.enabled = false;
    }

    // used by the UI controller to change the selection mode.
    public void SelectionMode()
    {
        if (selectionMode == false)
        {
            selectionMode = true;
        }
        else if (selectionMode == true)
        {
            selectionMode = false;
        }
    }

    // Check if the user selected a thing(s)
    void ModelManagerLeftMouseDown()
    {

        // If the left mouse button went down this frame, and we aren't in Selection Mode:
        if (!selectionMode)
        {
            // Spawn the gameobject currently assigned to Object to Place at the position of hte Camera's Focal point.
            // Then Set the Parent of the object we just spawned to the gameobject this script is on.
            GameObject go = (GameObject)Instantiate(ObjectToPlace, Camera.main.transform.parent);
            go.transform.SetParent(gameObject.transform);
            go.GetComponent<ComponentStatsManager>().ModelManger = this;
            SpawnedComponents.Add(go.GetComponent<ComponentStatsManager>());
            UniSelection.Objects.Add(go);
            return;
        }
        //OldSelectionSys();
    }

    void SelectionSys(GameObject Item, bool BoxSelect)
    {
        // Perform raycast and assign it to hitInfo.

        //If we hit something:
        if (Item)
        {
            // If the thing we hit has the tag Selectable and we are in Selection Mode:
            if (Item.CompareTag("Selectable") && selectionMode)
            {
                if (!SubCompInEdit) //if(hitInfo.transform.gameObject.GetComponent<ComponentStatsManager>().ComponentType == 1 && !SubCompInEdit)
                {
                    // If Right Mouse went down this frame, AND an object isn't selected:
                    if (!objectSelected)
                    {
                        // Set the MeshVisualizer in the thing we hit to Active, and disable the collider in the thing we hit.
                        SelectMain(Item.gameObject);
                    }
                    // If Right Mouse went down this frame, AND an object is selected AND the left control key is being held down/went down:
                    else if (objectSelected && (Input.GetButton("Left Control") || BoxSelect))
                    {
                        SelectSecondary(Item.gameObject);
                    }
                    else if (objectSelected && Item.gameObject != Selection[0])
                    {
                        Deselect();
                        SelectMain(Item.gameObject);
                    }
                }
            }
        }
        // if selection is greater than equal to 0, and the first item selected HAS a visualizer, then set the material to the oringial material then deselect
        // or else, just deselect.
        else if (Selection.Count - 1 >= 0)
        {
            if (!Selection[0].NoVisualizer)
            {
                if (Selection[0].GetComponent<MeshVisualizer>().Cols.Count - 1 < 0)
                {
                    if (Selection.Count - 1 >= 0)
                    {
                        if (Selection[0].transform.parent.name != gameObject.name)
                        {
                            Selection[0].transform.parent.gameObject.GetComponent<MeshRenderer>().material = originalMat;
                        }
                    }
                    Deselect();
                }
            }
            else
            {
                Deselect();
            }
        }
    }

    // Function used when selecting the first item in the "Selection" list.
    public void SelectMain(GameObject Main)
    {
        if (!Main.GetComponent<ComponentStatsManager>().NoVisualizer)
        {
            Main.GetComponent<MeshVisualizer>().enabled = true;
        }
        Main.gameObject.GetComponent<MeshCollider>().enabled = false;
        Selection.Add(Main.GetComponent<ComponentStatsManager>());
        UIController.Main = Selection[0];
        // we have now selected an Object, so set to true.
        // and finally spawn the transform handles for the object.
        objectSelected = true;
        //originalMat = Selection[0].gameObject.GetComponent<MeshRenderer>().material;
        //Selection[0].gameObject.GetComponent<MeshRenderer>().material = TransparentMat;
        InstantiateHandle();
    }

    void SelectSecondary(GameObject Secondary)
    {
        if (!Secondary.GetComponent<ComponentStatsManager>().NoVisualizer)
        {

            // Set the MeshVisualizer in the thing we hit to Active, and disable the collider in the thing we hit.
            Secondary.GetComponent<MeshVisualizer>().enabled = true;
            Secondary.GetComponent<MeshVisualizer>().disableGUI = true;
        }
        Secondary.GetComponent<MeshCollider>().enabled = false;

        // Add the MeshVisualizer on the thing we clicked on to the selection List.
        Selection.Add(Secondary.GetComponent<ComponentStatsManager>());
        if (!Selection[0].NoVisualizer)
        {
            Selection[0].GetComponent<MeshVisualizer>().Selection = Selection.Count - 1;
        }


        //Selection[Selection.Count - 1].gameObject.GetComponent<MeshRenderer>().material = TransparentMat;


        // Disable the GUI in the MeshVisualizer we just clicked on.
        // Set the secondObject selected to true.
        SecondObjSelected = true;
    }

    // Function used to start the detailing mode.
    public void StartDecalApplication()
    {
        DecalManager.enabled = true;
        DecalManager.StartDecalManager();
        UIController.gameObject.SetActive(false);
        this.enabled = false;
    }

    /// // Used to start the subcomponent editor, not currently used.
    ///void StartSubComponentEdit(GameObject SubComponent)
    ///{
    ///    SubCompInEdit = true;
    ///    SubComponentScript SubComponentScript = SubComponent.GetComponent<SubComponentScript>();
    ///    SubComponentScript.UIController = UIController;
    ///    SubComponentScript.ModelManger = this;
    ///    SubComponentScript.Handles = Handles;
    ///    if (ObjectToPlace.gameObject.GetComponent<ComponentStatsManager>().ComponentType != 1)
    ///    {
    ///        SubComponentScript.ObjectToPlace = null;
    ///    }
    ///    else
    ///    {
    ///        SubComponentScript.ObjectToPlace = ObjectToPlace;
    ///    }
    ///
    ///    SubComponentScript.TransparentMat = TransparentMat;
    ///    SubComponentScript.ParentMat = ParentMat;
    ///    SubComponentScript.originalMat = originalMat;
    ///    UIController.SubComponentEditingSet(SubComponentScript);
    ///    SubComponentScript.enabled = true;
    ///    SubComponentScript.selectionMode = selectionMode;
    ///    this.enabled = false;
    ///}

    // Runs the EditMesh() function in the current activeMainVisualizer, if the activeMainVisualizer is not null.
    public void RunEdit()
    {
        if (MeshInEdit)
        {
            Debug.Log("Trying to set Components for Selection");
            Debug.Log(SpawnedComponents.Count - 1);
            MeshInEdit = false;
            UniSelection.Objects.Clear();
            for (int i = 0; i < SpawnedComponents.Count; i++)
            {
                UniSelection.Objects.Add(SpawnedComponents[i].gameObject);
            }
        }            
        else if (!MeshInEdit)
        {
            Debug.Log("Clearing UniSelection list for Mesh Visualizer");
            MeshInEdit = true;
            UniSelection.Objects.Clear();
        }
            

        if (Selection.Count - 1 == 0)
            Selection[0].GetComponent<MeshVisualizer>().EditMesh();
        else if (Selection.Count - 1 > 0)
        {
            for (int i = 0; i < Selection.Count; i++)
            {
                if (!Selection[i].NoVisualizer)
                {
                    Selection[i].GetComponent<MeshVisualizer>().EditMesh();
                }


            }
        }
    }

    // Used to instantiate the Handles used for moving, scaling and rotating a component.
    void InstantiateHandle()
    {
        // Place a handles game object at the position of the activeMainVisualizer's gameObject, 
        // also set the handles parent to the gameobject of the activeMainVisualizer .
        GameObject go = (GameObject)Instantiate(Handles, Selection[0].transform);
        go.transform.SetParent(Selection[0].transform);
        UIController.handlesOn = go;
    }

    // Sets the Vetex Points of the given MeshVisualizer to Layer 0.
    void GetColliders(MeshVisualizer meshVisualizer)
    {
        // If activeSecondVisualizer.Cols list is not Empty.
        if (meshVisualizer.Cols.Count != 0)
        {
            //then
            //foreach item in the list activeSecondVisualizer.Cols
            foreach (var item in meshVisualizer.Cols)
            {
                //set the gameObject layer the item is attached to, to layer 0 (default layer)
                item.gameObject.layer = 0;
            }
        }
    }

    // Deselects everything.
    public void Deselect()
    {
        Debug.Log("Deselect ran");
        if (Selection.Count - 1 >= 0)
        {
            // If the Selection list is not null.
            if (Selection.Count - 1 >= 1)
            {
                // Run the DeselectSecondary function.
                DeselectSecondary();
            }
            // Run the ResetVisualizer helper function on the activeMainVisualizer.
            // Then Set the mainVisualizer  & var on the UI script  & the activeMainVisualizer var in this script to null.
            if (!Selection[0].NoVisualizer)
            {
                Selection[0].GetComponent<MeshVisualizer>().ResetVisualizer();
                //Selection[0].GetComponent<MeshRenderer>().material = originalMat;
            }
            Selection[0].GetComponent<Collider>().enabled = true;
            
            UIController.Main = null;
            Selection.Clear();
            // No objects are selected now, so set objectSelected to False.
            // Destroy the gameObject the HandleScript is attached to, to Despawn the handles.
            // Set the UI Controller's HandlesOn var to null.
            objectSelected = false;
            Destroy(GetComponentInChildren<HandleScript>().PosAxisSpawned);
            Destroy(GetComponentInChildren<HandleScript>().gameObject);
            UIController.handlesOn = null;
        }
    }

    // Only Deselects things in the Selection List. Name is not quite right.
    public void DeselectSecondary()
    {
        Debug.Log("Secondary Deselect ran");
        // for every MeshVisualizer in the Selection List:
        for (int i = 0; i < Selection.Count; i++)
        {
            // Run the ResetVisualizer helper function in the current MeshVisualizer.
            if (!Selection[i].NoVisualizer)
            {
                Selection[i].GetComponent<MeshVisualizer>().ResetVisualizer();
                //Selection[i].gameObject.GetComponent<MeshRenderer>().material = originalMat;
            }
            Selection[i].GetComponent<Collider>().enabled = true;
        }

        // No Secondary Object is selected now, so set secondObjectSelected to false.
        // Set activeSecondVisualizer to null.
        // Clear the selection list.
        SecondObjSelected = false;
    }

    // RayCast function, returns a Collider Component.
    public Collider DoRaycast()
    {
        // theCamera is assigned to the currently active camera.
        //create a ray using the "ScreenPointToRay" helper which takes the mouse positon, this returns a ray cast from the mouse's positon in the camera view.
        Camera theCamera = Camera.main;
        Ray ray = theCamera.ScreenPointToRay(Input.mousePosition);

        // If ray cast is true, ie we hit a thing:
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            // Return the hit infomation.
            return hitInfo.collider;
        }
        // No collider was hit, return null.
        return null;
    }

    // Used by the ship manager to get all the mesh colliders in the model.
    public void GetMeshColliders()
    {
        ShipManager.MeshColliders.AddRange(GetComponentsInChildren<MeshCollider>());
        ShipManager.MeshColliders.RemoveAt(0);
    }
}
