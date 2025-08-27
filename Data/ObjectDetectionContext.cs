using JetBrains.Annotations;
using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Objects.Abstract;

namespace Systems.SimpleDetection.Data
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