
# Spatial Grid Sensor for Unity ML-Agents
This is an experimental spatial grid sensor for the [Unity Machine Learning Agents Toolkit](https://github.com/Unity-Technologies/ml-agents). 

* [Concept](#Concept)  
* [Settings](#Settings)
* [Examples](#Examples)
* [Demo Video](https://youtu.be/9-VKcoASSX0)
<br/><br/>

![Overview](Sensor.png)
<br/><br/>

## Concept

The Spatial Grid Sensor can be used in addition or as an alternative to other agent sensors like cameras or raycasts. It detects colliders and maps their polar coordinates onto visual observations, using equirectangular projection. Add the [Spatial Grid Sensor Component](https://github.com/mbaske/grid-sensor/tree/master/Assets/Scripts/Sensors/SpatialGridSensor/SpatialGridSensorComponent.cs) to your agents for controlling their field of view and for setting various observation and encoding options.
<br/><br/>

## Settings 

### Buffer Size

The maximum number of colliders the sensor can detect at once.

### Observation Stack Size

The number of stacked observations. Enable stacking (set value > 1) if agents need to infer movement from observations.

### Compression Type

The compression type to use for the sensor, `PNG` or `None`.
<br/><br/>

### Layers

The layers used for detecting colliders.

### Tags

The tags used for filtering detected colliders.

### Channel Encoding

How to encode detection results as grid values. Together with the number of tags, this setting determines how many observation channels will be used.
- `Distances Only` - One channel per tag (distance).
- `One-Hot And Distances` - Two channels per tag (one-hot & distance).
- `One-Hot And Shortest Distance` - One channel per tag (one-hot) plus a single channel for the shortest distance measured, regardless of the tag.
<br/><br/>

### Detection Type

What to detect about a collider.
- `Position` - The position of the collider's transform.
- `Closest Point` - Point on the collider that's closest to the sensor.  
- `Shape` - A set of points roughly representing the collider's shape, including the closet point. 

If agents only need to be aware of collider positions, one of the first two options should be selected. 

The `Shape` option is preferable if agents require more detailed observations. Since the sensor might encounter all kinds of collider types (quads, spheres, meshes, concave, convex, etc.), I didn't try to extract and map their vertices. Instead, the detector scans each collider by performing a flood-fill, generating a set of points that is supposed to represent its shape. Use this with caution though, as it can impair performance quite a bit. Depending on collider sizes and **scan settings** below, this method can produce large numbers of points, all of which have to be converted to polar coordinates and mapped onto the grid repeatedly.

You can try setting a lower scan resolution, creating less points and apply **pixel blurring** to fill out the gaps. Strong blurring also causes some overhead however - I recommend tweaking scan and blur values together, while checking how they impact performance.

### Scan Resolution

The distance between points that represent a collider's shape. Lower values result in more detail and points.

### Scan Extent

The maximum axis-aligned point distance from the collider's center.

### Clear Cache On Reset

Whether to clear the collider shape cache on sensor reset at the end of each episode. Should be disabled if colliders don't change from one episode to the next. (Each collider is scanned once and then cached. Therefore multiple sensors detecting identical colliders shouldn't have different scan settings. If they do, then the first sensor decides how scanning is done and the settings of later sensors will be ignored.)

### Blur Strength

How strongly grid pixels should be blurred. This value is multiplied with a point's inverse distance, so that closer points are blurred more strongly. Blur Strength = 0 disables blurring.

### Blur Threshold

A cutoff value controlling how much of a blurred area is drawn onto the grid.
<br/><br/>

### Cell Arc

The arc angle of a single FOV grid cell in degrees. Determines the sensor resolution:  
`cell size at distance = PI * 2 * distance / (360 / cell arc)`  
The scene GUI wireframe shows grid cells at the maximum detection distance.
<br/><br/>

Use GUI handles or the settings below for constraining the sensor's field of view. Effective angles are rounded up depending on the `Cell Arc` value. Note that because of the projection of polar coordinates to grid pixels, positions near the poles appear increasingly distorted. If that becomes an issue, you can try adding multiple sensors with smaller FOVs and point them in different directions.

### Lat Angle North

The FOV's northern latitude (up) angle in degrees.

### Lat Angle South

The FOV's southern latitude (down) angle in degrees.

### Lon Angle

The FOV's longitude (left & right) angle in degrees.

### Min Distance

The minimum detection distance (near clipping).

### Max Distance

The maximum detection distance (far clipping).
<br/><br/>

### Distance Normalization

How to normalize distances values, `Linear` or `Weighted`. Use `Weighted` if observing distance changes at close range is more critical to agents than what happens farther away.

### Normalization Weight

Curvature strength applied to `Weighted` normalization.
<br/><br/>

## Examples

### Dogfight

Agents control spaceships flying through an asteroid field, using discrete actions for throttle, pitch and roll. Each agent observes its surroundings using two spatial grid sensors. A front-facing sensor detects the distances and shapes of asteroids and other ships, adding a stacked observation to indicate movement. An omnidirectional long-range sensor only detects other ships' positions. Agents are rewarded for speed and penalized for collisions. While an agent follows another one, it receives rewards inversely proportional to the distance and angle between ships, while the opponent is penalized accordingly.

### Driver

Similar to Dogfight, the agent uses discrete actions for driving a car down a procedurally generated road. It detects roadside poles and various obstacles with two sensors. Now, a front-facing long-range sensor enables the agent to look ahead while an omnidirectional short-range sensor helps with evading obstacles. Again, the agent is rewarded for speed and penalized for collisions.
