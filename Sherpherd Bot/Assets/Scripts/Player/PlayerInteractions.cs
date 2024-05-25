using Assets.Scripts.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] private PlayerUI ui;
    [SerializeField] private float interactableRange;
    //food related
    private bool hasFood;
    //[SerializeField] private Transform foodStall;
    [SerializeField] private GameObject food;
    public bool HasFood { get => hasFood ; private set {
            hasFood = value;
            food.SetActive(hasFood);
    } }

    public float InteractableRange { get => interactableRange; set => interactableRange = value; }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, InteractableRange);
    }

    public void GetFood()
    {
        HasFood = true;
    }

    public void GiveFood()
    {
        HasFood = false;
    }
}
