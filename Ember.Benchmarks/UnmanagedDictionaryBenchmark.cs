
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Ember.Collections;

namespace Ember.Benchmark;

[SimpleJob]
[MemoryDiagnoser]
[DisassemblyDiagnoser(exportHtml: true)]
public class UnmanagedDictionaryBenchmark {

    struct BigData {
        public long a, b, c, d, e, f;
    }

    struct SmolData {
        public long a;
    }

    private long[] keys;
    private BigData[] bigValues;
    private SmolData[] smolValues;
    private bool[] addOrRemove;
    private const int count = 2000000;
    private const int range = 1000;

    public enum SynchronizationType {
        Synchronous,
        Concurrent
    }

    [Params(SynchronizationType.Synchronous, SynchronizationType.Concurrent)]
    // [Params(SynchronizationType.Concurrent)]
    public SynchronizationType SyncType;

    public enum ValueType {
        Small,
        Big
    }

    [Params(ValueType.Small, ValueType.Big)]
    // [Params(ValueType.Big)]
    public ValueType ValType;

    [GlobalSetup]
    public void Setup() {
        keys = new long[count];
        bigValues = new BigData[count];
        smolValues = new SmolData[count];
        addOrRemove = new bool[count];
        Random random = new(0);
        for (int i = 0; i < count; i++) {
            keys[i] = random.NextInt64(range);
            bigValues[i] = new() {
                a = random.NextInt64(),
                b = random.NextInt64(),
                c = random.NextInt64(),
                d = random.NextInt64(),
                e = random.NextInt64(),
                f = random.NextInt64()
            };

            smolValues[i] = new() {
                a = random.NextInt64()
            };

            // addOrRemove[i] = true;
            addOrRemove[i] = random.NextDouble() < 0.85;
        }
    }

    [Benchmark]
    public int DictionaryMixed() {
        switch (SyncType, ValType) {
            case (SynchronizationType.Synchronous, ValueType.Big): {
                    Dictionary<long, BigData> regular = [];

                    for (int i = 0; i < count; i++) {
                        if (addOrRemove[i]) {
                            regular[keys[i]] = bigValues[i];
                        } else {
                            regular.Remove(keys[i], out _);
                        }
                    }

                    return regular.Count;
                }
            case (SynchronizationType.Synchronous, ValueType.Small): {
                    Dictionary<long, SmolData> regular = [];

                    for (int i = 0; i < count; i++) {
                        if (addOrRemove[i]) {
                            regular[keys[i]] = smolValues[i];
                        } else {
                            regular.Remove(keys[i], out _);
                        }
                    }

                    return regular.Count;
                }
            case (SynchronizationType.Concurrent, ValueType.Big): {
                    ConcurrentDictionary<long, BigData> regular = [];

                    for (int i = 0; i < count; i++) {
                        if (addOrRemove[i]) {
                            regular[keys[i]] = bigValues[i];
                        } else {
                            regular.Remove(keys[i], out _);
                        }
                    }

                    return regular.Count;
                }
            case (SynchronizationType.Concurrent, ValueType.Small): {
                    ConcurrentDictionary<long, SmolData> regular = [];

                    for (int i = 0; i < count; i++) {
                        if (addOrRemove[i]) {
                            regular[keys[i]] = smolValues[i];
                        } else {
                            regular.Remove(keys[i], out _);
                        }
                    }

                    return regular.Count;
                }
        }
        return -1;
    }

    [Benchmark]
    public int DictionaryAdd() {
        switch (SyncType, ValType) {
            case (SynchronizationType.Synchronous, ValueType.Big): {
                    Dictionary<long, BigData> regular = [];

                    for (int i = 0; i < count; i++) {
                        regular[keys[i]] = bigValues[i];
                    }

                    return regular.Count;
                }
            case (SynchronizationType.Synchronous, ValueType.Small): {
                    Dictionary<long, SmolData> regular = [];

                    for (int i = 0; i < count; i++) {
                        regular[keys[i]] = smolValues[i];
                    }

                    return regular.Count;
                }
            case (SynchronizationType.Concurrent, ValueType.Big): {
                    ConcurrentDictionary<long, BigData> regular = [];

                    for (int i = 0; i < count; i++) {
                        regular[keys[i]] = bigValues[i];
                    }

                    return regular.Count;
                }
            case (SynchronizationType.Concurrent, ValueType.Small): {
                    ConcurrentDictionary<long, SmolData> regular = [];

                    for (int i = 0; i < count; i++) {
                        regular[keys[i]] = smolValues[i];
                    }

                    return regular.Count;
                }
        }
        return -1;
    }
    
    [Benchmark]
    public int UnmanagedMixed() {
        switch (SyncType, ValType) {

            case (SynchronizationType.Concurrent, ValueType.Big): {
                    using UnmanagedConcurrentDictionary<long, BigData> unmanaged = UnmanagedConcurrentDictionary<long, BigData>.Allocate();

                    for (int i = 0; i < count; i++) {
                        if (addOrRemove[i]) {
                            unmanaged.Set(keys[i], in bigValues[i]);
                        } else {
                            unmanaged.Remove(keys[i], out _);
                        }
                    }

                    return unmanaged.Count;
                }
            case (SynchronizationType.Concurrent, ValueType.Small): {
                    using UnmanagedConcurrentDictionary<long, SmolData> unmanaged = UnmanagedConcurrentDictionary<long, SmolData>.Allocate();

                    for (int i = 0; i < count; i++) {
                        if (addOrRemove[i]) {
                            unmanaged.Set(keys[i], in smolValues[i]);
                        } else {
                            unmanaged.Remove(keys[i], out _);
                        }
                    }

                    return unmanaged.Count;
                }
            case (SynchronizationType.Synchronous, ValueType.Big): {
                    using UnmanagedDictionary<long, BigData> unmanaged = UnmanagedDictionary<long, BigData>.Allocate();

                    for (int i = 0; i < count; i++) {
                        if (addOrRemove[i]) {
                            unmanaged.Set(keys[i], in bigValues[i]);
                        } else {
                            unmanaged.Remove(keys[i], out _);
                        }
                    }

                    return unmanaged.Count;
                }
            case (SynchronizationType.Synchronous, ValueType.Small): {
                    using UnmanagedDictionary<long, SmolData> unmanaged = UnmanagedDictionary<long, SmolData>.Allocate();

                    for (int i = 0; i < count; i++) {
                        if (addOrRemove[i]) {
                            unmanaged.Set(keys[i], in smolValues[i]);
                        } else {
                            unmanaged.Remove(keys[i], out _);
                        }
                    }

                    return unmanaged.Count;
                }
        }
        return -1;
    }

    [Benchmark]
    public int UnmanagedAdd() {
        switch (SyncType, ValType) {

            case (SynchronizationType.Concurrent, ValueType.Big): {
                    using UnmanagedConcurrentDictionary<long, BigData> unmanaged = UnmanagedConcurrentDictionary<long, BigData>.Allocate();

                    for (int i = 0; i < count; i++) {
                        unmanaged.Set(keys[i], in bigValues[i]);
                    }

                    return unmanaged.Count;
                }
            case (SynchronizationType.Concurrent, ValueType.Small): {
                    using UnmanagedConcurrentDictionary<long, SmolData> unmanaged = UnmanagedConcurrentDictionary<long, SmolData>.Allocate();

                    for (int i = 0; i < count; i++) {
                        unmanaged.Set(keys[i], in smolValues[i]);
                    }

                    return unmanaged.Count;
                }
            case (SynchronizationType.Synchronous, ValueType.Big): {
                    using UnmanagedDictionary<long, BigData> unmanaged = UnmanagedDictionary<long, BigData>.Allocate();

                    for (int i = 0; i < count; i++) {
                        unmanaged.Set(keys[i], in bigValues[i]);
                    }

                    return unmanaged.Count;
                }
            case (SynchronizationType.Synchronous, ValueType.Small): {
                    using UnmanagedDictionary<long, SmolData> unmanaged = UnmanagedDictionary<long, SmolData>.Allocate();

                    for (int i = 0; i < count; i++) {
                        unmanaged.Set(keys[i], in smolValues[i]);
                    }

                    return unmanaged.Count;
                }
        }
        return -1;
    }
}