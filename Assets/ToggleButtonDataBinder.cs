﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using LMWidgets;

/**
 * This class is used with a toggle button in the game scene
 */
public class ToggleButtonDataBinder : DataBinderToggle
{
    bool toggle = false;

    override public bool GetCurrentData()
    {
        return toggle;
    }

    override protected void setDataModel(bool value)
    {
        toggle = value;
    }
}