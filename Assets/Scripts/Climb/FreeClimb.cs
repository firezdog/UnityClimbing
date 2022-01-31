using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeClimb : MonoBehaviour
{
	public Animator anim;
	public bool isClimbing;

	bool inPosition;
	bool isLerping;
	float t;
	Vector3 startPos;
	Vector3 targetPos;
	Quaternion startRot;
	Quaternion targetRot;
	public float positionOffset;
	public float offsetFromWall = 0.3f;
	public float speed_multiplier = 0.2f;
	float delta;

	public float climbSpeed = 3;
	public float rotateSpeed = 5;

	Transform helper;	// used for book-keeping

	private void Start()
	{
		Init();
		
	}

	private void Init()
	{
		helper = new GameObject().transform;
		helper.name = "ClimbHelper";
		CheckForClimb();
	}

	public void CheckForClimb()
	{
		// find climb position
		Vector3 origin = transform.position;
		origin.y += 1.4f;   // raise origin off ground
		Vector3 dir = transform.forward;
		RaycastHit hit;
		float maxDist = 5;
		if (Physics.Raycast(origin, dir, out hit, maxDist))
		{
			helper.position = PosWithOffset(origin, hit.point);
			InitForClimb(hit);
		}
	}

	void InitForClimb(RaycastHit hit)
	{
		isClimbing = true;
		// get correct rotation now from opposite of hit normal
		helper.transform.rotation = Quaternion.LookRotation(-hit.normal);
		startPos = transform.position;
		targetPos = hit.point + (hit.normal * offsetFromWall);
		t = 0; // in case not first time we are climbing
		inPosition = false;
		anim.CrossFade("climb_idle", 2);	// TODO: not programmatic
	}

	private void Update()
	{
		delta = Time.deltaTime;
		Tick(delta);
	}

	// for third-person controller (generic) along with Init
	public void Tick(float delta)
	{
		if (!inPosition && isClimbing)
		{
			GetInPostiion();
			return;
		}

		if (!isLerping)
		// finding target positions
		{
			float hor = Input.GetAxis("Horizontal");
			float vert = Input.GetAxis("Vertical");
	
			Vector3 h = helper.right * hor;
			Vector3 v = helper.up * vert;
			Vector3 moveDir = (h + v).normalized;

			// can we move in the moveDir?
			if (CanMove(moveDir) || !(moveDir == Vector3.zero)) {
				startPos = transform.position;
				targetPos = helper.position;
				isLerping = true;
				t = 0;
			}
		} 
		else // moving between positions
		{
			t += delta * climbSpeed;
			if (t > 1)
			{
				t = 1;
				isLerping = false;
			}
			Vector3 climbingPos = Vector3.Lerp(startPos, targetPos, t);
			transform.position = climbingPos;
			transform.rotation = Quaternion.Slerp(transform.rotation, helper.rotation, delta * rotateSpeed);
		}
	}

	bool CanMove(Vector3 moveDir)
	{
		Vector3 origin = transform.position;
		float dist = positionOffset;
		Vector3 dir = moveDir;
		Debug.DrawRay(origin, dir * dist, Color.blue);
		RaycastHit hit;
		
		if (Physics.Raycast(origin, dir, out hit, dist))
		{
			return false; // move dir encountered an obstacle -- TODO: reorient
		}

		origin += moveDir * dist;
		dir = helper.forward;
		float forwardLookDist = 0.5f;

		Debug.DrawRay(origin, dir * forwardLookDist, Color.yellow);

		if (Physics.Raycast(origin, dir, out hit, dist))
		{
			helper.position = PosWithOffset(origin, hit.point);
			helper.rotation = Quaternion.LookRotation(-hit.normal);
			return true;
		}

		// moving around a corner
		origin = dir * forwardLookDist;
		dir = -Vector3.up;

		Debug.DrawRay(origin, dir, Color.red);
		if (Physics.Raycast(origin, dir, out hit, forwardLookDist))
		{
			float angle = Vector3.Angle(helper.up, hit.normal);
			if (angle < 40)
			{
				helper.position = PosWithOffset(origin, hit.point);
				helper.rotation = Quaternion.LookRotation(-hit.normal);
				return true;
			}
		}


		return false;
	}

	// I think this function is animating the transition into the climb
	private void GetInPostiion()
	{
		// orientation and position for climb
		t += delta;

		if (t > 1)
		{
			t = 1;
			inPosition = true;

			// TODO: enable IK
		}

		Vector3 lerpedTargetPos = Vector3.Lerp(startPos, targetPos, t);
		transform.position = lerpedTargetPos;
		transform.rotation = Quaternion.Slerp(transform.rotation, helper.rotation, delta * rotateSpeed);
	}

	Vector3 PosWithOffset(Vector3 origin, Vector3 target)
	{
		Vector3 direction = (origin - target);
		direction.Normalize();
		Vector3 offset = direction * offsetFromWall;
		return target + offset;
	}
}
