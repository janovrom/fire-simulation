using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Utility
{

    public abstract class Variable<T> : ScriptableObject, ISerializationCallbackReceiver
    {

        [SerializeField]
        private T _defaultVariable = default;
        public T Value;


        private void OnEnable()
        {
            Value = _defaultVariable;
        }

        public void OnAfterDeserialize()
        {
            // Called during instantiation for example
            Value = _defaultVariable;
        }

        public void OnBeforeSerialize()
        {
        }
    }


}

