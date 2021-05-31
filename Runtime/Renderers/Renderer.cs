using Janovrom.Firesimulation.Runtime.Plants;
using System.Collections.Generic;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Renderers
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

        public abstract void NotifyStateChange(Plant plant);

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
