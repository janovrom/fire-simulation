using janovrom.firesimulation.Runtime.Plants;
using System.Collections.Generic;
using UnityEngine;

namespace janovrom.firesimulation.Runtime.Renderers
{
    public abstract class Renderer : MonoBehaviour
    {

        protected List<Plant> _renderables;


        public abstract void Render();
        public virtual void Clear()
        {
            _renderables = new List<Plant>();
        }

        public void Register(Plant renderable)
        {
            if (renderable is null)
                return;

            _renderables.Add(renderable);
            OnRenderableAdded(renderable);
        }

        public void Unregister(Plant renderable)
        {
            if (renderable is null)
                return;

            _renderables.Remove(renderable);
            OnRenderableRemoved(renderable);
        }

        protected virtual void OnRenderableAdded(Plant renderable)
        {
        }

        protected virtual void OnRenderableRemoved(Plant renderable)
        {
        }

        private void Awake()
        {
            _renderables = new List<Plant>();
        }

    }

}
