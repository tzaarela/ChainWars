using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookable : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log("trying to hook");
		if(collision.gameObject.CompareTag("Hook"))
		{
			var hookTip = collision.gameObject.GetComponent<HookTip>();
			hookTip.onGrab!.Invoke(gameObject);
		}
	}
}
