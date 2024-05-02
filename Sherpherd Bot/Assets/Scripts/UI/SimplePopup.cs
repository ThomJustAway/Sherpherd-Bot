using Patterns;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;
using System;

//public class SimplePopup : MonoBehaviour
//{
//    [SerializeField] private Animator animator;
//    [SerializeField] private TextMeshProUGUI popUpText;
//    private bool isUsed;

//    private void Start()
//    {
//        EventManager.Instance.AddListener(TypeOfEvent.ShowPopUp, (AgentActions<string, int>)ShowMessage);
//    }

//    public void ShowMessage(string message, int duration)
//    {
//        if (!isUsed)
//        {
//            isUsed = true;
//            popUpText.text = message.Trim();
//            SoundManager.Instance.PlayAudio(SFXClip.Notification);
//            StartCoroutine(CoroutineForPopUp(duration));
//        }

//        //else ignore the message for now.
//    }

//    private IEnumerator CoroutineForPopUp(int duration)
//    {
//        animator.SetBool("Activated", true);
//        yield return new WaitForSeconds(duration);
//        animator.SetBool("Activated", false);
//        isUsed = false;
//    }

//}