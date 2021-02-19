using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject hook;
	public Transform moveTarget;
	public LayerMask groundLayer;

	private Camera mainCamera;

	private void Awake()
	{
		mainCamera = Camera.main;
	}

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

			RaycastHit raycastHit;
			if(Physics.Raycast(ray, out raycastHit, 1000f, groundLayer))
			{
				moveTarget.position = raycastHit.point;
			}
		}

		if (Input.GetMouseButtonDown(1))
		{
			var hook = GetComponentInChildren<Hook>();


			var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

			RaycastHit raycastHit;
			if (Physics.Raycast(ray, out raycastHit, 1000f, groundLayer))
			{
				hook.Shoot(raycastHit.point + Vector3.up);
			}
		}
	}

	public void ShootHook()
	{

	}
}
