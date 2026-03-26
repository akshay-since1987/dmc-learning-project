using FluentAssertions;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Tests.Models;

public class PagedResultTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var items = new List<string> { "a", "b", "c" };
        var paged = new PagedResult<string>(items, totalCount: 10, pageIndex: 1, pageSize: 3);

        paged.Items.Should().HaveCount(3);
        paged.TotalCount.Should().Be(10);
        paged.PageIndex.Should().Be(1);
        paged.PageSize.Should().Be(3);
    }

    [Fact]
    public void TotalPages_ShouldCompute()
    {
        var paged = new PagedResult<int>([], totalCount: 25, pageIndex: 1, pageSize: 10);

        paged.TotalPages.Should().Be(3);
    }

    [Fact]
    public void HasPrevious_ShouldBeFalseForFirstPage()
    {
        var paged = new PagedResult<int>([], totalCount: 25, pageIndex: 1, pageSize: 10);

        paged.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasPrevious_ShouldBeTrueForSecondPage()
    {
        var paged = new PagedResult<int>([], totalCount: 25, pageIndex: 2, pageSize: 10);

        paged.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void HasNext_ShouldBeTrueWhenMorePagesExist()
    {
        var paged = new PagedResult<int>([], totalCount: 25, pageIndex: 1, pageSize: 10);

        paged.HasNext.Should().BeTrue();
    }

    [Fact]
    public void HasNext_ShouldBeFalseOnLastPage()
    {
        var paged = new PagedResult<int>([], totalCount: 25, pageIndex: 3, pageSize: 10);

        paged.HasNext.Should().BeFalse();
    }

    [Fact]
    public void EmptyResult_ShouldHaveZeroTotalPages()
    {
        var paged = new PagedResult<string>([], totalCount: 0, pageIndex: 1, pageSize: 10);

        paged.TotalPages.Should().Be(0);
        paged.HasNext.Should().BeFalse();
        paged.HasPrevious.Should().BeFalse();
    }
}
