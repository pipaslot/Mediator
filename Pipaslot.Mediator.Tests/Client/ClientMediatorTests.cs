using Pipaslot.Mediator.Client;
using Xunit;

namespace Pipaslot.Mediator.Tests.Client
{
    public class ClientMediatorTests
    {

        [Theory]
        [InlineData(
            "System.Collections.Generic.List`1[[My.Namespace.MyType, My.Assembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]",
            "System.Collections.Generic.List`1[[My.Namespace.MyType, My.Assembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e")]
        [InlineData(
            "System.Collections.Generic.List`1[[System.Int32]]",
            "System.Collections.Generic.List`1[[System.Int32, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e")]
        [InlineData(
            "System.Int32", 
            "System.Int32, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798")]

        public void ConvertTypeDefinitions(string expected,string value)
        {
            var result = ClientMediator.GetTypeWithoutAssembly(value);
            Assert.Equal(expected, result);
        }
    }
}
