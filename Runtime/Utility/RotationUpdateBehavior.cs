using Janovrom.Firesimulation.Runtime.Variables;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Utility
{

    [ExecuteInEditMode]
    public class RotationUpdateBehavior : MonoBehaviour
    {

        public FloatVariable AngleNormalized;

        private void LateUpdate()
        {
            transform.rotation = Quaternion.AngleAxis(-AngleNormalized * 360f, Vector3.up);
        }

    } 
}
