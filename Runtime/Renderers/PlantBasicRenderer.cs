using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace janovrom.firesimulation.Runtime.Renderers
{
    public class PlantBasicRenderer : Renderer<Plants.Plant>
    {

        protected override void OnRenderableAdded(Plants.Plant renderable)
        {
            base.OnRenderableAdded(renderable);
            renderable.gameObject.AddComponent();
        }

        public override void Render()
        {
            foreach (var renderable in _renderables)
            {
                // Render based on state
            }
        }
    }
}
