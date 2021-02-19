using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hook : MonoBehaviour
{
	[SerializeField] private Transform hookTip;

	[Header("Hook Settings")]
	[SerializeField] private float travelDistance = 30f;
	[SerializeField] private float shootSpeed = 3f;
	[Range(0,1)]
	[SerializeField] private float pullSpeed = 0.5f;

	[Range(3, 100)]
	[SerializeField] private float vertexCount = 12;
	
	private Transform hookPoint;
	private Transform releasePoint;
	private Vector3 direction;
	private LineRenderer lineRenderer;

	private bool canShoot = true;
	private bool isExpanding = false;
	private bool isPulling = false;
	private bool releasePointHit = false;

	private void Awake()
	{
		lineRenderer = GetComponentInChildren<LineRenderer>();
	}

	private void Start()
	{
		hookPoint = transform;
		releasePoint = new GameObject("releasePoint").transform;
		lineRenderer.useWorldSpace = true;
	}

	private void Update()
	{
		if (isExpanding)
		{
			hookTip.Translate(direction * shootSpeed * Time.deltaTime, Space.World);

			if ((releasePoint.position - hookTip.position).magnitude > travelDistance)
			{
				isExpanding = false;
				PullChain();
			}
		}
		else if(!isPulling)
			releasePoint.position = hookPoint.position;

		DrawChain();

	}

	public void Shoot(Vector3 targetPosition)
	{
		if (!canShoot)
			return;

		releasePoint.position = hookTip.position;
		direction = (targetPosition - hookTip.position).normalized;

		hookTip.SetParent(null);
		hookTip.GetComponent<HookTip>().CanGrabHookables = true;
		isExpanding = true;
		canShoot = false;
	}

	private void PullChain()
	{
		var positions = new Vector3[lineRenderer.positionCount];
		hookTip.GetComponent<HookTip>().CanGrabHookables = false;
		lineRenderer.GetPositions(positions);
		StartCoroutine(ChainPull(positions));
	}

	private IEnumerator ChainPull(Vector3[] positions)
	{
		isPulling = true;
		int pullIndex = positions.Length - 1;

		while (isPulling)
		{
			hookTip.position = Vector3.MoveTowards(hookTip.position, lineRenderer.GetPosition(pullIndex), pullSpeed);

			if(Vector3.Distance(hookTip.position, lineRenderer.GetPosition(pullIndex)) < 0.01f)
				pullIndex--;

			var distance = Vector3.Distance(hookTip.position, releasePoint.position);
			if (distance < 2f)
			{
				Debug.Log("ReleasePoint reached");
				releasePointHit = true;
			}

			if (pullIndex < 0)
			{
				ChainPullFinished();
			}
			yield return null;

		}

		yield return 0;
	}

	private void ChainPullFinished()
	{
		hookTip.SetParent(lineRenderer.transform);
		hookTip.GetComponent<HookTip>().ReleaseGrabbedObject();
		isPulling = false;
		canShoot = true;
		releasePointHit = false;
	}
	
	private void DrawChain()
	{
		var pointList = new List<Vector3>();

		if (!releasePointHit)
		{
			for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
			{
				Vector3 tangent1 = Vector3.Lerp(hookPoint.position, releasePoint.position, ratio);
				Vector3 tangent2 = Vector3.Lerp(releasePoint.position, hookTip.position, ratio);
				Vector3 curve = Vector3.Lerp(tangent1, tangent2, ratio);

				pointList.Add(curve);
			}
		}
		else
		{
			var midPosition = (hookTip.position + hookPoint.position) / 2;
			for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
			{
				Vector3 tangent1 = Vector3.Lerp(hookPoint.position, midPosition, ratio);
				Vector3 tangent2 = Vector3.Lerp(midPosition, hookTip.position, ratio);
				Vector3 curve = Vector3.Lerp(tangent1, tangent2, ratio);

				pointList.Add(curve);
			}
		}

		lineRenderer.positionCount = pointList.Count;
		lineRenderer.SetPositions(pointList.ToArray());
	}
}
