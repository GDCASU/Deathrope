﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

// Developer:   Kyle Aycock
// Date:        10/27/2017
// Description: Configurable controls class that can be saved and loaded
//              from a persistent data path. Controls are serialized in
//              JSON and the default Windows location is:
//              %USERPROFILE%\AppData\LocalLow\25PercentGames\Deathrope
//              (copy&paste that into a windows explorer address bar)
//
//              When adding/removing a control, it's necessary to add/remove
//              the relevant field, change the constructor accordingly, add/remove
//              a default setting in GenerateDefaultControls, and add/remove the
//              relevant Getter method (such as GetJump)

[Serializable]
public class Controls
{
    public string name;
    public int number;
    public ControlInput horizAxis;
    public ControlInput horizAxisAlt;
    public ControlInput vertAxis;
    public ControlInput vertAxisAlt;
    public ControlInput jumpButton;
    public ControlInput jumpButtonAlt;
    public ControlInput dashButton;
    public ControlInput dashButtonAlt;
    public ControlInput powerUpButton;
    public ControlInput powerUpButtonAlt;

    //path to the config file
    private static string cfgPath = Path.Combine(Application.persistentDataPath, "controls.ini");

    //The below is a sort of lookup table for converting between ugly Unity InputManager names
    //and nicer names for display in the config screen and in the actual config file itself
    private static Dictionary<string, string> axes = new Dictionary<string, string>()
    {
        {"Joystick 1 Axis 1", "Horizontal1" },
        {"Joystick 2 Axis 1", "Horizontal2" },
        {"Joystick 3 Axis 1", "Horizontal3" },
        {"Joystick 4 Axis 1", "Horizontal4" },
        {"Joystick 1 Axis 4", "Horizontal1.5" },
        {"Joystick 2 Axis 4", "Horizontal2.5" },
        {"Joystick 3 Axis 4", "Horizontal3.5" },
        {"Joystick 4 Axis 4", "Horizontal4.5" },
        {"Joystick 1 Axis 2", "Vertical1" },
        {"Joystick 2 Axis 2", "Vertical2" },
        {"Joystick 3 Axis 2", "Vertical3" },
        {"Joystick 4 Axis 2", "Vertical4" },
        {"Joystick 1 Axis 5", "Vertical1.5" },
        {"Joystick 2 Axis 5", "Vertical2.5" },
        {"Joystick 3 Axis 5", "Vertical3.5" },
        {"Joystick 4 Axis 5", "Vertical4.5" },
    };


    public Controls(int controller)
    {
        horizAxis = new ControlInput(KeyCode.None);
        horizAxisAlt = new ControlInput(KeyCode.None);
        vertAxis = new ControlInput(KeyCode.None);
        vertAxisAlt = new ControlInput(KeyCode.None);
        jumpButton = new ControlInput(KeyCode.None);
        jumpButtonAlt = new ControlInput(KeyCode.None);
        dashButton = new ControlInput(KeyCode.None);
        dashButtonAlt = new ControlInput(KeyCode.None);
        powerUpButton = new ControlInput(KeyCode.None);
        powerUpButtonAlt = new ControlInput(KeyCode.None);
        number = controller;
        name = "Player " + number;
    }

    public float GetHorizontal()
    {
        return (horizAxis.Get() != 0 ? horizAxis.Get() : horizAxisAlt.Get());
    }

    public float GetVertical()
    {
        return (vertAxis.Get() != 0 ? vertAxis.Get() : vertAxisAlt.Get());
    }

    public bool GetJump()
    {
        return jumpButton.Get() == 1 || jumpButtonAlt.Get() == 1;
    }

    public bool GetDash()
    {
        return dashButton.Get() == 1 || dashButtonAlt.Get() == 1;
    }

    public bool GetPowerUp()
    {
        return powerUpButton.Get() == 1 || powerUpButtonAlt.Get() == 1;
    }

    /// <summary>
    /// Saves the given controls to the config file. The slot it
    /// saves into is given by "c.number"
    /// </summary>
    /// <param name="c">Controls to save</param>
    public static void SaveToConfig(Controls c)
    {
        if (!File.Exists(cfgPath))
        {
            Debug.LogWarning("controls.ini not found, generating new one!");
            GenerateDefaultConfig();
        }
        try
        {
            ControlsConfig cc = JsonUtility.FromJson<ControlsConfig>(File.ReadAllText(cfgPath));
            cc.controls[c.number - 1] = c;
            File.WriteAllText(cfgPath, JsonUtility.ToJson(cc, true));
        }
        catch 
        {
            Debug.LogWarning("controls.ini was corrupted!");
            GenerateDefaultConfig();
            //SaveToConfig(c);
        }
    }

    /// <summary>
    /// Returns the controls for the given slot.
    /// </summary>
    /// <param name="controller">Slot from which to load</param>
    /// <returns>Controls object for given slot</returns>
    public static Controls LoadFromConfig(int controller)
    {
        if (!File.Exists(cfgPath))
        {
            Debug.LogWarning("controls.ini not found, generating new one!");
            GenerateDefaultConfig();
        }
        try
        {
            ControlsConfig cc = JsonUtility.FromJson<ControlsConfig>(File.ReadAllText(cfgPath));
            return cc.controls[controller - 1];
        }
        catch
        {
            Debug.LogWarning("controls.ini was corrupted!");
            GenerateDefaultConfig();
            return GenerateDefaultControls(controller);
        }
    }

    /// <summary>
    /// Overwrites config file with a default version
    /// </summary>
    public static void GenerateDefaultConfig()
    {
        StreamWriter cfg = new StreamWriter(File.Create(cfgPath));
        ControlsConfig cc = new ControlsConfig();
        cc.controls = new Controls[4];
        for (int i = 0; i < 4; i++)
        {
            cc.controls[i] = GenerateDefaultControls(i + 1);
        }
        cfg.Write(JsonUtility.ToJson(cc,true));
        cfg.Close();
    }

    /// <summary>
    /// Creates a Controls object and fills it with some preselected defaults
    /// </summary>
    /// <param name="controller">Which slot to generate controls for</param>
    /// <returns>The generated Controls object</returns>
    public static Controls GenerateDefaultControls(int controller)
    {
        Controls c = new Controls(controller);
        c.horizAxis = new ControlInput("Horizontal" + controller);
        c.vertAxis = new ControlInput("Vertical" + controller);
        c.jumpButton = new ControlInput((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + controller + "Button4"));
        c.dashButton = new ControlInput((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + controller + "Button8"));
        c.powerUpButton = new ControlInput((KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + controller + "Button2"));

        if (controller == 1)
        {
            c.horizAxisAlt = new ControlInput(KeyCode.D, KeyCode.A);
            c.vertAxisAlt = new ControlInput(KeyCode.W, KeyCode.S);
            c.jumpButtonAlt = new ControlInput(KeyCode.Space);
            c.dashButtonAlt = new ControlInput(KeyCode.Q);
            c.powerUpButtonAlt = new ControlInput(KeyCode.Z);
        }

        return c;
    }

    /// <summary>
    /// Returns the first axis going down the list that's been moved this frame
    /// </summary>
    /// <returns>Axis that was moved</returns>
    public static string GetMovedAxis()
    {
        foreach (KeyValuePair<string, string> entry in axes)
        {
            if (Input.GetAxis(entry.Value) != 0)
                return entry.Value;
        }
        return null;
    }

    /// <summary>
    /// Converts Unity InputManager axis name to human readable axis name
    /// </summary>
    /// <param name="axis">Unity InputManager axis name</param>
    /// <returns>Readable axis name</returns>
    public static string GetAxisName(string axis)
    {
        foreach (KeyValuePair<string, string> entry in axes)
        {
            if (entry.Value.Equals(axis))
                return entry.Key;
        }
        return null;
    }

    /// <summary>
    /// Returns a key that was pressed this frame, or KeyCode.None
    /// </summary>
    /// <returns>Key that was pressed this frame</returns>
    public static KeyCode GetPressedKey()
    {
        foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(k))
                return k;
        }

        return KeyCode.None;
    }

    struct ControlsConfig
    {
        public Controls[] controls;
    }


    /// <summary>
    /// This class represents a singular control, such as the dash button
    /// or horizontal movement. Supports both keycodes and InputManager axes.
    /// </summary>
    [Serializable]
    public class ControlInput
    {
        public KeyCode keyPos;
        public KeyCode keyNeg;
        public string axis;

        public ControlInput(KeyCode key)
        {
            keyPos = key;
            keyNeg = KeyCode.None;
        }

        public ControlInput(KeyCode positive, KeyCode negative)
        {
            keyPos = positive;
            keyNeg = negative;
        }

        public ControlInput(string axis)
        {
            this.axis = GetAxisName(axis);
        }

        /// <summary>
        /// Gets the current status of this control.
        /// </summary>
        /// <returns>If Axis, returns the current value of the axis, if Key, returns 1 if keyPos is pressed, -1 if keyNeg is pressed, 0 if neither</returns>
        public float Get()
        {
            if (!String.IsNullOrEmpty(axis))
                return Input.GetAxis(axes[axis]);
            else
                return Input.GetKey(keyPos) ? 1 : (Input.GetKey(keyNeg) ? -1 : 0);
        }
    }
}