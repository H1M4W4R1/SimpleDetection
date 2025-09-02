using Systems.SimpleCore.Operations;

namespace Systems.SimpleDetection.Operations
{
    public static class DetectionOperations
    {
        public const ushort SYSTEM_DETECTION = 0x0006;

        public const ushort ERROR_IS_GHOST = 1;
        public const ushort ERROR_INVALID_DETECTABLE_OBJECT = 2;

        public const ushort SUCCESS_IS_SEEN = 1;
        
        public static OperationResult Permitted()
            => OperationResult.Success(SYSTEM_DETECTION, OperationResult.SUCCESS_PERMITTED);
        
        public static OperationResult IsGhost()
            => OperationResult.Error(SYSTEM_DETECTION, ERROR_IS_GHOST);
        
        public static OperationResult IsSeen()
            => OperationResult.Success(SYSTEM_DETECTION, SUCCESS_IS_SEEN);

        public static OperationResult InvalidDetectableObject() =>
            OperationResult.Error(SYSTEM_DETECTION, ERROR_INVALID_DETECTABLE_OBJECT);
    }
}