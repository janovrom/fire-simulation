using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Variables
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

        public static implicit operator T(Variable<T> variable) => variable.Value;

        private void OnEnable()
        {
            Value = _defaultVariable;
        }

    }


}

