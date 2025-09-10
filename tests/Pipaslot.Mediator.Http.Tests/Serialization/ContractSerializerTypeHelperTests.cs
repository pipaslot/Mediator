using Pipaslot.Mediator.Http.Serialization;

namespace Pipaslot.Mediator.Http.Tests.Serialization;
public class ContractSerializerTypeHelperTests
{
    [Test]
    [Arguments("System.Collections.Generic.List`1[[My.Namespace.MyType, My.Assembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]", "System.Collections.Generic.List`1[[My.Namespace.MyType, My.Assembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e")]
    [Arguments("System.Collections.Generic.List`1[[System.Int32]]", "System.Collections.Generic.List`1[[System.Int32, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e")]
    [Arguments("System.Int32", "System.Int32, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798")]
    public void ConvertTypeDefinitions(string expected, string value)
    {
        var result = ContractSerializerTypeHelper.GetTypeWithoutAssembly(value);
        Assert.Equal(expected, result);
    }
}