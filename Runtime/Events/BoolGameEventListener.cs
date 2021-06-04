using UnityEngine;


namespace Janovrom.Firesimulation.Runtime.Events
{

    public class BoolGameEventListener : MonoBehaviour
    {

        public BoolGameEvent gameEvent;
        public BoolEvent actionTaken;


        private void OnEnable()
        {
            gameEvent.AddListener(this);
        }

        private void OnDisable()
        {
            gameEvent.RemoveListener(this);
        }

        public virtual void OnEventRaised(bool arg)
        {
            actionTaken.Invoke(arg);
        }

    }

}
