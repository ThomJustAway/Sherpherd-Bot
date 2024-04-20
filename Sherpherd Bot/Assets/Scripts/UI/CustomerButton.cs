using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
	//this is when the player hover on a button, it will play a hover clip.
	//public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
	//{
	//	[SerializeField] private SFXClip clipToPlayWhenClick;
	//	[SerializeField] private SFXClip clipToPlayWhenHovered;
	//	public UnityEvent actions;
	//	//when the player hover on the button, play the hover clip
	//	public void OnPointerEnter(PointerEventData eventData)
	//	{
	//		SoundManager.Instance.PlayAudio(clipToPlayWhenHovered);
	//	}

	//	//this is special case for the button for the connecting button.
	//	public void ClickSound()
	//	{
	//		SoundManager.Instance.PlayAudio(clipToPlayWhenClick);
	//	}

	//	public void OnPointerDown(PointerEventData eventData)
	//	{
	//		ClickSound();
	//		StartCoroutine(PlayingButtonBeforeActions());
	//	}

	//	private IEnumerator PlayingButtonBeforeActions()
	//	{
	//		yield return new WaitForSecondsRealtime(1);
	//		actions?.Invoke();
	//	}
	//}
}