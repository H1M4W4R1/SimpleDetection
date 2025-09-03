<div align="center">
  <h1>SimpleDetection</h1>
  <img src="https://github.com/H1M4W4R1/Detection2D/blob/master/Images/Screenshot0000.png" alt="Preview screenshot"/>
</div>

# About

SimpleDetection is a Unity3D system that creates detection capabilities.

*For requirements check .asmdef*

# Usage

## Creating a stateless detectable object

To create a detectable object you need to create a custom object class that inherits from
`DetectableObjectBase` and implement all methods you find useful.

```csharp
   public sealed class DetectablePoint : DetectableObjectBase
   {
       
   }
```

Methods to check twice, if those should be implemented:

* `UpdateDetectionPositions()` - used to update list of positions to be verified for detection, defaults to object's
  `transform.position`; Allows implementation of complex detection logic in side-scroller games.
* `GetDetectionPositionsCount` - should match with above method.

Methods to check:

* `OnDetected(context)` - called each frame object is detected by a detector.
* `OnObjectGhostDetected(context)` - called each frame object is detected by a detector, but cannot be detected.
  Requires detector to support ghost detection (implement `ISupportGhostDetection` interface)
* `OnObjectDetectionFailed(context)` - called each frame object is not detected by a detector.
* `CanBeDetected(context)` - called to check if object can be detected by a detector.

## Creating a stated detectable object

You can also implement `DetectableObjectWithStatesBase` - a version of `DetectableObjectBase` that
supports states.

All available state callbacks:

* `OnStayAsGhostDetected()` - called each frame object is detected by a detector, but cannot be detected.
* `OnStayAsDetected()` - called each frame object is detected by a detector and can be detected.
* `OnStayAsAnyDetected()` - called each frame object is detected by a detector.
* `OnStayAsUndetected()` - called each frame object is not detected by a detector.
* `OnDetectionStartAsGhost()` - called when object starts being detected by a detector, but cannot be detected.
* `OnDetectionStartAsDetected()` - called when object starts being detected by a detector and can be detected.
* `OnDetectionStartAsAny()` - called when object starts being detected by a detector.
* `OnDetectionEndAsGhost()` - called when object stops being detected by a detector, but was not able to be detected by
  a detector.
* `OnDetectionEndAsDetected()` - called when object stops being detected by a detector and was able to be detected.
* `OnDetectionEndAsAny()` - called when object stops being detected by a detector.

## Creating a detector

To create a detector you need to create a custom detector class that inherits from
`ObjectDetectorBase` and implement all required methods and ones you find useful.

```csharp
    public class CircularDetector : ObjectDetectorBase
    {
        [SerializeField] private float radius = 2f;    

        protected override IDetectionZone GetDetectionZone()
        {
            float3 position = transform.position;
            return new CircularDetectionZone(position.xy, radius);
        }
    }
```

Required methods:

* `GetDetectionZone()` - returns detection zone used to detect objects, detection zone is described later.

Recommended methods:

* `OnObjectDetected(obj)` - called each frame object is detected by a detector.
* `OnObjectDetectionFailed(obj)` - called each frame object is not detected by a detector.
* `OnObjectGhostDetected(obj)` - called each frame object is detected by a detector, but cannot be detected.
* `OnObjectDetectionStart(obj)` - called when object starts being detected by a detector (either ghost or regular)
* `OnObjectDetectionEnd(obj)` - called when object stops being detected by a detector (both ghost or regular)
* `CanBeDetected(obj)` - called to check if specific object can be detected by a detector, recommended to limit
  detectable object types.

Note: you can access all objects detected by detector by using `DetectedObjects` property.

Note: to support ghost detection detector should implement `ISupportGhostDetection` interface, otherwise
ghosts will be considered as **undetected**.

### References

For reference, you can see two example detectors already implemented:

* `CircularDetector`
* `FrustumDetector`

Both can also be extended for custom implementations.

## Detection zone

Detection zone is a structure that defines area in which objects can be detected. It is used by detectors to check if
object is in zone and if point is not obstructed by other objects.

Zones should be `struct` and implement `IDetectionZone` with all remarks of that interface
implemented in your zone object.

For reference see `CircularDetectionZone.cs` or `FrustumDetectionZone.cs`.
