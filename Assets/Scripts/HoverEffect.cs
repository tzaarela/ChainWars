using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HoverEffect : MonoBehaviour
{
	[Range(0, 10)]
	private float scaleEndValue = 1.2f;
	private float duration = 2f;

	public void Start()
	{
	}

	private void OnMouseEnter()
	{
		transform.DOScale(scaleEndValue, duration).SetEase(Ease.InBack);
	}
}
