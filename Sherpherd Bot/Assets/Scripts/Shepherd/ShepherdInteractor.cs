using Assets.Scripts.Player;
using Assets.Scripts.Shepherd.GOAP;
using System.Collections;
using UnityEngine;

namespace GOAPTHOM
{
    public class ShepherdInteractor : MonoBehaviour
    {
        PlayerInteractions player;
        [SerializeField] Shepherd shepherd;
        [SerializeField] PlayerUI ui;
        private void OnTriggerEnter(Collider other)
        {
            player = other.gameObject.GetComponent<PlayerInteractions>();
        }

        private void Update()
        {
            if (player == null) return;
            if (player.HasFood)
            {
                ui.ShowGiveFoodMessage();
                if (Input.GetKeyDown(KeyCode.E))
                {
                    player.GiveFood();
                    shepherd.RecieveFood();
                }
            }
            else
            {
                ui.HideMessage();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            player = null;
            
        }
    }
}