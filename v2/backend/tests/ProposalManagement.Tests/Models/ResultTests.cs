using FluentAssertions;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Tests.Models;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Failure_ShouldReturnFailureResult()
    {
        var result = Result.Failure("Something went wrong");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Something went wrong");
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public void Failure_WithCustomStatusCode_ShouldReturnCorrectCode()
    {
        var result = Result.Failure("Conflict", 409);

        result.StatusCode.Should().Be(409);
    }

    [Fact]
    public void NotFound_ShouldReturn404()
    {
        var result = Result.NotFound("Item not found");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Item not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public void Forbidden_ShouldReturn403()
    {
        var result = Result.Forbidden();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
    }
}

public class ResultOfTTests
{
    [Fact]
    public void Success_ShouldContainData()
    {
        var result = Result<string>.Success("hello");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be("hello");
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Failure_ShouldHaveNullData()
    {
        var result = Result<string>.Failure("error");

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
    }

    [Fact]
    public void NotFound_ShouldHaveNullData()
    {
        var result = Result<int>.NotFound();

        result.Data.Should().Be(0); // default(int)
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public void Forbidden_ShouldReturn403WithMessage()
    {
        var result = Result<object>.Forbidden("Access denied");

        result.StatusCode.Should().Be(403);
        result.Error.Should().Be("Access denied");
    }
}
