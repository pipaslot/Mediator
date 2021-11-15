using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Contracts;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests.Server
{
    public class RequestContractExecutor_DeserializeInputDataTests
    {
        private const string Name = "JSON name";
        private const int Number = 6;
        private Mock<IMediator> _mediatorMock = new();

        [Fact]
        public async Task PublicPropertyGettersAndSetters_WillPass()
        {
            await RunTest<PublicPropertyGettersAndSettersContract>(c => c.Name == Name && c.Number == Number);
        }

        [Fact]
        public async Task ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnly_WillPass()
        {
            await RunTest<ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract>(c => c.Name == Name && c.Number == Number);
        }

        [Fact]
        public async Task ConstructorWithNotMatchingBindingNamesAndWithPrivateGetter_WillFaill()
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                //This is weaknes of Microsoft.Text.Json serializer because if there is no parameterless  constructor and public setters, then it deserialize data via names in constructor parameters
                await RunTest<ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract>(c => c.Name == Name && c.Number == Number);
            });           
        }

        [Fact]
        public async Task PublicPropertyGetterAndInitSetter_WillPass()
        {
            await RunTest<PublicPropertyGetterAndInitSetterContract>(c => c.Name == Name && c.Number == Number);
        }

        [Fact]
        public async Task PositionalRecord_WillPass()
        {
            await RunTest<PositionalRecordContract>(c => c.Name == Name && c.Number == Number);
        }

        private async Task RunTest<TContract>(Expression<Func<TContract, bool>> match) where TContract : IMediatorAction
        {
            var sut = CreateConfigurator(c => c.AddActionsFromAssemblyOf<RequestContractExecutor_DeserializeInputDataTests>());

            var request = CreateRequest(typeof(TContract));
            await sut.ExecuteQuery(request, CancellationToken.None);

            _mediatorMock.Verify(m =>
                m.Dispatch(
                    It.Is<TContract>(match),
                    It.IsAny<CancellationToken>())
            , Times.Once);
        }

        private RequestContractExecutor CreateConfigurator(Action<PipelineConfigurator> setup)
        {
            var serviceCollctionMock = new Mock<IServiceCollection>();
            var configurator = new PipelineConfigurator(serviceCollctionMock.Object);
            setup(configurator);
            return new RequestContractExecutor(_mediatorMock.Object, configurator, null);
        }

        private MediatorRequestSerializable CreateRequest(Type objectType)
        {
            return new MediatorRequestSerializable()
            {
                ObjectName = objectType.AssemblyQualifiedName,
                Json = @"{""Name"":"""+ Name + @""", ""Number"":"+ Number + "}"
            };
        }

        public class PublicPropertyGettersAndSettersContract : IMessage
        {
            public string Name { get; set; }
            public int Number { get; set; }
        }

        public class ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract : IMessage
        {
            public ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract(string name, int number)
            {
                Name = name;
                Number = number;
            }

            public string Name { get; }
            public int Number { get; }
        }

        public class ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract : IMessage
        {
            public ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract(string par1, int par2)
            {
                Name = par1;
                Number = par2;
            }

            public string Name { get; }
            public int Number { get; }
        }

        public class PublicPropertyGetterAndInitSetterContract : IMessage
        {
            public string Name { get; init; }
            public int Number { get; init; }
        }

        public record PositionalRecordContract(string Name, int Number) : IMessage;

    }
}
