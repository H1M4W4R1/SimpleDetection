using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.SimpleDetection.Components.Detectors.Markers;
using Systems.SimpleDetection.Components.Detectors.Zones;
using Systems.SimpleDetection.Components.Objects.Abstract;
using Systems.SimpleDetection.Data;
using Systems.SimpleDetection.Data.Enums;
using Systems.SimpleDetection.Data.Settings;
using Systems.SimpleDetection.Data.Settings.Types;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.SimpleDetection.Components.Detectors.Abstract
{
    /// <summary>
    ///     Base class for all object detectors
    /// </summary>
    public abstract class ObjectDetectorBase : MonoBehaviour
    {
        private readonly List<DetectableObjectBase> _detectedObjects = new();

        [SerializeField]
        [Tooltip("Layer mask used to perform raycasts for this detector, should contain obstacle layers")]
        private LayerMask raycastLayerMask = 1;

        /// <summary>
        ///     Detection zone of this detector
        /// </summary>
        public IDetectionZone DetectionZone { get; private set; }

        /// <summary>
        ///     List of all detected objects
        /// </summary>
        public IReadOnlyList<DetectableObjectBase> DetectedObjects => _detectedObjects;

        /// <summary>
        ///     Checks if this detector supports ghost detection
        /// </summary>
        protected bool SupportsGhostDetection => this is ISupportGhostDetection;

        /// <summary>
        ///     Update detection zone of this detector.
        /// </summary>
        [NotNull] protected abstract IDetectionZone GetDetectionZone();

        /// <summary>
        ///     Checks if the given object is detected by this detector
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if the object is detected by this detector, false otherwise</returns>
        public bool IsDetected([NotNull] DetectableObjectBase obj) => _detectedObjects.Contains(obj);

        /// <summary>
        ///     Check if specific object can be detected by this detector
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if the object can be detected by this detector, false otherwise</returns>
        /// <remarks>
        ///     Should be used to verify custom types of objects for detectors using 'is' operator.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool CanBeDetected([NotNull] DetectableObjectBase obj) => true;

        /// <summary>
        ///     Updates detection zone data if necessary
        /// </summary>
        protected void FixedUpdate()
        {
            DetectionZone = GetDetectionZone();
            PerformDetection();
        }

        /// <summary>
        ///     Perform object detection.
        ///     This method is called automatically by `FixedUpdate` method.
        /// </summary>
        /// <remarks>
        ///     This method iterates over all detectable objects and checks if they are seen
        ///     by the detector. If object is seen, `OnDetected` event is called, otherwise
        ///     `OnDetectionAttempted` event is called.
        /// </remarks>
        protected void PerformDetection()
        {
            // Draw detection objects information
            IReadOnlyList<DetectableObjectBase> detectableObjects = DetectableObjectBase.GetAllDetectableObjects();
            for (int index = 0; index < detectableObjects.Count; index++)
            {
                DetectableObjectBase obj = detectableObjects[index];

                // Skip if object cannot be detected by this detector due to invalid type
                // or other reasons
                if (!CanBeDetected(obj)) continue;

                // Update when editor is not playing
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    obj.EnsureDetectionListIsCreated();
                    obj.UpdateDetectionPositions();
                }
#endif

                bool isSeen = false;

                // Construct context
                ObjectDetectionContext context = new(obj, this);

                // Skip if object cannot be detected and ghost detection is disabled
                if (!SupportsGhostDetection && !obj.CanBeDetected(context))
                {
                    obj._OnNotSeen(context);
                    obj.OnObjectDetectionFailed(context);
                    OnObjectDetectionFailed(obj);
                    TryRemoveDetectedObject(obj);
                    continue;
                }

                // Check if object is seen
                for (int i = 0; i < obj.DetectionPositions.Length; i++)
                {
                    float3 point = obj.DetectionPositions[i];

                    // Check if object is seen and break if seen
                    isSeen = DetectionZone.IsPointSeen(point, raycastLayerMask) == SpotResult.InsideSeen;
                    if (isSeen) break;
                }

                // Skip if object is not seen
                if (!isSeen)
                {
                    obj._OnNotSeen(context);
                    obj.OnObjectDetectionFailed(context);
                    OnObjectDetectionFailed(obj);
                    TryRemoveDetectedObject(obj);
                    continue;
                }

                // Perform events execution
                if (!SupportsGhostDetection || obj.CanBeDetected(context))
                {
                    obj._OnSeen(context);
                    obj.OnDetected(context);
                    OnObjectDetected(obj);
                    TryAddDetectedObject(obj);
                }
                else
                {
                    obj._OnSeen(context);
                    obj.OnObjectGhostDetected(context);
                    OnObjectGhostDetected(obj);
                    TryAddDetectedObject(obj);
                }
            }
        }

        /// <summary>
        ///     Called when an object is detected.
        /// </summary>
        /// <param name="obj">Detected object</param>
        /// <remarks>
        ///     This method is called on each detected object.
        ///     It is called after <see cref="DetectableObjectBase.OnDetected"/> method is called
        ///     on the detected object.
        /// </remarks>
        protected virtual void OnObjectDetected([NotNull] DetectableObjectBase obj)
        {
        }

        /// <summary>
        ///     Called when an object detection is attempted, but the object is not seen.
        /// </summary>
        /// <param name="obj">Object that was attempted to detect</param>
        /// <remarks>
        ///     This method is called on each detectable object that is not seen.
        ///     It is called after <see cref="DetectableObjectBase.OnObjectGhostDetected"/> method is called
        ///     on the detected object.
        /// </remarks>
        protected virtual void OnObjectGhostDetected([NotNull] DetectableObjectBase obj)
        {
        }

        /// <summary>
        ///     Called when an object detection is attempted, but the object is not seen
        ///     or when object cannot be seen and ghost processing is disabled.     
        /// </summary>
        /// <param name="obj">Object that was attempted to detect</param>
        /// <remarks>
        ///     This method is called on each detectable object that is not seen.
        ///     It is called after <see cref="DetectableObjectBase.OnObjectDetectionFailed"/> method is called
        ///     on the detected object.
        /// </remarks>
        protected virtual void OnObjectDetectionFailed([NotNull] DetectableObjectBase obj)
        {
        }

        /// <summary>
        ///     Called when an object detection starts.
        /// </summary>
        /// <param name="obj">Object that was detected</param>
        /// <remarks>
        ///     This method is called when object is newly detected.
        /// </remarks>
        protected virtual void OnObjectDetectionStart([NotNull] DetectableObjectBase obj)
        {
        }

        /// <summary>
        ///     Called when an object detection ends.
        /// </summary>
        /// <param name="obj">Object that was detected</param>
        /// <remarks>
        ///     This method is called when object is no longer detected.
        /// </remarks>
        protected virtual void OnObjectDetectionEnd([NotNull] DetectableObjectBase obj)
        {
        }

        /// <summary>
        ///     Removes the given object from the list of detected objects.
        /// </summary>
        /// <param name="obj">Object to remove from the list of detected objects</param>
        /// <remarks>
        ///     This method is called when object is no longer detected.
        /// </remarks>
        private void TryRemoveDetectedObject([NotNull] DetectableObjectBase obj)
        {
            int nRemoved = _detectedObjects.RemoveAll(o => ReferenceEquals(o, obj));
            if (nRemoved > 0) OnObjectDetectionEnd(obj);
        }

        /// <summary>
        ///     Adds the given object to the list of detected objects.
        /// </summary>
        /// <param name="obj">Object to add to the list of detected objects</param>
        /// <remarks>
        ///     This method is called when object is newly detected.
        /// </remarks>
        private void TryAddDetectedObject([NotNull] DetectableObjectBase obj)
        {
            if (_detectedObjects.Contains(obj)) return;
            _detectedObjects.Add(obj);
            OnObjectDetectionStart(obj);
        }

        protected void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (DetectionSettings.Instance.gizmosDrawModeForDetectors == GizmosDrawMode.Selected) return;
            DrawGizmos();
#endif
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (DetectionSettings.Instance.gizmosDrawModeForDetectors != GizmosDrawMode.Selected) return;
            DrawGizmos();
#endif
        }

        protected void DrawGizmos()
        {
#if UNITY_EDITOR
            // Update zone data and draw zone gizmos
            DetectionZone = GetDetectionZone();
            DetectionZone.DrawGizmos(raycastLayerMask);

            // Check if should debug-draw objects data
            if (!DetectionSettings.Instance.drawDetectionPoints) return;

            // Draw detection objects information
            IReadOnlyList<DetectableObjectBase> detectableObjects = DetectableObjectBase.GetAllDetectableObjects();
            for (int index = 0; index < detectableObjects.Count; index++)
            {
                DetectableObjectBase obj = detectableObjects[index];

                // Update when editor is not playing
                if (!Application.isPlaying)
                {
                    obj.EnsureDetectionListIsCreated();
                    obj.UpdateDetectionPositions();
                }

                // Check if object is seen
                for (int i = 0; i < obj.DetectionPositions.Length; i++)
                {
                    float3 point = obj.DetectionPositions[i];

                    // Check if object is seen and draw gizmos accordingly
                    SpotResult isSeen = DetectionZone.IsPointSeen(point, raycastLayerMask);

                    ObjectDetectionContext context = new(obj, this);
                    switch (isSeen)
                    {
                        case SpotResult.Outside:
                            Gizmos.color = DetectionSettings.Instance.gizmosColorObjectOutsideOfDetectionZone;
                            break;
                        case SpotResult.InsideObstructed:
                            Gizmos.color = DetectionSettings.Instance.gizmosColorObjectInsideZoneUndetected; break;
                        case SpotResult.InsideSeen
                            when obj.CanBeDetected(context):
                            Gizmos.color = DetectionSettings.Instance.gizmosColorObjectIndideZoneDetected; break;
                        case SpotResult.InsideSeen:
                            Gizmos.color = DetectionSettings.Instance.gizmosColorObjectInsideZoneGhost; break;
                    }

                    Gizmos.DrawSphere(point, DetectionSettings.Instance.detectionPointRadius);
                }

            }
#endif
        }
    }
}