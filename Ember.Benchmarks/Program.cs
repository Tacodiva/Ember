using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Running;

namespace Ember.Benchmark;

public class Program {

    public static void Main(string[] args) {
        var summary = BenchmarkRunner.Run<UnmanagedDictionaryBenchmark>(null, args);
    }
}