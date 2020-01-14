using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : UnitySingleton<InputController>
{
    public bool GetAttackKeyDown()
    {
        return Input.GetKeyDown(KeyCode.J);
    }

    public bool GetJumpKeyDown()
    {
        if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Space))
            return true;
        return false;
    }

    public bool GetCrouchKey()
    {
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            return true;
        return false;
    }

    public float GetHorizontalAxisRaw()
    {
        return Input.GetAxisRaw("Horizontal");
    }

    public float GetVerticalAxisRaw()
    {
        return Input.GetAxisRaw("Vertical");
    }
}
