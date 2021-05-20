using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace janovrom.firesimulation.Runtime.Renderers
{
    public abstract class Renderer<T>
    {

        protected List<T> _renderables = new List<T>();


        public abstract void Render();

        public void Register(T renderable)
        {
            _renderables.Add(renderable);
            OnRenderableAdded(renderable);
        }

        public void Unregister(T renderable)
        {
            _renderables.Remove(renderable);
            OnRenderableRemoved(renderable);
        }

        protected virtual void OnRenderableAdded(T renderable)
        {
        }

        protected virtual void OnRenderableRemoved(T renderable)
        {
        }

    }

}
