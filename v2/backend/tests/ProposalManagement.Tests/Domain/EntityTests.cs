using FluentAssertions;
using ProposalManagement.Domain.Common;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Tests.Domain;

public class BaseEntityTests
{
    [Fact]
    public void NewEntity_ShouldHaveDefaultValues()
    {
        var dept = new Department();

        dept.Id.Should().Be(Guid.Empty);
        dept.IsDeleted.Should().BeFalse();
        dept.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Department_ShouldStoreNames()
    {
        var dept = new Department
        {
            Id = Guid.NewGuid(),
            Name_En = "Water Supply",
            Name_Mr = "पाणी पुरवठा",
            Code = "WS",
            PalikaId = Guid.NewGuid()
        };

        dept.Name_En.Should().Be("Water Supply");
        dept.Name_Mr.Should().Be("पाणी पुरवठा");
        dept.Code.Should().Be("WS");
    }

    [Fact]
    public void Zone_ShouldStoreProperties()
    {
        var zone = new Zone
        {
            Id = Guid.NewGuid(),
            Name_En = "Zone A",
            Name_Mr = "झोन अ",
            Code = "ZA",
            PalikaId = Guid.NewGuid()
        };

        zone.Name_En.Should().Be("Zone A");
        zone.Code.Should().Be("ZA");
    }

    [Fact]
    public void Designation_ShouldInitialize()
    {
        var d = new Designation
        {
            Id = Guid.NewGuid(),
            Name_En = "City Engineer",
            Name_Mr = "शहर अभियंता",
            PalikaId = Guid.NewGuid()
        };

        d.Name_En.Should().Be("City Engineer");
        d.IsActive.Should().BeTrue();
        d.IsDeleted.Should().BeFalse();
    }
}
