using UnityEngine;
using UnityEngine.EventSystems;

public class CameraManger : MonoBehaviour
{
    public Camera theCamera;
    private Transform cameraRig;
    private Vector3 lastMousePos;

    public float OrbitSensitivity = 10;
    public bool HoldToOrbit = false;

    public float ZoomMultiplier = 2;
    public float minDistance = 2;
    public float maxDistance = 25;
    public bool InvertZoomDirection = false;
    public float PanSpeed = 0.1f;


    public float distanceMin = .5f;
    public float distanceMax = 15f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float yOffset = 1;

    public bool flightMode = false;

    // Start is called before the first frame update
    void Start()
    {
        if (theCamera == null)
        {
            theCamera = GetComponent<Camera>();

        }
        if (theCamera == null)
        {
            theCamera = Camera.main;
        }
        if (theCamera == null)
        {
            Debug.LogError("Could not find a camera.");
            return;
        }
        cameraRig = theCamera.transform.parent;
    }

    public void PanCamera()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //move the camera backwards or forwards based on the value of delta
        Vector3 actualChange = input * PanSpeed;

        actualChange = Quaternion.Euler(0, theCamera.transform.localRotation.eulerAngles.y, 0) * actualChange;

        Vector3 newPosition = cameraRig.transform.position + actualChange;

        cameraRig.transform.position += actualChange;
    }

    public void DollyCamera()
    {
        float delta = -Input.GetAxis("Mouse ScrollWheel");
        if (InvertZoomDirection)
        {
            delta = -delta;
        }
        //move the camera backwards or forwards based on the value of delta
        Vector3 actualChange = theCamera.transform.localPosition * ZoomMultiplier * delta;

        Vector3 newPosition = theCamera.transform.localPosition + actualChange;

        newPosition = newPosition.normalized * Mathf.Clamp(newPosition.magnitude, minDistance, maxDistance);

        theCamera.transform.localPosition += actualChange;
    }

    public void OrbitCamera()
    {
        //Fire2
        if (Input.GetButtonDown("Fire2"))
        {
            // The mouse was pressed ON THIS FRAME
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetButton("Fire2"))
        {
            // We are currently holding down the right mouse button

            // What is the current position of the mouse on the screen?
            Vector3 currentMousePos = Input.mousePosition;

            // In pixels!
            Vector3 mouseMovement = currentMousePos - lastMousePos;

            // Let's "orbit" the camera rig with our actual camera object
            // When we orbit, the distance from the rig stays the same, but
            // the angle changes.  Or another way to put, we want to rotate
            // the vector indicating the relative position of our camera from
            // the rig

            //Vector3 posRelativeToRig = theCamera.transform.localPosition;

            Vector3 rotationAngles = mouseMovement / OrbitSensitivity;

            if (HoldToOrbit)
            {
                rotationAngles *= Time.deltaTime;
            }

            // TODO: Fix me
            //Quaternion theOrbitalRotation = Quaternion.Euler( rotationAngles.y, rotationAngles.x, 0 );

            //posRelativeToRig = theOrbitalRotation * posRelativeToRig;

            theCamera.transform.RotateAround(cameraRig.position, theCamera.transform.right, -rotationAngles.y);
            theCamera.transform.RotateAround(cameraRig.position, theCamera.transform.up, rotationAngles.x);

            //cameraRig.Rotate( theOrbitalRotation.eulerAngles, Space.Self );

            // Make sure our camera is still looking are our focal point (i.e. the rig)

            Quaternion lookRotation = Quaternion.LookRotation(-theCamera.transform.localPosition);
            theCamera.transform.rotation = lookRotation;
            

            if (HoldToOrbit == false)
            {
                lastMousePos = currentMousePos;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        OrbitCamera();
        if (!Input.GetButton("Left Alt") && !Input.GetButton("Left Shift"))
        {
            DollyCamera();
        }
        
        PanCamera();
    }
}
