using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System.Net;
using Xunit;

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

        var actionMock = new Mock<IMediatorAction>();

        var serializerMock = new Mock<IContractSerializer>();
        serializerMock
            .Setup(s => s.SerializeRequest(It.IsAny<IMediatorAction>()))
            .Returns(new SerializedRequest(expectedJson, []));

        var formatter = new ServerMediatorUrlFormatter(options, serializerMock.Object);

        // Act
        var result = formatter.FormatHttpGet(actionMock.Object);

        // Assert
        var expectedUrl = $"{expectedEndpoint}?{expectedParamName}={decodedJson}";
        Assert.Equal(expectedUrl, result);
    }
}
