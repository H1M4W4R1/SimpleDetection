using Systems.SimpleDetection.Data.Enums;
using Systems.SimpleDetection.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Zones
{
    public readonly struct Frustum3DDetectionZone : IDetectionZone
    {
        private readonly float3 position;
        private readonly quaternion rotation;
        private readonly float farPlaneDistance;
        private readonly float nearPlaneDistance;
        private readonly float horizontalFieldOfView;
        private readonly float aspectRatio;

        public Frustum3DDetectionZone(
            float3 position,
            quaternion rotation,
            float farPlaneDistance,
            float nearPlaneDistance,
            float horizontalFieldOfView,
            float aspectRatio)
        {
            this.position = position;
            this.rotation = rotation;
            this.farPlaneDistance = farPlaneDistance;
            this.nearPlaneDistance = nearPlaneDistance;
            this.horizontalFieldOfView = horizontalFieldOfView;
            this.aspectRatio = aspectRatio;
        }

        [BurstCompile] public bool IsPointInZone(in float3 detectionPosition)
        {
            // Compute frustum planes
            NativeArray<float4> frustumPlanes = new(6, Allocator.TempJob);
            FrustumUtil.ExtractFrustumPlanes(position, rotation, GetFarPlaneHeight(), aspectRatio,
                nearPlaneDistance, farPlaneDistance, ref frustumPlanes);

            // Check if point is inside frustum
            bool isPointInZone = FrustumUtil.IsPointInFrustum(detectionPosition, frustumPlanes);

            // Clear memory and return
            frustumPlanes.Dispose();
            return isPointInZone;
        }

        [BurstDiscard] public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask)
        {
            // Ensure that point is in zone
            if (!IsPointInZone(detectionPosition)) return SpotResult.Outside;

            float3 desiredPosition = detectionPosition;

            // Compute planes
            NativeArray<float4> frustumPlanes = new(6, Allocator.TempJob);
            FrustumUtil.ExtractFrustumPlanes(position, rotation, GetFarPlaneHeight(), aspectRatio,
                nearPlaneDistance, farPlaneDistance, ref frustumPlanes);

            // Check if point is behind far plane
            if (!FrustumUtil.IsInside(desiredPosition, frustumPlanes[5]))
                desiredPosition = FrustumUtil.ClosestPointOnPlane(frustumPlanes[5], desiredPosition);

            // Check if point is behind near plane
            if (!FrustumUtil.IsInside(desiredPosition, frustumPlanes[4]))
                desiredPosition = FrustumUtil.ClosestPointOnPlane(frustumPlanes[4], desiredPosition);

            // Compute raycast data, a bit of hack with farPlaneDistance
            if (!Physics.Raycast(position, math.normalize(desiredPosition - position),
                    out RaycastHit hitObj, math.distance(desiredPosition, position), layerMask))
            {
                frustumPlanes.Dispose();
                return SpotResult.InsideSeen;
            }

            // Ensure that the raycast hit nothing
            frustumPlanes.Dispose();
            return ReferenceEquals(hitObj.collider, null) ? SpotResult.InsideSeen : SpotResult.InsideObstructed;
        }

        public void DrawGizmos(LayerMask raycastLayerMask)
        {
#if UNITY_EDITOR
            // Compute side lines
            FrustumUtil.ComputeFrustumGizmosLines(position, rotation, GetFarPlaneHeight(), aspectRatio,
                nearPlaneDistance, farPlaneDistance, out NativeArray<float3x2> lines);

            // Draw near plane lines
            for (int i = 0; i < 4; i++) Gizmos.DrawLine(lines[i].c0, lines[i].c1);

            lines.Dispose();

            // Draw grid of detection
            const int RES_X = 32; // horizontal resolution
            const int RES_Y = 18; // vertical resolution

            float farHeight = GetFarPlaneHeight();
            float farWidth = farHeight * aspectRatio;

            float nearHeight = farHeight * (nearPlaneDistance / farPlaneDistance);
            float nearWidth = farWidth * (nearPlaneDistance / farPlaneDistance);

            // Compute basis vectors
            float3 forward = math.mul(rotation, new float3(0, 0, 1));
            float3 up = math.mul(rotation, new float3(0, 1, 0));
            float3 right = math.mul(rotation, new float3(1, 0, 0));

            // Plane centers
            float3 nearCenter = position + forward * nearPlaneDistance;
            float3 farCenter = position + forward * farPlaneDistance;

            // Allocate raycast structures
            NativeArray<RaycastCommand> commands = new(RES_X * RES_Y, Allocator.TempJob);
            NativeArray<RaycastHit> results = new(RES_X * RES_Y, Allocator.TempJob);

            for (int y = 0; y < RES_Y; y++)
            {
                float v = (y / (float) (RES_Y - 1)) - 0.5f;
                for (int x = 0; x < RES_X; x++)
                {
                    float u = (x / (float) (RES_X - 1)) - 0.5f;

                    float3 nearPoint = nearCenter + right * (u * nearWidth) + up * (v * nearHeight);
                    float3 farPoint = farCenter + right * (u * farWidth) + up * (v * farHeight);

                    float3 dir = math.normalize(farPoint - nearPoint);

                    QueryParameters qp = new(layerMask: raycastLayerMask, false,
                        QueryTriggerInteraction.Ignore);
                    commands[y * RES_X + x] =
                        new RaycastCommand(nearPoint, dir, qp, math.distance(farPoint, nearPoint));
                }
            }

            // Schedule raycasts
            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 32);
            handle.Complete();

            // Draw grid of rays
            for (int y = 0; y < RES_Y; y++)
            {
                for (int x = 0; x < RES_X; x++)
                {
                    int index = y * RES_X + x;
                    
                    float v = (y / (float) (RES_Y - 1)) - 0.5f;
                    float u = (x / (float) (RES_X - 1)) - 0.5f;

                    float3 nearPoint = nearCenter + right * (u * nearWidth) + up * (v * nearHeight);
                    float3 farPoint = farCenter + right * (u * farWidth) + up * (v * farHeight);

                    float3 hitPoint = results[index].collider is not null ? results[index].point : farPoint;

                    // Draw side lines if necessary
                    if (y is 0 or RES_Y - 1 && x is 0 or RES_X - 1) Gizmos.DrawLine(nearPoint, hitPoint);

                    // Draw grid (towards right up)
                    int nY = y + 1;
                    int nX = x + 1;
                    int nIndex = nY * RES_X + x;

                    // Next point U and V
                    float nV = (nY / (float) (RES_Y - 1)) - 0.5f;
                    float nU = (nX / (float) (RES_X - 1)) - 0.5f;
                    
                    if (nY < RES_Y)
                    {
                        float3 nFarPoint = farCenter + right * (u * farWidth) + up * (nV * farHeight);
                        Gizmos.DrawLine(hitPoint, results[nIndex].collider is not null 
                            ? results[nIndex].point : nFarPoint);
                    }
                    
                    nIndex = y * RES_X + nX;

                    if (nX < RES_X)
                    {
                        float3 nFarPoint = farCenter + right * (nU * farWidth) + up * (v * farHeight);
                        Gizmos.DrawLine(hitPoint, results[nIndex].collider is not null 
                            ? results[nIndex].point : nFarPoint);
                    }
                }
            }

            commands.Dispose();
            results.Dispose();
#endif
        }

        [BurstCompile] private float GetFarPlaneHeight()
        {
            float d = math.length(farPlaneDistance - nearPlaneDistance);
            float horizontalFovRadians = math.radians(horizontalFieldOfView);

            // Far plane half-width
            float halfWidth = d * math.tan(horizontalFovRadians * 0.5f);
            float width = 2f * halfWidth;

            // Height using aspect ratio
            float height = width / aspectRatio;
            return height;
        }
    }
}