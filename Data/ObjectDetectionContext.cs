using JetBrains.Annotations;
using Systems.Detection2D.Components.Detectors.Abstract;
using Systems.Detection2D.Components.Objects.Abstract;

namespace Systems.Detection2D.Data
{
    /// <summary>
    ///     Context for object detection
    /// </summary>
    public readonly ref struct ObjectDetectionContext
    {
        [NotNull] public readonly DetectableObjectBase detectableObject;
        [NotNull] public readonly ObjectDetectorBase detector;

        public ObjectDetectionContext(
            [NotNull] DetectableObjectBase detectableObject,
            [NotNull] ObjectDetectorBase detector)
        {
            this.detector = detector;
            this.detectableObject = detectableObject;
        }
    }
}