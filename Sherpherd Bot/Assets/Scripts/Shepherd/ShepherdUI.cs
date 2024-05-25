using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShepherdUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject textboxTransform;
    [SerializeField] private Transform playerTransform;
    private IEnumerator coroutineBefore;
    private void Start()
    {
        SetTextBox();
    }
    private void Update()
    {
        //constantly face the player
        var targetDirection = playerTransform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(targetDirection);
    }

    public void SetTextBox(bool active = false, string text = "")
    {
        textboxTransform.SetActive(active);
        this.text.text = text;
    }

    public void SetTemporaryMessage(float timer, string text)
    {
        if(coroutineBefore != null) StopCoroutine(coroutineBefore);
        SetTextBox(true, text);
        coroutineBefore = TimerMessage(timer);
        StartCoroutine(coroutineBefore);
    }

    private IEnumerator TimerMessage(float timer)
    {
        float elapseTimer = 0f;
        while(elapseTimer < timer)
        {
            yield return null;
            print("running timer");
            elapseTimer += Time.deltaTime;
        }
        SetTextBox();
    }
}
