// 2020-01-06 实现了趴在墙上不会掉下来的功能
// 2020-01-07 允许角色倒吊在Monkeybars上，爬墙跳功能有Bug，不能正确地往墙的反面跳

using UnityEngine;

public enum GroundState
{
    GROUNDED,
    CLIMBING,
    HANGING,
    IN_AIR
}

public class CharacterController2D : MonoBehaviour
{
    private Rigidbody2D m_Rigidbody2D;
    private Animator m_Animator;

    [SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the character jumps.
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
    [SerializeField] private float m_AttackInterval = 2f;

    [SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character.
    [SerializeField] private LayerMask m_WhatIsMonkeyBars;						// A mask determining where the player can hang.
    [SerializeField] private LayerMask m_WhatIsWall;                            // A mask determining where the player can climb.
    [SerializeField] private LayerMask m_WhatCanBeHit;

    [SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings.
    [SerializeField] private Transform m_WallCheck;

    const float k_GroundedRadius = .2f;		// Radius of the overlap circle to determine if grounded.
    const float k_CeilingRadius = .2f;		// Radius of the overlap circle to determine if the player can stand up.
    const float k_WallRadius = .6f;			// Radius of the overlap circle to determine if the player is on the wall.

    private GroundState m_GroundState;
    private Vector3 m_Velocity = Vector3.zero;
    private float m_NextAttack = 0;
    private bool m_FacingRight = true;      // For determining which way the player is currently facing.
    private bool m_CanAttack = true;

    private float m_GravityScale;
    private float m_TransScaleX;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();

        m_GravityScale = m_Rigidbody2D.gravityScale;
        m_TransScaleX = transform.localScale.x;
    }

    private void Update()
    {
        float xscale = m_TransScaleX * (m_FacingRight ? 1 : -1);
        transform.localScale = new Vector3(xscale, transform.localScale.y, transform.localScale.z);
    }

    private void FixedUpdate()
    {
        m_GroundState = GroundState.IN_AIR;

        if (Physics2D.OverlapCircle(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround))
        {
            m_GroundState = GroundState.GROUNDED;
        }
        else if (Physics2D.OverlapCircle(m_WallCheck.position, k_WallRadius, m_WhatIsWall))
        {
            m_GroundState = GroundState.CLIMBING;
        }
        else if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsMonkeyBars))
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
        else
        {
            Collider2D wall = Physics2D.OverlapCircle(m_WallCheck.position, k_WallRadius, m_WhatIsWall);

            if (transform.position.x < wall.transform.position.x)
            {
                m_FacingRight = false;
            }
            else if (transform.position.x > wall.transform.position.x)
            {
                m_FacingRight = true;
            }
        }

        if (jump)
        {
            if (m_GroundState == GroundState.GROUNDED || m_GroundState == GroundState.HANGING)
            {
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            }
            else if (m_GroundState == GroundState.CLIMBING)
            {
                float xdir = m_FacingRight ? 1 : -1;
                m_Rigidbody2D.AddForce(new Vector2(2 * xdir, 1) * m_JumpForce);
            }
            m_GroundState = GroundState.IN_AIR;
        }
    }


    private void CheckAttack()
    {
        if (Time.time > m_NextAttack)
        {
            m_CanAttack = true;
        }

        if (m_CanAttack && InputController.Instance.GetAttackKeyDown())
        {
            Attack();
            m_CanAttack = false;
            m_NextAttack = Time.time + m_AttackInterval;
        }
    }


    private void Attack()
    {
        Debug.Log("Player attacked.");

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
                collider.GetComponent<Enemy>().ApplyDamage(1);
            }
        }
    }
}
