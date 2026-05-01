using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;
using FluentAssertions;

namespace EnergyShare_v3.Backend.Tests
{
    public class UserTests
    {
        [Fact]
        public void Create_ShouldSucceed_WhenEmailIsValid()
        {
            var result = User.Create("sarah.dupont@example.com");

            result.IsSuccess.Should().BeTrue();
            result.Value.Email.Should().Be("sarah.dupont@example.com");
            result.Value.UserName.Should().Be("sarah.dupont@example.com");
            result.Value.Status.Should().Be(UserStatus.Actif);
        }

        [Fact]
        public void Create_ShouldFail_WhenEmailIsMissing()
        {
            var result = User.Create("");

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void UpdateUserIdentity_ShouldUpdatePersonalInformation()
        {
            var user = User.Create("sarah.dupont@example.com").Value;

            user.UpdateUserIdentity(" Sarah ", " Dupont ", " 0470000012 ");

            user.FirstName.Should().Be("Sarah");
            user.LastName.Should().Be("Dupont");
            user.PhoneNumber.Should().Be("0470000012");
        }

        [Fact]
        public void UpdateLegalInformation_ShouldFail_WhenNumeroEntrepriseWithoutSocieteName()
        {
            var user = User.Create("contact@example.com").Value;

            var result = user.UpdateLegalInformation(null, "BE0123456789");

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Deactivate_ShouldSetStatusToInactive()
        {
            var user = User.Create("sarah.dupont@example.com").Value;

            user.Deactivate();

            user.Status.Should().Be(UserStatus.Inactif);
        }
    }
}