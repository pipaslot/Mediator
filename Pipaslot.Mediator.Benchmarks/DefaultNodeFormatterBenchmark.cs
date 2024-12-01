using BenchmarkDotNet.Attributes;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatting;

namespace Pipaslot.Mediator.Benchmarks;

[MemoryDiagnoser]
public class DefaultNodeFormatterBenchmark
{
    private static readonly RuleSet _ruleSet = CreateData();

    private static RuleSet CreateData(int depth = 3)
    {
        var set = new RuleSet([
            new Rule("R1: 1", "1", RuleOutcome.Allow),
            new Rule("R1: 2", "1", RuleOutcome.Deny),
            new Rule("R1: 2", "1", RuleOutcome.Unavailable),
        ]);
        for (var i = 0; i < depth; i++)
        {
            set.RuleSets.Add(CreateData(depth -1));
        }
        return set;
    }


    [Benchmark]
    public void Reduce()
    {
        var nodes = _ruleSet.Reduce();
    }

    [Benchmark]
    public void ReduceAndFormat()
    {
        var sut = new DefaultNodeFormatter();
        var nodes = _ruleSet.Reduce();
        sut.Format(nodes);
    }
}