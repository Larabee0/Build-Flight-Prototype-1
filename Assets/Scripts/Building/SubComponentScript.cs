using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SubComponentScript : MonoBehaviour
{
    // Refernces to other scripts
    [HideInInspector]
    public EditButtonHandler UIController;

    public ModelManger ModelManger;

    // Game Object references, the handle prefab and 
    // current object that will be placed when mouse 0 is pressed.
    public GameObject Handles;
    public GameObject ObjectToPlace;


    // Materials
    public Material TransparentMat;
    public Material ParentMat;

    [HideInInspector]
    public Material originalMat;


    // Bool logic variables
    [HideInInspector]
    public bool objectSelected = false;
    [HideInInspector]
    public bool secondObjectSelected = false;
    //[HideInInspector]
    public bool selectionMode = false;
    [HideInInspector]
    public bool MeshVisualizerInEditMode = false;


    // Lists    
    public List<ComponentStatsManager> Selection = null;// of the Selection.
    public List<GameObject> ClipBoard = new List<GameObject>();// of the copy pasteClipBoard


    // Position of the main visualizer last frame.
    private Vector3 posLastFrame = Vector3.zero;


    // Update is called once per frame
    void Update()
    {
        if (Selection.Count - 1 >= 0)
        {
            if (Selection[0].transform.parent.name != gameObject.name)
            {
                if (Selection[0].transform.parent.gameObject.GetComponent<MeshRenderer>().material != ParentMat)
                {
                    GameObject Temp = Selection[0].transform.parent.gameObject;
                    Temp.GetComponent<MeshRenderer>().material = ParentMat;
                }
            }
        }
        // Run selections function. Left Shift
        if (Input.GetButtonDown("Fire1"))
        {
            ModelManagerLeftMouseDown();
        }

        if (Input.GetKeyDown(KeyCode.P) && secondObjectSelected)
        {
            for (int i = 1; i < Selection.Count; i++)
            {
                Selection[i].gameObject.transform.parent = Selection[0].gameObject.transform;
            }
        }

        if (Input.GetKeyDown(KeyCode.O) && Input.GetButton("Left Shift"))
        {
            for (int i = 0; i < Selection.Count; i++)
            {
                Selection[i].gameObject.transform.SetParent(gameObject.transform);
            }
        }

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

        if (Input.GetButton("Left Control") && Input.GetKeyDown(KeyCode.C))
        {
            ClipBoard.Clear();
            if (objectSelected && Selection.Count - 1 >= 0)
            {
                //ClipBoard.Add(Selection[0].gameObject);
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
            // Then:
            // If the meshVisualizer at position 0's Cols List is Count is equal to 0 AND
            // if the Cols List count in the activeMainVisualizer is NOT equal to 0 OR
            // if the meshVisualizer at position 0's Cols List is Count is not equal to 0 AND
            // if the Cols List count in the activeMainVisualizer is equal to 0.
            MeshVisualizer SelectionZero = Selection[0].GetComponent<MeshVisualizer>();
            if (SelectionZero.Cols.Count == 0 && SelectionZero.Cols.Count != 0 || SelectionZero.Cols.Count != 0 && SelectionZero.Cols.Count == 0)
            {
                // Then:
                // For each item in the Selection List.
                for (int i = 0; i < Selection.Count; i++)
                {
                    // Run the Edit Mesh function on the current MeshVisualizer.
                    // Then Run the local function, Get Colliders, giving it the current meshVisualizer.
                    Selection[i].GetComponent<MeshVisualizer>().EditMesh();
                    GetColliders(Selection[i].GetComponent<MeshVisualizer>());
                }
            }

            // IF the activeMainVisualizer's Cols List count is equal to 0 AND the MeshVisualizer Cols List count is also 0.
            if (SelectionZero.Cols.Count == 0 && SelectionZero.Cols.Count == 0)
            {
                // If the M key went down this frame:
                if (Input.GetKeyDown(KeyCode.M))
                {
                    //Run the merge selection helper function in the ActiveMainVisualizer Script,
                    // passing in all the mesh visualizers in the selection list.
                    SelectionZero.MergeMesh(Selection);
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
        #endregion

        // If the Delete Key went down this frame and an Object is selected:
        if (Input.GetKeyDown(KeyCode.Delete) && objectSelected)
        {
            // Destroy the activeMainVisualizer's gameobject.
            // Set the acttiveMainVisualizer var to null, and selected object selected to false.

            for (int i = 0; i < Selection.Count; i++)
            {
                Destroy(Selection[i].gameObject);
            }
            Selection.Clear();
            if (secondObjectSelected)
            {
                secondObjectSelected = false;
            }

            UIController.handlesOn = null;
            UIController.Main = null;
            Selection.Clear();
            objectSelected = false;
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

        if (Input.GetKeyDown(KeyCode.J))
        {
            Vector3 OldScale = gameObject.transform.localScale;
            gameObject.transform.localScale = Vector3.one;
            Vector3 OldPos = gameObject.transform.position;
            gameObject.transform.position = Vector3.zero;
            Quaternion OldRot = gameObject.transform.rotation;
            gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);

            Deselect();
            ModelManger.SubCompInEdit = false;
            ModelManger.selectionMode = selectionMode;
            if (selectionMode)
            {
                SelectionModeToggler();
            }

            List<MeshFilter> ModelMeshes = new List<MeshFilter>();
            ModelMeshes.AddRange(gameObject.transform.GetComponentsInChildren<MeshFilter>());
            List<MeshRenderer> ModelRenderers = new List<MeshRenderer>();

            ModelRenderers.AddRange(gameObject.transform.GetComponentsInChildren<MeshRenderer>());

            gameObject.GetComponent<MeshRenderer>().enabled = false;
            CombineInstance[] combine = new CombineInstance[ModelMeshes.Count];

            for (int i = 1; i < ModelMeshes.Count; i++)
            {
                ModelMeshes[i].transform.rotation = OldRot;
            }
            for (int i = 0; i < ModelMeshes.Count; i++)
            {
                Debug.Log(ModelMeshes[i].name);
                combine[i].mesh = ModelMeshes[i].sharedMesh;
                combine[i].transform = ModelMeshes[i].transform.localToWorldMatrix;
                ModelMeshes[i].gameObject.SetActive(false);
            }

            Material[] Materials = new Material[ModelRenderers.Count];
            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i] = ModelRenderers[i].material;
            }

            gameObject.GetComponent<MeshFilter>().sharedMesh = new Mesh();
            gameObject.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine, false, true);
            gameObject.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            gameObject.GetComponent<MeshRenderer>().materials = Materials;
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            gameObject.GetComponent<MeshCollider>().enabled = true;


            for (int i = 1; i < ModelMeshes.Count; i++)
            {
                Destroy(ModelMeshes[i].gameObject);
            }
            gameObject.transform.position = OldPos;
            gameObject.transform.rotation = OldRot;
            gameObject.transform.localScale = OldScale;
            gameObject.SetActive(true);
            UIController.SubComponentEditing = false;
            ModelManger.enabled = true;
            this.enabled = false;
        }

    }

    void SelectionModeToggler()
    {
        if (UIController.SelectionModeToggle.GetComponent<Toggle>().isOn)
            UIController.SelectionModeToggle.GetComponent<Toggle>().isOn = false;
        else if (!UIController.SelectionModeToggle.GetComponent<Toggle>().isOn)
            UIController.SelectionModeToggle.GetComponent<Toggle>().isOn = true;
    }

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

    // Check if the user selected a thing(s)
    void ModelManagerLeftMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // If the left mouse button went down this frame, and we aren't in Selection Mode:
            if (!selectionMode)
            {
                // Spawn the gameobject currently assigned to Object to Place at the position of hte Camera's Focal point.
                // Then Set the Parent of the object we just spawned to the gameobject this script is on.
                GameObject go = (GameObject)Instantiate(ObjectToPlace, Camera.main.transform.parent);
                go.transform.SetParent(gameObject.transform);
                go.gameObject.tag = "SubComponentSelectable";
                Debug.Log(go.tag);
            }
            // Perform raycast and assign it to hitInfo.
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            //Debug.Log(hitInfo.transform.gameObject.name);

            //If we hit something:
            if (hit)
            {
                // If the thing we hit has the tag Selectable and we are in Selection Mode:
                if (hitInfo.transform.gameObject.tag == "SubComponentSelectable" && selectionMode)
                {
                    if (hitInfo.transform.gameObject.GetComponent<ComponentStatsManager>().ComponentType == 1)
                    {
                        // If Right Mouse went down this frame, AND an object isn't selected:
                        if (!objectSelected)
                        {
                            SelectMain(hitInfo.transform.gameObject);
                        }
                        // If Right Mouse went down this frame, AND an object is selected AND the left control key is being held down/went down:
                        else if (objectSelected && Input.GetButton("Left Control"))
                        {
                            // Set the MeshVisualizer in the thing we hit to Active, and disable the collider in the thing we hit.
                            hitInfo.transform.gameObject.GetComponent<MeshVisualizer>().enabled = true;
                            hitInfo.transform.gameObject.GetComponent<MeshCollider>().enabled = false;

                            // Add the MeshVisualizer on the thing we clicked on to the selection List.
                            Selection.Add(hitInfo.transform.gameObject.GetComponent<ComponentStatsManager>());
                            Selection[0].GetComponent<MeshVisualizer>().Selection = Selection.Count - 1;

                            Selection[Selection.Count - 1].gameObject.GetComponent<MeshRenderer>().material = TransparentMat;

                            // Disable the GUI in the MeshVisualizer we just clicked on.
                            // Set the secondObject selected to true.
                            Selection[Selection.Count - 1].GetComponent<MeshVisualizer>().disableGUI = true;
                            secondObjectSelected = true;
                        }
                        else if (objectSelected && hitInfo.transform.gameObject != Selection[0])
                        {
                            Deselect();
                            SelectMain(hitInfo.transform.gameObject);
                        }
                    }
                }
            }
            else if (Selection.Count - 1 >= 0)
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
        }
    }

    public void SelectMain(GameObject Main)
    {
        // Set the MeshVisualizer in the thing we hit to Active, and disable the collider in the thing we hit.
        Main.GetComponent<MeshVisualizer>().enabled = true;
        Main.GetComponent<MeshCollider>().enabled = false;

        // activeMainVisualizer is then set to the MeshVisualizer component of the thing we hit, 
        // this component is also assigned to hte main visualizer var in the UI script.
        Selection.Add(Main.GetComponent<ComponentStatsManager>());
        UIController.Main = Selection[0];
        // we have now selected an Object, so set to true.
        // and finally spawn the transform handles for the object.
        objectSelected = true;
        originalMat = Selection[0].gameObject.GetComponent<MeshRenderer>().material;
        Selection[0].gameObject.GetComponent<MeshRenderer>().material = TransparentMat;

        InstantiateHandle();
    }

    // Deselects everything.
    public void Deselect()
    {
        if (Selection.Count - 1 >= 0)
        {
            // If the Selection list is not null.
            if (Selection != null)
            {
                // Run the DeselectSecondary function.
                DeselectSecondary();
            }
            // Run the ResetVisualizer helper function on the activeMainVisualizer.
            // Then Set the mainVisualizer  & var on the UI script  & the activeMainVisualizer var in this script to null.
            Selection[0].GetComponent<MeshVisualizer>().ResetVisualizer();
            Selection[0].gameObject.GetComponent<MeshRenderer>().material = originalMat;
            UIController.Main = null;
            Selection.Clear();
            // No objects are selected now, so set objectSelected to False.
            // Destroy the gameObject the HandleScript is attached to, to Despawn the handles.
            // Set the UI Controller's HandlesOn var to null.
            objectSelected = false;
            Destroy(GetComponentInChildren<HandleScript>().gameObject);
            UIController.handlesOn = null;
        }
    }

    // Only Deselects things in the Selection List.
    public void DeselectSecondary()
    {
        // for every MeshVisualizer in the Selection List:
        for (int i = 0; i < Selection.Count; i++)
        {
            // Run the ResetVisualizer helper function in the current MeshVisualizer.
            Selection[i].GetComponent<MeshVisualizer>().ResetVisualizer();
            Selection[i].gameObject.GetComponent<MeshRenderer>().material = originalMat;
        }

        // No Secondary Object is selected now, so set secondObjectSelected to false.
        // Set activeSecondVisualizer to null.
        // Clear the selection list.
        secondObjectSelected = false;
    }

    // Instantiate a Handle.
    void InstantiateHandle()
    {
        // Place a handles game object at the position of the activeMainVisualizer's gameObject, 
        // also set the handles parent to the gameobject of the activeMainVisualizer .
        GameObject go = (GameObject)Instantiate(Handles, Selection[0].transform);
        go.transform.SetParent(Selection[0].transform);
        UIController.handlesOn = go;
    }
}
