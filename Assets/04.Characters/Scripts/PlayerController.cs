﻿using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D m_Rigidbody2D;
    private Animator m_Animator;

    private Vector3 m_DefaultScale;
    private float m_GravityScale;
    private float m_WallCheckDistance;

    [SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character.
    [SerializeField] private LayerMask m_WhatIsMonkeyBars;						// A mask determining where the player can hang.
    [SerializeField] private LayerMask m_WhatIsWall;                            // A mask determining where the player can climb.
    [SerializeField] private LayerMask m_WhatCanBeHit;

    [SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings.

    const float k_GroundCheckRadius = .2f;		// Radius of the overlap circle to determine if grounded.
    const float k_CeilingCheckRadius = .2f;		// Radius of the overlap circle to determine if the player can stand up.
    const float k_WallCheckRadius = .2f;            // Radius of the overlap circle to determine if the player is on the wall.

    private GroundState m_GroundState;
    [SerializeField] private bool m_FacingRight = true;      // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;
    private float m_NextAttack = 0;
    private bool m_CanAttack = true;

    [SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the character jumps.
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
    [SerializeField] private float m_AttackInterval = 2f;


    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();

        m_DefaultScale = transform.localScale;
        m_GravityScale = m_Rigidbody2D.gravityScale;		// Get default gravity scale.
        m_WallCheckDistance = Mathf.Abs(transform.localScale.x / 2);
    }

    private void Update()
    {
        float scalex = m_DefaultScale.x * (m_FacingRight ? 1 : -1);
        transform.localScale = new Vector3(scalex, transform.localScale.y, transform.localScale.z);

        if (Time.time > m_NextAttack)
        {
            m_CanAttack = true;
        }
    }

    private void FixedUpdate()
    {
        m_GroundState = GroundState.IN_AIR;

        Vector3 m_WallCheckL = transform.position - new Vector3(m_WallCheckDistance, 0, 0);
        Vector3 m_WallCheckR = transform.position + new Vector3(m_WallCheckDistance, 0, 0);

		// Check state
        if (Physics2D.OverlapCircle(m_GroundCheck.position, k_GroundCheckRadius, m_WhatIsGround))
        {
            m_GroundState = GroundState.GROUNDED;
        }
        else if (Physics2D.OverlapCircle(m_WallCheckL, k_WallCheckRadius, m_WhatIsWall))
        {
            m_GroundState = GroundState.CLIMBING;
            m_FacingRight = true;
        }
        else if (Physics2D.OverlapCircle(m_WallCheckR, k_WallCheckRadius, m_WhatIsWall))
        {
            m_GroundState = GroundState.CLIMBING;
            m_FacingRight = false;
        }
        else if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingCheckRadius, m_WhatIsMonkeyBars))
        {
            m_GroundState = GroundState.HANGING;
        }


        if (m_GroundState == GroundState.IN_AIR)
        {
            m_Rigidbody2D.gravityScale = m_GravityScale;
        }
        else
        {
            m_Rigidbody2D.gravityScale = 0;
        }
    }


    public void Move(Vector2 move, bool crouch, bool jump)
    {
        Vector3 targetVeclocity;

        if (m_GroundState == GroundState.GROUNDED && !crouch)
        {
            targetVeclocity = new Vector2(move.x, 0);
        }
        else if (m_GroundState == GroundState.CLIMBING)
        {
            targetVeclocity = new Vector2(0, move.y);
        }
        else if (m_GroundState == GroundState.HANGING)
        {
            targetVeclocity = new Vector2(move.x, 0);
        }
        else if (m_GroundState == GroundState.IN_AIR)
        {
            targetVeclocity = new Vector2(move.x, m_Rigidbody2D.velocity.y);
        }
        else
        {
            targetVeclocity = Vector3.zero;
        }

        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVeclocity, ref m_Velocity, m_MovementSmoothing);


        // If the player should flip...
        if (m_GroundState != GroundState.CLIMBING)
        {
            if (move.x > 0)
            {
                m_FacingRight = true;
            }
            else if (move.x < 0)
            {
                m_FacingRight = false;
            }
        }


        if (jump)
        {
            Vector2 force = Vector2.zero;

            if (m_GroundState == GroundState.GROUNDED || m_GroundState == GroundState.HANGING)
            {
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            }
            else if (m_GroundState == GroundState.CLIMBING)
            {
                force.x = m_JumpForce * (m_FacingRight ? 1 : -1);
                force.y = 0;

                m_Rigidbody2D.AddForce(force);
            }
            m_GroundState = GroundState.IN_AIR;
        }
    }


    private void Attack()
    {
        Debug.Log("Player attacked.");

        m_CanAttack = false;
        m_NextAttack = Time.time + m_AttackInterval;

        // Set attack animation.
        m_Animator.SetTrigger("attack" + Random.Range(1, 4).ToString());

        // Set attack circle collider.
        float x = transform.position.x + (m_FacingRight ? 1 : -1);
        Vector2 point = new Vector2(x, transform.position.y);
        float radius = .5f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(point, radius, m_WhatCanBeHit);
        foreach (Collider2D collider in colliders)
        {
            // If hit enemy...
            if (collider.gameObject.tag == "Enemy")
            {
                // ... enemy apply damage.
                // collider.GetComponent<Enemy>().ApplyDamage(1);
            }
        }
    }
}
