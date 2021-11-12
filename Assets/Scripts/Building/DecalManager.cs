using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.EventSystems;

public class DecalManager : MonoBehaviour
{
    [Header("Script References")]
    public ModelManger ModelManger;
    public DecalManagerUI GetDecalManagerUI;

    [Header("Settings")]
    public GameObject DecalToPlace;

    public bool PlacementMode = true;
    public bool roundingEnabled = false;
    public bool InvertZoomDirection = false;

    public GameObject DecaleManipulator;
    public GameObject DecalAreaVisual;
    public GameObject handles;

    public Material DefaultMat;
    public Material SelectMat;

    public LayerMask DecalManip;
    public LayerMask HandleLayerMask;

    public float RollMultiplier = 100;
    public float ScaleMultiplier = 0.1f;
    public float RollOffSet = 1f;
    public float ScaleOffset = 1f;
    public int roundingAccuracy = 1;

    [Header("Debug Info")]
    public List<DecalProjector> Details = new List<DecalProjector>();

    public List<GameObject> Selection = new List<GameObject>();
    private readonly List<GameObject> Handles = new List<GameObject>();
    private GameObject PerAxisHandle = null;

    private bool clickedOnThing = false;

    private Vector3 mouseLastFrame = Vector3.zero;
    private float LastDeltal = float.NaN;

    // Called when the decal manager is enabled, all it does is enable the UI.
    public void StartDecalManager()
    {
        GetDecalManagerUI.gameObject.SetActive(true);
    }

    // Changes the decal manager into movement mode.
    public void DecalMovementMode()
    {
        PlacementMode = false;
        Details.AddRange(gameObject.GetComponentsInChildren<DecalProjector>());
        for (int i = 0; i < Details.Count; i++)
        {
            // Spawn the gameobject currently assigned to Object to Place at the position of hte Camera's Focal point.
            // Then Set the Parent of the object we just spawned to the gameobject this script is on.
            _ = (GameObject)Instantiate(DecaleManipulator, Details[i].transform.position, Quaternion.identity, Details[i].transform);
            _ = (GameObject)Instantiate(DecalAreaVisual, Details[i].transform);
        }
    }

    // Changes the decal manager into placement mode.
    public void DecalPlacementMode()
    {
        Deselect();
        PlacementMode = true;
        for (int i = 0; i < Details.Count; i++)
        {
            Destroy(Details[i].transform.GetChild(0).gameObject);
            Destroy(Details[i].transform.GetChild(1).gameObject);
        }
        Details.Clear();
    }

    // function to go back into ship building mode.
    public void ShutDownDecalManager()
    {
        Deselect();
        DecalPlacementMode();
        GetDecalManagerUI.gameObject.SetActive(false);
        ModelManger.enabled = true;
        ModelManger.UIController.gameObject.SetActive(true);
        this.enabled = false;
    }

    // Every frame these things may happen.
    void Update()
    {
        // Main mouse button press register and functions.
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetButtonUp("Fire1"))
            {
                CheckLeftMouseUp();
            }
            if (Input.GetButtonDown("Fire1"))
            {
                CheckLeftClickDown();
            }
            if (Input.GetButton("Fire1"))
            {
                CheckLeftClick();
            }
        }

        // Mostly for Placement mode, moving a decal, rotating it, scaling it,
        // things that should only happen when a decal is selected.
        if (Selection.Count - 1 >= 0)
        {
            // Updates the position of the decals transform handle.
            Handles[0].transform.localPosition = Selection[0].transform.localPosition;

            // When holding left control and mouse 2, this rotates the decal to look were the camera is looking.
            // THIS NEEDS TO BE MORE INTUITIVE, Just left control seems ok.
            if (Input.GetButton("Left Control") && !Input.GetButton("Left Alt")&& !Input.GetButton("Left Shift"))//&& Input.GetButton("Fire2") && !EventSystem.current.IsPointerOverGameObject())
            {
                for (int i = 0; i < Selection.Count; i++)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(-Camera.main.transform.localPosition);

                    Selection[i].transform.localRotation = Quaternion.Euler(new Vector3(lookRotation.eulerAngles.x, lookRotation.eulerAngles.y, Selection[i].transform.localRotation.eulerAngles.z));
                }
            }


            // The Alt key functions allows the decal to be turned in place.
            // THIS JUST DOESNT WORK AT ALL.
            if (Input.GetButtonDown("Left Alt"))
            {
                RollOffSet = Selection[0].transform.localRotation.eulerAngles.z;
            }

            if (Input.GetButton("Left Alt"))
            {
                float delta = Input.GetAxis("Mouse ScrollWheel");
                if (InvertZoomDirection)
                {
                    delta = -delta;
                }

                
                RollOffSet = RollMultiplier * delta;
                if (LastDeltal != delta)
                {
                    for (int i = 0; i < Selection.Count; i++)
                    {
                        Vector3 temp = Selection[i].transform.localRotation.eulerAngles;
                        Selection[i].transform.localRotation = Quaternion.Euler(temp.x, temp.y, RollOffSet);
                    }
                }
                
                LastDeltal = delta;
            }


            // Shift key functions allow scaling using the scroll wheel.
            // This is fine.
            if (Input.GetButtonUp("Left Shift"))
            {
                ScaleOffset = 1f;
            }

            if (Input.GetButtonDown("Left Shift"))
            {
                ScaleOffset = Selection[0].GetComponent<DecalProjector>().size.x;
            }

            if (Input.GetButton("Left Shift"))
            {
                float delta = Input.GetAxis("Mouse ScrollWheel");
                if (InvertZoomDirection)
                {
                    delta = -delta;
                }
                for (int i = 0; i < Selection.Count; i++)
                {
                    Vector3 temp = Selection[i].GetComponent<DecalProjector>().size;
                    temp.x += ScaleMultiplier * delta;
                    temp.y += ScaleMultiplier * delta;
                    Selection[i].GetComponent<DecalProjector>().size = temp;
                    Selection[i].transform.Find("DecalArea(Clone)").transform.localScale = temp;
                }
            }
        }
    }


    // Left Mouse down function.
    void CheckLeftClickDown()
    {
        // If we are NOT in placement mode we do/checkFor these things.
        // Else we do/checkFor whats in the else.
        if (!PlacementMode)
        {
            // Do a raycast
            Collider theCollider = ModelManger.DoRaycast();
            // If we got a thing;
            if (theCollider != null)
            {
                clickedOnThing = false;
                bool SelectionAcc = false; // Might just be able to use clickedOnThing, but whatever.
                int maskForThisHitObject = 1 << theCollider.gameObject.layer;
                if ((maskForThisHitObject & DecalManip) != 0)
                {
                    // If we get here we clicked on a valid decal.
                    if (Selection.Count - 1 < 0)
                    {
                        
                        SelectionAcc = true;
                        clickedOnThing = true;

                        SetSelectedMat(theCollider);
                        Selection.Add(theCollider.transform.parent.gameObject);
                        InstantiateHandle();

                        // This calculates and sets mouseLastFrame, to prevent the decal from jumping.
                        float distanceToScreen = Camera.main.WorldToScreenPoint(Selection[0].transform.localPosition).z;
                        // If Per AxisHandle is NOT equal to null then:
                        if (PerAxisHandle != null)
                        {
                            // distanceToScreen is equal to the z component of the distance from the handle clicked on.
                            distanceToScreen = Camera.main.WorldToScreenPoint(PerAxisHandle.transform.localPosition).z;
                        }
                        mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
                    }
                    // Left control must be held down in order to select another item.
                    else if (Input.GetButton("Left Control") && Selection.Count - 1 >= 0)
                    {
                        // This does the same thing as if selection is less than 0, 
                        // but it checks if the item is already selected before adding it to the list.
                        if (!Selection.Contains(theCollider.transform.parent.gameObject))
                        {
                            SelectionAcc = true;
                            clickedOnThing = true;
                            SetSelectedMat(theCollider);
                            Selection.Add(theCollider.transform.parent.gameObject);

                            float distanceToScreen = Camera.main.WorldToScreenPoint(Selection[0].transform.localPosition).z;
                            // If Per AxisHandle is NOT equal to null then:
                            if (PerAxisHandle != null)
                            {
                                // distanceToScreen is equal to the z component of the distance from the handle clicked on.
                                distanceToScreen = Camera.main.WorldToScreenPoint(PerAxisHandle.transform.localPosition).z;
                            }
                            mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
                        }
                    }
                    // This does the same thing as Selection < 0 
                    // BUT is so if you have an object selected and you click on a deselected item,
                    // it then selects it and it becomes the primary.
                    else if (Selection.Count - 1 >= 0 && theCollider.transform.parent.gameObject != Selection[0])
                    {
                        Deselect();
                        SelectionAcc = true;
                        clickedOnThing = true;

                        SetSelectedMat(theCollider);
                        Selection.Add(theCollider.transform.parent.gameObject);
                        InstantiateHandle();

                        float distanceToScreen = Camera.main.WorldToScreenPoint(Selection[0].transform.localPosition).z;
                        // If Per AxisHandle is NOT equal to null then:
                        if (PerAxisHandle != null)
                        {
                            // distanceToScreen is equal to the z component of the distance from the handle clicked on.
                            distanceToScreen = Camera.main.WorldToScreenPoint(PerAxisHandle.transform.localPosition).z;
                        }
                        mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
                    }
                    // This is to stop selected items jumping when they are clicked on to move.
                    if (Selection.Count - 1 >= 0 && Selection.Contains(theCollider.transform.parent.gameObject))
                    {
                        clickedOnThing = true;
                        float distanceToScreen = Camera.main.WorldToScreenPoint(Selection[0].transform.localPosition).z;
                        // If Per AxisHandle is NOT equal to null then:
                        if (PerAxisHandle != null)
                        {
                            // distanceToScreen is equal to the z component of the distance from the handle clicked on.
                            distanceToScreen = Camera.main.WorldToScreenPoint(PerAxisHandle.transform.localPosition).z;
                        }
                        mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
                    }
                }
                
                if (SelectionAcc == false)
                {
                    // We get here if we clicked on the xyz axis handles, 
                    // this requires that an object is selected to work, 
                    // otherwise it just continues.

                    // If the Collider is on the handle layer mask:
                    maskForThisHitObject = 1 << theCollider.gameObject.layer;
                    if ((maskForThisHitObject & HandleLayerMask) != 0)
                    {
                        PerAxisHandle = theCollider.gameObject;
                        clickedOnThing = true;

                        if (Selection.Count - 1 >= 0)
                        {
                            float distanceToScreen = Camera.main.WorldToScreenPoint(Selection[0].transform.localPosition).z;
                            // If Per AxisHandle is NOT equal to null then:
                            if (PerAxisHandle != null)
                            {
                                // distanceToScreen is equal to the z component of the distance from the handle clicked on.
                                distanceToScreen = Camera.main.WorldToScreenPoint(PerAxisHandle.transform.localPosition).z;
                            }
                            mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
                        }
                    }
                }
            }
            // No thing, so we deselect().
            else
            {
                Deselect();
            }
        }
        else
        {
            // we place a decal and make it look were the camera is looking.
            GameObject go = (GameObject)Instantiate(DecalToPlace, Camera.main.transform.parent);
            go.transform.SetParent(gameObject.transform);

            Quaternion lookRotation = Quaternion.LookRotation(-Camera.main.transform.localPosition);

            go.transform.localRotation = Quaternion.Euler(new Vector3(lookRotation.eulerAngles.x, lookRotation.eulerAngles.y, go.transform.localRotation.eulerAngles.z));
        }

    }

    // Used to set the object being selected to a different material to indicate it has been selected.
    void SetSelectedMat(Collider theCollider)
    {
        theCollider.gameObject.GetComponent<MeshRenderer>().material = SelectMat;
    }


    // Main left click is down function.
    void CheckLeftClick()
    {
        if (Selection.Count - 1 >= 0 && !PlacementMode)
        {
            // if we arent in placement mode and we have something selected, we get here

            // If we clicked on a thing when the mouse went down, transform the thing.
            if (clickedOnThing)
            {
                TransformDecal();
            }

            // Maths to making transforming the decal smooth.
            float distanceToScreen = Camera.main.WorldToScreenPoint(Selection[0].transform.localPosition).z;
            // If Per AxisHandle is NOT equal to null then:
            if (PerAxisHandle != null)
            {
                // distanceToScreen is equal to the z component of the distance from the handle clicked on.
                distanceToScreen = Camera.main.WorldToScreenPoint(PerAxisHandle.transform.localPosition).z;
            }
            mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
        }
    }


    // Main left click up function.
    void CheckLeftMouseUp()
    {
        // PerAxisHandle set to null.
        PerAxisHandle = null;
        clickedOnThing = false;
    }

    // Transforming the decal, this is only for position.
    void TransformDecal()
    {
        // Get the Z component of the distance to the primary vertexPoint. (item 0 in vertexPosSelection/vertexSelection.
        float distanceToScreen = Camera.main.WorldToScreenPoint(Selection[0].transform.localPosition).z;

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

        // pos minus equals mouseLastFrame.
        pos -= mouseLastFrame;

        if (PerAxisHandle != null)
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

        for (int i = 0; i < Selection.Count; i++)
        {
            //Quaternion lookRotation = Quaternion.LookRotation(-Camera.main.transform.localPosition);
            Selection[i].transform.localPosition += pos;
        }
    }

    // Deselect the decal(s).
    void Deselect()
    {
        DestroyerHandles();
        for (int i = 0; i < Selection.Count; i++)
        {
            Selection[i].GetComponentInChildren<MeshRenderer>().material = DefaultMat;
        }
        Selection.Clear();
        Handles.Clear();
    }

    // Spawn a handle.
    void InstantiateHandle()
    {
        // Create a position from the position of the vertexPoint we need to spawn at.
        Vector3 point = Selection[0].transform.position;

        // Spawn the handle.
        // Set the handle's parent to the Vertex Point.
        GameObject go = (GameObject)Instantiate(handles, point, Quaternion.identity);
        go.transform.SetParent(this.transform);

        // Componsate for scale of the parent.
        // Add the Handle toe the Handles List.a
        //go.transform.localScale *= transform.localScale.magnitude * 0.75f;
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
}
#region broken

#endregion