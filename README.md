# Fire simulation
This package contains scripts and some starter assets for simplified fire simulation. The simulation is physically inspired (though not correct). It uses heat radiation from fire sources, heat dissipation (to simulate heat rising), and wind simulation. Simplified model is used for trees - cube with differently colored material based on plant's state.

## Installation
As a unity package, simply import the package and samples if needed. Also consider creating layers for terrain and plants as it is possible to set these as masks for physics raycasts.
Unity package does not support layers but the value is stored in the serialized field anyway. When importing to new project, simply add 3 new layers:
- User Layer 8 - Terrain
- User Layer 9 - Plant
- User Layer 10 - Gizmo

## Package structure
Structure of the package adheres to Unity package [layout](https://docs.unity3d.com/Manual/cus-layout.html) with two samples and runtime assets.

### Assets
This folder contains:
- Assets created using ScriptableObject - events, variables, bindings
- Plant materials
- Plant prefab

### Samples
There are 3 different samples. Two contain a simulation setup on either a grid (Small fire) or a terrain (Large fire). The last sample is a UI that can be used to control the simulation using additive scene loading. All samples operate on the same shared event system.

### Naming conventions
Mostly uses the standard [conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) with some exceptions:
1. Public/internal starts with upper
2. Private/protected starts with _
3. Const/Readonly has first letter upper

## Simulation
The fire is simulated in a configurable grid. When started two grids are created; one is used as a look-up table for fire sources (heat radiation) and the other is used for the heat transfer (wind and heat convection). As the simulation is done in a grid, the heat spread can be modified by heat transfer speed (the speed depends on the bounds size and grid resolution, use this to tweak the values on windless nights).

The plant model is simplified. Instead of using mass, fire temperature, heat capacity, flashpoint temperuture, and other parameters needed for fire simulation, they share the same fire temperature of 1200° Celsius and flashpoint temperature of 600° Celsius. The heat capacity, mass, and heat transfer for raising the internal temperature of the object is replaced by simple look-up in the grid and if the temperature in the cell is larger than the flashpoint temperature, the plant is lit on fire. 

Object on fire radiates heat based on its mass, material type, surface area. The time it is on fire is given by the mass and mass consumed by the fire per second. This computation is replaced by a timer on each plant. The heat radiation is computed only for 8-neighborhood and is inversely proportional to the distance of grid cells.

The wind propagation computes the offset in the grid and moves portion of the heat in one direction. For the purposes of wind spreading and heat radiation, one cell padding is introduced on all sides of the grid. If there is no wind, linear dissipation is used to simulate the wind up-drift.

After all this dissipation is called again to simulate other effects like boundary conditions, or heat transfer to ground.

This marks the end of the simulation and only the state of the objects is udpated and send to render.

## Rendering
Simulation can be modified by creating a renderer. Currently only simple renderer is in place. It contains 3 materials with gpu instancing enabled. These 3 materials are set as sharedMaterial for the plants (cubes) based on their state. The state change is done on each notify and render call then does nothing (but could be used for instanced rendering for example).

## User interface
User interface uses concepts from XAML and MVVM - bindings and dependency properties, which are nothing less than application of observable pattern.

The interface provides two buttons to generate/clear the plants. Generating the plants automatically runs the simulation. It can be stopped by the toggle. Wind speed and wind direction can be modified by two sliders. Wind speed can reach from 0 to 100 m/s and the direction is a normalized value <0,1> converted to degrees and a direction vector. The direction is visualized by an arrow in bottom right corner. It is possible to add/remove/light a plant by selecting the mode in a dropdown.

Camera movement is very basic implementation for mouse zoom, drag. Camera also stores the current position under mouse in the world in a shared variable (used to place plants - should be removed as it does not bring any benefit except that it could be used for keyboard shortcuts).