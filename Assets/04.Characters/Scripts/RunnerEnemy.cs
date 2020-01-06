using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerEnemy : Enemy
{
    [SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the character jumps.
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement.
    [SerializeField] private Transform[] m_JumpPoints;							// The position where the character takes off.
    [SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
    [SerializeField] private float moveSpeed = 1;
    protected bool m_IsGrounded;					// Whether or not the character is grounded.
    const float k_GroundedRadius = .2f;				// Radius of the overlap circle to determine if the player can stand up
    private Rigidbody2D m_Rigidbody2D;
    private Vector3 m_Velocity = Vector3.zero;

	private void Awake()
	{
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
    }

	private void FixedUpdate()
	{
        m_IsGrounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_IsGrounded = true;
            }
        }

        Move(moveSpeed * Time.fixedDeltaTime);
    }

    public void Move(float speed)
    {
        Vector3 targetVeclocity = new Vector2(speed * 10f, m_Rigidbody2D.velocity.y);
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVeclocity, ref m_Velocity, m_MovementSmoothing);

        if (m_IsGrounded && EnterJumpPos())
        {
            m_IsGrounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }
    }
    private bool EnterJumpPos()
    {
        foreach (Transform point in m_JumpPoints)
        {
            if (Vector3.Distance(transform.position, point.position) < .2)
            {
                return true;
            }
        }
        return false;
    }
}
