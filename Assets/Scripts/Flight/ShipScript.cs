using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipScript : MonoBehaviour
{
    private Rigidbody Self;
    public Quaternion Rotation = Quaternion.Euler(0, 0, 0);
    // Start is called before the first frame update
    void Start()
    {
        Self = GetComponent<Rigidbody>();
    }

    public void OnGUI()
    {
        GUILayout.Label("Heading: " + gameObject.transform.localRotation.eulerAngles.y);
        GUILayout.Label("Pitch: " + gameObject.transform.localRotation.eulerAngles.x);
        GUILayout.Label("Roll: " + gameObject.transform.localRotation.eulerAngles.z);
    }

    void FixedUpdate()
    {
        Self.MoveRotation(Camera.main.transform.rotation);
    }
}
