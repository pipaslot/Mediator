using NSubstitute;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System.Net;

namespace Pipaslot.Mediator.Http.Tests;

public class ServerMediatorUrlFormatterTests
{
    [Fact]
    public void FormatHttpGet_ReturnsCorrectUrl()
    {
        // Arrange
        var expectedJson = "some%20encoded%20json";
        var decodedJson = WebUtility.UrlDecode(expectedJson);
        var expectedParamName = MediatorConstants.ActionQueryParamName;
        var expectedEndpoint = "/api/mediator";

        var options = new ServerMediatorOptions
        {
            Endpoint = expectedEndpoint
        };

        var action = Substitute.For<IMediatorAction>();

        var serializer = Substitute.For<IContractSerializer>();
        serializer
            .SerializeRequest(Arg.Any<IMediatorAction>())
            .Returns(new SerializedRequest(expectedJson, []));

        var formatter = new ServerMediatorUrlFormatter(options, serializer);

        // Act
        var result = formatter.FormatHttpGet(action);

        // Assert
        var expectedUrl = $"{expectedEndpoint}?{expectedParamName}={decodedJson}";
        Assert.Equal(expectedUrl, result);
    }
}
