namespace Ember.Tests;

using System.Collections.Generic;
using Ember.Collections;


[Collection("UnmanagedTests")]
public class UnmanagedDictionaryTests {
    [Fact]
    public void Add_AddValue() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedDictionary<int, ulong> dictionary = UnmanagedDictionary<int, ulong>.Allocate();

            dictionary.Add(1, 1);

            Assert.Equal(1UL, dictionary[1]);

            dictionary.DebugValidate();
        });
    }

    [Fact]
    public void Remove_RemovesKeyValuePairFromDictionary() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedDictionary<int, ulong> dictionary = UnmanagedDictionary<int, ulong>.Allocate();
            dictionary.Add(1, 3);
            dictionary.Add(2, 4);

            Assert.True(dictionary.Remove(1));

            Assert.False(dictionary.ContainsKey(1));
            Assert.True(dictionary.ContainsKey(2));

            Assert.Single(dictionary, kvp => kvp.Key == 2 && kvp.Value == 4);
            Assert.Single(dictionary.Keys, key => key == 2);
            Assert.Single(dictionary.Values, value => value == 4);

            Assert.False(dictionary.Remove(1));
            Assert.True(dictionary.Remove(2));

            Assert.Empty(dictionary);
            Assert.Empty(dictionary.Keys);
            Assert.Empty(dictionary.Values);

            Assert.False(dictionary.Remove(2));

            dictionary.DebugValidate();
        });
    }

    [Fact]
    public void Count_ReturnsCorrectNumberOfItemsInDictionary() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedDictionary<int, ulong> dictionary = UnmanagedDictionary<int, ulong>.Allocate();
            dictionary.Add(1, 1UL);
            dictionary.Add(2, 2UL);
            dictionary.Add(0x7FFFFFFF, 3UL);
            dictionary.Add(-1, 4UL);

            dictionary.Remove(2);

            int count = dictionary.Count;

            Assert.Equal(3, count);

            dictionary.DebugValidate();
        });
    }

    [Fact]
    public void Clear_RemovesAllItemsFromDictionary() {
        TestUtils.CheckMemoryFreed(() => {

            using UnmanagedDictionary<int, ulong> dictionary = UnmanagedDictionary<int, ulong>.Allocate();
            dictionary.Add(1, 1);
            dictionary.Add(2, 2);
            dictionary.Add(0x7FFFFFFF, 3UL);
            dictionary.Add(-1, 4UL);

            dictionary.Clear();

            Assert.Empty(dictionary);

            dictionary.DebugValidate();
        });
    }

    [Fact]
    public void TryGetValue_ReturnsTrueAndSetsValueWhenKeyExists() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedDictionary<int, ulong> dictionary = UnmanagedDictionary<int, ulong>.Allocate();
            dictionary.Add(1, 5);
            dictionary.Add(2, 5);
            Assert.True(dictionary.Remove(2));
            dictionary.Set(1, 1);

            bool result = dictionary.TryGetValue(1, out ulong value);

            Assert.True(result);
            Assert.Equal(1UL, value);

            dictionary.Add(3, 9);
            Assert.True(dictionary.Remove(1));
            dictionary.Add(0x7FFFFFFF, 3UL);
            dictionary.Add(-1, 4UL);
            Assert.True(dictionary.Remove(0x7FFFFFFF));
            dictionary.Add(0x7FFFFFFF, 3UL);
            Assert.True(dictionary.Remove(0x7FFFFFFF));

            result = dictionary.TryGetValue(-1, out value);

            Assert.True(result);
            Assert.Equal(2, dictionary.Count);
            Assert.Equal(4UL, value);

            dictionary.DebugValidate();
        });
    }

    [Fact]
    public void TryGetValue_ReturnsTrueAndSetsValueWhenKeyExistsBadHash() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedDictionary<BadHashValue, ulong> dictionary = UnmanagedDictionary<BadHashValue, ulong>.Allocate();
            dictionary.Add(1, 5);
            dictionary.Add(2, 5);
            Assert.True(dictionary.Remove(2));
            dictionary.Set(1, 1);

            bool result = dictionary.TryGetValue(1, out ulong value);

            Assert.True(result);
            Assert.Equal(1UL, value);

            dictionary.Add(3, 9);
            Assert.True(dictionary.Remove(1));
            dictionary.Add(0x7FFFFFFF, 3UL);
            dictionary.Add(10, 4UL);
            Assert.True(dictionary.Remove(0x7FFFFFFF));
            dictionary.Add(0x7FFFFFFF, 3UL);
            Assert.True(dictionary.Remove(0x7FFFFFFF));

            result = dictionary.TryGetValue(10, out value);

            Assert.True(result);
            Assert.Equal(2, dictionary.Count);
            Assert.Equal(4UL, value);

            dictionary.DebugValidate();
        });
    }

    [Fact]
    public void TryGetValue_ReturnsFalseAndSetsDefaultValueWhenKeyDoesNotExist() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedDictionary<int, ulong> dictionary = UnmanagedDictionary<int, ulong>.Allocate();

            bool result = dictionary.TryGetValue(1, out ulong value);

            Assert.False(result);
            Assert.Equal(0UL, value);

            dictionary.DebugValidate();
        });
    }

    [Fact]
    public void TryGetValue_StressTest() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedDictionary<int, int> dictionary = UnmanagedDictionary<int, int>.Allocate();
            Dictionary<int, int> realDictionary = [];
            Random random = new(0);

            for (int i = 0; i < 100000; i++) {
                KeyValuePair<int, int> kvp = new(random.Next(10000), random.Next(10000));
                if (random.NextDouble() < 0.5) {
                    dictionary.Set(kvp.Key, kvp.Value);
                    realDictionary[kvp.Key] = kvp.Value;
                } else {
                    Assert.Equal(dictionary.Remove(kvp.Key), realDictionary.Remove(kvp.Key));
                }
                Assert.Equal(realDictionary.Count, dictionary.Count);
            }

            Assert.True(realDictionary.All(e => dictionary.Contains(e)));
            Assert.Equal(realDictionary.Keys.OrderBy(i => i), dictionary.Keys.OrderBy(i => i));
            Assert.Equal(realDictionary.Values.OrderBy(i => i), dictionary.Values.OrderBy(i => i));

            dictionary.DebugValidate();
        });
    }

    [Fact]
    public void TryGetValue_StressTestBadHash() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedDictionary<BadHashValue, int> dictionary = UnmanagedDictionary<BadHashValue, int>.Allocate();
            Dictionary<BadHashValue, int> realDictionary = [];
            Random random = new(0);

            for (int i = 0; i < 10000; i++) {
                KeyValuePair<BadHashValue, int> kvp = new((ulong)random.Next(1000), random.Next(1000));
                if (random.NextDouble() < 0.5) {
                    dictionary.Set(kvp.Key, kvp.Value);
                    realDictionary[kvp.Key] = kvp.Value;
                } else {
                    Assert.Equal(dictionary.Remove(kvp.Key), realDictionary.Remove(kvp.Key));
                }
                Assert.Equal(realDictionary.Count, dictionary.Count);
            }

            Assert.True(realDictionary.All(e => dictionary.Contains(e)));
            Assert.Equal(realDictionary.Keys.OrderBy(i => i), dictionary.Keys.OrderBy(i => i));
            Assert.Equal(realDictionary.Values.OrderBy(i => i), dictionary.Values.OrderBy(i => i));

            dictionary.DebugValidate();
        });
    }

    [Fact]
    public void Enumerator_ReturnsAllKeyValuePairsInDictionary() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedDictionary<int, ulong> dictionary = UnmanagedDictionary<int, ulong>.Allocate();
            dictionary.Add(1, 1UL);
            dictionary.Add(2, 2UL);
            dictionary.Add(3, 3UL);
            dictionary.Add(0x7FFFFFFF, 3UL);
            dictionary.Add(-1, 4UL);

            List<KeyValuePair<int, ulong>> keyValuePairs = new List<KeyValuePair<int, ulong>>();
            foreach (KeyValuePair<int, ulong> kvp in dictionary) {
                keyValuePairs.Add(kvp);
            }

            Assert.Equal(5, keyValuePairs.Count);
            Assert.Contains(new KeyValuePair<int, ulong>(1, 1UL), keyValuePairs);
            Assert.Contains(new KeyValuePair<int, ulong>(-1, 4UL), keyValuePairs);
            Assert.Contains(new KeyValuePair<int, ulong>(0x7FFFFFFF, 3UL), keyValuePairs);

            dictionary.DebugValidate();
        });
    }

    [Fact]
    public void Enumerator_ReturnsNoKeyValuePairsForEmptyDictionary() {
        TestUtils.CheckMemoryFreed(() => {
            using UnmanagedDictionary<int, ulong> dictionary = UnmanagedDictionary<int, ulong>.Allocate();

            dictionary.Add(0, 1UL);
            dictionary.Clear();

            List<KeyValuePair<int, ulong>> keyValuePairs = new List<KeyValuePair<int, ulong>>();
            foreach (KeyValuePair<int, ulong> kvp in dictionary) {
                keyValuePairs.Add(kvp);
            }

            Assert.Empty(keyValuePairs);

            dictionary.DebugValidate();
        });
    }
}