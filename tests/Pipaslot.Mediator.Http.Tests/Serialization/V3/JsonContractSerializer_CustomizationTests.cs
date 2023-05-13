using Moq;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using System;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3
{
    /// <summary>
    /// Test customization for ignoring read only properties enabled in mediator options
    /// </summary>
    public class JsonContractSerializer_IgnoreReadOnlyPropertiesCustomizationTests
    {
        [Fact]
        public void SerializeRequest_IgnorePropertiesWithoutPublicSetter()
        {
            var sut = CreateSerializer();

            var serialized = sut.SerializeRequest(new ActionWithReadOnlyProperties());
            Assert.DoesNotContain(nameof(ActionWithReadOnlyProperties.FullName), serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(nameof(ActionWithReadOnlyProperties.GetterOnly), serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(nameof(DtoWithReadOnlyProperties.FullName), serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(nameof(DtoWithReadOnlyProperties.GetterOnly), serialized, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void SerializeResponse_IgnorePropertiesWithoutPublicSetter()
        {
            var sut = CreateSerializer();

            var response = new MediatorResponse(true, new object[] { new ActionWithReadOnlyProperties() });
            var serialized = sut.SerializeResponse(response);
            Assert.DoesNotContain(nameof(ActionWithReadOnlyProperties.FullName), serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(nameof(ActionWithReadOnlyProperties.GetterOnly), serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(nameof(DtoWithReadOnlyProperties.FullName), serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(nameof(DtoWithReadOnlyProperties.GetterOnly), serialized, StringComparison.OrdinalIgnoreCase);
        }

        private IContractSerializer CreateSerializer()
        {
            var credibleProviderMock = new Mock<ICredibleProvider>();
            var optionsMock = new Mock<IMediatorOptions>();
            optionsMock.SetupGet(x => x.IgnoreReadOnlyProperties).Returns(true);
            return new JsonContractSerializer(credibleProviderMock.Object, optionsMock.Object);
        }

        public class ActionWithReadOnlyProperties : IMessage
        {
            public string FirstName { get; set; } = "F1";
            public string Lastname { get; set; } = "L1";

            public string FullName => $"{FirstName} {Lastname}";

            public string GetterOnly { get; private set; } = "P1";

            public DtoWithReadOnlyProperties Dto { get; set; } = new();
        }
        public class DtoWithReadOnlyProperties
        {
            public string FirstName { get; set; } = "F2";
            public string Lastname { get; set; } = "L2";

            public string FullName => $"{FirstName} {Lastname}";

            public string GetterOnly { get; private set; } = "P1";
        }

    }
}
