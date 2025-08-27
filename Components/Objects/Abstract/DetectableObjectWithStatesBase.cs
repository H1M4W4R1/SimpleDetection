using System;
using Systems.SimpleDetection.Data;
using Systems.SimpleDetection.Data.Enums;

namespace Systems.SimpleDetection.Components.Objects.Abstract
{
    /// <summary>
    ///     Detectable object with automatic state support.
    /// </summary>
    public abstract class DetectableObjectWithStatesBase : DetectableObjectBase
    {
        private DetectionState _state;

#region BASE Implementation

        protected internal sealed override void OnDetected(ObjectDetectionContext context)
        {
            base.OnDetected(context);
            TryUpdateState(DetectionState.Detected);
        }

        protected internal sealed override void OnObjectDetectionFailed(ObjectDetectionContext context)
        {
            base.OnObjectDetectionFailed(context);
            TryUpdateState(DetectionState.NotDetected);
        }

        protected internal sealed override void OnObjectGhostDetected(ObjectDetectionContext context)
        {
            base.OnObjectGhostDetected(context);
            TryUpdateState(DetectionState.GhostDetected);
        }

#endregion

        /// <summary>
        ///     Attempts to update the current state of object detection.
        /// </summary>
        /// <param name="newState">New state of object detection</param>
        /// <remarks>
        ///     This method is called by the detection system to try to update the state of the object.
        ///     It will handle both stay in specific state and transition between states.
        /// </remarks>
        private void TryUpdateState(DetectionState newState)
        {
            // Handle stay in specific state
            if (_state == newState)
            {
                switch (newState)
                {
                    case DetectionState.Unknown: break; // Do nothing if unknown
                    case DetectionState.Detected:
                        OnStayAsAnyDetected();
                        OnStayAsDetected();
                        break;
                    case DetectionState.GhostDetected:
                        OnStayAsAnyDetected();
                        OnStayAsGhostDetected();
                        break;
                    case DetectionState.NotDetected: OnStayAsUndetected(); break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            $"State {newState} is not supported. Something has changed?");
                }

                return;
            }

            // Handle transition
            DetectionState previousState = _state;
            _state = newState;

            // Perform transitions
            switch (previousState)
            {
                case DetectionState.Unknown: break; // Do nothing if unknown
                case DetectionState.NotDetected when newState == DetectionState.GhostDetected: // None -> Ghost
                    OnDetectionStartAsAny();
                    OnDetectionStartAsGhost();
                    break;
                case DetectionState.NotDetected when newState == DetectionState.Detected: // None -> Detected
                    OnDetectionStartAsAny();
                    OnDetectionStartAsDetected();
                    break;
                case DetectionState.GhostDetected // Ghost -> None
                    when newState == DetectionState.NotDetected:
                    OnDetectionEndAsAny();
                    OnDetectionEndAsGhost();
                    break;
                case DetectionState.Detected // Detected -> None
                    when newState == DetectionState.NotDetected:
                    OnDetectionEndAsAny();
                    OnDetectionEndAsDetected();
                    break;
                case DetectionState.GhostDetected // Ghost -> Detected
                    when newState == DetectionState.Detected:
                    OnDetectionEndAsGhost();
                    OnDetectionStartAsDetected();
                    break;
                case DetectionState.Detected // Detected -> Ghost
                    when newState == DetectionState.GhostDetected:
                    OnDetectionEndAsDetected();
                    OnDetectionStartAsGhost();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"State {newState} is not supported. Something has changed?");
            }
        }

        /// <summary>
        ///     Called when object is ghost detected for another frame.
        /// </summary>
        protected virtual void OnStayAsGhostDetected()
        {
        }

        /// <summary>
        ///     Called when object is detected for another frame.
        /// </summary>
        protected virtual void OnStayAsDetected()
        {
        }

        /// <summary>
        ///     Called when object is either detected or ghost detected for another frame.
        /// </summary>
        /// <remarks>
        ///     This method is called when object is either detected or ghost detected for another frame.
        ///     It is called after before <see cref="OnStayAsDetected"/> or <see cref="OnStayAsGhostDetected"/> method is called.
        /// </remarks>
        protected virtual void OnStayAsAnyDetected()
        {
        }

        /// <summary>
        ///     Called when object is not seen for another frame.
        /// </summary>
        protected virtual void OnStayAsUndetected()
        {
        }


        /// <summary>
        ///     Called when object starts being ghost detected.
        /// </summary>
        /// <remarks>
        ///     This method is called when the object is ghost detected for the first time.
        /// </remarks>
        protected virtual void OnDetectionStartAsGhost()
        {
        }

        /// <summary>
        ///     Called when object starts being detected.
        /// </summary>
        /// <remarks>
        ///     This method is called when the object is detected for the first time.
        /// </remarks>
        protected virtual void OnDetectionStartAsDetected()
        {
        }

        /// <summary>
        ///     Called when object starts being detected (either as detected or as ghost).
        /// </summary>
        /// <remarks>
        ///     This method is called when the object is detected or ghost detected for the first time.
        ///     It is called before <see cref="OnDetectionStartAsGhost"/> or <see cref="OnDetectionStartAsDetected"/>.
        ///     Not called when object is switching from ghost detected to detected or vice versa.
        /// </remarks>
        protected virtual void OnDetectionStartAsAny()
        {
        }

        /// <summary>
        ///     Called when object stops being ghost detected.
        /// </summary>
        /// <remarks>
        ///     This method is called when the object is no longer ghost detected.
        /// </remarks>
        protected virtual void OnDetectionEndAsGhost()
        {
        }

        /// <summary>
        ///     Called when object stops being detected.
        /// </summary>
        /// <remarks>
        ///     This method is called when the object is no longer detected.
        /// </remarks>
        protected virtual void OnDetectionEndAsDetected()
        {
        }

        /// <summary>
        ///     Called when object stops being detected (either as detected or as ghost).
        /// </summary>
        /// <remarks>
        ///     This method is called when the object is no longer detected or ghost detected.
        ///     It is called before <see cref="OnDetectionEndAsGhost"/> or <see cref="OnDetectionEndAsDetected"/>.
        ///     Not called when object is switching from ghost detected to detected or vice versa.
        /// </remarks>
        protected virtual void OnDetectionEndAsAny()
        {
        }
    }
}