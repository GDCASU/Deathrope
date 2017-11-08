﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Created by Ava Warfield 9/20/17

// Modified by Paul Gellai - This is a simple script that simply ensures that any player that touches an object with this script to it attached dies.

public class DeathPlane : MonoBehaviour
{
    public virtual void PlayerInteract()
    {
        // insert code here to control behaviour upon interaction with player objects

        // override in children using "public override void PlayerInteract()"
    }


    // when player 
    void OnTriggerEnter(Collider other)
    {
        other.transform.parent.GetComponent<Team>().KillTeam();
    }
}