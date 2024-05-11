using Assets.Scripts.Shepherd.GOAP;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Script that contain the barn behaviour.
/// </summary>
public class Barn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private int moneyCollected;
    public int MoneyCollected 
    { 
        get { return moneyCollected; }
        private set 
        { 
            moneyCollected = value;
            text.text = $"{moneyCollected}";
        } 
    }

    private void Start()
    {
        MoneyCollected = 0;
    }
    //If the shepherd wants to sell the fur. the shepherd would would earn money.
    public void SellFur(Shepherd shepherd)
    {
        int amount = shepherd.WoolAmount;
        shepherd.ResetWool();
        MoneyCollected += amount;
    }
}
