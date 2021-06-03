using System.Collections.Generic;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Events
{

    [CreateAssetMenu(fileName ="NewGameEvent", menuName ="FireSimulation/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {

        [SerializeField]
        private List<GameEventListener> _listeners = new List<GameEventListener>();


        public void AddListener(GameEventListener listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(GameEventListener listener)
        {
            _listeners.Remove(listener);
        }

        public void Invoke()
        {
            for (int i = _listeners.Count - 1; i >= 0; --i)
            {
                _listeners[i].OnEventRaised();
            }
        }
    }

}