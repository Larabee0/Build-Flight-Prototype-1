using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.HighDefinition;

public class DecalManagerUI : MonoBehaviour
{
    public GameObject CameraRig;
    public Slider TransparencySlider;
    private DecalManager DecalManager;

    // Lists of input fields for position, rotation and scaling input fields.
    [HideInInspector]
    public List<InputField> PosFields = new List<InputField>();
    [HideInInspector]
    public List<InputField> RotFields = new List<InputField>();
    [HideInInspector]
    public List<InputField> ScaleFields = new List<InputField>();
    [HideInInspector]
    public List<InputField> TillingFields = new List<InputField>();

    [HideInInspector]
    public List<InputField> OffSetFields = new List<InputField>();
    [HideInInspector]
    public InputField ProxField = null;
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
    public GameObject TillingButton;
    [HideInInspector]
    public GameObject OffSetButton;
    [HideInInspector]
    public GameObject CameraButton;

    // Start is called before the first frame update
    void Start()
    {
        // Grab the model manager.
        DecalManager = GameObject.FindObjectOfType<DecalManager>();
        Location = transform.Find("LocationButton").gameObject;
        Rotation = transform.Find("RotationButton").gameObject;
        Scale = transform.Find("ScaleButton").gameObject;
        TillingButton = transform.Find("TillingButton").gameObject;
        OffSetButton = transform.Find("DecalOffSetButton").gameObject;
        CameraButton = transform.Find("CameraLocationButton").gameObject;

        // Add to the list each input field for the 3 transform sections.
        PosFields.Add(Location.transform.Find("PositonX").gameObject.GetComponentInChildren<InputField>());
        PosFields.Add(Location.transform.Find("PositonY").gameObject.GetComponentInChildren<InputField>());
        PosFields.Add(Location.transform.Find("PositonZ").gameObject.GetComponentInChildren<InputField>());

        RotFields.Add(Rotation.transform.Find("RotationX").gameObject.GetComponentInChildren<InputField>());
        RotFields.Add(Rotation.transform.Find("RotationY").gameObject.GetComponentInChildren<InputField>());
        RotFields.Add(Rotation.transform.Find("RotationZ").gameObject.GetComponentInChildren<InputField>());

        ScaleFields.Add(Scale.transform.Find("ScaleAll").gameObject.GetComponentInChildren<InputField>());//all
        ScaleFields.Add(Scale.transform.Find("Width").gameObject.GetComponentInChildren<InputField>());//x
        ScaleFields.Add(Scale.transform.Find("Height").gameObject.GetComponentInChildren<InputField>());//y
        ScaleFields.Add(Scale.transform.Find("Proximity").gameObject.GetComponentInChildren<InputField>());//z

        TillingFields.Add(TillingButton.transform.Find("TotalDensity").gameObject.GetComponentInChildren<InputField>());
        TillingFields.Add(TillingButton.transform.Find("Columns").gameObject.GetComponentInChildren<InputField>());
        TillingFields.Add(TillingButton.transform.Find("Rows").gameObject.GetComponentInChildren<InputField>());

        OffSetFields.Add(OffSetButton.transform.Find("XOffSet").gameObject.GetComponentInChildren<InputField>());
        OffSetFields.Add(OffSetButton.transform.Find("YOffSet").gameObject.GetComponentInChildren<InputField>());

        CameraFields.Add(CameraButton.transform.Find("PositonX").gameObject.GetComponentInChildren<InputField>());
        CameraFields.Add(CameraButton.transform.Find("PositonY").gameObject.GetComponentInChildren<InputField>());
        CameraFields.Add(CameraButton.transform.Find("PositonZ").gameObject.GetComponentInChildren<InputField>());
    }

    void Update()
    {
        CameraFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "X = " + ((float)Math.Round(CameraRig.transform.position.x * 100000f) / 100000f).ToString();
        CameraFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Y = " + ((float)Math.Round(CameraRig.transform.position.y * 100000f) / 100000f).ToString();
        CameraFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Z = " + ((float)Math.Round(CameraRig.transform.position.z * 100000f) / 100000f).ToString();
        // If an object is select, ie the main visualizer exists. This is updated by the model manager:
        if (DecalManager.Selection.Count - 1 >= 0)
        {

            // Update the position placeholder text fields to show the current position of the main visualizer's gameobject.
            PosFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "X = " + ((float)Math.Round(DecalManager.Selection[0].transform.position.x * 100000f) / 100000f).ToString();
            PosFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Y = " + ((float)Math.Round(DecalManager.Selection[0].transform.position.y * 100000f) / 100000f).ToString();
            PosFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Z = " + ((float)Math.Round(DecalManager.Selection[0].transform.position.z * 100000f) / 100000f).ToString();

            // Update the rotation and scaling place holder fields for the current rotation and position of the main visualizer's gameobject.
            RotFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "X = " + ((float)Math.Round(DecalManager.Selection[0].transform.localRotation.eulerAngles.x * 100000f) / 100000f).ToString() + "°";
            RotFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Y = " + ((float)Math.Round(DecalManager.Selection[0].transform.localRotation.eulerAngles.y * 100000f) / 100000f).ToString() + "°";
            RotFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Z = " + ((float)Math.Round(DecalManager.Selection[0].transform.localRotation.eulerAngles.z * 100000f) / 100000f).ToString() + "°";

            ScaleFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "Scale = " + ((float)Math.Round(DecalManager.Selection[0].GetComponent<DecalProjector>().size.x * 100000f) / 100000f).ToString();
            ScaleFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Width = " + ((float)Math.Round(DecalManager.Selection[0].GetComponent<DecalProjector>().size.x * 100000f) / 100000f).ToString();
            ScaleFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Height = " + ((float)Math.Round(DecalManager.Selection[0].GetComponent<DecalProjector>().size.y * 100000f) / 100000f).ToString();
            ScaleFields[3].transform.Find("Placeholder").GetComponent<Text>().text = "Proximity = " + ((float)Math.Round(DecalManager.Selection[0].GetComponent<DecalProjector>().size.z * 100000f) / 100000f).ToString();

            TillingFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "Cols&Rows = " + ((float)Math.Round(DecalManager.Selection[0].GetComponent<DecalProjector>().uvScale.x * 100000f) / 100000f).ToString();
            TillingFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Cols = " + ((float)Math.Round(DecalManager.Selection[0].GetComponent<DecalProjector>().uvScale.x * 100000f) / 100000f).ToString();
            TillingFields[2].transform.Find("Placeholder").GetComponent<Text>().text = "Rows = " + ((float)Math.Round(DecalManager.Selection[0].GetComponent<DecalProjector>().uvScale.y * 100000f) / 100000f).ToString();

            OffSetFields[0].transform.Find("Placeholder").GetComponent<Text>().text = "X = " + ((float)Math.Round(DecalManager.Selection[0].GetComponent<DecalProjector>().uvBias.x * 100000f) / 100000f).ToString();
            OffSetFields[1].transform.Find("Placeholder").GetComponent<Text>().text = "Y = " + ((float)Math.Round(DecalManager.Selection[0].GetComponent<DecalProjector>().uvBias.y * 100000f) / 100000f).ToString();
        }
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
            float Temp = float.NaN;
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
            float Temp = float.NaN;
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
        if (DecalManager.Selection.Count - 1 >= 0)
        {
            if (float.IsNaN(pos.x))
            {
                pos.x = DecalManager.Selection[0].transform.localPosition.x;
            }
            if (float.IsNaN(pos.y))
            {
                pos.y = DecalManager.Selection[0].transform.localPosition.y;
            }
            if (float.IsNaN(pos.z))
            {
                pos.z = DecalManager.Selection[0].transform.localPosition.z;
            }

            Vector3 otherPos = pos - DecalManager.Selection[0].transform.localPosition;

            DecalManager.Selection[0].transform.localPosition = pos;

            for (int i = 1; i < DecalManager.Selection.Count; i++)
            {
                DecalManager.Selection[i].transform.localPosition += otherPos;
            }
        }
    }

    // Whenever any of the Rotation inputfields have finished being edited:
    public void TakeRotation()
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
            float Temp = float.NaN;
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
        if (DecalManager.Selection.Count - 1 >= 0)
        {
            // Check all Components of pos, to see if they are NaN, 
            // if they are, set the component to the current localRotation
            // of parent of the gameObject this script is attached to.
            if (float.IsNaN(Rotation.x))
            {
                Rotation.x = DecalManager.Selection[0].transform.localRotation.eulerAngles.x;
            }
            if (float.IsNaN(Rotation.y))
            {
                Rotation.y = DecalManager.Selection[0].transform.localRotation.eulerAngles.y;
            }
            if (float.IsNaN(Rotation.z))
            {
                Rotation.z = DecalManager.Selection[0].transform.localRotation.eulerAngles.z;
            }

            for (int i = 0; i < DecalManager.Selection.Count; i++)
            {
                DecalManager.Selection[i].transform.localRotation = Quaternion.Euler(Rotation);
            }
        }
    }

    // Whenever any of the Scale inputfields have finished being edited:
    public void TakeScale()
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
            float Temp = float.NaN;
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
                // Then: if this is field 3 (All Axis's) then Create a new Vector3 fromed from Temp as all 3 Axis's, assign this to Scale.
                if (i == 0)
                {
                    Scale = new Vector3(Temp, Temp, float.NaN);
                }
                else if (i == 1)
                {
                    Scale.x = Temp;
                }
                // Then: if this is field 1 (Y Axis) then assign Temp to the Y Axis value of Vector3 "Scale".
                else if (i == 2)
                {
                    Scale.y = Temp;
                }
                // Then: if this is field 2 (Z Axis) then assign Temp to the Z Axis value of Vector3 "Scale".
                else if (i == 3)
                {
                    Scale.z = Temp;
                }
            }
            // Select and Clear the current Input Field.
            ScaleFields[i].Select();
            ScaleFields[i].text = "";
        }

        if (DecalManager.Selection.Count - 1 >= 0)
        {
            // Check all Components of pos, to see if they are NaN, 
            // if they are, set the component to the current localScale
            // of parent of the gameObject this script is attached to.
            if (float.IsNaN(Scale.x) || Scale.x <= 0)
            {
                Scale.x = DecalManager.Selection[0].GetComponent<DecalProjector>().size.x;
            }
            if (float.IsNaN(Scale.y) || Scale.y <= 0)
            {
                Scale.y = DecalManager.Selection[0].GetComponent<DecalProjector>().size.y;
            }
            if (float.IsNaN(Scale.z) || Scale.z <= 0)
            {
                Scale.z = DecalManager.Selection[0].GetComponent<DecalProjector>().size.z;
            }

            for (int i = 0; i < DecalManager.Selection.Count; i++)
            {
                DecalManager.Selection[i].GetComponent<DecalProjector>().size = Scale;
                DecalManager.Selection[i].transform.Find("DecalArea(Clone)").transform.localScale = Scale;
            }
        }
    }

    // Whenever any of the Scale inputfields have finished being edited:
    public void TakeTilling()
    {
        // Then:
        // Create a new vector3 comprised of 3 float.NaNs (for checking in the handle script)
        Vector3 Tiles = new Vector2(float.NaN, float.NaN);

        // For every inputfield in the Scale Input fields list:
        for (int i = 0; i < TillingFields.Count; i++)
        {
            // Create a new float called temp, of value float.NaN
            // Create two strings, one is assigned contain whatever is in the user inputable text box.
            // The other is assigned string.Empty, this will be the value turned into a float then put into the Vector3 "Scale".
            float Temp = float.NaN;
            string TempString = TillingFields[i].transform.Find("Text").GetComponent<Text>().text;
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
                if (i == 1)
                {
                    Tiles.x = Temp;
                }
                // Then: if this is field 1 (Y Axis) then assign Temp to the Y Axis value of Vector3 "Scale".
                else if (i == 2)
                {
                    Tiles.y = Temp;
                }
                // Then: if this is field 3 (All Axis's) then Create a new Vector3 fromed from Temp as all 3 Axis's, assign this to Scale.
                else if (i == 0)
                {
                    Tiles = new Vector2(Temp, Temp);
                }
            }
            // Select and Clear the current Input Field.
            TillingFields[i].Select();
            TillingFields[i].text = "";
        }

        if (DecalManager.Selection.Count - 1 >= 0)
        {
            // Check all Components of pos, to see if they are NaN, 
            // if they are, set the component to the current localScale
            // of parent of the gameObject this script is attached to.
            if (float.IsNaN(Tiles.x) || Tiles.x <= 0)
            {
                Tiles.x = DecalManager.Selection[0].GetComponent<DecalProjector>().uvScale.x;
            }
            if (float.IsNaN(Tiles.y) || Tiles.y <= 0)
            {
                Tiles.y = DecalManager.Selection[0].GetComponent<DecalProjector>().uvScale.y;
            }

            for (int i = 0; i < DecalManager.Selection.Count; i++)
            {
                DecalManager.Selection[i].GetComponent<DecalProjector>().uvScale = Tiles;
            }
        }
    }

    // Whenever any of the Scale inputfields have finished being edited:
    public void TakeOffSet()
    {
        // Then:
        // Create a new vector3 comprised of 3 float.NaNs (for checking in the handle script)
        Vector3 OffSet = new Vector2(float.NaN, float.NaN);

        // For every inputfield in the Scale Input fields list:
        for (int i = 0; i < OffSetFields.Count; i++)
        {
            // Create a new float called temp, of value float.NaN
            // Create two strings, one is assigned contain whatever is in the user inputable text box.
            // The other is assigned string.Empty, this will be the value turned into a float then put into the Vector3 "Scale".
            float Temp = float.NaN;
            string TempString = OffSetFields[i].transform.Find("Text").GetComponent<Text>().text;
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
                    OffSet.x = Temp;
                }
                // Then: if this is field 1 (Y Axis) then assign Temp to the Y Axis value of Vector3 "Scale".
                else if (i == 1)
                {
                    OffSet.y = Temp;
                }
            }
            // Select and Clear the current Input Field.
            TillingFields[i].Select();
            TillingFields[i].text = "";
        }

        if (DecalManager.Selection.Count - 1 >= 0)
        {
            // Check all Components of pos, to see if they are NaN, 
            // if they are, set the component to the current localScale
            // of parent of the gameObject this script is attached to.
            if (float.IsNaN(OffSet.x) || OffSet.x < 0)
            {
                OffSet.x = DecalManager.Selection[0].GetComponent<DecalProjector>().uvBias.x;
            }
            if (float.IsNaN(OffSet.y) || OffSet.y < 0)
            {
                OffSet.y = DecalManager.Selection[0].GetComponent<DecalProjector>().uvBias.y;
            }

            for (int i = 0; i < DecalManager.Selection.Count; i++)
            {
                DecalManager.Selection[i].GetComponent<DecalProjector>().uvBias = OffSet;
            }
        }
    }

    public void ChangeTransparency()
    {
        if (DecalManager.Selection.Count - 1 >= 0)
        {

            for (int i = 0; i < DecalManager.Selection.Count; i++)
            {
                DecalManager.Selection[0].GetComponent<DecalProjector>().fadeFactor = TransparencySlider.value;
            }
        }
    }

    public void BuildMode()
    {
        DecalManager.ShutDownDecalManager();
    }

    public void MovementMode()
    {
        if (DecalManager.PlacementMode)
        {
            DecalManager.DecalMovementMode();
        }
        
    }
    public void PlacementMode()
    {
        if (!DecalManager.PlacementMode)
        {
            DecalManager.DecalPlacementMode();
        }

    }
}
