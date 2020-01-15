// 2020-01-08 00:56 添加角色动画：闲置、跑，播放时有Bug
// 2020-01-10 00:12 解决了部分动画切换不正确的问题，仍需继续检查
// 2020-01-12 23:12 解决了从墙面到地面的动画显示错误的问题
// 2020-01-15 18:03 整理代码，GroundState添加了在Shelf上的子状态，把按下每个按钮都单独写成了方法

using System;
using System.Collections;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    #region
    [SerializeField] private Transform bodyTransform;
    private new Rigidbody2D rigidbody2D;
    private Animator animator;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform lowerGroundCheck;
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private Transform leftWallCheck;
    [SerializeField] private Transform rightWallCheck;
    [SerializeField] private GameObject leftHitBox;
    [SerializeField] private GameObject rightHitBox;

    [SerializeField] private float gravityScale = 3;

    [Range(0, 10)] [SerializeField] private float runSpeed;
    [Range(0, 0.5f)] [SerializeField] private float movementSmoothing = .1f;
    [Range(0, 10)] [SerializeField] private float climbSpeed;
    [Range(0, 800)] [SerializeField] private float jumpForce;

    const float colliderCheckRadius = .1f;

    public enum GroundState
    {
        OnTheGround,
        OnTheWall,
        UnderTheShelf,
        OverTheShelf,
        InTheAir
    }

    public GroundState groundState { get; set; }

    private bool isFacingRight = true;
    private bool isCrouching = false;
    private bool isAttacking = false;

    private float xInput;
    private float yInput;
    private bool isHoldingCrouch;

    private bool isJumpPressed = false;
    private bool isAttackPressed;
    private Vector2 velocity = Vector2.zero;
    private LayerMask groundLayer;
    private LayerMask wallLayer;
    private LayerMask shelfLayer;
    #endregion


    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = bodyTransform.GetComponent<Animator>();
    }

    private void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        isHoldingCrouch = Input.GetAxisRaw("Horizontal") < 0 ? true : false;
        isJumpPressed = Input.GetKeyDown(KeyCode.K);
        isAttackPressed = Input.GetKeyDown(KeyCode.J);


        // Check state and facing direction.
        if (IsCheckerTouchingLayer(lowerGroundCheck, groundLayer))
        {
            if (IsCheckerTouchingLayer(groundCheck, groundLayer))
            {
                groundState = GroundState.OnTheGround;
            }
            else
            {
                groundState = GroundState.InTheAir;
            }
        }
        else
        {
            if(IsCheckerTouchingLayer(groundCheck, shelfLayer))
            {
                groundState = GroundState.OverTheShelf;
            }
            else if (IsCheckerTouchingLayer(ceilingCheck, shelfLayer))
            {
                groundState = GroundState.UnderTheShelf;
            }
            else
            {
                if (IsCheckerTouchingLayer(leftWallCheck, wallLayer))
                {
                    groundState = GroundState.OnTheWall;
                    isFacingRight = false;
                }
                else if (IsCheckerTouchingLayer(rightWallCheck, wallLayer))
                {
                    groundState = GroundState.OnTheWall;
                    isFacingRight = true;
                }
                else
                {
                    groundState = GroundState.InTheAir;
                }
            }
        }

        // Check flip
        CheckFlip();
    }


    private bool IsCheckerTouchingLayer(Transform checker, LayerMask layerMask)
    {
        return Physics2D.OverlapCircle(checker.position, colliderCheckRadius, layerMask);
    }


    private void CheckFlip()
    {
        if (groundState != GroundState.OnTheWall)
        {
            if (xInput > 0 && !isFacingRight || xInput < 0 && isFacingRight)
            {
                // Switch the way the player is labelled as facing.
                isFacingRight = !isFacingRight;

                // Multiply the player's x local scale by -1.
                Vector3 theScale = bodyTransform.localScale;
                theScale.x *= -1;
                bodyTransform.localScale = theScale;
            }
        }
    }


    private void FixedUpdate()
    {
        // Check Gravity
        CheckGravity();

        // Check move
        Move();

        if(isHoldingCrouch)
        {
            CrouchHolding();
        }
        else
        {
            CrouchReleased();
        }

        if (isJumpPressed)
        {
            JumpPressed();
        }

        if (isAttackPressed)
        {
            AttackPressed();
        }

        isJumpPressed = false;
    }


    private void CheckGravity()
    {
        if (groundState == GroundState.InTheAir)
        {
            rigidbody2D.gravityScale = this.gravityScale;
        }
        else
        {
            rigidbody2D.gravityScale = 0;
        }
    }


    private void Move()
    {
        Vector2 move = Vector2.zero;

        // Move...
        if (groundState == GroundState.OnTheGround)
        {
            if (!isCrouching && !isAttacking)
            {
                move = new Vector2(xInput * runSpeed, 0);
            }
        }
        else if (groundState == GroundState.OnTheWall)
        {
            if (!isAttacking)
            {
                move = new Vector2(0, yInput * climbSpeed);
            }
        }
        else if (groundState == GroundState.UnderTheShelf)
        {
            if (!isAttacking)
            {
                move = new Vector2(xInput * runSpeed, 0);
            }
        }
        else if (groundState == GroundState.InTheAir)
        {
            move = new Vector2(xInput * runSpeed, rigidbody2D.velocity.y);
        }

        rigidbody2D.velocity = Vector2.SmoothDamp(rigidbody2D.velocity, move, ref velocity, movementSmoothing);
    }


    private void CrouchHolding()
    {
        if(groundState == GroundState.OnTheGround || groundState == GroundState.OverTheShelf)
        {
            isCrouching = true;
        }
    }


    private void CrouchReleased()
    {
        isCrouching = false;
    }


    private void JumpPressed()
    {
        Vector2 force = Vector2.zero;

        if (groundState == GroundState.OnTheGround)
        {
            force = new Vector2(0, jumpForce);
        }
        else if (groundState == GroundState.OnTheWall)
        {
            force = new Vector2(jumpForce * (isFacingRight ? 1 : -1), jumpForce) / 2;
        }
        else if (groundState == GroundState.UnderTheShelf)
        {
            force = new Vector2(0, jumpForce);
        }
        else if (groundState == GroundState.OverTheShelf)
        {
            if (isCrouching)
            {
                force = new Vector2(0, -jumpForce) / 2;
            }
            else
            {
                force = new Vector2(0, jumpForce);
            }
        }

        rigidbody2D.AddForce(force);
    }


    private void AttackPressed()
    {
        isAttacking = true;

        if (Input.GetAxisRaw("Vertical") > 0)
        {
            StartCoroutine(Ninjutsu());
        }
        else
        {
            StartCoroutine(NormalAttack());
        }
    }


    private IEnumerator NormalAttack()
    {
        if (isFacingRight)
        {
            rightHitBox.SetActive(true);
        }
        else
        {
            leftHitBox.SetActive(true);
        }

        if (groundState == GroundState.OnTheGround)
        {
            if(isCrouching)
            {

            }
            else
            {

            }
        }
        else if (groundState == GroundState.OnTheWall)
        {

        }
        else if (groundState == GroundState.UnderTheShelf)
        {

        }
        else if (groundState == GroundState.InTheAir)
        {

        }

        yield return new WaitForSeconds(.2f);

        leftHitBox.SetActive(false);
        rightHitBox.SetActive(false);
        isAttacking = false;
    }


    private IEnumerator Ninjutsu()
    {
        yield return new WaitForSeconds(.2f);
        isAttacking = false;
    }
}
