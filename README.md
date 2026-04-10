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
    // Optional: Override to provide custom detection behavior
    protected internal override void OnDetected(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        Debug.Log($"Detected by {context.detector.name}");
    }

    protected internal override void OnObjectDetectionFailed(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        Debug.Log($"Lost detection from {context.detector.name}");
    }
}
```

2. **Create a Detector**

```csharp
using Systems.SimpleDetection.Components.Detectors.Base;

public sealed class PlayerDetector : Circle2DDetector
{
    // Inherit from Circle2DDetector for 2D circular detection
    // Configure radius in Inspector
    
    protected override void OnObjectDetected(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        Debug.Log($"Detected {context.detectableObject.name}");
    }
}
```

3. **Assign Components**
   - Add your `DetectableObjectBase` subclass to objects you want to detect (e.g., Player)
   - Add your `ObjectDetectorBase` subclass to detectors (e.g., Enemy, Sensor)
   - Configure detection parameters (radius, angle, etc.) in the Inspector

## Detection Zones

### 2D Detection Zones

**Circle2DDetector**
- Detects objects within a circular area
- Parameter: `radius` (detection radius in units)
- Use case: Basic range detection, proximity sensors

**Frustum2DDetector**
- Detects objects within a 2D vision cone (bird's eye view)
- Parameters: `angle` (field of view), `distance` (detection range)
- Use case: Enemy vision, AI perception with direction awareness

### 3D Detection Zones

**Sphere3DDetector**
- Detects objects within a spherical volume
- Parameter: `radius` (detection radius in units)
- Use case: 3D proximity detection, sound/vibration sensors

**Frustum3DDetector**
- Detects objects within a 3D perspective frustum (camera-like vision)
- Parameters: `angle` (vertical FOV), `aspectRatio` (width/height), `nearPlaneDistance`, `farPlaneDistance`
- Use case: Character vision with realistic viewing frustum, camera-based detection

## Advanced Usage

### Ghost Detection

Objects that fail the `CanBeDetected()` check can still be "ghost detected" (visually spotted but not actually detected):

```csharp
public class StealthPlayer : DetectableObjectBase
{
    private bool isStealthed = false;

    protected internal override OperationResult CanBeDetected(ObjectDetectionContext context)
    {
        if (isStealthed)
            return DetectionOperations.IsGhost();  // Visually seen but marked as ghost
        return DetectionOperations.Permitted();    // Normal detection
    }

    protected internal override void OnObjectGhostDetected(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        // Handle ghost detection (e.g., partial information to detector)
    }
}
```

### Detector Configuration

```csharp
public sealed class CustomDetector : Circle2DDetector
{
    protected override void OnObjectDetectionStart(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        // Called when an object first enters detection
    }

    protected override void OnObjectDetectionEnd(in ObjectDetectionContext context, in OperationResult detectionResult)
    {
        // Called when an object leaves detection
    }

    // Access detected objects
    public void LogDetectedObjects()
    {
        foreach (var obj in DetectedObjects)
        {
            Debug.Log($"Currently detecting: {obj.name}");
        }
    }
}
```

### Multiple Detection Points

Override `UpdateDetectionPositions()` to support objects with multiple detection points:

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

Access global detection settings via `DetectionSettings.Instance`:

- `drawDetectionPoints`: Enable/disable point rendering
- `gizmosDrawModeForDetectors`: Control when detection zones display
- Gizmo colors for different detection states (detected, ghost, obstructed, outside)
- `detectionPointRadius`: Size of debug spheres

### Gizmo Visualization

The system provides real-time gizmo visualization:
- **Green**: Objects detected and visible
- **Yellow**: Objects visible but ghost-detected
- **Red**: Objects inside zone but line-of-sight blocked
- **Gray**: Objects outside detection zone

## Performance Considerations

- Detection runs in `FixedUpdate()` for consistent frame-rate independent behavior
- Uses Unity Burst compilation for high-performance zone calculations
- `UnsafeList<float3>` for efficient detection point storage
- Only raycasts for points inside the detection zone
- Early-exit optimization when first point is detected

## License

MIT License - See LICENSE.md for details
