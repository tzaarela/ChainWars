using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class LobbyPanel : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
{
	[HideInInspector] public TextMeshProUGUI lobbyName;
	[HideInInspector] public string lobbyId;
	[HideInInspector] public bool isSelected;

	public Action<LobbyPanel> onLobbySelected;

	[SerializeField] private Color highlightColor;
	[SerializeField] private Color selectedColor;
	[SerializeField] private Image background;

	private Color defaultColor;

	private void Start()
	{
		defaultColor = background.color;
	}

	public void SelectPanel()
	{
		isSelected = true;
		background.color = selectedColor;
	}

	public void DeselectPanel()
	{
		isSelected = false;
		background.color = defaultColor;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		onLobbySelected(this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (isSelected)
			background.color = selectedColor;
		else
			background.color = defaultColor;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (isSelected)
			background.color = selectedColor;
		else
			background.color = highlightColor;
	}
}
