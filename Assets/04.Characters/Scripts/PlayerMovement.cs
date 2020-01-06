using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController2D character;

    public float runSpeed = 40f;
    public float climbSpeed = 30f;
    Vector2 move = Vector2.zero;
    bool jump = false;
    bool crouch = false;

    void Update()
    {
        move.x = Input.GetAxisRaw("Horizontal") * runSpeed;
        move.y = Input.GetAxisRaw("Vertical") * climbSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        }
        else if (Input.GetButtonUp("Crouch"))
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
