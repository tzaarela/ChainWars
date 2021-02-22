using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookTip : NetworkBehaviour
{
	public bool canGrabHookables;
	public Action onObjectGrabbed;
	public Guid guid;
	
	private GameObject grabbedObject;

	public void GrabObject(GameObject gameObject)
	{
		if (canGrabHookables)
		{
			this.grabbedObject = gameObject;
			this.grabbedObject.transform.parent = transform;
			onObjectGrabbed?.Invoke();
		}
	}

	public void ReleaseGrabbedObject()
	{
		if(grabbedObject != null)
		{
			grabbedObject.transform.SetParent(null);
			grabbedObject = null;
		}
	}
}
