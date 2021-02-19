using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookTip : MonoBehaviour
{
	private GameObject grabbedObject;
	private bool canGrabHookables;

    public bool CanGrabHookables 
	{
		get { return canGrabHookables; }
		set
		{
			canGrabHookables = value;
			HandleGrabSubscription();
		}
	}

	private void HandleGrabSubscription()
	{
		if (canGrabHookables)
			onGrab += HandleOnGrab;
		else
			onGrab -= HandleOnGrab;
	}

	public Action<GameObject> onGrab;

	public void Awake()
	{
		onGrab += HandleOnGrab;
	}

	public void ReleaseGrabbedObject()
	{
		if(grabbedObject != null)
		{
			grabbedObject.transform.SetParent(null);
			grabbedObject = null;
		}
	}

	private void HandleOnGrab(GameObject grabbedObject)
	{
		if(canGrabHookables)
		{
			this.grabbedObject = grabbedObject;
			this.grabbedObject.transform.parent = transform;
		}
	}
}
