using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookTip : NetworkBehaviour
{
	[SyncVar]
	public bool canGrabHookables;
	public Action<GameObject> onObjectGrabbed;
	public Action<GameObject> onObjectReleased;

	public Guid playerGuid;
	
	[SyncVar]
	private GameObject grabbedObject;

	public void GrabObject(GameObject gameObject)
	{
		if (canGrabHookables)
		{
			grabbedObject = gameObject;
			//this.grabbedObject.transform.parent = transform;
			onObjectGrabbed?.Invoke(grabbedObject);
		}
	}

	public void ReleaseGrabbedObject()
	{
		if(grabbedObject != null)
		{
			onObjectReleased?.Invoke(grabbedObject);
		}
	}
}
