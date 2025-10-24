// sample test 
using Xunit;
using CMCS.Models;

namespace CMCS.UnitTest
{
    public class ClaimTests
    {

        [Fact]
        public void Claim_TotalAmount_ShouldBeCalculatedCorrectly()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 10,
                HourlyRate = 250.00m
            };

            // Act
            var total = claim.CalculateTotal();

            // Assert
            Assert.Equal(2500.00m, tota);
        }

        [Fact]
        public void Claim_ShouldNotAcceptNegativeHours()
        {
            // Arrange
            var claim = new Claim();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => claim.HoursWorked = -5);
        }

        [Fact]
        public void Claim_Description_ShouldNotBeEmpty()
        {
            // Arrange
            var claim = new Claim();

            // Act
            claim.Description = "";

            // Assert
            Assert.True(string.IsNullOrEmpty(claim.Description));
        }

    }
}
