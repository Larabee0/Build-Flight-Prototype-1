//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//namespace MFlight.Demo

public class Hud : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private MouseFlightController mouseFlight = null;

    [Header("HUD Elements")]
    [SerializeField] private RectTransform boresight = null;
    [SerializeField] private RectTransform mousePos = null;

    private Camera playerCam = null;
    private Plane Plane;
    public ShipManager ShipManager;

    private Text ThrottleIndicator;
    private Text VelocityIndicator;
    private Rigidbody ShipRigidbody;

    private List<Slider> TrimIndicators = new List<Slider>();

    void Start()
    {
        ShipManager = GameObject.FindObjectOfType<ShipManager>();
        Plane = GameObject.FindObjectOfType<Plane>();
        ShipRigidbody = Plane.GetComponent<Rigidbody>();
        ThrottleIndicator = transform.Find("ThrottleIndicator").GetComponentInChildren<Text>();
        VelocityIndicator = transform.Find("VelocityIndicator").GetComponentInChildren<Text>();
        TrimIndicators.Add(transform.Find("YAW").GetComponent<Slider>());
        TrimIndicators.Add(transform.Find("ROLL").GetComponent<Slider>());
        TrimIndicators.Add(transform.Find("PITCH").GetComponent<Slider>());
    }

    private void Awake()
    {
        
        if (mouseFlight == null)
            Debug.LogError(name + ": Hud - Mouse Flight Controller not assigned!");

        playerCam = mouseFlight.GetComponentInChildren<Camera>();
        if (playerCam == null)
            Debug.LogError(name + ": Hud - No camera found on assigned Mouse Flight Controller!");

    }

    private void Update()
    {
        if (mouseFlight == null || playerCam == null)
            return;
        ThrottleIndicator.text = "THR: " + Convert.ToInt32(Plane.ThrottlePosition ).ToString() + "%";

        if(ShipRigidbody.velocity.magnitude < 1f)
        {
            VelocityIndicator.text = "SPD: " + (Math.Round(ShipRigidbody.velocity.magnitude * 1000f) / 1000f).ToString() + "m/s";
        }
        else if(ShipRigidbody.velocity.magnitude < 10f)
        {
            VelocityIndicator.text = "SPD: " + (Math.Round(ShipRigidbody.velocity.magnitude * 100f) / 100f).ToString() + "m/s";
        }
        else if (ShipRigidbody.velocity.magnitude < 100f)
        {
            VelocityIndicator.text = "SPD: " + (Math.Round(ShipRigidbody.velocity.magnitude * 10f) / 10f).ToString() + "m/s";
        }
        else if (ShipRigidbody.velocity.magnitude < 1000f)
        {
            VelocityIndicator.text = "SPD: " + Convert.ToInt32(ShipRigidbody.velocity.magnitude).ToString() + "m/s";
        }

        TrimIndicators[0].value = Plane.Yaw;
        TrimIndicators[1].value = Plane.Roll;
        TrimIndicators[2].value = Plane.Pitch;



        UpdateGraphics(mouseFlight);
    }

    private void UpdateGraphics(MouseFlightController controller)
    {
        if (boresight != null)
        {
            boresight.position = playerCam.WorldToScreenPoint(controller.BoresightPos);
            boresight.gameObject.SetActive(boresight.position.z > 1f);
        }

        if (mousePos != null)
        {
            mousePos.position = playerCam.WorldToScreenPoint(controller.MouseAimPos);
            mousePos.gameObject.SetActive(mousePos.position.z > 1f);
        }
    }

    public void SetReferenceMouseFlight(MouseFlightController controller)
    {
        mouseFlight = controller;
    }

    public void BuildMode()
    {
        ShipManager.BuildMode();
    }
}
