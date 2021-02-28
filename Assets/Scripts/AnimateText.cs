using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimateText : MonoBehaviour
{
	public TMPro.TextMeshProUGUI text;
	public TMPro.TextMeshProUGUI input;

	private void Awake()
	{
		if (text == null)
			text = GetComponent<TMPro.TextMeshProUGUI>();
	}
}
