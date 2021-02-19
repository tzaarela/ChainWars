using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
	[SerializeField]
	private float travelDistance = 30f;
	[SerializeField]
	private float shootSpeed = 3f;
	[SerializeField]
	private float pullSpeed = 6f;
	[SerializeField]
	private Transform hookTip;

	private Vector3 startPosition;
	private bool isShot;
	private bool isPulling;
	private LineRenderer lineRenderer;

	private void Awake()
	{
		lineRenderer = GetComponentInChildren<LineRenderer>();
	}

	private void Update()
	{
		if (isShot)
		{
			lineRenderer.useWorldSpace = true;
			hookTip.Translate(Vector3.forward * shootSpeed * Time.deltaTime);

			lineRenderer.SetPosition(0, transform.position);
			lineRenderer.SetPosition(1, hookTip.position);

			if ((startPosition - hookTip.position).magnitude > travelDistance)
			{
				isPulling = true;
				isShot = false;
			}
		}

		else if (isPulling)
		{
			lineRenderer.useWorldSpace = false;
			hookTip.localPosition = transform.position + Vector3.forward;
			lineRenderer.SetPosition(0, transform.localPosition);
			lineRenderer.SetPosition(1, hookTip.localPosition);
			isPulling = false;
		}
	}

	public void Shoot()
	{
		startPosition = hookTip.position;
		isShot = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		
	}
}
