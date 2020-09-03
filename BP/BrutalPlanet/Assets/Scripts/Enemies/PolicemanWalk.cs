using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolicemanWalk : MonoBehaviour
{
    public bool drawDebugRaycasts = true;
	public Animator animator;

	[Header("Movement Properties")]
	public float speed = 2f;
    public float speedWalking = 2f;
	public float crouchSpeedDivisor = 0.6f;
	public float coyoteDuration = .05f;
	public float maxFallSpeed = -5f;

	[Header("Jump Properties")]
	public float jumpForce = 6.3f;
	public float crouchJumpBoost = 2.5f;
	public float hangingJumpForce = 15f;
	public float jumpHoldForce = 1.9f;
	public float jumpHoldDuration = .1f;

	[Header("Environment Check Properties")]
	public float footOffset = .2f;			//X Offset of feet raycast
	public float eyeHeight = .6f;			//Height of wall checks
	public float reachOffset = .4f;			//X offset for wall grabbing
	public float headClearance = .3f;		//Space needed above the player's head
	public float groundDistance = .2f;		//Distance player is considered to be on the ground
	public float grabDistance = .3f;		//The reach distance for wall grabs
	public LayerMask groundLayer;			//Layer of the ground

	[Header ("Status Flags")]
	public bool isOnGround;
	public bool IsRunning;
	public bool isJumping;
	public bool isHanging;
	public bool isCrouching;
	public bool isHeadBlocked;
	public bool isClimbing;
    public bool isWalking;
    public bool leftCheckFoot;
    public bool rightCheckFoot;
    public bool wallCheckDir;
    public bool WaitStanding = false;

	InputControllScript input;
	BoxCollider2D bodyCollider;
	Rigidbody2D rigidBody;
	
	float jumpTime;
	float coyoteTime;
	float playerHeight;
	float currentSpeed = 0;

	float originalXScale;
	int direction = 1;

	Vector2 colliderStandSize;
	Vector2 colliderStandOffset;
	Vector2 colliderCrouchSize;
	Vector2 colliderCrouchOffset;

	const float smallAmount = .05f;

	bool changedPos = false;
	Vector3 pos1Climb;
	Vector3 pos2Climb;

    float xVelocity = 0;


    void Start ()
	{
		rigidBody = GetComponent<Rigidbody2D>();
		bodyCollider = GetComponent<BoxCollider2D>();

		originalXScale = transform.localScale.x;
		playerHeight = bodyCollider.size.y;
		colliderStandSize = bodyCollider.size;
		colliderStandOffset = bodyCollider.offset;
		colliderCrouchSize = new Vector2(bodyCollider.size.x, bodyCollider.size.y / 2f);
		colliderCrouchOffset = new Vector2(bodyCollider.offset.x, -0.5f);
	}

	void FixedUpdate()
	{
		if (isCrouching){
			bodyCollider.size = colliderCrouchSize;
			bodyCollider.offset = colliderCrouchOffset;
		}
		PhysicsCheck();
		GroundMovement();		
		MidAirMovement();
		if (Mathf.Abs(currentSpeed) == 0 || !isOnGround || isCrouching || isHanging || isJumping) {
			IsRunning = false;
			animator.SetBool("IsPoliceRanning", false);
			}
		if (IsRunning == true){
			animator.SetBool("IsPoliceRanning", true);
		}
		//if (isHanging == true){
		//	animator.SetBool("IsHangingPoliceman", true);
		//}
		//if (isHanging == false || isClimbing){
		//	animator.SetBool("IsHangingPoliceman", false);
		//}
		//if (isCrouching == false){
		//	animator.SetBool("IsCroulPoliceman", false);
		//}
		//animator.SetBool("canClimbLedgePoliceman", isClimbing);
	}

	void PhysicsCheck()
	{
		isOnGround = false;
		isHeadBlocked = false;

		RaycastHit2D leftCheck = Raycast(new Vector2(-footOffset, -0.9f), Vector2.down, groundDistance);
		RaycastHit2D rightCheck = Raycast(new Vector2(footOffset, -0.9f), Vector2.down, groundDistance);
		if (leftCheck || rightCheck)
			isOnGround = true;
        if (leftCheck)
        {
            leftCheckFoot = true;
        } else 
        {
            leftCheckFoot = false;
        }
        if (rightCheck)
        {
            rightCheckFoot = true;
        } else 
        {
            rightCheckFoot = false;
        }
		RaycastHit2D headCheck = Raycast(new Vector2(0f, bodyCollider.size.y-1f), Vector2.up, headClearance);
		if (headCheck)
			isHeadBlocked = true;


		Vector2 grabDir = new Vector2(direction, 0f);
		RaycastHit2D blockedCheck = Raycast(new Vector2(footOffset * direction, playerHeight-1f), grabDir, grabDistance);
		RaycastHit2D ledgeCheck = Raycast(new Vector2(reachOffset * direction, playerHeight-1f), Vector2.down, grabDistance);
		RaycastHit2D wallCheck = Raycast(new Vector2(footOffset * direction, eyeHeight), grabDir, grabDistance);
        if (wallCheck)
        {
            wallCheckDir = true;
        } else 
        {
            wallCheckDir = false;
        }

		if (!isOnGround && !isHanging && rigidBody.velocity.y < 0f && 
			ledgeCheck && wallCheck && !blockedCheck)
		{ 
			Vector3 pos = transform.position;
			pos.x += (wallCheck.distance - smallAmount) * direction;
			pos.y -= ledgeCheck.distance;
			transform.position = pos;
			rigidBody.bodyType = RigidbodyType2D.Static;
			isHanging = true;
		}
	}

	void GroundMovement()
	{
		if (isHanging)
			return;
        
        if (WaitStanding) xVelocity = 0;
        if (!WaitStanding) xVelocity = speed*direction;
        if (isOnGround){
            currentSpeed = xVelocity;
            
            if (rightCheckFoot & !leftCheckFoot)
            {
                if(WaitStanding) return;
                Vector3 pos = transform.position;
			    pos.x +=  smallAmount * -direction;
			    pos.y = transform.position.y;
			    transform.position = pos;
                WaitStanding = true;
                Invoke("FlipCharacterSpeed", 2);
                currentSpeed = xVelocity;
            } 
            if (leftCheckFoot & !rightCheckFoot)
            {
                if(WaitStanding) return;
                Vector3 pos = transform.position;
			    pos.x +=  smallAmount * -direction;
			    pos.y = transform.position.y;
			    transform.position = pos;
                WaitStanding = true;
                Invoke("FlipCharacterSpeed", 2);
                currentSpeed = xVelocity;
            } 

            if (wallCheckDir)
            {
                if(WaitStanding) return;
                WaitStanding = true;
                Invoke("FlipCharacterSpeed", 2);
                currentSpeed = xVelocity;
            } 
		
        }

		if (isCrouching)
			xVelocity /= crouchSpeedDivisor;

		rigidBody.velocity = new Vector2(xVelocity, rigidBody.velocity.y);
		if (isOnGround && !isCrouching && Mathf.Abs(currentSpeed) > 0) IsRunning = true;
		if (isOnGround)
			coyoteTime = Time.time + coyoteDuration;
	}

	void MidAirMovement()
	{
		if (isHanging)
		{
			ledgeClimb();
		}
	}

	void FlipCharacterDirection()
	{
		direction *= -1;
		Vector3 scale = transform.localScale;
		scale.x = originalXScale * direction;
		transform.localScale = scale;
	}
    
    void FlipCharacterSpeed(){
        FlipCharacterDirection();
        WaitStanding = false;
    }

	void Crouch()
	{
		isCrouching = true;
		bodyCollider.size = colliderCrouchSize;
		bodyCollider.offset = colliderCrouchOffset;
		//animator.SetBool("IsCroul", isCrouching);
	}

	void StandUp()
	{
		if (isHeadBlocked)
			return;
		isCrouching = false;
		bodyCollider.size = colliderStandSize;
		bodyCollider.offset = colliderStandOffset;
	}

	void ledgeClimb()
	{
		pos1Climb = transform.position;
		if(isHanging == true){
			isClimbing = true;
			if(changedPos == false){
				pos2Climb.x += pos1Climb.x + (1f - smallAmount) * direction;
				pos2Climb.y += pos1Climb.y + 1.8f;
				changedPos = true;
				isHanging = false;
			}
			
		}
		if (isClimbing)
		{
			//animator.SetBool("canClimbLedge", isClimbing);
			transform.position = pos1Climb;
		}
	}

	public void FinishLedgeClimb()
	{
		isClimbing = false;
		//animator.SetBool("canClimbLedge", isClimbing);
		transform.position = pos2Climb;
		changedPos = false;
		rigidBody.bodyType = RigidbodyType2D.Dynamic;
		Crouch();
		pos2Climb.x = 0;
		pos2Climb.y = 0;
	}


	RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length)
	{
		return Raycast(offset, rayDirection, length, groundLayer);
	}

	RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length, LayerMask mask)
	{
		Vector2 pos = transform.position;
		RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDirection, length, mask);
		if (drawDebugRaycasts)
		{
			Color color = hit ? Color.red : Color.green;
			Debug.DrawRay(pos + offset, rayDirection * length, color);
		}
		return hit;
	}
}
