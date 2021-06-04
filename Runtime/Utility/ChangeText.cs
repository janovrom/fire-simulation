using UnityEngine;
using UnityEngine.UI;

namespace Janovrom.Firesimulation.Runtime.Utility
{

    public class ChangeText : MonoBehaviour
    {

        public Text Text;
        public string[] Texts;
        public int CurrentText = 0;

        public void CycleText()
        {
            CurrentText = (CurrentText + 1) % Texts.Length;
            Text.text = Texts[CurrentText];
        }

    }

}

