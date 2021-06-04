using Janovrom.Firesimulation.Runtime.Events;
using System;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Bindings
{

    [CreateAssetMenu(fileName = "NewBoolBinding", menuName = "FireSimulation/Bindings/Bool Binding")]
    [Serializable]
    public class BoolBinding : ScriptableObject
    {

        [SerializeField]
        private bool _defaultValue = false;

        [SerializeField]
        private bool _runtimeValue;

        public bool Value
        {
            get
            {
                return _runtimeValue;
            }

            set
            {
                if (_runtimeValue != value)
                {
                    _runtimeValue = value;
                    ValueChangedEvent.Invoke(_runtimeValue);
                }
            }
        }

        public BoolGameEvent ValueChangedEvent;


        private void OnEnable()
        {
            _runtimeValue = _defaultValue;
            ValueChangedEvent.Invoke(_runtimeValue);
        }

    }

}
