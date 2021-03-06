﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FluentLang.Shared
{
	public class ImmutableArrayDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		private ImmutableArray<KeyValuePair<TKey, TValue>> _entries;

		public ImmutableArrayDictionary(ImmutableArray<KeyValuePair<TKey, TValue>> entries)
		{
			_entries = entries;
		}

		public TValue this[TKey key]
		{
			get
			{
				foreach (var (k, v) in _entries)
				{
					if (EqualityComparer<TKey>.Default.Equals(k, key))
					{
						return v;
					}
				}
				throw new ArgumentOutOfRangeException(nameof(key));
			}
		}

		public IEnumerable<TKey> Keys => _entries.Select(x => x.Key);

		public IEnumerable<TValue> Values => _entries.Select(x => x.Value);

		public int Count => _entries.Length;

		public bool ContainsKey(TKey key)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			IEnumerable<KeyValuePair<TKey, TValue>> enumerable = _entries;
			return enumerable.GetEnumerator();
		}

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member because of nullability attributes.
		public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member because of nullability attributes.
		{
			foreach (var (k, v) in _entries)
			{
				if (EqualityComparer<TKey>.Default.Equals(k, key))
				{
					value = v;
					return true;
				}
			}
			value = default!;
			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
