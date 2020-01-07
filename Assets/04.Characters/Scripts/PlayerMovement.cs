using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController2D character;
    private InputController input;

    public float runSpeed = 4;
    public float climbSpeed = 3;
    Vector2 move = Vector2.zero;
    bool jump = false;
    bool crouch = false;

    private void Awake()
    {
        character = GetComponent<CharacterController2D>();
        input = InputController.Instance;
    }

    void Update()
    {
        move.x = input.XMovement * runSpeed;
        move.y = input.YMovement * climbSpeed;

        if (input.GetJumpKeyDown())
        {
            jump = true;
        }

        if (input.GetCrouchKey())
        {
            crouch = true;
        }
        else
        {
            crouch = false;
        }
    }

    void FixedUpdate()
    {
        // Move our character
        character.Move(move, crouch, jump);
        jump = false;
    }
}
