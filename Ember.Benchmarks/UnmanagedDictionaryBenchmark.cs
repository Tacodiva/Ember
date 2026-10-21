
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Ember.Collections;

namespace Ember.Benchmark;

[SimpleJob]
// [MemoryDiagnoser]
public class UnmanagedDictionaryBenchmark {

    private const int Count = 1000000;
    private const int KeyRange = 1000;

    public long[] Keys;
    public long[] Values;
    public bool[] Add;

    public ConcurrentDictionary<long, long> DictMicrosoftReadConcurrent;
    public InlineConcurrentDictionary<long, long> DictEmberReadConcurrent;

    public Dictionary<long, long> DictMicrosoftReadSimple;
    public InlineDictionary<long, long> DictEmberReadSimple;

    [GlobalSetup]
    public void Setup() {
        Keys = new long[Count];
        Values = new long[Count];
        Add = new bool[Count];

        DictMicrosoftReadConcurrent = [];
        DictEmberReadConcurrent = InlineConcurrentDictionary<long, long>.Allocate(Count);
        DictMicrosoftReadSimple = [];
        DictEmberReadSimple = InlineDictionary<long, long>.Allocate();

        Random random = new(0);
        for (int i = 0; i < Count; i++) {
            // Keys[i] = random.NextInt64(KeyRange) - KeyRange / 2;
            Keys[i] = random.NextInt64(KeyRange);
            Values[i] = random.NextInt64();
            Add[i] = random.NextDouble() < 0.5f;

            DictMicrosoftReadConcurrent[Keys[i]] = Values[i];
            DictEmberReadConcurrent[Keys[i]] = Values[i];
            DictMicrosoftReadSimple[Keys[i]] = Values[i];
            DictEmberReadSimple[Keys[i]] = Values[i];
        }
    }

    [Benchmark]
    public int MicrosoftMixedConcurrent() {
        ConcurrentDictionary<long, long> dict = [];

        for (int i = 0; i < Count; i++) {
            if (Add[i]) {
                dict[Keys[i]] = Values[i];
            } else {
                dict.Remove(Keys[i], out _);
            }
        }

        return dict.Count;
    }

    [Benchmark]
    public int EmberMixedConcurrent() {
        InlineConcurrentDictionary<long, long> dict = InlineConcurrentDictionary<long, long>.Allocate();

        for (int i = 0; i < Count; i++) {
            if (Add[i]) {
                dict[Keys[i]] = Values[i];
            } else {
                dict.Remove(Keys[i]);
            }
        }

        return dict.Count;
    }

    [Benchmark]
    public int MicrosoftAddConcurrent() {
        ConcurrentDictionary<long, long> dict = [];

        for (int i = 0; i < Count; i++) {
            dict[Keys[i]] = Values[i];
        }

        return dict.Count;
    }

    [Benchmark]
    public int EmberAddConcurrent() {
        InlineConcurrentDictionary<long, long> dict = InlineConcurrentDictionary<long, long>.Allocate();

        for (int i = 0; i < Count; i++) {
            dict.Set(Keys[i], in Values[i]);
        }

        return dict.Count;
    }

    [Benchmark]
    public long MicrosoftReadConcurrent() {
        long sum = 0;
        for (int i = 0; i < Count; i++) {
            sum += DictMicrosoftReadConcurrent.TryGetValue(Keys[i], out long data) ? data : -1;
        }
        return sum;
    }

    [Benchmark]
    public long EmberReadConcurrent() {
        long sum = 0;
        for (int i = 0; i < Count; i++) {
            sum += DictEmberReadConcurrent.TryGetValueAAA(Keys[i], out long data) ? data : -1;
        }
        return sum;
    }


    [Benchmark]
    public int MicrosoftMixedSimple() {
        Dictionary<long, long> dict = [];

        for (int i = 0; i < Count; i++) {
            if (Add[i]) {
                dict[Keys[i]] = Values[i];
            } else {
                dict.Remove(Keys[i], out _);
            }
        }

        return dict.Count;
    }

    [Benchmark]
    public int EmberMixedSimple() {
        InlineDictionary<long, long> dict = InlineDictionary<long, long>.Allocate();

        for (int i = 0; i < Count; i++) {
            if (Add[i]) {
                dict[Keys[i]] = Values[i];
            } else {
                dict.Remove(Keys[i]);
            }
        }

        return dict.Count;
    }

    [Benchmark]
    public int MicrosoftAddSimple() {
        Dictionary<long, long> dict = [];

        for (int i = 0; i < Count; i++) {
            dict[Keys[i]] = Values[i];
        }

        return dict.Count;
    }

    [Benchmark]
    public int EmberAddSimple() {
        InlineDictionary<long, long> dict = InlineDictionary<long, long>.Allocate();

        for (int i = 0; i < Count; i++) {
            dict.Set(Keys[i], in Values[i]);
        }

        return dict.Count;
    }

    [Benchmark]
    public long MicrosoftReadSimple() {
        long sum = 0;
        for (int i = 0; i < Count; i++) {
            sum += DictMicrosoftReadSimple.TryGetValue(Keys[i], out long data) ? data : -1;
        }
        return sum;
    }

    [Benchmark]
    public long EmberReadSimple() {
        long sum = 0;
        for (int i = 0; i < Count; i++) {
            sum += DictEmberReadSimple.TryGetValue(Keys[i], out long data) ? data : -1;
        }
        return sum;
    }
}