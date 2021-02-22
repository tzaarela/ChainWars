using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerHandler : NetworkBehaviour
{
    public GameObject hook;
	private Transform moveTarget;
	public LayerMask groundLayer;
	public LayerMask localPlayer;
	public LayerMask remotePlayer;

	[SyncVar]
	public Guid playerGuid;

	private Camera mainCamera;

	private void Awake()
	{
		mainCamera = Camera.main;
	} 

	private void Start()
	{
		if (!isLocalPlayer)
		{
			gameObject.layer = LayerMask.NameToLayer("RemotePlayer");
			return;
		}

		CmdStart();

		moveTarget = new GameObject("moveTarget").transform;
		GetComponent<Pathfinding.AIDestinationSetter>().target = moveTarget.transform;

		gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
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


	}

	public void MoveToMousePosition()
	{
		var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

		RaycastHit raycastHit;
		if (Physics.Raycast(ray, out raycastHit, 1000f, groundLayer))
		{
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
