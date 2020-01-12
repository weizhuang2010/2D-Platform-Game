// 2020-01-08 23:00 解决了蹬墙跳的bug
// 2020-01-08 00:56 添加角色动画：闲置、跑，播放时有Bug
// 2020-01-10 00:12 解决了部分动画切换不正确的问题，仍需继续检查

using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController2D m_Character;
    private Rigidbody2D m_Rigidbody2D;
    [SerializeField] private Transform m_Body;
    private Animator m_BodyAnimator;

    private float m_GravityScale;

    [SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character.
    [SerializeField] private LayerMask m_WhatIsMonkeyBars;						// A mask determining where the player can hang.
    [SerializeField] private LayerMask m_WhatIsWall;                            // A mask determining where the player can climb.
    [SerializeField] private LayerMask m_WhatCanBeHit;

    [SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings.
    [SerializeField] private Transform m_WallCheckL;
    [SerializeField] private Transform m_WallCheckR;


    const float k_GroundCheckRadius = .2f;		// Radius of the overlap circle to determine if grounded.
    const float k_CeilingCheckRadius = .2f;		// Radius of the overlap circle to determine if the player can stand up.
    const float k_WallCheckRadius = .2f;            // Radius of the overlap circle to determine if the player is on the wall

    public GroundState groundState { get => m_GroundState; }

    private GroundState m_GroundState;
    private Vector3 m_Velocity = Vector3.zero;
    private float m_NextAttack = 0;
    private bool m_CanAttack = true;

    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement

    public float jumpForce = 400f;							// Amount of force added when the character jumps.
    public float runSpeed = 4;
    public float climbSpeed = 3;
    public float m_AttackInterval = 2f;


    private void Awake()
    {
        m_Character = GetComponent<CharacterController2D>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_BodyAnimator = m_Body.GetComponent<Animator>();

        m_GravityScale = m_Rigidbody2D.gravityScale;
    }


    private void Update()
    {
        // Attack check
        if (Time.time > m_NextAttack)
        {
            m_CanAttack = true;
        }
    }


    private void FixedUpdate()
    {
        Debug.Log(m_GroundState.ToString());


        // if (FeetTouchGround())
        // {
        //     m_GroundState = GroundState.GROUNDED;
        // }
        // else
        // {
        //     if (HeadTouchMonkeyBars())
        //     {
        //         m_GroundState = GroundState.HANGING;
        //     }
        //     else
        //     {
        //         if (BodyTouchWall())
        //         {
        //             m_GroundState = GroundState.ONWALL;
        //         }
        //         else
        //         {
        //             m_GroundState = GroundState.IN_AIR;
        //         }
        //     }
        // }


        // 重力检测
        if (m_GroundState == GroundState.HANGING || m_GroundState == GroundState.ONWALL)
        {
            m_Rigidbody2D.gravityScale = 0;
        }
        else
        {
            m_Rigidbody2D.gravityScale = m_GravityScale;
        }


        Move();


        if (InputController.Instance.GetJumpKeyDown())
        {
            Jump();
        }


        Animate();







        if (m_Character.FeetTouchGround())
        {

        }

    }


    private bool FeetTouchGround()
    {
        if (Physics2D.OverlapCircle(m_GroundCheck.position, k_GroundCheckRadius, m_WhatIsGround))
            return true;
        return false;
    }


    private bool HeadTouchMonkeyBars()
    {
        if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingCheckRadius, m_WhatIsMonkeyBars))
            return true;
        return false;
    }


    private bool LeftBodyTouchWall()
    {
        if (Physics2D.OverlapCircle(m_WallCheckL.position, k_WallCheckRadius, m_WhatIsWall))
            return true;
        return false;
    }


    private bool RightBodyTouchWall()
    {
        if (Physics2D.OverlapCircle(m_WallCheckR.position, k_WallCheckRadius, m_WhatIsWall))
            return true;
        return false;
    }


    private bool BodyTouchWall()
    {
        if (LeftBodyTouchWall() || RightBodyTouchWall())
            return true;
        return false;
    }


    public void Move()
    {
        float xMovement = InputController.Instance.GetHorizontalAxisRaw() * runSpeed;
        float yMovement = InputController.Instance.GetVerticalAxisRaw() * climbSpeed;

        // 移动检测
        Vector3 targetVeclocity = Vector3.zero;

        if (m_GroundState == GroundState.GROUNDED)
        {
            targetVeclocity = new Vector2(xMovement, m_Rigidbody2D.velocity.y);
        }
        else if (m_GroundState == GroundState.ONWALL)
        {
            targetVeclocity = new Vector2(m_Rigidbody2D.velocity.x, yMovement);
        }
        else if (m_GroundState == GroundState.HANGING)
        {
            targetVeclocity = new Vector2(xMovement, m_Rigidbody2D.velocity.y);
        }
        else if (m_GroundState == GroundState.IN_AIR)
        {
            targetVeclocity = new Vector2(xMovement, m_Rigidbody2D.velocity.y);
        }

        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVeclocity, ref m_Velocity, m_MovementSmoothing);
    }


    private void Jump()
    {
        if (m_GroundState == GroundState.GROUNDED)
        {
            m_Rigidbody2D.AddForce(new Vector2(0, jumpForce));
            m_GroundState = GroundState.IN_AIR;
        }
        else if (m_GroundState == GroundState.ONWALL)
        {
            GameController.Instance.wallBlocks.SetActive(false);
            if (LeftBodyTouchWall())
            {
                m_Rigidbody2D.AddForce(new Vector2(jumpForce, jumpForce) * .75f);
            }
            else if (RightBodyTouchWall())
            {
                m_Rigidbody2D.AddForce(new Vector2(-jumpForce, jumpForce) * .75f);
            }
            m_GroundState = GroundState.IN_AIR;
        }
    }


    private void SetAnimTrigger(string triggerName)
    {
        string[] triggers = GetAnimTriggers();
        bool exist = false;
        foreach (string trig in triggers)
        {
            if (trig == triggerName) exist = true;
            m_BodyAnimator.ResetTrigger(trig);
        }
        if (exist)
        {
            m_BodyAnimator.SetTrigger(triggerName);
        }
        else
        {
            throw new NotImplementedException();
        }
    }


    private string[] GetAnimTriggers()
    {
        string[] t = new string[]
        {
            "run",
            "jump",
            "fall",
            "onwall",
            "stop"
        };
        return t;
    }


    // 动画检测
    private void Animate()
    {
        if (m_GroundState == GroundState.GROUNDED ||
            m_GroundState == GroundState.IN_AIR)
        {
            if (InputController.Instance.GetHorizontalAxisRaw() > 0)
            {
                FacingRight();
            }
            else if (InputController.Instance.GetHorizontalAxisRaw() < 0)
            {
                FacingLeft();
            }
        }
        if (m_GroundState == GroundState.ONWALL)
        {
            if (LeftBodyTouchWall())
            {
                FacingLeft();
            }
            else if (RightBodyTouchWall())
            {
                FacingRight();
            }
        }



        if (m_GroundState == GroundState.GROUNDED)
        {
            SetAnimTrigger("stop");
        }
        else if (m_GroundState == GroundState.IN_AIR)
        {
            SetAnimTrigger("jump");
        }
        else if (m_GroundState == GroundState.ONWALL)
        {
            SetAnimTrigger("onwall");
        }




        // if (m_GroundState == GroundState.GROUNDED && InputController.Instance.GetHorizontalAxisRaw() != 0)
        // {
        //     SetAnimTrigger("run");
        // }
        // else if (m_GroundState == GroundState.GROUNDED && InputController.Instance.GetHorizontalAxisRaw() == 0)
        // {
        //     SetAnimTrigger("stop");
        // }

        // if (m_GroundState == GroundState.IN_AIR && m_Rigidbody2D.velocity.y < 0)
        // {
        //     SetAnimTrigger("fall");
        // }
        // else if (m_GroundState == GroundState.IN_AIR && m_Rigidbody2D.velocity.y > 0)
        // {
        //     SetAnimTrigger("jump");
        // }

        // if (m_GroundState == GroundState.IN_AIR && BodyTouchWall())
        // {
        //     SetAnimTrigger("onwall");
        // }

        // if (m_GroundState == GroundState.ONWALL)
        // {
        //     SetAnimTrigger("onwall");

        //     if (InputController.Instance.GetJumpKeyDown())
        //     {
        //         SetAnimTrigger("jump");
        //     }
        // }




        // if (m_LastGroundState != m_GroundState)
        // {
        //     if (m_GroundState == GroundState.IN_AIR)
        //     {
        //         SetAnimTrigger("jump");
        //     }
        //     if (m_GroundState == GroundState.ONWALL)
        //     {
        //         SetAnimTrigger("onwall");
        //     }
        //     if (m_GroundState == GroundState.GROUNDED)
        //     {
        //         if (InputController.Instance.GetHorizontalAxisRaw() == 0)
        //         {
        //             SetAnimTrigger("stop");
        //         }
        //         else
        //         {
        //             SetAnimTrigger("run");
        //         }
        //     }
        // }
        // else
        // {
        //     if (BodyTouchWall() && InputController.Instance.GetJumpKeyDown())
        //     {
        //         SetAnimTrigger("jump");
        //     }
        //     if (m_GroundState == GroundState.IN_AIR && m_Rigidbody2D.velocity.y <= 0)
        //     {
        //         SetAnimTrigger("fall");
        //     }
        // }
    }

    private void FacingRight()
    {
        m_Body.localScale = new Vector3(1, 1, 1);
    }

    private void FacingLeft()
    {
        m_Body.localScale = new Vector3(-1, 1, 1);
    }

    private void Attack()
    {
        // Debug.Log("Player attacked.");

        // m_CanAttack = false;
        // m_NextAttack = Time.time + m_AttackInterval;

        // // Set attack animation.
        // m_BodyAnimator.SetTrigger("attack" + UnityEngine.Random.Range(1, 4).ToString());

        // // Set attack circle collider.
        // float x = transform.position.x + (m_FacingRight ? 1 : -1);
        // Vector2 point = new Vector2(x, transform.position.y);
        // float radius = .5f;
        // Collider2D[] colliders = Physics2D.OverlapCircleAll(point, radius, m_WhatCanBeHit);
        // foreach (Collider2D collider in colliders)
        // {
        //     // If hit enemy...
        //     if (collider.gameObject.tag == "Enemy")
        //     {
        //         // ... enemy apply damage.
        //         // collider.GetComponent<Enemy>().ApplyDamage(1);
        //     }
        // }
    }


    private void OnCollisionEnter(Collision other)
    {
        if (other == null)
        {
            m_GroundState = GroundState.IN_AIR;
        }
        else
        {
            if (other.gameObject.tag == "Ground")
            {
                m_GroundState = GroundState.GROUNDED;
            }
            else if (other.gameObject.tag == "MonkeyBars")
            {
                m_GroundState = GroundState.HANGING;
            }
            else if (other.gameObject.tag == "Wall")
            {
                m_GroundState = GroundState.ONWALL;
            }
        }
    }
}
