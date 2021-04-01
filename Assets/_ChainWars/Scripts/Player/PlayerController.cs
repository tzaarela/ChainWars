using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Assets._ChainWars.Scripts.Player;
using Mapster;
using Pathfinding;
using TMPro;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask localPlayer;
	[SerializeField] private LayerMask remotePlayer;
	[SerializeField] private PlayerDefaults playerDefaults;
	[SerializeField] private Image healthbar;

	private Transform moveTarget;
	private Cinemachine.CinemachineVirtualCamera virtualCamera;
	private Camera vcamRaycastCamera;
	private Animator animator;
	private Rigidbody rigidBodyPlayer;
	private Hook hook;
	private AIPath aiPath;
	
	[SyncVar] public Guid playerGuid;
	[SyncVar] public Player playerData;

	private Camera mainCamera;

	private void Awake()
	{
		mainCamera = Camera.main;
		virtualCamera = GameObject.Find("VCam").GetComponent<Cinemachine.CinemachineVirtualCamera>();
		vcamRaycastCamera = virtualCamera.GetComponentInChildren<Camera>();
		animator = GetComponent<Animator>();
		rigidBodyPlayer = GetComponent<Rigidbody>();
		hook = GetComponent<Hook>();
		aiPath = GetComponent<AIPath>();
		playerData = playerDefaults.Adapt<Player>();
	} 

	private void Start()
	{
		if (!isLocalPlayer)
		{
			gameObject.layer = LayerMask.NameToLayer("RemotePlayer");
			return;
		}

		aiPath.maxSpeed = playerData.RunSpeed;

		gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
		virtualCamera.Follow = transform;

		moveTarget = new GameObject("moveTarget").transform;
		GetComponent<AIDestinationSetter>().target = moveTarget.transform;

		UpdateUI();
		CmdStart();

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

	public void TakeDamage(float damage)
	{
		playerData.Health -= damage;
		UIController.Instance.HealthText.text = playerData.Health.ToString();
		healthbar.fillAmount = playerData.Health * 0.01f;
	}

	public void AddHealth(float health)
	{
		playerData.Health += health;
		UIController.Instance.HealthText.text = playerData.Health.ToString();
		healthbar.fillAmount = playerData.Health * 0.01f;
	}

	private void UpdateUI()
	{
		UIController.Instance.HealthText.text = playerData.Health.ToString();
		UIController.Instance.RunSpeedText.text = playerData.RunSpeed.ToString();
		UIController.Instance.MeleeDamageText.text = playerData.MeleeDamage.ToString();
		UIController.Instance.HookDamageText.text = playerData.HookDamage.ToString();
		UIController.Instance.HookLengthText.text = playerData.HookLength.ToString();
		UIController.Instance.HookSpeedText.text = playerData.HookSpeed.ToString();
	}
}
