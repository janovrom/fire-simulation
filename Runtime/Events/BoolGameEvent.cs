using System.Collections.Generic;
using UnityEngine;


namespace Janovrom.Firesimulation.Runtime.Events
{

    [CreateAssetMenu(fileName ="NewBoolGameEvent", menuName = "FireSimulation/Events/Bool Game Event")]
    public class BoolGameEvent : ScriptableObject
    {

        [SerializeField]
        private List<BoolGameEventListener> _listeners = new List<BoolGameEventListener>();


        public void AddListener(BoolGameEventListener listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(BoolGameEventListener listener)
        {
            _listeners.Remove(listener);
        }

        public void Invoke(bool arg)
        {
            for (int i = _listeners.Count - 1; i >= 0; --i)
            {
                _listeners[i].OnEventRaised(arg);
            }
        }
    }

}