using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization
{
    public abstract class ContractSerializer_InterfaceTestBase : ContractSerializerBaseTest
    {
        [Fact]
        public void Response_ResultTypeIsInterface_WillKeepTheResultType()
        {
            var sut = CreateSerializer();
            var result = new Result();
            var responseString = sut.SerializeResponse(new MediatorResponse(true, new object[] { result }));
            var deserialized = sut.DeserializeResponse<IResult>(responseString);
            Assert.Equal(result.GetType(), deserialized.Result.GetType());
        }

        public interface IResult
        {
        }

        public class Result : IResult
        {
            public int Index { get; set; }
        }
    }
}