using Authorization.API.Util;
using System;
using Xunit;

namespace Authorization.API.Tests.Util
{
    public class HashHelperTests
    {
        [Fact]
        [Trait("HashHelper", "ComputeSha256_Success")]
        public void ComputeSha256FromString_ReturnHashValue_Success()
        {
            IHashHelper hashHelper = new HashHelper();

            byte[] hashCalculated = hashHelper.ComputeSha256FromString("pijama");
            byte[] hashExpected = { 115, 57, 166, 45, 177, 29, 139, 95, 190, 27, 171, 105, 166, 192,
                160, 197, 143, 239, 137, 153, 126, 193, 165, 62, 183, 43, 182, 217, 75, 168, 83, 151 };

            Assert.Equal(hashExpected, hashCalculated);
        }

        [Fact]
        [Trait("HashHelper", "ComputeSha256_Failure")]
        public void ComputeSha256FromString_ReturnHashValue_Failure()
        {
            IHashHelper hashHelper = new HashHelper();

            byte[] hashCalculated = hashHelper.ComputeSha256FromString("random");
            byte[] hashExpected = { 115, 57, 166, 45, 177, 29, 139, 95, 190, 27, 171, 105, 166, 192,
                160, 197, 143, 239, 137, 153, 126, 193, 165, 62, 183, 43, 182, 217, 75, 168, 83, 151 };

            Assert.NotEqual(hashExpected, hashCalculated);
        }

        [Fact]
        [Trait("HashHelper", "CompareStringToSHA256_Success")]
        public void CompareStringToSHA256_ReturnBoolean_True()
        {
            IHashHelper hashHelper = new HashHelper();

            byte[] hashExpected = { 115, 57, 166, 45, 177, 29, 139, 95, 190, 27, 171, 105, 166, 192,
                160, 197, 143, 239, 137, 153, 126, 193, 165, 62, 183, 43, 182, 217, 75, 168, 83, 151 };
            String stringUnhashed = "pijama";

            bool returnValue = hashHelper.CompareStringToSHA256(stringUnhashed , hashExpected);

            Assert.True(returnValue);
        }

        [Fact]
        [Trait("HashHelper", "CompareStringToSHA256_Failure")]
        public void CompareStringToSHA256_ReturnBoolean_False()
        {
            IHashHelper hashHelper = new HashHelper();

            byte[] hashExpected = { 115, 57, 166, 45, 177, 29, 139, 95, 190, 27, 171, 105, 166, 192,
                160, 197, 143, 239, 137, 153, 126, 193, 165, 62, 183, 43, 182, 217, 75, 168, 83, 151 };
            String stringUnhashed = "random";

            bool returnValue = hashHelper.CompareStringToSHA256(stringUnhashed, hashExpected);

            Assert.False(returnValue);
        }

    }
}

