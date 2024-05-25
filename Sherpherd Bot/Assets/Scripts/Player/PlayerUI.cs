using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Player
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private GameObject textbox;
        [SerializeField] private TextMeshProUGUI text;

        public void ShowCollectFoodMessage()
        {
            SetTextBox("Press E to collect food", true);
        }

        public void HideMessage() 
        {
            SetTextBox("", false);
        }

        public void ShowGiveFoodMessage()
        {
            SetTextBox("Press E to give food", true);
        }


        private void SetTextBox(string text , bool active)
        {
            textbox.SetActive(active);
            this.text.text = text;
        }
    }
}