using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MouseHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Range(0, 10)]
	[SerializeField] private float scaleEndValue = 1.05f;
	[SerializeField] private float duration = 0.2f;

	public void OnPointerEnter(PointerEventData eventData)
	{
		transform.DOScale(scaleEndValue, duration).SetEase(Ease.OutSine);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		transform.DOScale(1, duration).SetEase(Ease.InSine);
	}
}
