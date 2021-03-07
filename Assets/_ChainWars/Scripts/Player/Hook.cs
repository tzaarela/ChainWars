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
	[SerializeField] private GameObject startPointPrefab;

	[Header("Hook Settings")]
	[SerializeField] private float travelDistance = 30f;
	[Range(0, 5)]
	[SerializeField] private float shootSpeed = 3f;
	[Range(0,1)]
	[SerializeField] private float pullSpeed = 0.5f;
	[Range(3, 100)]
	[SerializeField] private float vertexCount = 12;
	[SerializeField] private float releaseObjectDistance = 2f;

	[SyncVar] private Transform hookTip;
	[SyncVar] private Transform startPoint;
	[SyncVar] private Vector3 direction;
	[SyncVar] private Transform chain;
	[SyncVar] private bool canShoot = true;
	[SyncVar] private bool isExpanding = false;
	[SyncVar] private bool isPulling = false;
	[SyncVar] private bool startPointHit = false;
	[SyncVar] private bool isGrabbing = false;

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
			if ((startPoint.transform.position - hookTip.transform.position).magnitude > travelDistance)
			{
				PullChain(null);
			}
		}
	}

	[ClientRpc]
	private void RpcDrawChain()
	{
		if (hookTip == null || chain == null || startPoint == null)
			return;
		
		var pointList = new List<Vector3>();


		if (!startPointHit)
		{
			for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
			{
				Vector3 tangent1 = Vector3.Lerp(transform.position + Vector3.up, startPoint.position, ratio);
				Vector3 tangent2 = Vector3.Lerp(startPoint.position, hookTip.transform.position, ratio);
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

		direction = (targetPosition - (transform.position + Vector3.up)).normalized;
		direction = new Vector3(direction.x, 0, direction.z);

		InstantiateHookTipAndChain(playerGuid);


		isExpanding = true;
		canShoot = false;
	}

	private void InstantiateHookTipAndChain(Guid playerGuid)
	{
		hookTip = Instantiate(hookTipPrefab, transform.position + Vector3.up, Quaternion.identity).transform;
		hookTip.rotation = Quaternion.LookRotation(direction) * Quaternion.FromToRotation(Vector3.right, Vector3.forward);
		hookTip.GetComponent<HookTip>().canGrabHookables = true;
		hookTip.GetComponent<HookTip>().playerGuid = playerGuid;
		hookTip.GetComponent<HookTip>().onObjectGrabbed += RpcHandleOnGrabObject;
		hookTip.GetComponent<HookTip>().onObjectReleased += RpcHandleOnObjectReleased;
		NetworkServer.Spawn(hookTip.gameObject, connectionToClient);

		chain = Instantiate(chainPrefab, transform.position + Vector3.up, Quaternion.identity).transform;
		chain.gameObject.GetComponent<LineRenderer>().useWorldSpace = true;
		NetworkServer.Spawn(chain.gameObject, connectionToClient);

		startPoint = Instantiate(startPointPrefab, transform.position + Vector3.up, Quaternion.identity).transform;
		startPoint.position = hookTip.transform.position;
		NetworkServer.Spawn(startPoint.gameObject, connectionToClient);
	}

	[ClientRpc]
	private void RpcHandleOnGrabObject(GameObject grabbedObject)
	{
		if (hasAuthority)
		{
			//grabbedObjectConnection = grabbedObject.GetComponent<NetworkIdentity>().connectionToClient;
			//grabbedObject.transform.SetParent(hookTip);
			CmdPullChain(grabbedObject);
		}
	}

	[ClientRpc]
	private void RpcHandleOnObjectReleased(GameObject releasedObject)
	{
		//releasedObject.transform.SetParent(null, true);
		//releasedObject.GetComponent<NetworkIdentity>().AssignClientAuthority(grabbedObjectConnection);
	}

	[Command]
	private void CmdPullChain(GameObject grabbedObject)
	{
		if (grabbedObject != null)
		{
			grabbedObject.GetComponent<Pathfinding.AIPath>().canSearch = false;
			isGrabbing = true;
		}

		PullChain(grabbedObject);
	}

	private void PullChain(GameObject grabbedObject)
	{
		isExpanding = false;
		var positions = new Vector3[chain.GetComponent<LineRenderer>().positionCount];
		hookTip.GetComponent<HookTip>().canGrabHookables = false;
		chain.GetComponent<LineRenderer>().GetPositions(positions);
		isPulling = true;
		StartCoroutine(ChainPull(positions, grabbedObject));
	}

	private IEnumerator ChainPull(Vector3[] positions, GameObject grabbedObject)
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

			if (grabbedObject != null && Vector3.Distance(grabbedObject.transform.position, transform.position) < releaseObjectDistance)
			{
				Debug.Log("Releasing");
				isGrabbing = false;
				grabbedObject.GetComponent<Pathfinding.AIPath>().canSearch = true;
				grabbedObject = null;
			}

			RpcChainPull(pullIndex, grabbedObject);
			

			if(Vector3.Distance(hookTip.transform.position, chain.GetComponent<LineRenderer>().GetPosition(pullIndex)) < 0.01f)
				pullIndex--;

			var distance = Vector3.Distance(hookTip.transform.position, startPoint.position);
			if (distance < 2f)
			{
				SetReleasePointHit();
			}

			if (pullIndex < 0)
			{
				ChainPullFinished();
			}
			yield return null;

		}

		yield return 0;
	}

	[ClientRpc]
	private void RpcChainPull(int pullIndex, GameObject grabbedObject)
	{
		if (hookTip == null)
			return;

		if (chain.GetComponent<LineRenderer>().positionCount <= pullIndex)
			return;

		hookTip.transform.position = Vector3.MoveTowards(hookTip.transform.position, chain.GetComponent<LineRenderer>().GetPosition(pullIndex), pullSpeed);

		if(grabbedObject != null && isGrabbing)
		{
			grabbedObject.transform.position = hookTip.transform.position;
			grabbedObject.transform.position = new Vector3(grabbedObject.transform.position.x, 0, grabbedObject.transform.position.z);
		}
	}

	private void SetReleasePointHit()
	{
		startPointHit = true;
	}

	private void ChainPullFinished()
	{
		//hookTip.transform.SetParent(lineRenderer.transform);
		hookTip.GetComponent<HookTip>().ReleaseGrabbedObject();
		NetworkServer.Destroy(hookTip.gameObject);
		NetworkServer.Destroy(chain.gameObject);
		NetworkServer.Destroy(startPoint.gameObject);
		isPulling = false;
		canShoot = true;
		startPointHit = false;
	}
}
