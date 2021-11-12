using System;
using UnityEngine;
/// <summary>
/// Proof of concept for ramping of thruster instead of thrust being tied directly to the throttle %
/// Also concept of efficeny look up table
/// </summary>
public class ThrusterTest : MonoBehaviour
{

    [Header("Script References")]
    public ComponentStatsManager CSManager;



    [Header("Input Throttle and settings")]
    public float ThrottlePosition = 0;
    public float ThrottleSens = 50f;

    [Header("Thruster settings and information")]
    public float ThrusterPosition = 0;
    public int intThrusterPosition;
    public float ThrusterSens = 12.5f;
    public float ThrusterOutput = 0f;
    public float ThrusterEfficiency = 0.00f;
    public float ThrustBase = 10f;
    public float ThrustMax;

    private readonly float[] EfficiencyCurve = {
            0.0f,0.50f,0.62f,0.68f,0.74f,0.842f,0.86f,0.876f,0.883f,0.891f,0.899f,0.904f,0.909f,0.913f,0.917f,0.919f,0.92f,0.923f,0.924f,0.925f,0.926f,0.9265f,0.9267f,0.9268f,0.9269f,0.9269f,0.9269f
            ,0.9269f,0.9269f,0.9268f,0.9268f,0.9267f,0.9264f,0.9263f,0.9262f,0.926f,0.9255f,0.9249f,0.9245f,0.9235f,0.921f,0.9185f,0.9165f,0.916f,0.9158f,0.9155f,0.9153f,0.9151f,0.9149f,0.914f
            ,0.9126f,0.9115f,0.91f,0.9086f,0.9078f,0.906f,0.9055f,0.9049f,0.903f,0.901f,0.8997f,0.8984f,0.8964f,0.895f,0.8943f,0.892f,0.8905f,0.8888f,0.8865f,0.8858f,0.8849f,0.8833f,0.8812f,0.8798f
            ,0.878f,0.8776f,0.8749f,0.8737f,0.8712f,0.8693f,0.8676f,0.8653f,0.8632f,0.8625f,0.8602f,0.858f,0.8576f,0.8566f,0.8536f,0.85f,0.8437f,0.8428f,0.8417f,0.8404f,0.8387f,0.8377f,0.836f,0.8299f
            ,0.826f,0.825f
    };


    // Start is called before the first frame update
    void Start()
    {
        GetMaxThrust();
    }

    // Update is called once per frame
    void Update()
    {
        ThrottlePosition = CSManager.Plane.ThrottlePosition;


        // Handles the thruster % output, so it tries to match the user throttle input but it takes time to reach it.
        if(ThrusterPosition < ThrottlePosition)
        {
            ThrusterPosition += ThrusterSens * (1 + (ThrusterPosition / 50)) * Time.deltaTime;
            if (ThrusterPosition > 100f)
            {
                ThrusterPosition = 100f;
            }
        }
        else if(ThrusterPosition > ThrottlePosition)
        {
            ThrusterPosition -= ThrusterSens * (1 + (ThrusterPosition / 50)) * Time.deltaTime;
            if(ThrusterPosition < 0f)
            {
                ThrusterPosition = 0f;
            }
        }
        GetThrust();
    }

    // Get the current thrust value this frame.
    void GetThrust()
    {
        ThrusterOutput = ThrusterPosition * ThrustBase;
        intThrusterPosition = Convert.ToInt32(ThrusterPosition);
        if(intThrusterPosition == 0)
        {
            ThrusterEfficiency = EfficiencyCurve[0];
        }
        else
        {
            ThrusterEfficiency = EfficiencyCurve[intThrusterPosition - 1];
        }
        
    }

    // Calculate maximum thruster.
    public void GetMaxThrust()
    {
        ThrustMax = ThrustBase * 100f;
    }
}
