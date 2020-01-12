// 2020-01-12 14:27 应该算是修复了蹬墙跳的bug

using System;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    private GameController m_GameController;
    private Rigidbody2D m_Rigidbody2D;
    [SerializeField] private Transform m_Body;
    private Animator m_Animator;

    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private Transform m_CeilingCheck;
    [SerializeField] private Transform m_WallCheckL;
    [SerializeField] private Transform m_WallCheckR;
    [SerializeField] private Transform m_OnWallGroundCheck;

    [Range(0, 3)] [SerializeField] private float m_GravityScale = 3;
    [Range(0, 0.5f)] [SerializeField] private float m_MovementSmoothing = .1f;  // How much to smooth out the movement
    private Vector3 m_Velocity = Vector3.zero;

    [Range(0, 10)] public float runSpeed = 4.5f;
    [Range(0, 5)] public float climbSpeed = 3;
    [Range(500, 1000)] public float jumpForce = 630f;

    const float m_BlockCheckRadius = .1f;

    private GroundState m_GroundState;
    private Vector3 m_InitBodyScale;
    private bool m_Jump;


    private void Awake()
    {
        m_GameController = GameController.Instance;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Animator = m_Body.GetComponent<Animator>();
        m_InitBodyScale = m_Body.localScale;
    }


    private void Update()
    {
        if (InputController.Instance.GetJumpKeyDown())
            m_Jump = true;

        if (!BodyTouchWall() && !OnWallTouchGround())
        {
            float scaleX = m_Body.localScale.x;
            if (InputController.Instance.GetHorizontalAxisRaw() < 0)
            {
                scaleX = -m_InitBodyScale.x;
            }
            else if (InputController.Instance.GetHorizontalAxisRaw() > 0)
            {
                scaleX = m_InitBodyScale.x;
            }
            m_Body.localScale = new Vector3(scaleX, m_InitBodyScale.y, m_InitBodyScale.z);
        }
    }


    private void FixedUpdate()
    {
        if (FeetTouchGround())
        {
            m_Rigidbody2D.gravityScale = m_GravityScale;
            SetAnimTrigger(InputController.Instance.GetHorizontalAxisRaw() == 0 ? "stop" : "run");
            m_WallCheckL.gameObject.SetActive(false);
            m_WallCheckR.gameObject.SetActive(false);
            m_CeilingCheck.gameObject.SetActive(false);
            m_OnWallGroundCheck.gameObject.SetActive(false);
            m_GroundState = GroundState.GROUNDED;
        }
        else if (HeadTouchMonkeyBars())
        {
            m_Rigidbody2D.gravityScale = 0;
            m_WallCheckR.gameObject.SetActive(false);
            m_WallCheckR.gameObject.SetActive(false);
            m_GroundCheck.gameObject.SetActive(false);
            m_OnWallGroundCheck.gameObject.SetActive(true);
            m_GroundState = GroundState.HANGING;
        }
        else if (OnWallTouchGround())
        {
            m_Rigidbody2D.gravityScale = m_GravityScale / 2;
            m_CeilingCheck.gameObject.SetActive(false);
            m_WallCheckL.gameObject.SetActive(false);
            m_WallCheckR.gameObject.SetActive(false);
            m_GroundCheck.gameObject.SetActive(true);
            SetAnimTrigger("fall");
            m_GroundState = GroundState.WALL_TO_GROUND;
        }
        else if (BodyTouchWall())
        {
            m_Rigidbody2D.gravityScale = 0;
            SetAnimTrigger(m_Rigidbody2D.velocity.y == 0 ? "onwall" : "onwall");
            m_CeilingCheck.gameObject.SetActive(false);
            m_GroundCheck.gameObject.SetActive(false);
            m_OnWallGroundCheck.gameObject.SetActive(true);
            m_GroundState = GroundState.ONWALL;
        }
        else
        {
            m_Rigidbody2D.gravityScale = m_GravityScale;
            SetAnimTrigger(m_Rigidbody2D.velocity.y > 0 ? "jump" : "fall");
            m_WallCheckL.gameObject.SetActive(true);
            m_WallCheckR.gameObject.SetActive(true);
            m_CeilingCheck.gameObject.SetActive(true);
            m_GroundCheck.gameObject.SetActive(true);
            m_OnWallGroundCheck.gameObject.SetActive(false);
            m_GroundState = GroundState.IN_AIR;
        }


        Debug.Log(m_GroundState);

        Move(m_Jump);

        m_Jump = false;
    }


    public bool FeetTouchGround()
    {
        if (m_GroundCheck.gameObject.activeInHierarchy &&
            Physics2D.OverlapCircle(m_GroundCheck.position, m_BlockCheckRadius, m_GameController.GroundLayer))
            return true;
        return false;
    }


    public bool HeadTouchMonkeyBars()
    {
        if (m_CeilingCheck.gameObject.activeInHierarchy &&
            Physics2D.OverlapCircle(m_CeilingCheck.position, m_BlockCheckRadius, m_GameController.MonkeyBarsLayer))
            return true;
        return false;
    }


    public bool LeftBodyTouchWall()
    {
        if (m_WallCheckL.gameObject.activeInHierarchy &&
            Physics2D.OverlapCircle(m_WallCheckL.position, m_BlockCheckRadius, m_GameController.WallLayer))
            return true;
        return false;
    }


    public bool RightBodyTouchWall()
    {
        if (m_WallCheckR.gameObject.activeInHierarchy &&
            Physics2D.OverlapCircle(m_WallCheckR.position, m_BlockCheckRadius, m_GameController.WallLayer))
            return true;
        return false;
    }


    private bool OnWallTouchGround()
    {
        if (m_OnWallGroundCheck.gameObject.activeInHierarchy &&
            Physics2D.OverlapCircle(m_OnWallGroundCheck.position, m_BlockCheckRadius, m_GameController.GroundLayer))
            return true;
        return false;
    }


    public bool BodyTouchWall()
    {
        if (LeftBodyTouchWall() || RightBodyTouchWall())
            return true;
        return false;
    }


    public bool TouchMultipleTerrain()
    {
        if (BodyTouchWall() && HeadTouchMonkeyBars())
            return true;

        if (FeetTouchGround() && HeadTouchMonkeyBars())
            return true;

        if (FeetTouchGround() && BodyTouchWall())
            return true;

        return false;
    }


    public bool TouchNoTerrain()
    {
        if (FeetTouchGround() || BodyTouchWall() || HeadTouchMonkeyBars())
            return false;
        return true;
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
            "stop"
        };
    }


    private void Move(bool jump)
    {
        float xMovement = InputController.Instance.GetHorizontalAxisRaw() * runSpeed;
        float yMovement = InputController.Instance.GetVerticalAxisRaw() * climbSpeed;

        // 移动检测
        Vector3 targetVeclocity = Vector3.zero;

        if (m_GroundState == GroundState.GROUNDED)
        {
            targetVeclocity = new Vector2(xMovement, m_Rigidbody2D.velocity.y);
        }
        else if (m_GroundState == GroundState.ONWALL && !OnWallTouchGround())
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


        if (jump)
        {
            Vector2 force = Vector2.zero;

            if (m_GroundState == GroundState.GROUNDED)
            {
                force = new Vector2(0, jumpForce);
            }
            else if (m_GroundState == GroundState.ONWALL)
            {
                if (LeftBodyTouchWall())
                    force = new Vector2(jumpForce, jumpForce);
                else if (RightBodyTouchWall())
                    force = new Vector2(-jumpForce, jumpForce);
            }

            m_Rigidbody2D.AddForce(force);
        }
    }
}
