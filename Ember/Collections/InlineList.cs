
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;

// namespace Ember.Collections;

// public struct InlineList<T> : IList<T> {

//     private T[]? _data;
//     private int _count;

//     public void Init(int capacity = 2) {
//         if (capacity != 0) {
//             _data = new T[capacity];
//         }

//         _count = 0;
//     }

//     [Conditional("EMBER_SAFETY_CHECKS")]
//     internal readonly void ValidateSelf() {
// #if EMBER_SAFETY_CHECKS
//         if (_data == null) {
//             if (_count != 0) throw new InvalidOperationException($"{nameof(_count)} != 0");
//         } else {
//             if (_count > _data.Length) throw new InvalidOperationException($"{nameof(_count)} > {nameof(_data)}.Length");
//             if (_count < 0) throw new InvalidOperationException($"{nameof(_count)} < 0");
//         }
// #endif
//     }

//     [Conditional("EMBER_SAFETY_CHECKS")]
//     private readonly void ValidateIndex(int index) {
//         if (index >= Count) throw new IndexOutOfRangeException(nameof(index));
//         ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
//     }

//     public T this[int index] {
//         readonly get => Get(index);
//         set => throw new System.NotImplementedException();
//     }

//     public readonly int Count => _count;
//     public readonly bool IsReadOnly => false;

//     public readonly T Get(int index) {
//         ValidateSelf();
//         ValidateIndex(index);
//         return _data![index];
//     }

//     public readonly void Set(int index, in T value) {
//         ValidateSelf();
//         ValidateIndex(index);
//         _data![index] = value;
//     }

//     public void Add(T item) {
//         ValidateSelf();

//         if (_data == null) {
//             _data = new T[2];
//             _data[0] = item;
//             _count = 1;
//             return;
//         }

//         int index = _count++;

//         if (_data.Length <= index) {
//             Array.Resize(ref _data, _data.Length * 2);
//         }

//         _data[index] = item;
//     }

//     public void Clear() {
//         ValidateSelf();

//         if (_data == null)
//             return;

//         for (int i = 0; i < _count; i++)
//             _data[i] = default!;

//         _count = 0;
//     }

//     public readonly bool Contains(T item) {
//         ValidateSelf();

//         if (_data == null) return false;
//         return _data.Contains(item);
//     }

//     public readonly void CopyTo(T[] array, int arrayIndex) {
//         ValidateSelf();

//         if (_data == null) return;
//         _data.CopyTo(array, arrayIndex);
//     }

//     public IEnumerator<T> GetEnumerator() {
//         throw new System.NotImplementedException();
//     }

//     public int IndexOf(T item) {
//         throw new System.NotImplementedException();
//     }

//     public void Insert(int index, T item) {
//         throw new System.NotImplementedException();
//     }

//     public bool Remove(T item) {
//         throw new System.NotImplementedException();
//     }

//     public void RemoveAt(int index) {
//         throw new System.NotImplementedException();
//     }

//     IEnumerator IEnumerable.GetEnumerator() {
//         throw new System.NotImplementedException();
//     }
// }