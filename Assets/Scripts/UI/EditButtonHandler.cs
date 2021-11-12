using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ik its called EditButtonHandler, but this is the UI controller script
/// </summary>
public class EditButtonHandler : MonoBehaviour
{
    public GameObject CameraRig;
    // References to other scripts.
    [HideInInspector]
    public ModelManger ModelManger;
    [HideInInspector]
    public ComponentStatsManager Main;

    public ShipManager ShipManager;

    // Lists of input fields for position, rotation and scaling input fields.
    [HideInInspector]
    public List<InputField> PosFields = new List<InputField>();
    [HideInInspector]
    public List<InputField> RotFields = new List<InputField>();
    [HideInInspector]
    public List<InputField> ScaleFields = new List<InputField>();
    [HideInInspector]
    public List<InputField> CameraFields = new List<InputField>();

    // References to various gameobjects.
    [HideInInspector]
    public GameObject Location;
    [HideInInspector]
    public GameObject Rotation;
    [HideInInspector]
    public GameObject Scale;
    [HideInInspector]
    public GameObject CameraButton;
    [HideInInspector]
    public GameObject handlesOn;
    [HideInInspector]
    public GameObject SelectionModeToggle;
    [HideInInspector]
    public GameObject ResetMeshButton;
    [HideInInspector]
    public GameObject RoundingInputField;
    [HideInInspector]
    public GameObject AddQuadButton;
    [HideInInspector]
    public GameObject QuadSelection;

    private SubComponentScript SubComponentScript;

    public bool SubComponentEditing = false;

    // Bool for if statement logic.
    private bool RotationAndScaling = true;

    // Start is called before the first frame update
    void Start()
    {
        // Grab the model manager.
        ModelManger = GameObject.FindObjectOfType<ModelManger>();

        ShipManager = GameObject.FindObjectOfType<ShipManager>();

        // Assign the gameobject varaiables to the children of the right name (on the canvas).
        Location = transform.Find("LocationButton").gameObject;
        Rotation = transform.Find("RotationButton").gameObject;
        Scale = transform.Find("ScaleButton").gameObject;
        CameraButton = transform.Find("CameraLocationButton").gameObject;
        SelectionModeToggle = transform.Find("SelectionMode").gameObject;
        ResetMeshButton = transform.Find("ResetMesh").gameObject;
        RoundingInputField = transform.Find("RoundingButton").gameObject;
        AddQuadButton = transform.Find("AddingMode").gameObject;
        QuadSelection = transform.Find("QuadSelection").gameObject;

        // Add to the list each input field for the 3 transform sections.
        PosFields.Add(Location.transform.Find("PositonX").gameObject.GetComponentInChildren<InputField>());
        PosFields.Add(Location.transform.Find("PositonY").gameObject.GetComponentInChildren<InputField>());
        PosFields.Add(Location.transform.Find("PositonZ").gameObject.GetComponentInChildren<InputField>());

        RotFields.Add(Rotation.transform.Find("RotationX").gameObject.GetComponentInChildren<InputField>());
        RotFields.Add(Rotation.transform.Find("RotationY").gameObject.GetComponentInChildren<InputField>());
        RotFields.Add(Rotation.transform.Find("RotationZ").gameObject.GetComponentInChildren<InputField>());

        ScaleFields.Add(Scale.transform.Find("ScaleX").gameObject.GetComponentInChildren<InputField>());
        ScaleFields.Add(Scale.transform.Find("ScaleY").gameObject.GetComponentInChildren<InputField>());
        ScaleFields.Add(Scale.transform.Find("ScaleZ").gameObject.GetComponentInChildren<InputField>());
        ScaleFields.Add(Scale.transform.Find("ScaleAll").gameObject.GetComponentInChildren<InputField>());

        CameraFields.Add(CameraButton.transform.Find("PositonX").gameObject.GetComponentInChildren<InputField>());
        CameraFields.Add(CameraButton.transform.Find("PositonY").gameObject.GetComponentInChildren<InputField>());
        CameraFields.Add(CameraButton.transform.Find("PositonZ").gameObject.GetComponentInChildren<InputField>());
    }

    // Called every frame.
    void Update()
    {
        CameraFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "X = " + ((float)Math.Round(CameraRig.transform.position.x * 100000f) / 100000f).ToString();
        CameraFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Y = " + ((float)Math.Round(CameraRig.transform.position.y * 100000f) / 100000f).ToString();
        CameraFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Z = " + ((float)Math.Round(CameraRig.transform.position.z * 100000f) / 100000f).ToString();
        // If an object is select, ie the main visualizer exists. This is updated by the model manager:
        if (Main)
        {
            if (!Main.NoVisualizer)
            {
                if (Main.MeshVisualizer.Cols.Count - 1 < 0)
                {
                    TransformComponent();
                }
                else if (Main.MeshVisualizer.Cols.Count - 1 >= 0)
                {
                    if (Main.MeshVisualizer.vertexSelection.Count - 1 >= 0)
                    {
                        // Update the position placeholder text fields to show the current position of the main visualizer's gameobject.
                        PosFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "X = " + ((float)Math.Round(Main.MeshVisualizer.Cols[Main.MeshVisualizer.vertexSelection[0]].transform.position.x * 100000f) / 100000f).ToString();
                        PosFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Y = " + ((float)Math.Round(Main.MeshVisualizer.Cols[Main.MeshVisualizer.vertexSelection[0]].transform.position.y * 100000f) / 100000f).ToString();
                        PosFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Z = " + ((float)Math.Round(Main.MeshVisualizer.Cols[Main.MeshVisualizer.vertexSelection[0]].transform.position.z * 100000f) / 100000f).ToString();
                    }
                    if (Main.MeshVisualizer.NewTrianglesSelection.Count - 1 >= 0)
                    {
                        // Update the position placeholder text fields to show the current position of the main visualizer's gameobject.
                        PosFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "X = " + ((float)Math.Round(Main.MeshVisualizer.NewTrianglePoints[Main.MeshVisualizer.NewTrianglesSelection[0]].transform.position.x * 100000f) / 100000f).ToString();
                        PosFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Y = " + ((float)Math.Round(Main.MeshVisualizer.NewTrianglePoints[Main.MeshVisualizer.NewTrianglesSelection[0]].transform.position.y * 100000f) / 100000f).ToString();
                        PosFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Z = " + ((float)Math.Round(Main.MeshVisualizer.NewTrianglePoints[Main.MeshVisualizer.NewTrianglesSelection[0]].transform.position.z * 100000f) / 100000f).ToString();
                    }
                }
                else if (Main.MeshVisualizer.Cols.Count - 1 >= 0 && Main.MeshVisualizer.vertexSelection.Count - 1 < 0)
                {
                    // Update the position placeholder text fields to show the current position of the main visualizer's gameobject.
                    PosFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "X = 0";
                    PosFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Y = 0";
                    PosFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Z = 0";
                }
            }
            else
            {
                TransformComponent();
            }
        }
        if (!ModelManger.MeshInEdit)
        {
            if (SelectionModeToggle.activeSelf && !SubComponentEditing)
            {
                if (SelectionModeToggle.GetComponent<Toggle>().interactable && ModelManger.objectSelected)
                {
                    SelectionModeToggle.GetComponent<Toggle>().interactable = false;
                }
                if (!SelectionModeToggle.GetComponent<Toggle>().interactable && !ModelManger.objectSelected)
                {
                    SelectionModeToggle.GetComponent<Toggle>().interactable = true;
                }
            }
            else if (SelectionModeToggle.activeSelf && SubComponentEditing)
            {
                if (SelectionModeToggle.GetComponent<Toggle>().interactable && SubComponentScript.objectSelected)
                {
                    SelectionModeToggle.GetComponent<Toggle>().interactable = false;
                }
                if (!SelectionModeToggle.GetComponent<Toggle>().interactable && !SubComponentScript.objectSelected)
                {
                    SelectionModeToggle.GetComponent<Toggle>().interactable = true;
                }
            }
        }

        if (RoundingInputField.activeSelf)
        {
            RoundingInputField.transform.Find("DPs").transform.Find("Placeholder").GetComponent<Text>().text = Main.MeshVisualizer.roundingAccuracy.ToString() + " Decimal Place";
        }
    }

    void TransformComponent()
    {
        // If rotation and scale is true:
        if (RotationAndScaling)
        {
            // Update the position placeholder text fields to show the current position of the main visualizer's gameobject.
            PosFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "X = " + ((float)Math.Round(Main.transform.position.x * 100000f) / 100000f).ToString();
            PosFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Y = " + ((float)Math.Round(Main.transform.position.y * 100000f) / 100000f).ToString();
            PosFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Z = " + ((float)Math.Round(Main.transform.position.z * 100000f) / 100000f).ToString();

            // Update the rotation and scaling place holder fields for the current rotation and position of the main visualizer's gameobject.
            RotFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "X = " + ((float)Math.Round(Main.transform.localRotation.eulerAngles.x * 100000f) / 100000f).ToString() + "°";
            RotFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Y = " + ((float)Math.Round(Main.transform.localRotation.eulerAngles.y * 100000f) / 100000f).ToString() + "°";
            RotFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Z = " + ((float)Math.Round(Main.transform.localRotation.eulerAngles.z * 100000f) / 100000f).ToString() + "°";

            ScaleFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "X = " + ((float)Math.Round(Main.transform.localScale.x * 100000f) / 100000f).ToString();
            ScaleFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Y = " + ((float)Math.Round(Main.transform.localScale.y * 100000f) / 100000f).ToString();
            ScaleFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Z = " + ((float)Math.Round(Main.transform.localScale.z * 100000f) / 100000f).ToString();
            ScaleFields[3].transform.Find("Placeholder").GetComponent<Text>().text = "XYZ = " + ((float)Math.Round(Main.transform.localScale.x * 100000f) / 100000f).ToString();
        }
    }

    public void SubComponentEditingSet(SubComponentScript SubComponentScriptIntake)
    {
        SubComponentScript = SubComponentScriptIntake;
        SubComponentEditing = true;
    }

    public void TakeCameraPositon()
    {
        // Create a new vector3 comprised of 3 float.NaNs (for checking in the handle script)
        Vector3 pos = new Vector3(float.NaN, float.NaN, float.NaN);

        // For every inputfield in the Position Input fields list:
        for (int i = 0; i < PosFields.Count; i++)
        {
            // Create a new float called temp, of value float.NaN
            // Create two strings, one is assigned contain whatever is in the user inputable text box.
            // The other is assigned string.Empty, this will be the value turned into a float then put into the Vector3 "pos".
            float Temp;
            string TempString = CameraFields[i].transform.Find("Text").GetComponent<Text>().text;
            string TempNumber = string.Empty;
            //For the length of the text field:
            for (int c = 0; c < TempString.Length; c++)
            {
                // If the current character is a digit, add it to the TempNumber string.
                if (Char.IsDigit(TempString[c]))
                {
                    TempNumber += TempString[c];
                }
                // If the current character is a Decmial place and TempString doesn NOT contain a decmial place, add the decmial place to TempString.
                if (TempString[c] == '.' && !TempNumber.Contains("."))
                {
                    TempNumber += TempString[c];
                }
                // If the current charcter is a negative symbol and the tempnumber string has a length of 0 (so we can't add two negative symbols, and so no numbers are in front of it).
                if (TempNumber.Length <= 0 && TempString[c] == '-')
                {
                    TempNumber += TempString[c];
                }
            }
            // If TempNumber.length is greater than 0.
            if (TempNumber.Length > 0)
            {
                // Turn tempNumber into a float.
                Temp = float.Parse(TempNumber);
                // Then: if this is field 0 (X Axis) then assign Temp to the X Axis value of Vector3 "pos".
                if (i == 0)
                {
                    pos.x = Temp;
                }
                // Then: if this is field 1 (Y Axis) then assign Temp to the Y Axis value of Vector3 "pos".
                else if (i == 1)
                {
                    pos.y = Temp;
                }
                // Then: if this is field 2 (Z Axis) then assign Temp to the Z Axis value of Vector3 "pos".
                else if (i == 2)
                {
                    pos.z = Temp;
                }
            }
            // Select and Clear the current Input Field.
            CameraFields[i].Select();
            CameraFields[i].text = "";
        }
        // Check all Components of pos, to see if they are NaN, 
        // if they are, set the component to the current position
        // of parent of the gameObject this script is attached to.
        if (float.IsNaN(pos.x))
        {
            pos.x = CameraRig.transform.position.x;
        }
        if (float.IsNaN(pos.y))
        {
            pos.y = CameraRig.transform.position.y;
        }
        if (float.IsNaN(pos.z))
        {
            pos.z = CameraRig.transform.position.z;
        }

        // Set the position of the parent of the gameObject this script is attached to, to pos.
        CameraRig.transform.position = pos;
    }

    // Whenever any of the Position inputfields have finished being edited:
    public void TakePositon()
    {
        // Create a new vector3 comprised of 3 float.NaNs (for checking in the handle script)
        Vector3 pos = new Vector3(float.NaN, float.NaN, float.NaN);

        // For every inputfield in the Position Input fields list:
        for (int i = 0; i < PosFields.Count; i++)
        {
            // Create a new float called temp, of value float.NaN
            // Create two strings, one is assigned contain whatever is in the user inputable text box.
            // The other is assigned string.Empty, this will be the value turned into a float then put into the Vector3 "pos".
            float Temp;
            string TempString = PosFields[i].transform.Find("Text").GetComponent<Text>().text;
            string TempNumber = string.Empty;
            //For the length of the text field:
            for (int c = 0; c < TempString.Length; c++)
            {
                // If the current character is a digit, add it to the TempNumber string.
                if (Char.IsDigit(TempString[c]))
                {
                    TempNumber += TempString[c];
                }
                // If the current character is a Decmial place and TempString doesn NOT contain a decmial place, add the decmial place to TempString.
                if (TempString[c] == '.' && !TempNumber.Contains("."))
                {
                    TempNumber += TempString[c];
                }
                // If the current charcter is a negative symbol and the tempnumber string has a length of 0 (so we can't add two negative symbols, and so no numbers are in front of it).
                if (TempNumber.Length <= 0 && TempString[c] == '-')
                {
                    TempNumber += TempString[c];
                }
            }
            // If TempNumber.length is greater than 0.
            if (TempNumber.Length > 0)
            {
                // Turn tempNumber into a float.
                Temp = float.Parse(TempNumber);
                // Then: if this is field 0 (X Axis) then assign Temp to the X Axis value of Vector3 "pos".
                if (i == 0)
                {
                    pos.x = Temp;
                }
                // Then: if this is field 1 (Y Axis) then assign Temp to the Y Axis value of Vector3 "pos".
                else if (i == 1)
                {
                    pos.y = Temp;
                }
                // Then: if this is field 2 (Z Axis) then assign Temp to the Z Axis value of Vector3 "pos".
                else if (i == 2)
                {
                    pos.z = Temp;
                }
            }
            // Select and Clear the current Input Field.
            PosFields[i].Select();
            PosFields[i].text = "";
        }
        if (!Main.NoVisualizer)
        {
            if (Main.MeshVisualizer.Cols.Count - 1 < 0)
            {
                // If the Handles gameobject exists:
                if (handlesOn)
                {
                    // Get the handle script component and run the MoveTo Function passing in the Vector3 "pos".
                    handlesOn.GetComponent<HandleScript>().MoveTo(pos);
                }
                // If the Main Visualizer is not null then:
                if (Main != null)
                {
                    // Run the RequireData Helper function in the MainVisualizer. To update the MeshManager's positional Data.
                    Main.MeshVisualizer.RequireData();
                }
            }
            else if (Main.MeshVisualizer.Cols.Count - 1 >= 0)
            {
                Main.MeshVisualizer.MoveVertexByTyping(pos);
            }
        }
        else
        {
            // If the Handles gameobject exists:
            if (handlesOn)
            {
                // Get the handle script component and run the MoveTo Function passing in the Vector3 "pos".
                handlesOn.GetComponent<HandleScript>().MoveTo(pos);
            }
        }
        
    }

    // Whenever any of the Rotation inputfields have finished being edited:
    public void TakeRotation()
    {
        // If rotation and scaling is true:
        if (RotationAndScaling)
        {
            // Then:
            // Create a new vector3 comprised of 3 float.NaNs (for checking in the handle script)
            Vector3 Rotation = new Vector3(float.NaN, float.NaN, float.NaN);

            // For every inputfield in the rotation input fields list:
            for (int i = 0; i < RotFields.Count; i++)
            {
                // Create a new float called temp, of value float.NaN
                // Create two strings, one is assigned contain whatever is in the user inputable text box.
                // The other is assigned string.Empty, this will be the value turned into a float then put into the Vector3 "Rotation".
                float Temp;
                string TempString = RotFields[i].transform.Find("Text").GetComponent<Text>().text;
                string TempNumber = string.Empty;
                //For the length of the text field:
                for (int c = 0; c < TempString.Length; c++)
                {
                    // If the current character is a digit, add it to the TempNumber string.
                    if (Char.IsDigit(TempString[c]))
                    {
                        TempNumber += TempString[c];
                    }
                    // If the current character is a Decmial place and TempString doesn NOT contain a decmial place, add the decmial place to TempString.
                    if (TempString[c] == '.' && !TempString.Contains("."))
                    {
                        TempNumber += TempString[c];
                    }
                    // If the current charcter is a negative symbol and the tempnumber string has a length of 0 (so we can't add two negative symbols, and so no numbers are in front of it).
                    if (TempNumber.Length <= 0 && TempString[c] == '-')
                    {
                        TempNumber += TempString[c];
                    }
                }
                // If TempNumber.length is greater than 0.
                if (TempNumber.Length > 0)
                {
                    // Turn tempNumber into a float.
                    Temp = float.Parse(TempNumber);
                    // Then: if this is field 0 ( X Axis) then assign Temp to the X Axis value of Vector3 "Rotation".
                    if (i == 0)
                    {
                        Rotation.x = Temp;
                    }
                    // Then: if this is field 1 ( Y Axis) then assign Temp to the Y Axis value of Vector3 "Rotation".
                    else if (i == 1)
                    {
                        Rotation.y = Temp;
                    }
                    // Then: if this is field 2 ( Z Axis) then assign Temp to the Z Axis value of Vector3 "Rotation".
                    else if (i == 2)
                    {
                        Rotation.z = Temp;
                    }
                }
                // Select and Clear the current Input Field.
                RotFields[i].Select();
                RotFields[i].text = "";
            }
            // If the Handles gameobject exists:
            if (handlesOn)
            {
                // Get the handle script component and run the RotateTo Function passing in the Vector3 "Rotation".
                handlesOn.GetComponent<HandleScript>().RotateTo(Rotation);
            }
            // If the Main Visualizer is not null then:
            if (Main != null)
            {
                if (!Main.NoVisualizer)
                {
                    // Run the RequireData Helper function in the MainVisualizer. To update the MeshManager's positional Data.
                    Main.MeshVisualizer.RequireData();
                }
            }
        }
    }

    // Whenever any of the Scale inputfields have finished being edited:
    public void TakeScale()
    {
        // If rotation and scaling is true:
        if (RotationAndScaling)
        {
            // Then:
            // Create a new vector3 comprised of 3 float.NaNs (for checking in the handle script)
            Vector3 Scale = new Vector3(float.NaN, float.NaN, float.NaN);

            // For every inputfield in the Scale Input fields list:
            for (int i = 0; i < ScaleFields.Count; i++)
            {
                // Create a new float called temp, of value float.NaN
                // Create two strings, one is assigned contain whatever is in the user inputable text box.
                // The other is assigned string.Empty, this will be the value turned into a float then put into the Vector3 "Scale".
                float Temp;
                string TempString = ScaleFields[i].transform.Find("Text").GetComponent<Text>().text;
                string TempNumber = string.Empty;
                //For the length of the text field:
                for (int c = 0; c < TempString.Length; c++)
                {
                    // If the current character is a digit, add it to the TempNumber string.
                    if (Char.IsDigit(TempString[c]))
                    {
                        TempNumber += TempString[c];
                    }
                    // If the current character is a Decmial place and TempString doesn NOT contain a decmial place, add the decmial place to TempString.
                    if (TempString[c] == '.' && !TempNumber.Contains("."))
                    {
                        TempNumber += TempString[c];
                    }
                }
                // If TempNumber.length is greater than 0.
                if (TempNumber.Length > 0)
                {
                    // Turn tempNumber into a float.
                    Temp = float.Parse(TempNumber);
                    // Then: if this is field 0 (X Axis) then assign Temp to the X Axis value of Vector3 "Scale".
                    if (i == 0)
                    {
                        Scale.x = Temp;
                    }
                    // Then: if this is field 1 (Y Axis) then assign Temp to the Y Axis value of Vector3 "Scale".
                    else if (i == 1)
                    {
                        Scale.y = Temp;
                    }
                    // Then: if this is field 2 (Z Axis) then assign Temp to the Z Axis value of Vector3 "Scale".
                    else if (i == 2)
                    {
                        Scale.z = Temp;
                    }
                    // Then: if this is field 3 (All Axis's) then Create a new Vector3 fromed from Temp as all 3 Axis's, assign this to Scale.
                    else if (i == 3)
                    {
                        Scale = new Vector3(Temp, Temp, Temp);
                    }
                }
                // Select and Clear the current Input Field.
                ScaleFields[i].Select();
                ScaleFields[i].text = "";
            }
            // If the Handles gameobject exists:
            if (handlesOn)
            {
                // Get the handle script component and run the ScaleTo Function passing in the Vector3 "Scale".
                handlesOn.GetComponent<HandleScript>().ScaleTo(Scale);
            }
            // If the Main Visualizer is not null then:
            if (Main != null)
            {
                if (!Main.NoVisualizer)
                {
                    // Run the RequireData Helper function in the MainVisualizer. To update the MeshManager's positional Data.
                    Main.MeshVisualizer.RequireData();
                }
            }
        }
    }

    public void FlightMode()
    {
        ShipManager.FlightMode();
    }

    // when the edit button is pressed down.
    public void EditMesh()
    {
        // If ObjectSelect is true on the model manager script
        if (ModelManger.objectSelected && !SubComponentEditing)
        {
            if (!Main.NoVisualizer)
            {
                // Then:
                // If Rotation and Scaling is true:
                if (RotationAndScaling)
                {
                    // Then:
                    // Set Rotation and Scaling to false and Disable their input field categories, so the UI elements are hidden.
                    RotationAndScaling = false;
                    Rotation.SetActive(false);
                    Scale.SetActive(false);
                    //SelectionModeToggle.SetActive(false);
                    RectTransform Temp = SelectionModeToggle.GetComponent<RectTransform>();
                    Temp.anchoredPosition = new Vector2( -315.0f, Temp.anchoredPosition.y);//-230
                    SelectionModeToggle.GetComponent<Toggle>().interactable = true;
                }
                else
                {
                    // Then:
                    // Set Rotation and Scaling to true and Disable their input field categories, so the UI elements are shown.
                    RotationAndScaling = true;
                    Rotation.SetActive(true);
                    Scale.SetActive(true);
                    //SelectionModeToggle.SetActive(true);
                    if (!SelectionModeToggle.GetComponent<Toggle>().isOn)
                    {
                        SelectionModeToggle.GetComponent<Toggle>().isOn = true;
                    }
                    RectTransform Temp = SelectionModeToggle.GetComponent<RectTransform>();
                    Temp.anchoredPosition = new Vector2(-230.0f, Temp.anchoredPosition.y);//-315
                    SelectionModeToggle.GetComponent<Toggle>().interactable = false;
                    
                }
                // Run the edit mesh helper function in the model manager.
                ModelManger.RunEdit();
                if (ResetMeshButton.activeSelf)
                {
                    ResetMeshButton.SetActive(false);
                    RoundingInputField.SetActive(false);
                    AddQuadButton.SetActive(false);
                    if (QuadSelection.activeSelf)
                    {
                        QuadSelection.SetActive(false);
                    }
                }
                else if (!ResetMeshButton.activeSelf)
                {
                    ResetMeshButton.SetActive(true);
                    RoundingInputField.SetActive(true);
                    AddQuadButton.SetActive(true);
                }
            }
        }
    }

    public void AddQuad()
    {
        Main.MeshVisualizer.AddQuadMode();
        if (QuadSelection.activeSelf)
        {
            QuadSelection.SetActive(false);
        }
        else
        {
            QuadSelection.SetActive(true);
        }
    }

    public void TogglePlacementMode()
    {
        Main.MeshVisualizer.TogglePlacementMode();        
    }

    public void RoundingEnable()
    {
        if (Main.MeshVisualizer.roundingEnabled)
        {
            Main.MeshVisualizer.roundingEnabled = false;
            RoundingInputField.transform.Find("StatusField").gameObject.GetComponent<Text>().text = "Disabled";
        }
        else if (!Main.MeshVisualizer.roundingEnabled)
        {
            Main.MeshVisualizer.roundingEnabled = true; // StatusField
            RoundingInputField.transform.Find("StatusField").gameObject.GetComponent<Text>().text = "Enabled";
        }
    }

    public void RoundingGetInput()
    {
        int DPs;
        string TempString = RoundingInputField.transform.Find("DPs").gameObject.GetComponentInChildren<InputField>().transform.Find("Text").GetComponent<Text>().text;
        string TempNumber = string.Empty;
        //For the length of the text field:
        for (int c = 0; c < TempString.Length; c++)
        {
            // If the current character is a digit, add it to the TempNumber string.
            if (Char.IsDigit(TempString[c]))
            {
                TempNumber += TempString[c];
            }
        }
        // If TempNumber.length is greater than 0.
        if (TempNumber.Length > 0)
        {
            DPs = Convert.ToInt32(TempNumber);
            Main.MeshVisualizer.roundingAccuracy = DPs;
        }
        // Select and Clear the current Input Field.
        RoundingInputField.transform.Find("DPs").gameObject.GetComponentInChildren<InputField>().Select();
        RoundingInputField.transform.Find("DPs").gameObject.GetComponentInChildren<InputField>().text = "";
    }

    public void DecalMode()
    {
        ModelManger.StartDecalApplication();
    }

    public void ResetMesh()
    {
        Main.MeshVisualizer.ResetMesh();
    }

    public void SelectionMode()
    {
        if (!ModelManger.MeshInEdit)
        {
            if (!SubComponentEditing)
                ModelManger.SelectionMode();
            else
                SubComponentScript.SelectionMode();
        }
        else
        {
            VertexSelectionMode();
        }
    }

    void VertexSelectionMode()
    {
        Main.MeshVisualizer.SelectionMode();
    }

    // Whenever the Location Button is Pressed.
    public void LocationButton()
    {
        // If the Handles gameObject exists then run the position Mode function.
        if (handlesOn && !ModelManger.MeshInEdit)
            handlesOn.GetComponent<HandleScript>().PositionMode();
        if(ModelManger.MeshInEdit && Main.MeshVisualizer.VertexSelectionMode)
        {
            VertexSelectionMode();
        }
    }

    // Whenever the Rotation Button is Pressed.
    public void RotationButton()
    {
        // If the Handles gameObject exists then run the Rotation Mode function.
        if (handlesOn)
            handlesOn.GetComponent<HandleScript>().RotMode();
    }

    // Whenever the Scale Button is Pressed.
    public void ScaleButton()
    {
        // If the Handles gameObject exists then run the Scale Mode function.
        if (handlesOn)
            handlesOn.GetComponent<HandleScript>().ScaleMode();
    }

    // Whenever teh Deselect Button is Pressed
    public void Deselect()
    {
        if (ModelManger.MeshInEdit)
        {
            EditMesh();
        }
        // Then:
        // If Rotation and Scaling is true:
        if (!RotationAndScaling)
        {
            // Then:
            // Set Rotation and Scaling to false and Disable their input field categories, so the UI elements are hidden.
            RotationAndScaling = true;
            Rotation.SetActive(true);
            Scale.SetActive(true);
        }
        // if there is an object selected, run the descelection function in the model manager.
        if (ModelManger.objectSelected)
            ModelManger.Deselect();
        RoundingInputField.SetActive(false);
    }
}
