using EnergyShare_v3.Domain.Entities.PointsAccesses;
using FluentAssertions;

namespace EnergyShare_v3.Backend.Tests
{
    public class PointAccessTests
    {
        [Fact]
        public void Create_ShouldSucceed_WhenDataIsValid()
        {
            var result = PointAccess.Create(
                userId: Guid.NewGuid(),
                adresseLine1: "Rue des Fleurs 12",
                codePostal: "1000",
                fournisseur: "Engie",
                smartMeter: "1SJ-TEST-0001",
                ean: "541448900000000001",
                isInjectionPoint: true);

            result.IsSuccess.Should().BeTrue();

            var pointAccess = result.Value;
            pointAccess.AdresseLine1.Should().Be("Rue des Fleurs 12");
            pointAccess.CodePostal.Should().Be("1000");
            pointAccess.Fournisseur.Should().Be("Engie");
            pointAccess.IsInjectionPoint.Should().BeTrue();
            pointAccess.EstActif.Should().BeTrue();
        }

        [Fact]
        public void Create_ShouldFail_WhenUserIdIsEmpty()
        {
            var result = PointAccess.Create(
                userId: Guid.Empty,
                adresseLine1: "Rue des Fleurs 12",
                codePostal: "1000",
                fournisseur: "Engie",
                smartMeter: "1SJ-TEST-0001",
                ean: "541448900000000001",
                isInjectionPoint: true);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldFail_WhenCodePostalIsInvalid()
        {
            var result = PointAccess.Create(
                userId: Guid.NewGuid(),
                adresseLine1: "Rue des Fleurs 12",
                codePostal: "ABC",
                fournisseur: "Engie",
                smartMeter: "1SJ-TEST-0001",
                ean: "541448900000000001",
                isInjectionPoint: true);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldFail_WhenFournisseurIsMissing()
        {
            var result = PointAccess.Create(
                userId: Guid.NewGuid(),
                adresseLine1: "Rue des Fleurs 12",
                codePostal: "1000",
                fournisseur: "",
                smartMeter: "1SJ-TEST-0001",
                ean: "541448900000000001",
                isInjectionPoint: true);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Desactiver_ShouldSetEstActifToFalse()
        {
            var pointAccess = PointAccess.Create(
                userId: Guid.NewGuid(),
                adresseLine1: "Rue des Fleurs 12",
                codePostal: "1000",
                fournisseur: "Engie",
                smartMeter: "1SJ-TEST-0001",
                ean: "541448900000000001",
                isInjectionPoint: true).Value;

            var result = pointAccess.Desactiver();

            result.IsSuccess.Should().BeTrue();
            pointAccess.EstActif.Should().BeFalse();
            pointAccess.DesactiveAt.Should().NotBeNull();
        }

        [Fact]
        public void Desactiver_ShouldBeIdempotent_WhenAlreadyInactive()
        {
            var pointAccess = PointAccess.Create(
                userId: Guid.NewGuid(),
                adresseLine1: "Rue des Fleurs 12",
                codePostal: "1000",
                fournisseur: "Engie",
                smartMeter: "1SJ-TEST-0001",
                ean: "541448900000000001",
                isInjectionPoint: true).Value;

            pointAccess.Desactiver();

            var secondResult = pointAccess.Desactiver();

            secondResult.IsSuccess.Should().BeTrue();
            pointAccess.EstActif.Should().BeFalse();
        }

        [Fact]
        public void SetCoordinates_ShouldSetLatitudeAndLongitude()
        {
            var pointAccess = PointAccess.Create(
                userId: Guid.NewGuid(),
                adresseLine1: "Avenue des Arts 21",
                codePostal: "1000",
                fournisseur: "Engie",
                smartMeter: "1SJ-TEST-0001",
                ean: "541448900000000001",
                isInjectionPoint: false).Value;

            pointAccess.SetCoordinates(
                latitude: 50.84611067348397,
                longitude: 4.369348589036705);

            pointAccess.Latitude.Should().Be(50.84611067348397);
            pointAccess.Longitude.Should().Be(4.369348589036705);
        }

    }
}
