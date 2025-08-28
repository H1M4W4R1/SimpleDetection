using System.Runtime.CompilerServices;
using Systems.SimpleDetection.Data.Enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Zones
{
    [BurstCompile] public readonly struct Sphere3DDetectionZone : IDetectionZone
    {
        private readonly float3 position;
        private readonly float radius;
        private readonly float radiusSq;

        public Sphere3DDetectionZone(float3 position, float radius)
        {
            this.position = position;
            this.radius = radius;
            radiusSq = radius * radius;
        }

        /// <summary>
        ///     Checks if a point is inside the zone.
        /// </summary>
        /// <param name="detectionPosition">Point to check</param>
        /// <returns>true if point is inside the zone, false otherwise</returns>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointInZone(in float3 detectionPosition)
            => math.lengthsq(detectionPosition - position) <= radiusSq;

        /// <summary>
        ///     Checks if a point is seen (i.e. not obscured) by the zone.
        /// </summary>
        /// <param name="detectionPosition">Point to check</param>
        /// <param name="layerMask">Layer mask to use for raycast</param>
        /// <returns>true if point is seen, false otherwise</returns>
        /// <remarks>
        ///     This method will first check if the point is inside the zone, and if not, will return false.
        ///     Then it will cast a ray from the point to the center of the zone and check if it hits anything.
        ///     If it does, then the point is obscured and the method will return false. Otherwise, it will return true.
        /// </remarks>
        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask)
        {
            // Ensure that point is in zone
            if (!IsPointInZone(detectionPosition)) return SpotResult.Outside;

            // Compute raycast data
            if (!Physics.Raycast(detectionPosition, math.normalize(position - detectionPosition),
                    out RaycastHit hitObj, math.min(math.distance(detectionPosition, position), radius),
                    layerMask))
                return SpotResult.InsideSeen;

            // Ensure that the raycast hit nothing
            return ReferenceEquals(hitObj.collider, null) ? SpotResult.InsideSeen : SpotResult.InsideObstructed;
        }

        /// <summary>
        ///     Draws a wire sphere gizmo at the position of the zone to represent it.
        ///     This is a burst-discarded method, meaning that it will not be compiled
        ///     by the Burst compiler, and will only be executed in the editor.
        /// </summary>
        [BurstDiscard] public void DrawGizmos(LayerMask raycastLayerMask)
        {
#if UNITY_EDITOR
            const int N_XZ = 90;
            const int N_Y = 90 / 2;

            // Generate points
            UnsafeList<float3> points = default;
            GenerateWireSpherePoints(ref points, N_XZ, N_Y, Allocator.TempJob);

            // Perform raycasts
            NativeList<RaycastCommand> commands = default;
            NativeList<RaycastHit> results = default;
            BuildRaycastCommands(ref commands, ref results, points, raycastLayerMask);
            
            // Convert to arrays
            NativeArray<RaycastCommand> commandsArray = commands.AsArray();
            NativeArray<RaycastHit> resultsArray = results.AsArray();
        
            // Schedule raycasts and wait for them to finish
            JobHandle raycastJob = RaycastCommand.ScheduleBatch(commandsArray, resultsArray, 16);
            raycastJob.Complete();
            
            // Perform point updates
            for (int i = 0; i < points.Length; i++)
            {
                RaycastHit result = resultsArray[i];
                if (!ReferenceEquals(result.collider, null)) points[i] = result.point;
            }

            // Draw gizmos
            for (int iy = 0; iy <= N_Y; iy++)
            {
                int rowStart = iy * N_XZ;

                // Horizontal lines
                for (int ix = 0; ix < N_XZ; ix++)
                {
                    int next = rowStart + (ix + 1) % N_XZ;
                    Gizmos.DrawLine(points[rowStart + ix], points[next]);
                }

                // Vertical lines
                if (iy >= N_Y) continue;

                int nextRow = (iy + 1) * N_XZ;
                for (int ix = 0; ix < N_XZ; ix++)
                {
                    Gizmos.DrawLine(points[rowStart + ix], points[nextRow + ix]);
                }
            }
            
            // Dispose of mid-calculation data
            commands.Dispose();
            results.Dispose();
            commandsArray.Dispose();
            resultsArray.Dispose();
            points.Dispose();
#endif
        }

        [BurstCompile] private void GenerateWireSpherePoints(
            ref UnsafeList<float3> points,
            int xzCount,
            int yCount,
            Allocator allocator)
        {
            if (!points.IsCreated) points = new UnsafeList<float3>(xzCount * yCount, allocator);
            points.Clear();

            // Latitude: from -π/2 (south pole) to +π/2 (north pole)
            for (int iy = 0; iy <= yCount; iy++)
            {
                float v = (float) iy / yCount;
                float theta = (v - 0.5f) * math.PI; // latitude

                float y = math.sin(theta) * radius;
                float r = math.cos(theta) * radius;

                for (int ix = 0; ix < xzCount; ix++)
                {
                    float u = (float) ix / xzCount;
                    float phi = u * math.PI * 2f; // longitude

                    float x = math.cos(phi) * r;
                    float z = math.sin(phi) * r;

                    points.Add(new float3(x, y, z) + position);
                }
            }
        }

        [BurstCompile] private void BuildRaycastCommands(
            ref NativeList<RaycastCommand> commands,
            ref NativeList<RaycastHit> results,
            in UnsafeList<float3> points,
            int layerMask)
        {
            if (!commands.IsCreated) commands = new NativeList<RaycastCommand>(Allocator.TempJob);
            if (!results.IsCreated) results = new NativeList<RaycastHit>(Allocator.TempJob);

            commands.Clear();

            // Add raycast
            for(int i = 0; i < points.Length; i++)
                AddRay(ref commands, ref results, position, points[i], layerMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] private static void AddRay(
            ref NativeList<RaycastCommand> commands,
            ref NativeList<RaycastHit> results,
            in float3 a,
            in float3 b,
            int layerMask)
        {
            float3 dir = b - a;
            float dist = math.length(dir);
            if (Mathf.Approximately(dist, 0f)) return;

            QueryParameters parameters = new()
            {
                hitTriggers = QueryTriggerInteraction.Ignore,
                hitBackfaces = false,
                layerMask = layerMask,
                hitMultipleFaces = false
            };

            // Create raycast command
            RaycastCommand command = new RaycastCommand(a, math.normalize(dir), parameters, dist);
            commands.Add(command);
            results.Add(default);
        }
    }
}