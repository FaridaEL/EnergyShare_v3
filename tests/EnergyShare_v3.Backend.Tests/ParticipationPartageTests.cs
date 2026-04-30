using EnergyShare_v3.Domain.Entities;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Entities.PointsAccesses;
using EnergyShare_v3.Domain.Enums;
using FluentAssertions;

namespace EnergyShare_v3.Backend.Tests;

public class ParticipationPartageTests
{
    [Fact]
    public void DefinirCommeInterlocuteurUnique_ShouldFail_WhenRoleIsNotVendeur()
    {
        var pointAccess = PointAccess.Create(
        userId: Guid.NewGuid(),
        adresseLine1: "Rue des Fleurs 12",
        codePostal: "1000",
        fournisseur: "Engie",
        smartMeter: "1SJ-TEST-0001",
        ean: "541448900000000001",
        isInjectionPoint: true
         ).Value;

        var participation = new ParticipationPartage
        {
            Id = Guid.NewGuid(),
            UserRolePartage = UserRolePartage.Acheteur,
            PointAccessId = pointAccess.Id,
            PointAccess = pointAccess
        };

        var result = participation.DefinirCommeInterlocuteurUnique();

        result.IsSuccess.Should().BeFalse();
        participation.IsInterlocuteurUnique.Should().BeFalse();
    }

    [Fact]
    public void CommuniquerPreavis_ShouldFail_WhenDateIsBeforeJoinedAt()
    {
        var participation = new ParticipationPartage
        {
            Id = Guid.NewGuid(),
            PointAccessId = Guid.NewGuid(),
            JoinedAt = new DateTime(2026, 4, 10)
        };

        var result = participation.CommuniquerPreavis(new DateTime(2026, 4, 1));

        result.IsSuccess.Should().BeFalse();
        participation.DateCommunicationPreavis.Should().BeNull();
        participation.DateSortiePlanifiee.Should().BeNull();
    }

    [Fact]
    public void CommuniquerPreavis_ShouldSucceed_WhenDateIsValid()
    {
        var participation = new ParticipationPartage
        {
            Id = Guid.NewGuid(),
            PointAccessId = Guid.NewGuid(),
            JoinedAt = new DateTime(2026, 4, 1)
        };

        var datePreavis = new DateTime(2026, 4, 10);

        var result = participation.CommuniquerPreavis(datePreavis);

        result.IsSuccess.Should().BeTrue();
        participation.DateCommunicationPreavis.Should().Be(datePreavis);
        participation.DateSortiePlanifiee.Should().Be(datePreavis.AddDays(21));
    }

    [Fact]
    public void Quitter_ShouldFail_WhenExitDateIsBeforeJoinedAt()
    {
        var participation = new ParticipationPartage
        {
            Id = Guid.NewGuid(),
            PointAccessId = Guid.NewGuid(),
            JoinedAt = new DateTime(2026, 4, 10)
        };

        var result = participation.Quitter(new DateTime(2026, 4, 1));

        result.IsSuccess.Should().BeFalse();
        participation.ExitAt.Should().BeNull();
    }

    [Fact]
    public void Quitter_ShouldFail_WhenPreavisIsNotRespected()
    {
        var participation = new ParticipationPartage
        {
            Id = Guid.NewGuid(),
            PointAccessId = Guid.NewGuid(),
            JoinedAt = new DateTime(2026, 4, 1)
        };

        participation.CommuniquerPreavis(new DateTime(2026, 4, 10));

        var result = participation.Quitter(new DateTime(2026, 4, 20));

        result.IsSuccess.Should().BeFalse();
        participation.ExitAt.Should().BeNull();
    }

    [Fact]
    public void Quitter_ShouldSucceed_WhenExitDateIsValid()
    {
        var participation = new ParticipationPartage
        {
            Id = Guid.NewGuid(),
            PointAccessId = Guid.NewGuid(),
            JoinedAt = new DateTime(2026, 4, 1)
        };

        var datePreavis = new DateTime(2026, 4, 10);
        participation.CommuniquerPreavis(datePreavis);

        var dateSortie = datePreavis.AddDays(21);

        var result = participation.Quitter(dateSortie);

        result.IsSuccess.Should().BeTrue();
        participation.ExitAt.Should().Be(dateSortie);
    }
}