using Systems.Detection2D.Data;
using Systems.Detection2D.Data.Enums;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Detection2D.Components.Detectors.Zones
{
    /// <summary>
    ///     Detection zone - represents a zone where objects can be detected.
    ///     This is usually a 2D shape that defines the area where objects can be detected.
    /// </summary>
    /// <remarks>
    ///     Interfaces implementing this interface should be marked with [BurstCompile] attribute.
    ///     Implementations should also be marked with [MethodImpl(MethodImplOptions.AggressiveInlining)] attribute.
    /// </remarks>
    public interface IDetectionZone
    {
        /// <summary>
        ///     Check if point is inside detection zone
        /// </summary>
        /// <param name="detectionPosition">Position to check</param>
        /// <returns>True if point is inside detection zone</returns>
        public bool IsPointInZone(in float3 detectionPosition);

        /// <summary>
        ///     Check if point is seen, performs raycast to check if point is visible
        /// </summary>
        /// <param name="detectionPosition">Position to check</param>
        /// <param name="layerMask">Layer mask to use for raycast</param>
        /// <returns>True if point is seen</returns>
        /// <remarks>
        ///     Has to be marked with [BurstDiscard] attribute.
        ///     Update when Unity releases new Physics2D API with MT support.
        /// </remarks>
        public SpotResult IsPointSeen(in float3 detectionPosition, int layerMask);
        
        /// <summary>
        ///     Draw gizmos for detection zone
        /// </summary>
        /// <remarks>
        ///     Must be marked with [BurstDiscard] attribute
        /// </remarks>
        public void DrawGizmos(LayerMask raycastLayerMask);
    }
}