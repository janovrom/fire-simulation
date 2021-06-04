using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Utility
{
    public class ExitBehavior : MonoBehaviour
    {

        public void ExitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

    } 
}
