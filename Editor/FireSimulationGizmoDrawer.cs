using Janovrom.Firesimulation.Runtime.Simulation;
using UnityEditor;
using UnityEngine;

namespace Janovrom.Firesimulation.Editor
{
    public class FireSimulationGizmoDrawer
    {

        internal static bool EnableGizmo = true;

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmoForFireSimulation(FireSimulation simulation, GizmoType gizmoType)
        {
            Gizmos.DrawWireSphere(simulation.SimulationBounds.center, 1f);
            Gizmos.DrawWireCube(simulation.SimulationBounds.center, simulation.SimulationBounds.size);

            if (!EnableGizmo)
                return;

            float dx = simulation.SimulationBounds.size.x / simulation.ResolutionX;
            float dz = simulation.SimulationBounds.size.z / simulation.ResolutionY;
            var cellOffset = new Vector3(dx * 0.5f, simulation.SimulationBounds.size.y * 0.5f, dz * 0.5f);
            for (int x = 0; x < simulation.ResolutionX; ++x)
            {
                for (int y = 0; y < simulation.ResolutionY; ++y)
                {
                    var center = simulation.SimulationBounds.min + cellOffset + new Vector3(x * dx, 0f, y * dz);
                    Gizmos.DrawWireCube(center, new Vector3(dx, 10f, dz));
                }
            }
        }

    }
}
