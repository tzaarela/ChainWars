using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hook : NetworkBehaviour
{
	[SerializeField] private GameObject hookTipPrefab;
	[SerializeField] private GameObject chainPrefab;
	[SerializeField] private GameObject releasePointPrefab;

	[Header("Hook Settings")]
	[SerializeField] private float travelDistance = 30f;
	[SerializeField] private float shootSpeed = 3f;
	[Range(0,1)]
	[SerializeField] private float pullSpeed = 0.5f;
	[Range(3, 100)]
	[SerializeField] private float vertexCount = 12;
	
	[SyncVar] private Transform hookTip;
	[SyncVar] private Transform releasePoint;
	[SyncVar] private Vector3 direction;
	[SyncVar] private Transform chain;
	[SyncVar] private bool canShoot = true;
	[SyncVar] private bool isExpanding = false;
	[SyncVar] private bool isPulling = false;
	[SyncVar] private bool releasePointHit = false;

	private void FixedUpdate()
	{
		if (!hasAuthority)
			return;

		CmdUpdate();
	}

	[Command]
	private void CmdUpdate()
	{
		if (isExpanding || isPulling)
		{
			RpcDrawChain();
		}

		if (isExpanding)
		{
			RpcMoveHookTip();
			if ((releasePoint.transform.position - hookTip.transform.position).magnitude > travelDistance)
			{
				PullChain();
			}
		}
	}

	[ClientRpc]
	private void RpcDrawChain()
	{
		if (hookTip == null || chain == null || releasePoint == null)
			return;
		
		var pointList = new List<Vector3>();


		if (!releasePointHit)
		{
			for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
			{
				Vector3 tangent1 = Vector3.Lerp(transform.position + Vector3.up, releasePoint.position, ratio);
				Vector3 tangent2 = Vector3.Lerp(releasePoint.position, hookTip.transform.position, ratio);
				Vector3 curve = Vector3.Lerp(tangent1, tangent2, ratio);

				pointList.Add(curve);
			}
		}
		else
		{
			var midPosition = (hookTip.transform.position + transform.position + Vector3.up) / 2;
			for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
			{
				Vector3 tangent1 = Vector3.Lerp(transform.position + Vector3.up, midPosition, ratio);
				Vector3 tangent2 = Vector3.Lerp(midPosition, hookTip.transform.position, ratio);
				Vector3 curve = Vector3.Lerp(tangent1, tangent2, ratio);

				pointList.Add(curve);
			}
		}

		chain.GetComponent<LineRenderer>().positionCount = pointList.Count;
		chain.GetComponent<LineRenderer>().useWorldSpace = true;
		chain.GetComponent<LineRenderer>().SetPositions(pointList.ToArray());
	}

	[ClientRpc]
	private void RpcMoveHookTip()
	{
		if (hookTip == null)
			return;

		hookTip.transform.Translate(direction * shootSpeed, Space.World);
	}

	[Command]
	public void CmdShoot(Vector3 targetPosition, Guid playerGuid)
	{
		if (!canShoot)
			return;

		InstantiateHookTipAndChain(playerGuid);

		direction = (targetPosition - hookTip.transform.position).normalized;
		direction = new Vector3(direction.x, 0, direction.z);

		isExpanding = true;
		canShoot = false;
	}

	private void InstantiateHookTipAndChain(Guid playerGuid)
	{
		hookTip = Instantiate(hookTipPrefab, transform.position + Vector3.up, Quaternion.identity).transform;
		hookTip.GetComponent<HookTip>().canGrabHookables = true;
		hookTip.GetComponent<HookTip>().playerGuid = playerGuid;
		hookTip.GetComponent<HookTip>().onObjectGrabbed += RpcHandleOnGrabObject;
		hookTip.GetComponent<HookTip>().onObjectReleased += RpcHandleOnObjectReleased;
		NetworkServer.Spawn(hookTip.gameObject, connectionToClient);

		chain = Instantiate(chainPrefab, transform.position + Vector3.up, Quaternion.identity).transform;
		chain.gameObject.GetComponent<LineRenderer>().useWorldSpace = true;
		NetworkServer.Spawn(chain.gameObject, connectionToClient);

		releasePoint = Instantiate(releasePointPrefab, transform.position + Vector3.up, Quaternion.identity).transform;
		releasePoint.position = hookTip.transform.position;
		NetworkServer.Spawn(releasePoint.gameObject, connectionToClient);
	}

	[ClientRpc]
	private void RpcHandleOnGrabObject(GameObject grabbedObject)
	{
		grabbedObject.transform.SetParent(hookTip);
	}

	private void RpcHandleOnObjectReleased(GameObject releasedObject)
	{
		releasedObject.transform.SetParent(null, true);
	}

	private void PullChain()
	{
		isExpanding = false;
		var positions = new Vector3[chain.GetComponent<LineRenderer>().positionCount];
		hookTip.GetComponent<HookTip>().canGrabHookables = false;
		chain.GetComponent<LineRenderer>().GetPositions(positions);
		isPulling = true;
		StartCoroutine(ChainPull(positions));
	}

	

	private IEnumerator ChainPull(Vector3[] positions)
	{
		int pullIndex = positions.Length - 1;

		while (isPulling)
		{
			if (hookTip == null || chain == null)
				yield return 0;

			if (chain.GetComponent<LineRenderer>().positionCount < pullIndex)
			{
				pullIndex--;
				yield return null;
			}
			RpcChainPull(pullIndex);
			

			if(Vector3.Distance(hookTip.transform.position, chain.GetComponent<LineRenderer>().GetPosition(pullIndex)) < 0.01f)
				pullIndex--;

			var distance = Vector3.Distance(hookTip.transform.position, releasePoint.position);
			if (distance < 2f)
			{
				if(hasAuthority)
					SetReleasePointHit();
			}

			if (pullIndex < 0)
			{
				if(hasAuthority)
					ChainPullFinished();
			}
			yield return null;

		}

		yield return 0;
	}

	[ClientRpc]
	private void RpcChainPull(int pullIndex)
	{
		hookTip.transform.position = Vector3.MoveTowards(hookTip.transform.position, chain.GetComponent<LineRenderer>().GetPosition(pullIndex), pullSpeed);
	}

	private void SetReleasePointHit()
	{
		releasePointHit = true;
	}

	private void ChainPullFinished()
	{
		//hookTip.transform.SetParent(lineRenderer.transform);
		hookTip.GetComponent<HookTip>().ReleaseGrabbedObject();
		Destroy(hookTip.gameObject);
		Destroy(chain.gameObject);
		Destroy(releasePoint.gameObject);
		isPulling = false;
		canShoot = true;
		releasePointHit = false;
	}
}
