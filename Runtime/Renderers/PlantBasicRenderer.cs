using Janovrom.Firesimulation.Runtime.Plants;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Renderers
{

    public class PlantBasicRenderer : Renderer
    {

        public Material PlantMaterial;
        public Material OnFireMaterial;
        public Material BurnedDownMaterial;

        public override void Clear()
        {
            base.Clear();
        }

        public override void NotifyStateChange(Plant plant)
        {
            var renderer = plant.GetComponent<MeshRenderer>();
            if (renderer is null)
                return;

            switch (plant.State)
            {
                case State.Normal:
                    renderer.sharedMaterial = PlantMaterial;
                    break;
                case State.OnFire:
                    renderer.sharedMaterial = OnFireMaterial;
                    break;
                case State.Burned:
                    renderer.sharedMaterial = BurnedDownMaterial;
                    break;
            };
        }

        public override void Render()
        {
        }

        protected override void OnRenderableAdded(Plant renderable)
        {
            base.OnRenderableAdded(renderable);
            NotifyStateChange(renderable);
        }

    }
}
