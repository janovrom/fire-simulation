using janovrom.firesimulation.Runtime.Plants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace janovrom.firesimulation.Runtime.Renderers
{

    public class PlantBasicRenderer : Renderer
    {

        public Material PlantMaterial;
        public Material OnFireMaterial;
        public Material BurnedDownMaterial;

        private readonly List<MeshRenderer> _renderers = new List<MeshRenderer>();

        public override void Clear()
        {
            base.Clear();
            _renderers.Clear();
        }

        protected override void OnRenderableAdded(Plants.Plant renderable)
        {
            base.OnRenderableAdded(renderable);
            var renderer = renderable.gameObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = PlantMaterial;
            _renderers.Add(renderer);
        }

        protected override void OnRenderableRemoved(Plant renderable)
        {
            base.OnRenderableRemoved(renderable);
            _renderers.Remove(renderable.GetComponent<MeshRenderer>());
        }

        public override void Render()
        {
            for (int i = 0; i < _renderables.Count; ++i)
            {
                Material newMaterial = null;
                switch(_renderables[i].State)
                {
                    case State.Normal:
                        newMaterial = PlantMaterial;
                        break;
                    case State.OnFire:
                        newMaterial = OnFireMaterial;
                        break;
                    case State.Burned:
                        newMaterial = BurnedDownMaterial;
                        break;
                };

                if (_renderers[i].sharedMaterial != newMaterial)
                {
                    _renderers[i].sharedMaterial = newMaterial;
                }
            }
        }
    }
}
