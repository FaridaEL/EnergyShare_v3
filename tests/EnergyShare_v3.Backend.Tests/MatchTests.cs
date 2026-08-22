using EnergyShare_v3.Domain.Entities.Matchs;
using FluentAssertions;


namespace EnergyShare_v3.Backend.Tests
{
    public class MatchTests
    {
        [Fact]
        public void Create_ShouldFail_WhenSellerAndBuyerAreTheSame()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var result = Match.Create(id, id, 1.5m);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }


        [Fact]
        public void Create_ShouldFail_WhenDistanceIsNegative()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            var buyerId = Guid.NewGuid();

            // Act
            var result = Match.Create(sellerId, buyerId, -1m);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldSucceed_WhenDataIsValid()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            var buyerId = Guid.NewGuid();

            // Act
            var result = Match.Create(sellerId, buyerId, 2.3m);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.PointAccessVendeurId.Should().Be(sellerId);
            result.Value.PointAccessAcheteurId.Should().Be(buyerId);
            result.Value.DistanceCalculee.Should().Be(2.3m);
        }

        [Fact]
        public void Create_ShouldFail_WhenSellerIdIsEmpty()
        {
            var result = Match.Create(Guid.Empty, Guid.NewGuid(), 1.5m);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldFail_WhenBuyerIdIsEmpty()
        {
            var result = Match.Create(Guid.NewGuid(), Guid.Empty, 1.5m);

            result.IsSuccess.Should().BeFalse();
        }

        //[Fact]
        //public void CalculerDistanceKm_ShouldReturnAbout4Point73Km()
        //{
        //    var distance = SearchPotentialMatchesHandler.CalculerDistanceKm(
        //        50.851504,
        //        4.356767,
        //        50.866007,
        //        4.293434);

        //    distance.Should().BeApproximately(4.73m, 0.05m);
        //}


    }

}
