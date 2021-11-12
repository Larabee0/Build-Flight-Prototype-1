using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericSystem : MonoBehaviour
{
    public string Identifier = "Generic";
    public float PowerDraw = 0;
    public float PowerOutput = 0;

}

public class SystemsManager : MonoBehaviour
{
    List<Reactor> Reactors = new List<Reactor>();
    List<Battery> Batteries = new List<Battery>();
    List<Capacitor> Capacitors = new List<Capacitor>();

    List<Engine> Engines = new List<Engine>();

    List<FTL> FTLDrives = new List<FTL>();

    List<Shield> Shields = new List<Shield>();

    List<Weapon> Weapons = new List<Weapon>();



    void Start()
    {

    }

    void Reset()
    {

    }

    void FixedUpdate()
    {

    }
}