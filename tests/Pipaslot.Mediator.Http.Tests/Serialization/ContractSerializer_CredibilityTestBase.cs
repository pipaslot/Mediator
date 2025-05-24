using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization;

public abstract class ContractSerializer_CredibilityTestBase : ContractSerializerBaseTest
{
    [Fact]
    public void Request_ShouldCallVerifyCredibility()
    {
        var action = new PublicPropertyGetterAndInitSetterContract();
        VerifyRequestCredibility(action, action.GetType());
    }

    protected void VerifyRequestCredibility(IMediatorAction action, params Type[] toBeVerified)
    {
        CredibleProviderMock = new Mock<ICredibleProvider>(MockBehavior.Strict);
        foreach (var type in toBeVerified)
        {
            CredibleProviderMock
                .Setup(p => p.VerifyCredibility(type));
        }

        var sut = CreateSerializer();
        var serialized = sut.SerializeRequest(action);
        sut.DeserializeRequest(serialized.Json.ConvertToStream(), serialized.Streams);

        CredibleProviderMock.VerifyAll();
    }

    [Fact]
    public void Response_SingleObject_ShouldCallVerifyCredibility()
    {
        var result = new Result();
        VerifyResponseCredibility(result, result.GetType());
    }

    [Fact]
    public void Response_Collection_ShouldCallVerifyCredibility()
    {
        var result = Array.Empty<Result>();
        VerifyResponseCredibility(result, result.GetType());
    }

    protected void VerifyResponseCredibility(object result, params Type[] toBeVerified)
    {
        CredibleProviderMock = new Mock<ICredibleProvider>(MockBehavior.Strict);
        foreach (var type in toBeVerified)
        {
            CredibleProviderMock
                .Setup(p => p.VerifyCredibility(type));
        }

        var sut = CreateSerializer();
        var responseString = sut.SerializeResponse(new MediatorResponse(true, [result]));
        sut.DeserializeResponse<Result>(responseString);

        CredibleProviderMock.VerifyAll();
    }

    public class Result
    {
        public int Index { get; set; }
    }

    public class PublicPropertyGetterAndInitSetterContract : IMessage
    {
        public string Name { get; init; } = "";
    }
}