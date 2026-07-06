using NUnit.Framework;
using Systems.SimpleDetection.Data.Enums;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Tests
{
    public sealed class ObjectDetectionLifecycleTests : SimpleDetectionTestBase
    {
        [Test]
        public void DetectNow_AddsVisibleObjectAndOnlyStartsOnce()
        {
            TestDetectableObject detectableObject = CreateDetectableObject("Detectable", Vector3.zero);
            TestObjectDetector detector = CreateDetector("Detector");

            detector.DetectNow();

            Assert.AreEqual(1, detector.DetectedObjects.Count);
            Assert.IsTrue(detector.IsDetected(detectableObject));
            Assert.AreEqual(1, detectableObject.DetectedBy.Count);
            Assert.AreSame(detector, detectableObject.DetectedBy[0]);
            Assert.AreEqual(1, detector.DetectionStartCount);
            Assert.AreEqual(1, detector.DetectedCount);
            Assert.AreEqual(1, detectableObject.DetectedCount);

            detector.DetectNow();

            Assert.AreEqual(1, detector.DetectedObjects.Count);
            Assert.AreEqual(1, detector.DetectionStartCount);
            Assert.AreEqual(2, detector.DetectedCount);
            Assert.AreEqual(2, detectableObject.DetectedCount);
        }

        [Test]
        public void DetectNow_RemovesObjectWhenItLeavesZone()
        {
            TestDetectableObject detectableObject = CreateDetectableObject("Detectable", Vector3.zero);
            TestObjectDetector detector = CreateDetector("Detector");
            detector.DetectNow();

            detector.TestZone.Result = SpotResult.Outside;
            detector.DetectNow();

            Assert.AreEqual(0, detector.DetectedObjects.Count);
            Assert.AreEqual(0, detectableObject.DetectedBy.Count);
            Assert.AreEqual(1, detector.DetectionEndCount);
            Assert.AreEqual(1, detector.FailedCount);
            Assert.AreEqual(1, detectableObject.FailedCount);
        }

        [Test]
        public void DetectNow_DoesNotAddRejectedObjectWhenGhostDetectionIsUnsupported()
        {
            TestDetectableObject detectableObject = CreateDetectableObject("Detectable", Vector3.zero);
            detectableObject.SetGhost(true);
            TestObjectDetector detector = CreateDetector("Detector");

            detector.DetectNow();

            Assert.AreEqual(0, detector.DetectedObjects.Count);
            Assert.AreEqual(0, detectableObject.DetectedBy.Count);
            Assert.AreEqual(0, detector.GhostDetectedCount);
            Assert.AreEqual(1, detector.FailedCount);
            Assert.AreEqual(1, detectableObject.FailedCount);
        }

        [Test]
        public void DetectNow_AddsRejectedObjectAsGhostWhenSupported()
        {
            TestDetectableObject detectableObject = CreateDetectableObject("Detectable", Vector3.zero);
            detectableObject.SetGhost(true);
            TestGhostObjectDetector detector = CreateGhostDetector("Ghost Detector");

            detector.DetectNow();

            Assert.AreEqual(1, detector.DetectedObjects.Count);
            Assert.IsTrue(detector.IsDetected(detectableObject));
            Assert.AreEqual(1, detectableObject.DetectedBy.Count);
            Assert.AreEqual(1, detector.GhostDetectedCount);
            Assert.AreEqual(1, detectableObject.GhostDetectedCount);

            detector.DetectNow();

            Assert.AreEqual(2, detectableObject.GhostDetectedCount);
        }

        [Test]
        public void DetectNow_SeesObjectWhenAnyDetectionPositionIsVisible()
        {
            TestDetectableObject detectableObject = CreateDetectableObject("Multi Point", Vector3.zero);
            detectableObject.SetCustomPositions(new float3[]
            {
                new float3(-1f, 0f, 0f),
                new float3(2f, 0f, 0f)
            });

            TestObjectDetector detector = CreateDetector("Detector");
            detector.TestZone.UseMinimumSeenX = true;
            detector.TestZone.MinimumSeenX = 1f;

            detector.DetectNow();

            Assert.AreEqual(1, detector.DetectedObjects.Count);
            Assert.AreEqual(1, detectableObject.DetectedCount);
        }

        [Test]
        public void DetectNow_TransitionsStateBetweenDetectedGhostAndUndetected()
        {
            TestStateDetectableObject detectableObject = CreateStateDetectableObject("Detectable", Vector3.zero);
            TestGhostObjectDetector detector = CreateGhostDetector("Detector");
            detector.DetectNow();

            detector.DetectNow();
            Assert.AreEqual(1, detectableObject.StayAsDetectedCount);
            Assert.AreEqual(1, detectableObject.StayAsAnyDetectedCount);

            detectableObject.SetGhost(true);
            detector.DetectNow();

            Assert.AreEqual(1, detectableObject.DetectionEndAsDetectedCount);
            Assert.AreEqual(1, detectableObject.DetectionStartAsGhostCount);
            Assert.AreEqual(0, detectableObject.DetectionStartAsAnyCount);

            detector.TestZone.Result = SpotResult.Outside;
            detector.DetectNow();

            Assert.AreEqual(1, detectableObject.DetectionEndAsGhostCount);
            Assert.AreEqual(1, detectableObject.DetectionEndAsAnyCount);
        }
    }
}
