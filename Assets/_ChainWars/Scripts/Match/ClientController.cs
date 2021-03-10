using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientController : NetworkBehaviour
{
	public static ClientController Instance { get; protected set; }

	 private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}
}
