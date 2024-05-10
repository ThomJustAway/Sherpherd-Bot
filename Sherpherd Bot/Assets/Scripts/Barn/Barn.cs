using Assets.Scripts.Shepherd.GOAP;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    public void SellFur(Shepherd shepherd)
    {
        int amount = shepherd.WoolAmount;
        shepherd.SetWool(0);
        MoneyCollected += amount;
    }
}
