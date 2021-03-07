using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask localPlayer;
	[SerializeField] private LayerMask remotePlayer;

	private Transform moveTarget;
	private Cinemachine.CinemachineVirtualCamera virtualCamera;
	private Camera vcamRaycastCamera;
	private Animator animator;
	private Rigidbody rigidBodyPlayer;
	

	[SyncVar]
	public Guid playerGuid;

	private Camera mainCamera;

	private void Awake()
	{
		mainCamera = Camera.main;
		virtualCamera = GameObject.Find("VCam").GetComponent<Cinemachine.CinemachineVirtualCamera>();
		vcamRaycastCamera = virtualCamera.GetComponentInChildren<Camera>();
		animator = GetComponent<Animator>();
		rigidBodyPlayer = GetComponent<Rigidbody>();
	} 

	private void Start()
	{
		if (!isLocalPlayer)
		{
			gameObject.layer = LayerMask.NameToLayer("RemotePlayer");
			return;
		}

		gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
		virtualCamera.Follow = transform;

		CmdStart();

		moveTarget = new GameObject("moveTarget").transform;
		GetComponent<Pathfinding.AIDestinationSetter>().target = moveTarget.transform;

	}

	[Command]
	private void CmdStart()
	{
		playerGuid = Guid.NewGuid();
	}

	public void Update()
	{
		if (!isLocalPlayer)
			return;

		if (Input.GetMouseButtonDown(0))
		{
			MoveToMousePosition();
		}

		if (Input.GetMouseButtonDown(1))
		{
			HookAtMousePosition();
		}

		UpdateAnimations();
	}

	private void UpdateAnimations()
	{
		if (Mathf.Abs(rigidBodyPlayer.velocity.x) > 0.1f || Mathf.Abs(rigidBodyPlayer.velocity.z) > 0.1f)
			animator.SetBool("isRunning", true);
		else
			animator.SetBool("isRunning", false);
	}


	public void MoveToMousePosition()
	{
		var ray = vcamRaycastCamera.ScreenPointToRay(Input.mousePosition);

		RaycastHit raycastHit;
		if (Physics.Raycast(ray, out raycastHit, 1000f, groundLayer))
		{
			Debug.DrawLine(vcamRaycastCamera.transform.position, raycastHit.point, Color.red);
			moveTarget.position = raycastHit.point;
		}
	}

	public void HookAtMousePosition()
	{
		var hook = GetComponent<Hook>();
		var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

		RaycastHit raycastHit;
		if (Physics.Raycast(ray, out raycastHit, 1000f, groundLayer))
		{
			hook.CmdShoot(raycastHit.point + Vector3.up, playerGuid);
		}
	}
}
