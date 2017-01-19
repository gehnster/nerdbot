/* Fortuna
 * By: smithc
 * GitHub: https://github.com/smithc/Fortuna
 * LICENSE:
 * MIT License

Copyright (c) 2016 smithc

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Nerdbot.Utilities.Fortuna.Accumulator
{
    public sealed class FortunaAccumulator : IEntropyAccumulator
    {
        private const int MaxNumberOfSources = 255;
        private const int NumberOfPools = 32;
        private const int MinEntropyPoolSize = 64;

        private readonly Pool[] _entropyPools = new Pool[NumberOfPools];
        private readonly IEventScheduler _eventScheduler;
        private readonly IReadOnlyCollection<IEntropyProvider> _entropyProviders;

        private int _sourceCount;
        private int _runningCount;
        private int _requestDataCount;

        public bool HasEnoughEntropy => _entropyPools[0].Size > MinEntropyPoolSize;

        public FortunaAccumulator(IEventScheduler eventScheduler, IEnumerable<IEntropyProvider> entropyProviders)
        {
            if (eventScheduler == null) throw new ArgumentNullException(nameof(eventScheduler));
            if (entropyProviders == null) throw new ArgumentNullException(nameof(entropyProviders));
            _eventScheduler = eventScheduler;

            _entropyProviders = new ReadOnlyCollection<IEntropyProvider>(entropyProviders.ToList());

            InitializePools();
            RegisterEntropySources(_entropyProviders);

            _eventScheduler.EntropyAvailable += AccumulateEntropy;
        }

        public byte[] GetRandomDataFromPools()
        {
            if (!HasEnoughEntropy)
                throw new InvalidOperationException("The accumulator does not yet have enough entropy to generate data.");

            const int maxSeedSize = NumberOfPools * 32;

            var requestForDataCount = _requestDataCount++;
            var randomData = new byte[maxSeedSize];

            var bufferIndex = 0;
            for (var poolIndex = 0; poolIndex < NumberOfPools; poolIndex++)
            {
                if (requestForDataCount%Math.Pow(2, poolIndex) != 0)
                {
                    // We can break out the first time we hit this condition
                    break;
                }

                var poolData = _entropyPools[poolIndex].ReadFromPool();

                Debug.Assert(poolData.Length == 32);
                    
                poolData.CopyTo(randomData, bufferIndex);
                bufferIndex += poolData.Length;
            }

            return randomData;
        }

        #region Private Functions

        private void InitializePools()
        {
            for (var i = 0; i < _entropyPools.Length; i++)
            {
                _entropyPools[i] = new Pool();
            }
        }

        private void RegisterEntropySources(IEnumerable<IEntropyProvider> entropyProviders)
        {
            foreach (var entropyProvider in entropyProviders)
            {
                var eventSource = _sourceCount++;
                if (eventSource > MaxNumberOfSources)
                    throw new InvalidOperationException($"Cannot configure more than {MaxNumberOfSources} for accumulating entropy.");

                _eventScheduler.ScheduleEvent(eventSource, entropyProvider.GetScheduledEvent());
            }
        }

        private void AccumulateEntropy(int eventSource, byte[] entropy)
        {
            var runningCount = Interlocked.Increment(ref _runningCount) - 1;
            var poolSize = _entropyPools.Length;

            var poolIndex = runningCount % poolSize;
            _entropyPools[poolIndex].AddEventData(eventSource, entropy);
        }

        #endregion

        #region IDisposable Implementation

        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed) return;

            _eventScheduler.EntropyAvailable -= AccumulateEntropy;
            foreach (var pool in _entropyPools)
            {
                pool?.Dispose();
            }

            foreach (var provider in _entropyProviders.OfType<IDisposable>())
            {
                provider.Dispose();
            }

            _isDisposed = true;
        }

        #endregion
    }
}
