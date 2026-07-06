using NUnit.Framework;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Systems.SimpleDetection.Data.Enums;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Tests
{
    public sealed class DetectionZoneTests : SimpleDetectionTestBase
    {
        [Test]
        public void Circle2D_IncludesCenterAndBoundaryButRejectsOutside()
        {
            Circle2DDetectionZone zone = new Circle2DDetectionZone(new float2(0f, 0f), 2f);

            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 0f, 0f)));
            Assert.IsTrue(zone.IsPointInZone(new float3(2f, 0f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(2.01f, 0f, 0f)));
        }

        [Test]
        public void Circle2D_ReportsSeenOutsideAndObstructed()
        {
            Circle2DDetectionZone zone = new Circle2DDetectionZone(new float2(0f, 0f), 2f);

            Assert.AreEqual(SpotResult.InsideSeen, zone.IsPointSeen(new float3(0f, 0f, 0f), 1));
            Assert.AreEqual(SpotResult.Outside, zone.IsPointSeen(new float3(3f, 0f, 0f), 1));

            GameObject obstacle = Track(new GameObject("2D Obstacle"));
            obstacle.transform.position = new Vector3(0.5f, 0f, 0f);
            BoxCollider2D collider = obstacle.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.25f, 0.25f);
            Physics2D.SyncTransforms();

            Assert.AreEqual(SpotResult.InsideObstructed, zone.IsPointSeen(new float3(1f, 0f, 0f), 1));
        }

        [Test]
        public void Frustum2D_UsesForwardHalfAngleAndRadius()
        {
            Frustum2DDetectionZone zone = new Frustum2DDetectionZone(
                new float2(0f, 0f),
                new float2(0f, 1f),
                90f,
                3f);

            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 0f, 0f)));
            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 2f, 0f)));
            Assert.IsTrue(zone.IsPointInZone(new float3(1f, 1f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(2f, 1f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, -1f, 0f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, 3.1f, 0f)));
        }

        [Test]
        public void Sphere3D_IncludesCenterAndBoundaryButRejectsOutside()
        {
            Sphere3DDetectionZone zone = new Sphere3DDetectionZone(new float3(0f, 0f, 0f), 2f);

            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 0f, 0f)));
            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 0f, 2f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, 0f, 2.01f)));
        }

        [Test]
        public void Sphere3D_ReportsSeenOutsideAndObstructed()
        {
            Sphere3DDetectionZone zone = new Sphere3DDetectionZone(new float3(0f, 0f, 0f), 2f);

            Assert.AreEqual(SpotResult.InsideSeen, zone.IsPointSeen(new float3(0f, 0f, 0f), 1));
            Assert.AreEqual(SpotResult.Outside, zone.IsPointSeen(new float3(3f, 0f, 0f), 1));

            GameObject obstacle = Track(new GameObject("3D Obstacle"));
            obstacle.transform.position = new Vector3(0.5f, 0f, 0f);
            BoxCollider collider = obstacle.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.25f, 0.25f, 0.25f);
            Physics.SyncTransforms();

            Assert.AreEqual(SpotResult.InsideObstructed, zone.IsPointSeen(new float3(1f, 0f, 0f), 1));
        }

        [Test]
        public void Frustum3D_UsesNearFarFieldOfViewAndAspect()
        {
            Frustum3DDetectionZone zone = new Frustum3DDetectionZone(
                new float3(0f, 0f, 0f),
                quaternion.identity,
                10f,
                1f,
                90f,
                1f);

            Assert.IsTrue(zone.IsPointInZone(new float3(0f, 0f, 1f)));
            Assert.IsTrue(zone.IsPointInZone(new float3(2f, 0f, 5f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, 0f, 0.5f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(0f, 0f, 11f)));
            Assert.IsFalse(zone.IsPointInZone(new float3(6f, 0f, 5f)));
        }
    }
}
