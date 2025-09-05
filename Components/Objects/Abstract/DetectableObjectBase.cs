using System.Collections.Generic;
using Systems.SimpleCore.Operations;
using Systems.SimpleDetection.Components.Detectors.Abstract;
using Systems.SimpleDetection.Data;
using Systems.SimpleDetection.Operations;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Objects.Abstract
{
    /// <summary>
    ///     Represents object that may be detected using Detection system.
    ///     This is a base class for all components related to detection
    ///     of objects in 2D space.
    /// </summary>
    public abstract class DetectableObjectBase : MonoBehaviour
    {
        private readonly List<ObjectDetectorBase> _detectedBy = new();
        private UnsafeList<float3> _detectionPositions;

        /// <summary>
        ///     List of all detectable objects
        /// </summary>
        internal static readonly List<DetectableObjectBase> AllObjects = new();

        /// <summary>
        ///     Object won't be seen even if it is detected by detector.
        /// </summary>
        [field: SerializeField] public bool IsGhost { get; protected set; }
        
        /// <summary>
        ///     Access to detection positions list.
        /// </summary>
        public ref UnsafeList<float3> DetectionPositions => ref _detectionPositions;

        /// <summary>
        ///     List of detectors that can see this object (incl. ghosts).
        /// </summary>
        public IReadOnlyList<ObjectDetectorBase> DetectedBy => _detectedBy;

        /// <summary>
        ///     Check if object can be detected at all
        /// </summary>
        /// <remarks>
        ///     This method is intended to support ghost objects or
        ///     conditional detection (for example when player is doing forbidden action).
        /// </remarks>
        protected internal virtual OperationResult CanBeDetected(ObjectDetectionContext context)
        {
            if (IsGhost) return DetectionOperations.IsGhost();
            return DetectionOperations.Permitted();
        }

        /// <summary>
        ///     Get amount of positions where object can be detected
        /// </summary>
        /// <returns>Amount of positions where object can be detected</returns>
        protected internal virtual int GetDetectionPositionsCount() => 1;

        /// <summary>
        ///     Ensure that positions list is created.
        /// </summary>
        /// <remarks>
        ///     This method is called by the detection system to ensure that the positions
        ///     list is created. If the list is not created, it will be created with the
        ///     correct size and allocator.
        /// </remarks>
        internal void EnsureDetectionListIsCreated()
        {
            // Ensure that positions list is created
            if (!DetectionPositions.IsCreated)
                DetectionPositions = new UnsafeList<float3>(GetDetectionPositionsCount(), Allocator.Persistent);
        }

        internal void _OnSeen(ObjectDetectionContext context)
        {
            if (!_detectedBy.Contains(context.detector)) _detectedBy.Add(context.detector);
        }

        internal void _OnNotSeen(ObjectDetectionContext context)
        {
            ObjectDetectorBase detector = context.detector;
            _detectedBy.RemoveAll(o => ReferenceEquals(o, detector));
        }

        /// <summary>
        ///     Called when object is detected
        /// </summary>
        protected internal virtual void OnDetected(in ObjectDetectionContext context, in OperationResult detectionResult)
        {
        }

        /// <summary>
        ///     Called when object is being seen by detector, however
        ///     it cannot be detected at the moment (see <see cref="CanBeDetected"/> method).
        /// </summary>
        protected internal virtual void OnObjectGhostDetected(in ObjectDetectionContext context, in OperationResult detectionResult)
        {
        }

        /// <summary>
        ///     Called when object is not seen or when object cannot be detected and ghost processing
        ///     of detector is disabled.
        /// </summary>
        /// <remarks>
        ///     Implemented to support state-based detection solutions.
        /// </remarks>
        protected internal virtual void OnObjectDetectionFailed(in ObjectDetectionContext context, in OperationResult detectionResult)
        {
        }

        /// <summary>
        ///     Updates the positions where the object can be detected.
        /// </summary>
        /// <remarks>
        ///     This method is called by the detection system to update the positions
        ///     list with the new positions of the object. The default implementation
        ///     clears the positions list and adds the position of the object's
        ///     transform. If the object has multiple positions where it can be
        ///     detected, this method should be overridden to add all the positions
        ///     to the list.
        /// </remarks>
        protected internal virtual void UpdateDetectionPositions()
        {
            // Clear positions list and add new positions
            DetectionPositions.Clear();
            DetectionPositions.Add(transform.position);
        }

        protected void FixedUpdate()
        {
            // Ensure that positions list is created before updating it
            EnsureDetectionListIsCreated();

            // Perform data update
            UpdateDetectionPositions();
        }

        protected void Awake()
        {
            AllObjects.Add(this);
        }

        protected void OnDestroy()
        {
            AllObjects.Remove(this);

            // Clear allocations
            if (DetectionPositions.IsCreated) DetectionPositions.Dispose();
        }

        internal static IReadOnlyList<DetectableObjectBase> GetAllDetectableObjects()
        {
#if !UNITY_EDITOR
            return AllObjects;
#else
            if (Application.isPlaying) return AllObjects;

            // Don't know why array has IReadOnlyList implemented, but it works
            // so who tf cares?
            return FindObjectsByType<DetectableObjectBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#endif
        }
    }
}