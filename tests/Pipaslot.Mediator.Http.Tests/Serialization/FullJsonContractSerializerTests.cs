using Pipaslot.Mediator.Http.Serialization;
using System;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization
{
    public class FullJsonContractSerializerTests : ContractSerializerTestBase
    {
        [Fact]
        public void DeserializeResponse_ShouldCallVerifyCredibility()
        {
            var exception = new Exception();
            ResultProviderMock
                .Setup(p => p.VerifyCredibility(typeof(Result)))
                .Throws(exception);

            var sut = CreateSerializer();
            var responseString = sut.SerializeResponse(new MediatorResponse(true, new object[] { new Result() }, new string[0]));
            var actualException = Assert.Throws<MediatorHttpException>(() => sut.DeserializeResponse<Result>(responseString));

            Assert.Equal(exception, actualException.InnerException);
        }

        protected override IContractSerializer CreateSerializer()
        {
            return new FullJsonContractSerializer(ActionProviderMock.Object, ResultProviderMock.Object);
        }
    }
}
