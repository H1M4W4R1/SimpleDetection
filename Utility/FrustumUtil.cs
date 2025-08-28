using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Systems.SimpleDetection.Utility
{
    [BurstCompile] public static class FrustumUtil
    {
        /// <summary>
        ///     Build a plane from 3 points (counter-clockwise order).
        ///     Returned as float4(normal.xyz, distance).
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CreatePlane(in float3 a, in float3 b, in float3 c, out float4 plane)
        {
            float3 normal = math.normalize(math.cross(b - a, c - a));
            float distance = -math.dot(normal, a);
            plane = new float4(normal, distance);
        }

        /// <summary>
        ///     Check if a point is inside plane.
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInside(in float3 point, in float4 plane)
        {
            return math.dot(plane.xyz, point) + plane.w >= 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ClosestPointOnPlane(in float4 plane, in float3 point)
        {
            float3 normal = plane.xyz;
            float distance = plane.w;

            // Signed distance from point to plane
            float signedDist = math.dot(normal, point) + distance;

            // Project point onto plane
            return point - signedDist * normal;
        }
        
        /// <summary>
        ///     Computes the 6 frustum planes as float4(x,y,z,d).
        /// </summary>
        [BurstCompile] public static void ExtractFrustumPlanes(
            in float3 camPos,
            in quaternion camRot,
            float verticalSize,
            float aspect,
            float near,
            float far,
            ref NativeArray<float4> planes)
        {
            // Assume that planes length must be six
            Assert.IsTrue(planes.IsCreated, "Planes array must be created");
            Assert.IsTrue(planes.Length == 6, "Planes array length must be 6");
            
            // Basis vectors
            float3 forward = math.mul(camRot, new float3(0, 0, 1));
            float3 up = math.mul(camRot, new float3(0, 1, 0));
            float3 rightV = math.mul(camRot, new float3(1, 0, 0));

            // Near/Far centers
            float3 nearCenter = camPos + forward * near;
            float3 farCenter = camPos + forward * far;

            // Half sizes
            float halfFarHeight = verticalSize * 0.5f;
            float halfFarWidth = halfFarHeight * aspect;
            float halfNearHeight = halfFarHeight * (near / far);
            float halfNearWidth = halfNearHeight * aspect;

            // Near plane corners
            float3 ntl = nearCenter + up * halfNearHeight - rightV * halfNearWidth;
            float3 ntr = nearCenter + up * halfNearHeight + rightV * halfNearWidth;
            float3 nbl = nearCenter - up * halfNearHeight - rightV * halfNearWidth;
            float3 nbr = nearCenter - up * halfNearHeight + rightV * halfNearWidth;

            // Far plane corners
            float3 ftl = farCenter + up * halfFarHeight - rightV * halfFarWidth;
            float3 ftr = farCenter + up * halfFarHeight + rightV * halfFarWidth;
            float3 fbl = farCenter - up * halfFarHeight - rightV * halfFarWidth;
            float3 fbr = farCenter - up * halfFarHeight + rightV * halfFarWidth;

            // Planes
            CreatePlane(ntl, ftl, fbl, out float4 left);
            CreatePlane(ftl, ntr, ftr, out float4 top);
            CreatePlane(ftr, ntr, nbr, out float4 right);
            CreatePlane(fbr, nbl, fbl, out float4 bottom);
            CreatePlane(ftl, ftr, fbr, out float4 nearP);
            CreatePlane(nbr, ntr, ntl, out float4 farP);

            planes[0] = left;
            planes[1] = top;
            planes[2] = right;
            planes[3] = bottom;
            planes[4] = nearP;
            planes[5] = farP;
        }

        /// <summary>
        ///     Quickly check if point is inside view frustrum
        /// </summary>
        [BurstCompile] public static bool IsPointInFrustum(in float3 point, in NativeArray<float4> planes)
        {
            Assert.IsTrue(planes.IsCreated, "Planes array must be created");
            Assert.AreEqual(6, planes.Length, "Planes array length must be 6");
            for (int i = 0; i < 6; i++)
            {
                if (!IsInside(point, planes[i])) return false;
            }

            return true;
        }


        /// <summary>
        ///     Draws frustum gizmos from planes (NativeArray length = 6).
        ///     Planes must be in order: left, right, top, bottom, near, far.
        /// </summary>
        /// <remarks>
        ///     Lines array is allocated within method, you shall discard it when not used anymore.
        /// </remarks>
        [BurstCompile] public static void ComputeFrustumGizmosLines(
            in float3 camPos,
            in quaternion camRot,
            float verticalSize,
            float aspect,
            float near,
            float far,
            out NativeArray<float3x2> lines,
            Allocator linesAllocator = Allocator.TempJob)
        {
            // Frustum lines array
            lines = new NativeArray<float3x2>(12, linesAllocator);
           
            // Basis vectors
            float3 forward = math.mul(camRot, new float3(0, 0, 1));
            float3 up = math.mul(camRot, new float3(0, 1, 0));
            float3 rightV = math.mul(camRot, new float3(1, 0, 0));

            // Near/Far centers
            float3 nearCenter = camPos + forward * near;
            float3 farCenter = camPos + forward * far;

            // Half sizes
            float halfFarHeight = verticalSize * 0.5f;
            float halfFarWidth = halfFarHeight * aspect;
            float halfNearHeight = halfFarHeight * (near / far);
            float halfNearWidth = halfNearHeight * aspect;

            // Near plane corners
            float3 ntl = nearCenter + up * halfNearHeight - rightV * halfNearWidth;
            float3 ntr = nearCenter + up * halfNearHeight + rightV * halfNearWidth;
            float3 nbl = nearCenter - up * halfNearHeight - rightV * halfNearWidth;
            float3 nbr = nearCenter - up * halfNearHeight + rightV * halfNearWidth;

            // Far plane corners
            float3 ftl = farCenter + up * halfFarHeight - rightV * halfFarWidth;
            float3 ftr = farCenter + up * halfFarHeight + rightV * halfFarWidth;
            float3 fbl = farCenter - up * halfFarHeight - rightV * halfFarWidth;
            float3 fbr = farCenter - up * halfFarHeight + rightV * halfFarWidth;
            
            // Near plane
            lines[0] = new float3x2(ntl, ntr);
            lines[1] = new float3x2(ntr, nbr);
            lines[2] = new float3x2(nbr, nbl);
            lines[3] = new float3x2(nbl, ntl);

            // Far plane
            lines[4] = new float3x2(ftl, ftr);
            lines[5] = new float3x2(ftr, fbr);
            lines[6] = new float3x2(fbr, fbl);
            lines[7] = new float3x2(fbl, ftl);

            // Edges
            lines[8] = new float3x2(ntl, ftl);
            lines[9] = new float3x2(ntr, ftr);
            lines[10] = new float3x2(nbl, fbl);
            lines[11] = new float3x2(nbr, fbr);
        }
        
    }
}