using NUnit.Framework;
using Systems.SimpleCore.Operations;
using Systems.SimpleDetection.Operations;

namespace Systems.SimpleDetection.Tests
{
    public sealed class DetectionOperationResultTests : SimpleDetectionTestBase
    {
        [Test]
        public void Permitted_ReturnsDetectionSuccess()
        {
            OperationResult result = DetectionOperations.Permitted();

            Assert.IsTrue(result);
            Assert.IsTrue(OperationResult.IsFromSystem(result, DetectionOperations.SYSTEM_DETECTION));
            Assert.AreEqual(OperationResult.SUCCESS_PERMITTED, result.resultCode);
        }

        [Test]
        public void ErrorFactories_ReturnDetectionErrors()
        {
            AssertError(DetectionOperations.IsGhost(), DetectionOperations.ERROR_IS_GHOST);
            AssertError(
                DetectionOperations.InvalidDetectableObject(),
                DetectionOperations.ERROR_INVALID_DETECTABLE_OBJECT);
        }

        [Test]
        public void IsSeen_ReturnsSpecificSuccessCode()
        {
            OperationResult result = DetectionOperations.IsSeen();

            Assert.IsTrue(result);
            Assert.IsTrue(OperationResult.IsFromSystem(result, DetectionOperations.SYSTEM_DETECTION));
            Assert.AreEqual(DetectionOperations.SUCCESS_IS_SEEN, result.resultCode);
        }

        private static void AssertError(OperationResult result, ushort expectedCode)
        {
            Assert.IsFalse(result);
            Assert.IsTrue(OperationResult.IsError(result));
            Assert.IsTrue(OperationResult.IsFromSystem(result, DetectionOperations.SYSTEM_DETECTION));
            Assert.AreEqual(expectedCode, result.resultCode);
        }
    }
}
