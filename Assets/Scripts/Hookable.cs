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
				Debug.Log("Grabbing object");
				CmdGrabObject(hookTip);
			}
		}
	}

	[Command]
	private void CmdGrabObject(HookTip hookTip)
	{
			//Dont hook yourself
			if (GetComponent<PlayerHandler>().playerGuid == hookTip.playerGuid)
				return;

			if (hookTip.canGrabHookables)
				hookTip.GrabObject(gameObject);
	}
}
