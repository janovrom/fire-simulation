using Janovrom.Firesimulation.Runtime.Plants;
using UnityEngine;

namespace Janovrom.Firesimulation.Runtime.Renderers
{

    /// <summary>
    /// <inheritdoc/>
    /// <br/>
    /// Simple renderer of plants. It assumes each game object
    /// with <see cref="Plant"/> component also contains (itself
    /// or in children) a MeshRenderer. It can then set the sharedMaterial
    /// based on the plant's state. This renderer supports three
    /// different states - burning, burned, and normal. For best performance
    /// it is suggested to enable GPU instancing on each material.
    /// </summary>
    public class PlantBasicRenderer : Renderer
    {

        public Material PlantMaterial;
        public Material OnFireMaterial;
        public Material BurnedDownMaterial;

        public override void Clear()
        {
            base.Clear();
        }

        /// <summary>
        /// <inheritdoc/><br/>
        /// Gets MeshRenderer in component or in children and sets its
        /// sharedMaterial based on the plant's state.
        /// </summary>
        /// <param name="plant"></param>
        public override void NotifyStateChange(Plant plant)
        {
            var renderer = plant.GetComponentInChildren<MeshRenderer>();
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

        /// <summary>
        /// All is handled in <see cref="NotifyStateChange(Plant)"/>.
        /// </summary>
        public override void Render()
        {
        }

        /// <summary>
        /// <inheritdoc/><br/>
        /// When plant is added, it notifies about the state change (also
        /// serves as initialization) by calling <see cref="NotifyStateChange(Plant)"/>.
        /// </summary>
        /// <param name="renderable"></param>
        protected override void OnRenderableAdded(Plant renderable)
        {
            base.OnRenderableAdded(renderable);
            NotifyStateChange(renderable);
        }

    }
}
