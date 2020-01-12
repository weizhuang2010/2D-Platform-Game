// 2020-01-08 00:56 添加角色动画：闲置、跑，播放时有Bug
// 2020-01-10 00:12 解决了部分动画切换不正确的问题，仍需继续检查
// 2020-01-12 23:12 解决了从墙面到地面的动画显示错误的问题

using System;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private Transform m_Body;
    private Rigidbody2D m_Rigidbody2D;
    private Animator m_Animator;

    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private Transform m_WallCheckL;
    [SerializeField] private Transform m_WallCheckR;

    const float k_ColliderCheckRadius = .1f;

    [Range(0, 10)] [SerializeField] private float m_RunSpeed;
    [Range(0, 0.5f)] [SerializeField] private float m_MovementSmoothing = .1f;
    [Range(0, 10)] [SerializeField] private float m_ClimbSpeed;
    [Range(0, 800)] [SerializeField] private float m_JumpForce;

    private LayerMask m_WhatIsGround;
    private LayerMask m_WhatIsWall;

    private GroundState m_GroundState;
    private bool m_FacingRight = true;
    private Vector2 m_Velocity = Vector2.zero;

    private Vector2 m_MovementInput = Vector2.zero;
    private bool m_Jump = false;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Animator = m_Body.GetComponent<Animator>();
        m_WhatIsGround = GameController.Instance.GroundLayer;
        m_WhatIsWall = GameController.Instance.WallLayer;
    }

    private void Update()
    {
        m_MovementInput.x = InputController.Instance.GetHorizontalAxisRaw();
        m_MovementInput.y = InputController.Instance.GetVerticalAxisRaw();

        if (InputController.Instance.GetJumpKeyDown()) m_Jump = true;

        if (FeetTouchGround())
        {
            m_GroundState = GroundState.GROUNDED;
        }
        else if (BodyTouchWall())
        {
            m_GroundState = GroundState.ONWALL;
        }
        else
        {
            m_GroundState = GroundState.IN_AIR;
        }

        if (m_GroundState == GroundState.GROUNDED)
        {
            m_WallCheckL.gameObject.SetActive(false);
            m_WallCheckR.gameObject.SetActive(false);
            m_Rigidbody2D.gravityScale = 1;
            SetAnimTrigger("idle");
            CheckFlip();
        }
        else if (m_GroundState == GroundState.ONWALL)
        {
            m_Rigidbody2D.velocity = Vector2.zero;
            m_Rigidbody2D.gravityScale = 0;
            SetAnimTrigger("onwall");
        }
        else if (m_GroundState == GroundState.IN_AIR)
        {
            m_WallCheckL.gameObject.SetActive(true);
            m_WallCheckR.gameObject.SetActive(true);
            m_Rigidbody2D.gravityScale = 1;
            SetAnimTrigger("fall");
            CheckFlip();
        }
    }


    private void FixedUpdate()
    {
        if (m_GroundState == GroundState.GROUNDED)
        {
            Move(new Vector2(m_MovementInput.x * m_RunSpeed, 0));
            if (m_Jump) Jump(new Vector2(0, m_JumpForce));
        }
        else if (m_GroundState == GroundState.ONWALL)
        {
            Move(new Vector2(0, m_MovementInput.y * m_ClimbSpeed));
            if (m_Jump) Jump(new Vector2(0, m_JumpForce));
        }
        else if (m_GroundState == GroundState.IN_AIR)
        {
            Move(new Vector2(m_MovementInput.x * m_RunSpeed, m_Rigidbody2D.velocity.y));
        }
    }


    private void Move(Vector2 move)
    {
        m_Rigidbody2D.velocity = Vector2.SmoothDamp(m_Rigidbody2D.velocity, move, ref m_Velocity, m_MovementSmoothing);
    }


    private void Jump(Vector2 force)
    {
        m_Rigidbody2D.AddForce(force);
        m_Jump = false;
    }


    private void CheckFlip()
    {
        if (m_MovementInput.x > 0 && !m_FacingRight || m_MovementInput.x < 0 && m_FacingRight)
        {
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = m_Body.localScale;
            theScale.x *= -1;
            m_Body.localScale = theScale;
        }
    }


    private bool FeetTouchGround()
    {
        if (m_GroundCheck.gameObject.activeInHierarchy &&
            Physics2D.OverlapCircle(m_GroundCheck.position, k_ColliderCheckRadius, m_WhatIsGround))
            return true;
        return false;
    }

    private bool LeftBodyTouchWall()
    {
        if (m_WallCheckL.gameObject.activeInHierarchy &&
            Physics2D.OverlapCircle(m_WallCheckL.position, k_ColliderCheckRadius, m_WhatIsWall))
            return true;
        return false;
    }

    private bool RightBodyTouchWall()
    {
        if (m_WallCheckR.gameObject.activeInHierarchy &&
            Physics2D.OverlapCircle(m_WallCheckR.position, k_ColliderCheckRadius, m_WhatIsWall))
            return true;
        return false;
    }

    private bool BodyTouchWall()
    {
        if (LeftBodyTouchWall() || RightBodyTouchWall())
            return true;
        return false;
    }


    private void SetAnimTrigger(string triggerName)
    {
        string[] triggers = GetAnimTriggers();
        bool exist = false;

        foreach (string trig in triggers)
        {
            if (trig == triggerName) exist = true;
            m_Animator.ResetTrigger(trig);
        }

        if (exist)
            m_Animator.SetTrigger(triggerName);
        else
            throw new NotImplementedException();
    }


    private string[] GetAnimTriggers()
    {
        return new string[]
        {
            "run",
            "jump",
            "fall",
            "onwall",
            "idle"
        };
    }
}
