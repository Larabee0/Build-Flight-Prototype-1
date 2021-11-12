using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls the handles for moving an object around, scaling and rotating it in build mode. this does not control handles in the decal manager or meshVisualizer.
/// </summary>
public class HandleScript : MonoBehaviour
{
    [Header("References to other Scripts")]
    public ModelManger Root;

    [Header("Prefab Game Object Lists")]
    public List<GameObject> PositionHandles = new List<GameObject>();
    public List<GameObject> RotHandles = new List<GameObject>();
    public List<GameObject> ScaleHandles = new List<GameObject>();

    [Header("Prefabs")]
    public GameObject PosAxis;
    public GameObject PosAxisSpawned;

    [Header("Undo/Redo Lists")]
    public List<Vector3> UndoList = new List<Vector3>();
    public List<Vector3> RedoList = new List<Vector3>();

    [Header("Debug Info")]
    public int Mode = 0;
    // EditMode bool. are we in edit mode? true/false.    
    public bool editMode = false;

    // Handle gameObject, the current handle that was clicked.
    [SerializeField] private GameObject Handle = null;

    // Various Vector3 values.
    private Vector3 lastMousePos = Vector3.zero;
    //private Vector3 LastScale = Vector3.one;
    private Vector3 mouseLastFrame = Vector3.zero;

    // When this script is spawned, cos it can only be spawned.
    void Start()
    {
        // Grabs the modelmanager from the ComponentStatsManager.
        // Spawns the Positional Axis, these do not rotate with the object, so are spawned "unparented".
        // Then grab references to all the handle gameObjects.
        Root = GetComponentInParent<ComponentStatsManager>().ModelManger;

        GameObject go = (GameObject)Instantiate(PosAxis, this.transform.position, Quaternion.identity, this.transform.parent.gameObject.transform.parent);
        PosAxisSpawned = go;
        PositionHandles.Add(PosAxisSpawned.transform.Find("XAxisPos").gameObject);
        PositionHandles.Add(PosAxisSpawned.transform.Find("YAxisPos").gameObject);
        PositionHandles.Add(PosAxisSpawned.transform.Find("ZAxisPos").gameObject);

        RotHandles.Add(transform.Find("XAxisRot").gameObject);
        RotHandles.Add(transform.Find("YAxisRot").gameObject);
        RotHandles.Add(transform.Find("ZAxisRot").gameObject);

        ScaleHandles.Add(transform.Find("XAxisScale").gameObject);
        ScaleHandles.Add(transform.Find("YAxisScale").gameObject);
        ScaleHandles.Add(transform.Find("ZAxisScale").gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // This Script REQUIRES a ModelManger reference.
        if(Root == null)
        {
            Root = GetComponentInParent<ComponentStatsManager>().ModelManger;
        }
        if (Root == null)
        {
            Debug.LogError("No ComponentStatsManager or ComponentStatsManager Missing references to ModelManger\nShutting Down HandleScript.");
            
            this.enabled = false;
        }

        // Left Mouse Checks.
        if (Input.GetButtonDown("Fire1"))
        {
            LeftClickDown();
        }
        if (Input.GetButton("Fire1"))
        {
            LeftClick();
        }
        if (Input.GetButtonUp("Fire1"))
        {
            LeftClickUp();
        }

        // If we are in Mode 0 then Undo Redo functions are allowed to work.
        if (Mode == 0)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z) && UndoList.Count - 1 >= 0)
            {
                if (UndoList.Count - 1 > 0)
                {
                    gameObject.transform.parent.position = UndoList[UndoList.Count - 2];
                    RedoList.Add(UndoList[UndoList.Count - 1]);
                    UndoList.RemoveAt(UndoList.Count - 1);
                }
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y) && RedoList.Count - 1 >= 0)
            {
                gameObject.transform.parent.position = RedoList[0];
                UndoList.Add(RedoList[0]);
                RedoList.RemoveAt(0);
            }
        }

        // Updates the position of hte positional Transform Handles to the current position of the component.
        PosAxisSpawned.transform.position = this.transform.parent.position;

        ///Debug.Log(transform.lossyScale);
        ///if (transform.parent.transform.localScale != LastScale)
        ///{
        ///    Debug.Log(transform.lossyScale - transform.localScale);
        ///    transform.localScale -=  (transform.localScale - transform.lossyScale)/10;
        ///}
        ///    
        ///LastScale = transform.parent.transform.localScale;
    }

    // Check left mouse down.
    void LeftClickDown()
    {
        if (!editMode)
        {
            // Do a ray cast, assign its output to theCollider var.
            // If the Collider is not null:
            Collider theCollider = Root.DoRaycast();
            if (theCollider != null)
            {
                // Handle is assigned to the gameObject the collider is attached to.
                Handle = theCollider.gameObject;

                // Convert where the mouse distance to the gameObject this script is attached into a Vector3, then take the Z component of that Vector and assign it to the float distanceToScreen.
                float distanceToScreen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
                // Convert where the mouse is into a positon in the world, assign it to the Vector 3 mouseLastFrame.
                // get the currnet mouse position, assign it to the Vector3 LastMousePos.
                mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
                lastMousePos = Input.mousePosition;
                // Add the pre modified position to the undo redo system.
                if (UndoList.Count - 1 > 0)
                {
                    if (gameObject.transform.parent.position != UndoList[UndoList.Count - 1])
                    {
                        UndoList.Add(gameObject.transform.parent.position);
                    }
                }
                else
                {
                    UndoList.Add(gameObject.transform.parent.position);
                }
            }
        }
    }

    // Check left mouse up.
    void LeftClickUp()
    {
        // When the right Mouse button goes up and we aren't in editMode:
        if (!editMode)
        {
            // Set the Handle var to null.
            Handle = null;
            // Add the final position to the undo redo system.
            if (UndoList.Count - 1 > 0)
            {
                if (gameObject.transform.parent.position != UndoList[UndoList.Count - 1])
                {
                    UndoList.Add(gameObject.transform.parent.position);
                }
            }
            else
            {
                UndoList.Add(gameObject.transform.parent.position);
            }
        }
    }

    // if left click is down now, move all the things.
    void LeftClick()
    {
        // If the left Mouse Button is being pressed AND the Handle Var is not null AND we aren't in editMode:
        if (Handle != null && !editMode)
        {
            
            // If the Collider we clicked has the tag "Handles":
            if (Handle.CompareTag("Handles"))
            {
                // Convert where the mouse distance to the gameObject this script is attached into a Vector3, then take the Z component of that Vector and assign it to the float distanceToScreen.
                float distanceToScreen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

                // Create a new Vector3 called Pos, assign it the Vector3 result of Screen to world point helper, 
                // which takes, the mouse position (X,Y) and the float distanceToScreen as Z.
                Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));

                // pos is equal to itself minus the Vector3 mouseLastFrame.
                pos -= mouseLastFrame;

                // If Mode is set to 0:
                if (Mode == 0)//position Mode
                {
                    // If the handle we clicked on is named AllAxis:
                    if (Handle.name == "AllAxis")
                    {
                        // The parent gameObject of the gameObject this scrpit is attached to is equal to its current position,
                        // plus the Vector3 Pos.
                        gameObject.transform.parent.position = gameObject.transform.position + pos;
                    }
                    // If the handle we clicked on is named XAxisPos:
                    else if (Handle.name == "XAxisPos")
                    {
                        // The parent gameObject of the gameObject this scrpit is attached to is equal to its current position,
                        // the X axis gets the X component of Pos added to it.
                        gameObject.transform.parent.position = new Vector3(gameObject.transform.position.x + pos.x, gameObject.transform.position.y, gameObject.transform.position.z);
                    }
                    // If the handle we clicked on is named YAxisPos:
                    else if (Handle.name == "YAxisPos")
                    {
                        // The parent gameObject of the gameObject this scrpit is attached to is equal to its current position,
                        // the Z axis gets the Z component of Pos added to it.
                        gameObject.transform.parent.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + pos.y, gameObject.transform.position.z);
                    }
                    // If the handle we clicked on is named ZAxisPos:
                    else if (Handle.name == "ZAxisPos")
                    {
                        // The parent gameObject of the gameObject this scrpit is attached to is equal to its current position,
                        // the Z axis gets the Z component of Pos added to it.
                        gameObject.transform.parent.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z + pos.z);
                    }
                }

                // If Mode is set to 1:
                else if (Mode == 1) // rotation mode
                {
                    // Create a new Vector3 CurrentMousePos, assign it the Mouse Position this frame.
                    // Create a new Vector3 MouseMovement, assign itCurrentMousePos take away, lastMousePos,
                    // resulting in the difference between the mouse this frame and last frame.
                    Vector3 CurrentMousePos = Input.mousePosition;
                    Vector3 MouseMovement = CurrentMousePos - lastMousePos;

                    // Create a float called mag, assign it the magnitude, the length, of the Vector3 MouseMovement.
                    float mag = MouseMovement.magnitude;
                    // If the X component or Y Component of CurrentMousePos is less than the X or Y Component of lastMousePos:
                    if (CurrentMousePos.x < lastMousePos.x || CurrentMousePos.y < lastMousePos.y)
                    {
                        // Make mag equal to itself except negative.
                        mag = -mag;
                    }

                    // Create a Vector3, CurrentRotation, assign it the current local Rotation of the parent of the gameObject this script is attached to
                    // as rotations are Quaternions, and Quaternion cannot be stored as Vector3's get the euler Angles.
                    // Create a new Vector3, Rotation Angles, create it from the float mag divided by 10. (mag div 10 is assigned to all components of RotationAngles).
                    Vector3 CurrentRotation = gameObject.transform.parent.localRotation.eulerAngles;
                    Vector3 RotationAngles = new Vector3(mag / 10, mag / 10, mag / 10);

                    // If the handle we clicked on is named XAxisRot:
                    if (Handle.name == "XAxisRot")
                    {
                        // The parent gameObject of the gameObject this scrpit is attached to is equal to its current localRotation,
                        // the X axis gets the X component of RotationAngles are added to it.
                        gameObject.transform.parent.localRotation = Quaternion.Euler(new Vector3(CurrentRotation.x + RotationAngles.x, CurrentRotation.y, CurrentRotation.z));
                    }
                    // If the handle we clicked on is named YAxisRot:
                    else if (Handle.name == "YAxisRot")
                    {
                        // The parent gameObject of the gameObject this scrpit is attached to is equal to its current localRotation,
                        // the Y axis gets the Y component of RotationAngles are added to it.
                        gameObject.transform.parent.localRotation = Quaternion.Euler(new Vector3(CurrentRotation.x, CurrentRotation.y + RotationAngles.y, CurrentRotation.z));
                    }
                    // If the handle we clicked on is named ZAxisRot:
                    else if (Handle.name == "ZAxisRot")
                    {
                        // The parent gameObject of the gameObject this scrpit is attached to is equal to its current localRotation,
                        // the Z axis gets the Z component of RotationAngles are added to it.
                        gameObject.transform.parent.localRotation = Quaternion.Euler(new Vector3(CurrentRotation.x, CurrentRotation.y, CurrentRotation.z + RotationAngles.z));
                    }
                }

                // If Mode is set to 2:
                else if (Mode == 2)//scale mode
                {
                    // Create a new Vector3 CurrentMousePos, assign it the Mouse Position this frame.
                    // Create a new Vector3 MouseMovement, assign itCurrentMousePos take away, lastMousePos,
                    // resulting in the difference between the mouse this frame and last frame.
                    Vector3 CurrentMousePos = Input.mousePosition;
                    Vector3 MouseMovement = CurrentMousePos - lastMousePos;

                    // Create a float called mag, assign it the magnitude, the length, of the Vector3 MouseMovement.
                    float mag = MouseMovement.magnitude;
                    // If the X component or Y Component of CurrentMousePos is less than the X or Y Component of lastMousePos:
                    if (CurrentMousePos.x < lastMousePos.x || CurrentMousePos.y < lastMousePos.y)
                    {
                        // Make mag equal to itself except negative.
                        mag = -mag;
                    }

                    // Create a Vector3, CurrentScale, assign it the current local Scale of the parent of the gameObject this script is attached to
                    // Create a new Vector3, Scale, create it from the float mag divided by 500. (mag div 500 is assigned to all components of CurrentScale).
                    Vector3 CurrentScale = gameObject.transform.parent.localScale;
                    Vector3 Scale = new Vector3(mag / 500, mag / 500, mag / 500);

                    // If the handle we clicked on is named AllAxis:
                    if (Handle.name == "AllAxis")
                    {
                        // The parent gameObject of the gameObject this scrpit is attached to is equal to its current local scale,
                        // plus the Vector3 Scale.
                        gameObject.transform.parent.localScale += Scale;
                    }
                    else if (Handle.name == "XAxisScale" || Handle.transform.parent.name == "XAxisScale")
                    {
                        // The parent gameObject of the gameObject this scrpit is attached to is equal to its current local scale,
                        // the X axis gets the X component of Pos added to it.
                        gameObject.transform.parent.localScale = new Vector3(CurrentScale.x + Scale.x, CurrentScale.y, CurrentScale.z);
                    }
                    else if (Handle.name == "YAxisScale" || Handle.transform.parent.name == "YAxisScale")
                    {
                        // The parent gameObject of the gameObject this scrpit is attached to is equal to its current local scale,
                        // the Y axis gets the Y component of Pos added to it.
                        gameObject.transform.parent.localScale = new Vector3(CurrentScale.x, CurrentScale.y + Scale.x, CurrentScale.z);
                    }
                    else if (Handle.name == "ZAxisScale" || Handle.transform.parent.name == "ZAxisScale")
                    {
                        // The parent gameObject of the gameObject this scrpit is attached to is equal to its current local scale,
                        // the Z axis gets the Z component of Pos added to it.
                        gameObject.transform.parent.localScale = new Vector3(CurrentScale.x, CurrentScale.y, CurrentScale.z + Scale.x);
                    }
                }

                // Convert where the mouse distance to the gameObject this script is attached into a Vector3, then take the Z component of that Vector and assign it to the float distanceToScreen.
                distanceToScreen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
                // Create a new Vector3 called Pos, assign it the Vector3 result of Screen to world point helper, 
                // which takes, the mouse position (X,Y) and the float distanceToScreen as Z.
                mouseLastFrame = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));

                // Update lastMousePos to the current Mouse Pos.
                lastMousePos = Input.mousePosition;
            }
        }
    }

    #region Handle Mode Helpers

    // PositionMode Helper Function.
    public void PositionMode()
    {
        // Mode is set to 0.
        Mode = 0;
        // For every item in PositionHandles, set gameObject to Active.
        PosAxisSpawned.SetActive(true);
        // For every item in RotHandles, set gameObject to inActive.
        for (int i = 0; i < RotHandles.Count; i++)
        {
            RotHandles[i].SetActive(false);
        }
        // For every item in ScaleHandles, set gameObject to inActive.
        for (int i = 0; i < ScaleHandles.Count; i++)
        {
            ScaleHandles[i].SetActive(false);
        }
    }

    public void RotMode()
    {
        // Mode is set to 1.
        Mode = 1;
        // For every item in PositionHandles, set gameObject to inActive.
        PosAxisSpawned.SetActive(false);
        // For every item in RotHandles, set gameObject to Active.
        for (int i = 0; i < RotHandles.Count; i++)
        {
            RotHandles[i].SetActive(true);
        }
        // For every item in ScaleHandles, set gameObject to inActive.
        for (int i = 0; i < ScaleHandles.Count; i++)
        {
            ScaleHandles[i].SetActive(false);
        }
    }

    public void ScaleMode()
    {
        // Mode is set to 2.
        Mode = 2;
        // For every item in PositionHandles, set gameObject to inActive.
        PosAxisSpawned.SetActive(false);
        // For every item in RotHandles, set gameObject to inActive.
        for (int i = 0; i < RotHandles.Count; i++)
        {
            RotHandles[i].SetActive(false);
        }
        // For every item in ScaleHandles, set gameObject to Active.
        for (int i = 0; i < ScaleHandles.Count; i++)
        {
            ScaleHandles[i].SetActive(true);
        }
    }

    #endregion

    #region Transform Helpers

    // MoveTo Helper Function.
    public void MoveTo(Vector3 pos)
    {
        // Check all Components of pos, to see if they are NaN, 
        // if they are, set the component to the current position
        // of parent of the gameObject this script is attached to.
        if (float.IsNaN(pos.x))
        {
            pos.x = gameObject.transform.parent.position.x;
        }
        if (float.IsNaN(pos.y))
        {
            pos.y = gameObject.transform.parent.position.y;
        }
        if (float.IsNaN(pos.z))
        {
            pos.z = gameObject.transform.parent.position.z;
        }

        // Set the position of the parent of the gameObject this script is attached to, to pos.
        gameObject.transform.parent.position = pos;
    }

    // RotateTo Helper Function.
    public void RotateTo(Vector3 Rotation)
    {
        // Check all Components of pos, to see if they are NaN, 
        // if they are, set the component to the current localRotation
        // of parent of the gameObject this script is attached to.
        if (float.IsNaN(Rotation.x))
        {
            Rotation.x = gameObject.transform.parent.localRotation.eulerAngles.x;
        }
        if (float.IsNaN(Rotation.y))
        {
            Rotation.y = gameObject.transform.parent.localRotation.eulerAngles.y;
        }
        if (float.IsNaN(Rotation.z))
        {
            Rotation.z = gameObject.transform.parent.localRotation.eulerAngles.z;
        }

        // Set the localRotation of the parent of the gameObject this script is attached to, to Rotation.
        gameObject.transform.parent.localRotation = Quaternion.Euler(Rotation);
    }

    // ScaleTo Helper Function.
    public void ScaleTo(Vector3 Scale)
    {
        // Check all Components of pos, to see if they are NaN, 
        // if they are, set the component to the current localScale
        // of parent of the gameObject this script is attached to.
        if (float.IsNaN(Scale.x) || Scale.x <= 0)
        {
            Scale.x = gameObject.transform.parent.localScale.x;
        }
        if (float.IsNaN(Scale.y) || Scale.y <= 0)
        {
            Scale.y = gameObject.transform.parent.localScale.y;
        }
        if (float.IsNaN(Scale.z) || Scale.z <= 0)
        {
            Scale.z = gameObject.transform.parent.localScale.z;
        }

        // Set the localScale of the parent of the gameObject this script is attached to, to Scale.
        gameObject.transform.parent.localScale = Scale;
    }

    #endregion

    // Function to Hide the Handles.
    public void HideHandles()
    {
        // For every child of the gameObject this script is attached to:
        for (int i = 0; i < transform.childCount; i++)
        {
            // Set the Child to Inactive.
            transform.GetChild(i).gameObject.SetActive(false);
        }
        PosAxisSpawned.SetActive(false);
        // Edit Mode is set to true.
        editMode = true;
    }

    // Function to Show the Handles.
    public void ShowHandles()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            // Set the Child to Inactive.
            transform.GetChild(i).gameObject.SetActive(true);
        }
        // Checks the Mode and runs the Handle Mode Helper corisponding to the Mode.
        if (Mode == 0)
        {
            PositionMode();
        }
        else if(Mode == 1)
        {
            RotMode();
        }
        else if(Mode == 2)
        {
            ScaleMode();
        }

        // EditMode is set to false.
        editMode = false;
    }
}
