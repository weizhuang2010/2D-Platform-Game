// 2020-01-08 00:56 添加角色动画：闲置、跑，播放时有Bug
// 2020-01-10 00:12 解决了部分动画切换不正确的问题，仍需继续检查
// 2020-01-12 23:12 解决了从墙面到地面的动画显示错误的问题

using System;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public enum Location
    {
        Ground,
        Wall,
        Shelf,
        Air
    }

    [SerializeField] private Transform body;
    private Rigidbody2D rigidbody2D;
    private Animator animator;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheckL;
    [SerializeField] private Transform wallCheckR;

    const float colliderCheckRadius = .1f;

    [Range(0, 10)] [SerializeField] private float runSpeed;
    [Range(0, 0.5f)] [SerializeField] private float movementSmoothing = .1f;
    [Range(0, 10)] [SerializeField] private float climbSpeed;
    [Range(0, 800)] [SerializeField] private float jumpForce;

    public Location location { get; set; }
    private bool isFacingRight = true;
    private Vector2 velocity = Vector2.zero;

    private Vector2 movementInput = Vector2.zero;
    //private bool jumpPressed = false;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = body.GetComponent<Animator>();
    }

    private void Update()
    {
        movementInput.x = InputController.Instance.GetHorizontalAxisRaw();
        movementInput.y = InputController.Instance.GetVerticalAxisRaw();

        //if (InputController.Instance.GetJumpKeyDown())
        //{
        //    jumpPressed = true;
        //}

        if (FeetTouchGround())
        {
            location = Location.Ground;
        }
        else if (BodyTouchWall())
        {
            location = Location.Wall;
        }
        else
        {
            location = Location.Air;
        }


        if (location == Location.Ground)
        {
            wallCheckL.gameObject.SetActive(false);
            wallCheckR.gameObject.SetActive(false);
            rigidbody2D.gravityScale = 1;
            SetAnimTrigger("idle");
            CheckFlip();
        }
        else if (location == Location.Wall)
        {
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.gravityScale = 0;
            SetAnimTrigger("onwall");
        }
        else if (location == Location.Air)
        {
            wallCheckL.gameObject.SetActive(true);
            wallCheckR.gameObject.SetActive(true);
            rigidbody2D.gravityScale = 1;
            SetAnimTrigger("fall");
            CheckFlip();
        }
    }


    private void FixedUpdate()
    {
        Vector2 move = Vector2.zero;

        if (location == Location.Ground)
        {
            if (InputController.Instance.GetAttackKeyDown())
            {
                if (InputController.Instance.GetVerticalAxisRaw() > 0)
                {
                    Ninjutsu();
                    move = new Vector2(movementInput.x * runSpeed, 0);
                }
                else if (InputController.Instance.GetVerticalAxisRaw() < 0)
                {
                    NormalAttack();
                }
                else
                {
                    move = new Vector2(movementInput.x * runSpeed, 0);
                }
            }
        }
        else if (location == Location.Wall)
        {
            if(InputController.Instance.GetAttackKeyDown())
            {
                Ninjutsu();
            }
            else
            {
                move = new Vector2(0, movementInput.y * climbSpeed);
            }
        }
        else if (location == Location.Air)
        {
            if(InputController.Instance.GetAttackKeyDown())
            {
                if(InputController.Instance.GetVerticalAxisRaw() > 0)
                {
                    Ninjutsu();
                }
                else
                {
                    NormalAttack();
                }
            }
            move = new Vector2(movementInput.x * runSpeed, rigidbody2D.velocity.y);
        }

        rigidbody2D.velocity = Vector2.SmoothDamp(rigidbody2D.velocity, move, ref velocity, movementSmoothing);


        if (InputController.Instance.GetJumpKeyDown())
        {
            Vector2 force = Vector2.zero;

            if (location == Location.Ground || location == Location.Shelf)
            {
                rigidbody2D.AddForce(new Vector2(0, jumpForce));
                SetAnimTrigger("jump");
            }
            else if (location == Location.Wall)
            {
                if (isFacingRight)
                {
                    force = new Vector2(-jumpForce, jumpForce) / 2;
                }
                else
                {
                    force = new Vector2(jumpForce, jumpForce) / 2;
                }
                SetAnimTrigger("jump");
            }

            rigidbody2D.AddForce(force);
        }
    }

    private void Ninjutsu()
    {
        SetAnimTrigger("ninjutsu");
    }

    private void NormalAttack()
    {
        SetAnimTrigger("ninjutsu");
    }


    private void CheckFlip()
    {
        if (movementInput.x > 0 && !isFacingRight || movementInput.x < 0 && isFacingRight)
        {
            // Switch the way the player is labelled as facing.
            isFacingRight = !isFacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = body.localScale;
            theScale.x *= -1;
            body.localScale = theScale;
        }
    }


    private bool FeetTouchGround()
    {
        if (groundCheck.gameObject.activeInHierarchy &&
            Physics2D.OverlapCircle(groundCheck.position, colliderCheckRadius, GameController.groundLayer))
            return true;
        return false;
    }

    private bool LeftBodyTouchWall()
    {
        if (wallCheckL.gameObject.activeInHierarchy &&
            Physics2D.OverlapCircle(wallCheckL.position, colliderCheckRadius, GameController.wallLayer))
            return true;
        return false;
    }

    private bool RightBodyTouchWall()
    {
        if (wallCheckR.gameObject.activeInHierarchy &&
            Physics2D.OverlapCircle(wallCheckR.position, colliderCheckRadius, GameController.wallLayer))
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
            animator.ResetTrigger(trig);
        }

        if (exist)
            animator.SetTrigger(triggerName);
        else
            throw new NotImplementedException();
    }


    private string[] GetAnimTriggers()
    {
        return new string[]
        {
            "idle",
            "run",
            "attack",
            "crouch_idle",
            "crouch_attack",
            "ninjutsu",
            "jump",
            "jump_attack",
            "jump_ninjutsu",
            "fall",
            "onwall_idle",
            "onwall_move",
            "onwall_ninjutsu",
            "onshelf_idle",
            "onshelf_move",
            "onshelf_ninjutsu"
        };
    }
}
