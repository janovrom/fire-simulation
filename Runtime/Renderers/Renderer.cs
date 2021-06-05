using Janovrom.Firesimulation.Runtime.Plants;
using System.Collections.Generic;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Renderers
{

    /// <summary>
    /// Base behavior class for plant rendering. It can be notified
    /// by a plant's change. When notified, the renderer updates
    /// the visible state. Each plant should be registered using
    /// <see cref="Register(Plant)"/> since the implementation can
    /// cache some data related to rendering.
    /// </summary>
    public abstract class Renderer : MonoBehaviour
    {

        protected List<Plant> _renderables;

        /// <summary>
        /// Updates the state of all plants if needed.
        /// </summary>
        public abstract void Render();

        /// <summary>
        /// Clears the list of registered plants. This method 
        /// should be overriden when additional resources are
        /// stored.
        /// </summary>
        public virtual void Clear()
        {
            _renderables = new List<Plant>();
        }

        /// <summary>
        /// Stores the plant in a list and invokes virtual
        /// <see cref="Renderer.OnRenderableAdded(Plant)"/>.
        /// </summary>
        /// <param name="renderable"></param>
        public void Register(Plant renderable)
        {
            if (renderable is null)
                return;

            _renderables.Add(renderable);
            OnRenderableAdded(renderable);
        }
        
        /// <summary>
        /// Removes the plant from a list and invokes virtual
        /// <see cref="Renderer.OnRenderableRemoved(Plant)"/>.
        /// </summary>
        /// <param name="renderable"></param>
        public void Unregister(Plant renderable)
        {
            if (renderable is null)
                return;

            _renderables.Remove(renderable);
            OnRenderableRemoved(renderable);
        }

        public abstract void NotifyStateChange(Plant plant);

        /// <summary>
        /// Override to implement additional functionality (i.e. caching of data).
        /// </summary>
        /// <param name="renderable"></param>
        protected virtual void OnRenderableAdded(Plant renderable)
        {
        }

        /// <summary>
        /// Override to implement additional functionality (i.e. clearin of cached data).
        /// </summary>
        /// <param name="renderable"></param>
        protected virtual void OnRenderableRemoved(Plant renderable)
        {
        }

        private void Awake()
        {
            _renderables = new List<Plant>();
        }

    }

}
