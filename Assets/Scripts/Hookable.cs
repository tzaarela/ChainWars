using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Hookable : NetworkBehaviour
{
	public Guid guid;

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.CompareTag("Hook"))
		{

			Debug.Log("trying to hook");
			var hookTip = collision.gameObject.GetComponent<HookTip>();

			//Dont hook yourself
			if (guid == hookTip.guid)
				return;

			if(hookTip.canGrabHookables)
				hookTip.GrabObject(gameObject);
		}
	}
}
