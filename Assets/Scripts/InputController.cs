using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : UnitySingleton<InputController>
{
    private float _xMovement;
    private float _yMovement;

    public float XMovement { get => _xMovement; }
    public float YMovement { get => _yMovement; }


    private void Update()
    {
        _xMovement = Input.GetAxisRaw("Horizontal");
        _yMovement = Input.GetAxisRaw("Vertical");
    }


    public bool GetAttackKeyDown()
    {
        if (Input.GetKeyDown(KeyCode.J))
            return true;
        return false;
    }

    public bool GetCrouchKey()
    {
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            return true;
        return false;
    }


    public bool GetJumpKeyDown()
    {
        if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Space))
            return true;
        return false;
    }
}
