// 2020-01-06 实现了趴在墙上不会掉下来的功能

using UnityEngine;

public enum GroundState
{
    IN_AIR,
    GROUNDED,
    CLIMBING,
    HANGING
}

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the character jumps.
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
    [SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character.
    [SerializeField] private LayerMask m_WhatIsMonkeyBars;						// A mask determining where the player can hang.
    [SerializeField] private LayerMask m_WhatIsWall;							// A mask determining where the player can climb.
    [SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings.

    // A position marking where to check for wall.
    [SerializeField] private Transform m_WallCheckL;
    [SerializeField] private Transform m_WallCheckR;

    const float k_GroundedRadius = .2f;		// Radius of the overlap circle to determine if grounded.
    const float k_CeilingRadius = .2f;		// Radius of the overlap circle to determine if the player can stand up.
    const float k_WallRadius = .2f;			// Radius of the overlap circle to determine if the player is on the wall.
    const float k_GravityScale = 3f;
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;      // For determining which way the player is currently facing.

    // m_CeilingCheck.position, k_CeilingRadius, m_WhatIsMonkeyBars

    private Vector3 m_Velocity = Vector3.zero;
    private GroundState m_GroundState;
    private GroundState m_LastGroundState;


    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
    }


    private void FixedUpdate()
    {
        // m_Grounded = false;
        m_GroundState = GroundState.IN_AIR;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] feetColliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < feetColliders.Length; i++)
        {
            if (feetColliders[i].gameObject != gameObject)
            {
                m_GroundState = GroundState.GROUNDED;
            }
        }

        if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsMonkeyBars))
        {
            m_GroundState = GroundState.HANGING;
        }

        if (Physics2D.OverlapCircle(m_WallCheckR.position, k_WallRadius, m_WhatIsWall))
        {
            m_GroundState = GroundState.CLIMBING;
            m_FacingRight = true;
        }

        else if (Physics2D.OverlapCircle(m_WallCheckL.position, k_WallRadius, m_WhatIsWall))
        {
            m_GroundState = GroundState.CLIMBING;
            m_FacingRight = false;
        }

        Move(Vector3.zero, false, false);
    }


    public void Move(Vector2 move, bool crouch, bool jump)
    {
        // If the input isn't crouch...
        if (!crouch)
        {
            // ... but the character has a ceiling preventing them from standing up...
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                // ... keep them crouching.
                crouch = true;
            }
        }

        // When can the player move horizontally...
        if ((m_GroundState == GroundState.GROUNDED && !crouch) || m_GroundState == GroundState.IN_AIR || m_GroundState == GroundState.HANGING)
        {
            Vector3 targetVeclocity = new Vector2(move.x, m_Rigidbody2D.velocity.y);
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVeclocity, ref m_Velocity, m_MovementSmoothing);
        }

        // When can the player move vertically...
        if (m_GroundState == GroundState.CLIMBING)
        {
            Vector3 targetVeclocity = new Vector2(0, move.y);
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVeclocity, ref m_Velocity, m_MovementSmoothing);
        }

        // If the player should flip...
        if (m_GroundState != GroundState.CLIMBING)
        {
            // If the input is moving the player right and the player is facing left, then flip the player.
            if (move.x > 0 && !m_FacingRight) Flip();
            // Otherwise if the input is moving the player left and the player is facing right, also flip the player.
            else if (move.x < 0 && m_FacingRight) Flip();
        }

        // If the player is not in the air and the input is jump...
        if (m_GroundState != GroundState.IN_AIR && jump)
        {
            // If the player is on the ground or hanging...
            if (m_GroundState == GroundState.GROUNDED || m_GroundState == GroundState.HANGING)
            {
                // ... add a force in the vertical direction.
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            }
            // Otherwise if the player is climbing on the wall...
            else if (m_GroundState == GroundState.CLIMBING)
            {
                // flip the player and add a force in the horizontal and vertical directions.
                Flip();
                m_Rigidbody2D.AddForce(new Vector2(m_FacingRight ? -1 : 1, 1) * m_JumpForce);
            }

            m_GroundState = GroundState.IN_AIR;
        }

        if (m_LastGroundState == GroundState.IN_AIR && m_GroundState == GroundState.CLIMBING)
        {
            m_Rigidbody2D.gravityScale = 0;
        }
        else if (m_LastGroundState == GroundState.CLIMBING && m_GroundState == GroundState.IN_AIR)
        {
            m_Rigidbody2D.gravityScale = k_GravityScale;
        }

        m_LastGroundState = m_GroundState;
    }


    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
