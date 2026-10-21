using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Ember.Collections;
using Ember.Utils;
using Humanizer;

namespace Ember.Benchmark;


public class Program {

    public static void Main(string[] args) {

        // const int Count = 1000000;
        // const int KeyRange = 1000;

        // InlineConcurrentDictionary<long, long> dict = InlineConcurrentDictionary<long, long>.Allocate();

        // Random random = new(0);
        // for (int i = 0; i < Count; i++) {
        //     long key = random.NextInt64(KeyRange) - KeyRange / 2;
        //     long value = random.NextInt64();

        //     dict.Set(key, value);
        // }

        // var bm = new UnmanagedDictionaryBenchmark() {
        // };

        // bm.Setup();

        // // Console.WriteLine(bm.MicrosoftMixedSimple());
        // // Console.WriteLine(bm.EmberMixedSimple());
        // Console.WriteLine(bm.EmberReadConcurrent());
        // Console.WriteLine(bm.MicrosoftReadSimple());

        // Console.WriteLine(bm.EmberReadConcurrent());

        var summary = BenchmarkRunner.Run<UnmanagedDictionaryBenchmark>(
            ManualConfig.Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator),
            args
        );
    }
}

/*

Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
   at Ember.Collections.InlineConcurrentHashTable`5.ResizeBuckets(BucketTable oldTable, Bucket& triggerBucket) in /dat/Projects/CS/Game/Libraries/Guac/Libraries/Ember/Ember/Collections/InlineConcurrentHashTable.cs:line 238
   at Ember.Benchmark.UnmanagedDictionaryBenchmark.Setup() in /dat/Projects/CS/Game/Libraries/Guac/Libraries/Ember/Ember.Benchmarks/UnmanagedDictionaryBenchmark.cs:line 47
   at Ember.Benchmark.Program.Main(String[] args) in /dat/Projects/CS/Game/Libraries/Guac/Libraries/Ember/Ember.Benchmarks/Program.cs:line 20

*/