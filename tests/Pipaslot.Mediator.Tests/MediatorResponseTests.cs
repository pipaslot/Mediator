using System.Collections.Generic;

namespace Pipaslot.Mediator.Tests;

/// <summary>
/// <see cref="MediatorResponse{TResult}"/> exposes <see cref="IMediatorResponse{TResult}.Result"/> as the first
/// result of the requested type. These tests cover reading it when the response holds no value of that type (typically
/// a failed response), which must yield <c>default</c> rather than throw.
/// </summary>
public class MediatorResponseTests
{
    [Fact]
    public void Result_ResponseWithoutResultsOfValueType_ReturnsDefault()
    {
        IMediatorResponse<int> sut = new MediatorResponse<int>(false, new List<object>());

        var result = sut.Result;

        Assert.Equal(0, result);
    }

    [Fact]
    public void Result_FailedResponseCreatedFromErrorMessageForValueType_ReturnsDefault()
    {
        IMediatorResponse<int> sut = new MediatorResponse<int>("error");

        var result = sut.Result;

        Assert.Equal(0, result);
    }

    [Fact]
    public void Result_ResponseWithoutResultsOfReferenceType_ReturnsNull()
    {
        IMediatorResponse<string> sut = new MediatorResponse<string>(false, new List<object>());

        var result = sut.Result;

        Assert.Null(result);
    }

    [Fact]
    public void Result_ResponseContainingValueOfRequestedType_ReturnsIt()
    {
        IMediatorResponse<int> sut = new MediatorResponse<int>(true, new List<object> { "other", 42 });

        var result = sut.Result;

        Assert.Equal(42, result);
    }
}
