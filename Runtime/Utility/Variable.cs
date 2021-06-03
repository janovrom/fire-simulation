using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Utility
{

    public abstract class Variable<T> : ScriptableObject, ISerializationCallbackReceiver
    {

        [SerializeField]
        private T _defaultVariable = default;
        public T Value;

        public void OnAfterDeserialize()
        {
            // Called during instantiation for example
            Value = _defaultVariable;
        }

        public void OnBeforeSerialize()
        {
        }

        public void SetValue(T value)
        {
            Value = value;
        }

        private void OnEnable()
        {
            Value = _defaultVariable;
        }

    }


}

