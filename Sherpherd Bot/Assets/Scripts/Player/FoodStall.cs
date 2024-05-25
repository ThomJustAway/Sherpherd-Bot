using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class FoodStall : MonoBehaviour
    {
        PlayerInteractions player;
        [SerializeField] PlayerUI ui;
        private void OnTriggerEnter(Collider collision)
        {
            player = collision.gameObject.GetComponent<PlayerInteractions>();
        }

        private void Update()
        {
            if(player == null) return;
            if (!player.HasFood)
            {
                ui.ShowCollectFoodMessage();
                print("stay");
                if (Input.GetKeyDown(KeyCode.E))
                {
                    player.GetFood();
                }
            }
            else
            {
                ui.HideMessage();
            }
        }

        private void OnTriggerExit(Collider collision)
        {
            player = null;  
        }
    }
}