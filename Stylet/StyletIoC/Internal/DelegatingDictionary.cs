using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace StyletIoC.Internal
{
    internal interface IDelegatingDictionary<TKey, TValue>
    {
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);
        TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);
        TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory);
        ICollection<TValue> Values { get; }
    }

    internal class TrivialDelegatingDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, IDelegatingDictionary<TKey, TValue>
    { }

    internal class DelegatingDictionary<TKey, TValue> : IDelegatingDictionary<TKey, TValue>
    {
        private readonly IDelegatingDictionary<TKey, TValue> parentDictionary;
        private readonly ConcurrentDictionary<TKey, TValue> ourDictionary = new ConcurrentDictionary<TKey, TValue>();
        private readonly Func<TValue, TValue> translator;

        public static IDelegatingDictionary<TKey, TValue> Create()
        {
            return new TrivialDelegatingDictionary<TKey, TValue>();
        }

        public static IDelegatingDictionary<TKey, TValue> Create(IDelegatingDictionary<TKey, TValue> parentDictionary)
        {
            return Create(parentDictionary, val => val);
        }

        public static IDelegatingDictionary<TKey, TValue> Create(IDelegatingDictionary<TKey, TValue> parentDictionary, Func<TValue, TValue> translator)
        {
            return new DelegatingDictionary<TKey, TValue>(parentDictionary, translator);
        }

        public DelegatingDictionary(IDelegatingDictionary<TKey, TValue> parentDictionary, Func<TValue, TValue> translator)
        {
            this.parentDictionary = parentDictionary;
            this.translator = translator;
        }

        public bool ContainsKey(TKey key)
        {
            return this.ourDictionary.ContainsKey(key) || this.parentDictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.ourDictionary.TryGetValue(key, out value))
                return true;

            TValue tempValue;
            if (!this.parentDictionary.TryGetValue(key, out tempValue))
                return false;

            value = this.translator(tempValue);
            this.ourDictionary.TryAdd(key, value);
            return true;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue value;
            if (this.parentDictionary.TryGetValue(key, out value))
                return value;

            return this.ourDictionary.GetOrAdd(key, valueFactory);
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            // This is true for all of the current applications in StyletIoC, but may not be true for other applications
            // If it's in our dictionary, update
            // If it's in our parent dictionary, run it through the translator then the updateValueFactory
            // If it's in neither, add it to our dictionary

            TValue returnValue;
            bool success;

            do
            {
                TValue outValue;
                if (this.ourDictionary.TryGetValue(key, out outValue))
                {
                    returnValue = updateValueFactory(key, outValue);
                    success = this.ourDictionary.TryUpdate(key, returnValue, outValue);
                }
                else if (this.parentDictionary.TryGetValue(key, out outValue))
                {
                    returnValue = updateValueFactory(key, this.translator(outValue));
                    success = this.ourDictionary.TryAdd(key, returnValue);
                }
                else
                {
                    returnValue = addValueFactory(key);
                    success = this.ourDictionary.TryAdd(key, returnValue);
                }
            }
            while (!success);

            return returnValue;
        }

        public ICollection<TValue> Values
        {
            get { return this.ourDictionary.Values; }
        }
    }
}
