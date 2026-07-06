using System.Collections.Generic;
using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Components.Detectors.Markers;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Systems.SimpleDetection.Components.Objects.Abstract;
using Systems.SimpleDetection.Data;
using Systems.SimpleDetection.Data.Enums;
using Systems.SimpleDetection.Operations;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.SimpleDetection.Tests
{
    public abstract class SimpleDetectionTestBase
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int index = _createdObjects.Count - 1; index >= 0; index--)
            {
                Object createdObject = _createdObjects[index];
                if (createdObject) Object.DestroyImmediate(createdObject);
            }

            _createdObjects.Clear();
        }

        protected TestDetectableObject CreateDetectableObject(string objectName, Vector3 position)
        {
            GameObject gameObject = Track(new GameObject(objectName));
            gameObject.transform.position = position;
            TestDetectableObject detectableObject = gameObject.AddComponent<TestDetectableObject>();
            return detectableObject;
        }

        protected TestStateDetectableObject CreateStateDetectableObject(string objectName, Vector3 position)
        {
            GameObject gameObject = Track(new GameObject(objectName));
            gameObject.transform.position = position;
            TestStateDetectableObject detectableObject = gameObject.AddComponent<TestStateDetectableObject>();
            return detectableObject;
        }

        protected TestObjectDetector CreateDetector(string objectName)
        {
            GameObject gameObject = Track(new GameObject(objectName));
            TestObjectDetector detector = gameObject.AddComponent<TestObjectDetector>();
            return detector;
        }

        protected TestGhostObjectDetector CreateGhostDetector(string objectName)
        {
            GameObject gameObject = Track(new GameObject(objectName));
            TestGhostObjectDetector detector = gameObject.AddComponent<TestGhostObjectDetector>();
            return detector;
        }

        protected TUnityObject Track<TUnityObject>(TUnityObject unityObject)
            where TUnityObject : Object
        {
            _createdObjects.Add(unityObject);
            return unityObject;
        }

        protected static void AssertSimilar(OperationResult expected, OperationResult actual)
        {
            Assert.IsTrue(
                OperationResult.AreSimilar(expected, actual),
                "Expected similar result to " + expected.resultCode + " but received " + actual.resultCode);
        }
    }

    public sealed class TestDetectionZone : IDetectionZone
    {
        public SpotResult Result { get; set; } = SpotResult.InsideSeen;
        public bool UseMinimumSeenX { get; set; }
        public float MinimumSeenX { get; set; }

        public bool IsPointInZone(in float3 detectionPosition)
        {
            return IsPointSeen(detectionPosition, 0) != SpotResult.Outside;
        }

        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask)
        {
            if (UseMinimumSeenX && detectionPosition.x < MinimumSeenX) return SpotResult.Outside;
            return Result;
        }

        public void DrawGizmos(LayerMask raycastLayerMask)
        {
        }
    }

    public class TestObjectDetector : ObjectDetectorBase
    {
        public readonly TestDetectionZone TestZone = new TestDetectionZone();

        public bool RejectDetection { get; set; }
        public int DetectedCount { get; private set; }
        public int GhostDetectedCount { get; private set; }
        public int FailedCount { get; private set; }
        public int DetectionStartCount { get; private set; }
        public int DetectionEndCount { get; private set; }

        public void DetectNow()
        {
            FixedUpdate();
        }

        protected override IDetectionZone GetDetectionZone()
        {
            return TestZone;
        }

        protected override OperationResult CanDetect(in ObjectDetectionContext context)
        {
            OperationResult baseResult = base.CanDetect(context);
            if (!baseResult) return baseResult;
            if (RejectDetection) return DetectionOperations.InvalidDetectableObject();
            return baseResult;
        }

        protected override void OnObjectDetected(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            DetectedCount++;
            base.OnObjectDetected(context, detectionResult);
        }

        protected override void OnObjectGhostDetected(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            GhostDetectedCount++;
            base.OnObjectGhostDetected(context, detectionResult);
        }

        protected override void OnObjectDetectionFailed(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            FailedCount++;
            base.OnObjectDetectionFailed(context, detectionResult);
        }

        protected override void OnObjectDetectionStart(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            DetectionStartCount++;
            base.OnObjectDetectionStart(context, detectionResult);
        }

        protected override void OnObjectDetectionEnd(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            DetectionEndCount++;
            base.OnObjectDetectionEnd(context, detectionResult);
        }
    }

    public sealed class TestGhostObjectDetector : TestObjectDetector, ISupportGhostDetection
    {
    }

    public sealed class TestDetectableObject : DetectableObjectBase
    {
        private float3[] _customPositions;

        public bool RejectDetection { get; set; }
        public int DetectedCount { get; private set; }
        public int GhostDetectedCount { get; private set; }
        public int FailedCount { get; private set; }

        public void SetGhost(bool isGhost)
        {
            IsGhost = isGhost;
        }

        public void SetCustomPositions(float3[] positions)
        {
            _customPositions = positions;
        }

        protected override OperationResult CanBeDetected(ObjectDetectionContext context)
        {
            if (RejectDetection) return DetectionOperations.InvalidDetectableObject();
            return base.CanBeDetected(context);
        }

        protected override int GetDetectionPositionsCount()
        {
            if (ReferenceEquals(_customPositions, null)) return base.GetDetectionPositionsCount();
            return _customPositions.Length;
        }

        protected override void UpdateDetectionPositions()
        {
            if (ReferenceEquals(_customPositions, null))
            {
                base.UpdateDetectionPositions();
                return;
            }

            DetectionPositions.Clear();
            for (int index = 0; index < _customPositions.Length; index++)
            {
                DetectionPositions.Add(_customPositions[index]);
            }
        }

        protected override void OnDetected(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            DetectedCount++;
            base.OnDetected(context, detectionResult);
        }

        protected override void OnObjectGhostDetected(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            GhostDetectedCount++;
            base.OnObjectGhostDetected(context, detectionResult);
        }

        protected override void OnObjectDetectionFailed(
            in ObjectDetectionContext context,
            in OperationResult detectionResult)
        {
            FailedCount++;
            base.OnObjectDetectionFailed(context, detectionResult);
        }
    }

    public sealed class TestStateDetectableObject : DetectableObjectWithStatesBase
    {
        public bool RejectDetection { get; set; }
        public int StayAsDetectedCount { get; private set; }
        public int StayAsGhostDetectedCount { get; private set; }
        public int StayAsAnyDetectedCount { get; private set; }
        public int StayAsUndetectedCount { get; private set; }
        public int DetectionStartAsDetectedCount { get; private set; }
        public int DetectionStartAsGhostCount { get; private set; }
        public int DetectionStartAsAnyCount { get; private set; }
        public int DetectionEndAsDetectedCount { get; private set; }
        public int DetectionEndAsGhostCount { get; private set; }
        public int DetectionEndAsAnyCount { get; private set; }

        public void SetGhost(bool isGhost)
        {
            IsGhost = isGhost;
        }

        protected override OperationResult CanBeDetected(ObjectDetectionContext context)
        {
            if (RejectDetection) return DetectionOperations.InvalidDetectableObject();
            return base.CanBeDetected(context);
        }

        protected override void OnStayAsDetected()
        {
            StayAsDetectedCount++;
        }

        protected override void OnStayAsGhostDetected()
        {
            StayAsGhostDetectedCount++;
        }

        protected override void OnStayAsAnyDetected()
        {
            StayAsAnyDetectedCount++;
        }

        protected override void OnStayAsUndetected()
        {
            StayAsUndetectedCount++;
        }

        protected override void OnDetectionStartAsDetected()
        {
            DetectionStartAsDetectedCount++;
        }

        protected override void OnDetectionStartAsGhost()
        {
            DetectionStartAsGhostCount++;
        }

        protected override void OnDetectionStartAsAny()
        {
            DetectionStartAsAnyCount++;
        }

        protected override void OnDetectionEndAsDetected()
        {
            DetectionEndAsDetectedCount++;
        }

        protected override void OnDetectionEndAsGhost()
        {
            DetectionEndAsGhostCount++;
        }

        protected override void OnDetectionEndAsAny()
        {
            DetectionEndAsAnyCount++;
        }
    }
}
