using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Systems.SimpleDetection.Utility
{
    public static class MathExtensions
    {
        /// <summary>
        ///     Rotates the given vector by the given angle in radians, counterclockwise.
        /// </summary>
        /// <param name="v">The vector to rotate.</param>
        /// <param name="angle">The angle in radians to rotate the vector by.</param>
        /// <returns>The rotated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float2 Rotate(float2 v, float angle)
        {
            float s = math.sin(angle);
            float c = math.cos(angle);

            return new float2(
                v.x * c - v.y * s,
                v.x * s + v.y * c
            );
        }

        /// <summary>
        ///     Rotates the given vector by the given angle in radians about the given axis.
        /// </summary>
        /// <param name="v">The vector to rotate.</param>
        /// <param name="axis">The axis to rotate the vector around.</param>
        /// <param name="angle">The angle in radians to rotate the vector by.</param>
        /// <returns>The rotated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Rotate(float3 v, float3 axis, float angle)
        {
            float s = math.sin(angle);
            float c = math.cos(angle);

            float3 k = math.normalize(axis);

            return v * c + math.cross(k, v) * s + k * (math.dot(k, v) * (1f - c));
        }
    }
}