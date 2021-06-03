using UnityEngine;
using UnityEngine.Events;

namespace Janovrom.Firesimulation.Runtime.Events
{

    public class GameEventListener : MonoBehaviour
    {

        public GameEvent gameEvent;
        public UnityEvent actionTaken;


        private void OnEnable()
        {
            gameEvent.AddListener(this);
        }

        private void OnDisable()
        {
            gameEvent.RemoveListener(this);
        }

        public virtual void OnEventRaised()
        {
            actionTaken.Invoke();
        }

    }

}
