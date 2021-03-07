using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Hookable : NetworkBehaviour
{

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Hook"))
		{
			var hookTip = other.gameObject.GetComponent<HookTip>();

			if(hasAuthority)
			{
				CmdGrabObject(hookTip);
			}
		}
	}

	[Command]
	private void CmdGrabObject(HookTip hookTip)
	{
			if (hookTip == null)
				return;

			//Dont hook yourself
			if (GetComponent<PlayerController>().playerGuid == hookTip.playerGuid)
				return;

			if (hookTip.canGrabHookables)
				Debug.Log("Grabbing object");
				hookTip.GrabObject(gameObject);
	}
}
