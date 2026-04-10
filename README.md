# SimpleDetection

A high-performance object detection system for Unity that enables spatial awareness using geometric detection zones with raycast-based line-of-sight validation. Supports both 2D and 3D detection scenarios with configurable shapes including circles, spheres, and frustums.

## Features

- **Multiple Detection Shapes**: Circle (2D), Sphere (3D), Frustum (2D & 3D vision cones)
- **Line-of-Sight Detection**: Raycast integration to verify visibility with obstacle support
- **Ghost Detection**: Optional support for detecting objects that fail detection checks without line-of-sight
- **Performance Optimized**: Uses Unity Burst compilation and high-performance collections
- **Gizmo Visualization**: Real-time debugging and editor visualization of detection zones and states
- **State-Based Detection**: Support for conditional detection based on object state

## Requirements

- **Unity Version**: 2022.2 or later (for Burst compilation and modern APIs)
- **Dependencies**:
  - `Unity.Burst` (Burst compiler for performance)
  - `Unity.Collections` (UnsafeList and memory management)
  - `Unity.Mathematics` (Float3 math types)
  - `SimpleCore` (Operation result handling and core systems)

## Quick Start

### Basic Setup

1. **Create a Detectable Object**

```csharp
using Systems.SimpleDetection.Components.Objects.Abstract;
using UnityEngine;

public class Player : DetectableObjectBase
{
    // Called every FixedUpdate while detected
    protected internal override void OnDetected(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        Debug.Log($"Detected by {context.detector.name}");
    }

    // Called every FixedUpdate while not detected
    protected internal override void OnObjectDetectionFailed(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        Debug.Log($"Not detected by {context.detector.name}");
    }
}
```

For detection enter/exit events rather than per-frame callbacks, use `DetectableObjectWithStatesBase`:

```csharp
public class Player : DetectableObjectWithStatesBase
{
    protected override void OnDetectionStartAsDetected() => Debug.Log("Detection started");
    protected override void OnDetectionEndAsDetected()   => Debug.Log("Detection ended");
    protected override void OnStayAsDetected()           => Debug.Log("Still detected");
}
```

2. **Create a Detector**

```csharp
using Systems.SimpleDetection.Components.Detectors.Base;

public sealed class EnemyDetector : Circle2DDetector
{
    // Configure radius in Inspector

    protected override void OnObjectDetected(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        Debug.Log($"Detected {context.detectableObject.name}");
    }

    protected override void OnObjectDetectionStart(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        Debug.Log($"{context.detectableObject.name} entered detection zone");
    }

    protected override void OnObjectDetectionEnd(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        Debug.Log($"{context.detectableObject.name} left detection zone");
    }
}
```

3. **Assign Components**
   - Add your `DetectableObjectBase` subclass to objects you want to detect (e.g., Player)
   - Add your `ObjectDetectorBase` subclass to detectors (e.g., Enemy, Sensor)
   - Configure detection parameters (radius, angle, etc.) in the Inspector
   - Set the `Raycast Layer Mask` on the detector to include obstacle layers

## Detection Zones

### 2D Detection Zones

**Circle2DDetector**
- Detects objects within a circular area
- Parameter: `radius` (detection radius in units)
- Use case: Basic range detection, proximity sensors

**Frustum2DDetector**
- Detects objects within a 2D vision cone (bird's eye view)
- Parameters: `angle` (field of view in degrees), `radius` (detection range)
- Use case: Enemy vision, AI perception with direction awareness

### 3D Detection Zones

**Sphere3DDetector**
- Detects objects within a spherical volume
- Parameter: `radius` (detection radius in units)
- Use case: 3D proximity detection, sound/vibration sensors

**Frustum3DDetector**
- Detects objects within a 3D perspective frustum (camera-like vision)
- Parameters: `angle` (horizontal field of view in degrees), `aspectRatio` (width/height), `nearPlaneDistance`, `farPlaneDistance`
- Use case: Character vision with realistic viewing frustum, camera-based detection

## Advanced Usage

### Ghost Detection

Ghost detection lets a detector physically spot an object that is not fully detectable (e.g., a stealthed player). To use it:

1. The **detector** must implement `ISupportGhostDetection`. Without this marker interface, objects that fail `CanBeDetected()` are skipped entirely.
2. The **detectable object** must return `DetectionOperations.IsGhost()` from `CanBeDetected()`.

The simplest way to mark an object as a ghost is the built-in `IsGhost` serialized property on `DetectableObjectBase`. For dynamic control, override `CanBeDetected()`:

```csharp
using Systems.SimpleDetection.Components.Detectors.Markers;
using Systems.SimpleDetection.Components.Detectors.Base;

// Detector must implement ISupportGhostDetection to process ghost objects
public sealed class AlertDetector : Circle2DDetector, ISupportGhostDetection
{
    protected override void OnObjectGhostDetected(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        Debug.Log($"Ghost spotted: {context.detectableObject.name}");
    }
}
```

```csharp
using Systems.SimpleDetection.Components.Objects.Abstract;

public class StealthPlayer : DetectableObjectBase
{
    private bool _isStealthed;

    protected internal override OperationResult CanBeDetected(ObjectDetectionContext context)
    {
        if (_isStealthed)
            return DetectionOperations.IsGhost();   // Physically seen but marked as ghost
        return DetectionOperations.Permitted();      // Normal detection
    }

    protected internal override void OnObjectGhostDetected(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        // Detector spotted us but we are in ghost state
    }
}
```

### Querying Detected Objects

```csharp
public sealed class CustomDetector : Circle2DDetector
{
    // Access detected objects
    public void LogDetectedObjects()
    {
        foreach (var obj in DetectedObjects)
        {
            Debug.Log($"Currently detecting: {obj.name}");
        }
    }

    // Check if a specific object is currently detected
    public bool IsSeeingPlayer(DetectableObjectBase player) => IsDetected(player);
}
```

On the detectable object side, `DetectedBy` lists all detectors currently seeing it (including ghosts):

```csharp
public class Player : DetectableObjectBase
{
    public bool IsSeenByAnyone => DetectedBy.Count > 0;
}
```

### Multiple Detection Points

Override `UpdateDetectionPositions()` to support objects with multiple detection points. The detector uses an early-exit: the object is considered detected as soon as any single point is seen.

```csharp
public class Vehicle : DetectableObjectBase
{
    [SerializeField] private Transform[] checkPoints;

    protected internal override int GetDetectionPositionsCount() => checkPoints.Length;

    protected internal override void UpdateDetectionPositions()
    {
        DetectionPositions.Clear();
        foreach (var point in checkPoints)
        {
            DetectionPositions.Add(point.position);
        }
    }
}
```

## Settings & Debugging

### Detection Settings

Configure global detection settings via `DetectionSettings.Instance` (auto-created ScriptableObject under `Assets/Resources/DetectionSettings.asset`):

- `drawDetectionPoints`: Enable/disable per-object detection point spheres
- `detectionPointRadius`: Size of debug spheres (0.05–1.0 units)
- `gizmosDrawModeForDetectors`: `Selected` (only selected detectors) or `Always`
- Gizmo colors for each detection state (all configurable)

### Gizmo Visualization

The system draws real-time gizmos in the Scene view. Default colors:

| Color | Meaning |
|-------|---------|
| Blue  | Object is outside the detection zone |
| Red   | Object is inside the zone and detected (line-of-sight clear, `CanBeDetected` passes) |
| Green | Object is inside the zone but line-of-sight is blocked |
| Yellow | Object is inside the zone, line-of-sight is clear, but `CanBeDetected` fails (ghost) |

All colors are customizable in `DetectionSettings`.

## Performance Considerations

- Detection runs in `FixedUpdate()` for consistent frame-rate independent behavior
- Uses Unity Burst compilation for high-performance zone calculations
- `UnsafeList<float3>` for efficient detection point storage
- Only raycasts for points inside the detection zone
- Early-exit optimization when first detection point is confirmed visible

## License

MIT License - See LICENSE.md for details
