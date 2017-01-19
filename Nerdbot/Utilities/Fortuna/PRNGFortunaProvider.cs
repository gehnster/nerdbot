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
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Nerdbot.Utilities.Fortuna.Accumulator;
using Nerdbot.Utilities.Fortuna.Generator;

namespace Nerdbot.Utilities.Fortuna
{
    public sealed class PRNGFortunaProvider : RandomNumberGenerator, IReseedableFortunaProvider
    {
        private static readonly TimeSpan OneHundredMilliseconds = TimeSpan.FromMilliseconds(100);

        private readonly IGenerator _generator;
        private readonly IEntropyAccumulator _accumulator;

        private DateTime _lastReseedTime = DateTime.MinValue;
        private int _reseedCount;

        public PRNGFortunaProvider(IGenerator generator, IEntropyAccumulator accumulator)
        {
            if (generator == null) throw new ArgumentNullException(nameof(generator));
            if (accumulator == null) throw new ArgumentNullException(nameof(accumulator));
            _generator = generator;
            _accumulator = accumulator;
        }

        public new IPRNGFortunaProvider Create()
        {
            return PRNGFortunaProviderFactory.Create();
        }

        public void InitializePRNG(CancellationToken token = default(CancellationToken))
        {
            // Setup the PRNG here, and wait for data
            while (!_accumulator.HasEnoughEntropy)
            {
                token.ThrowIfCancellationRequested();
                Thread.Sleep(1);
            }
        }

        public async Task InitializePRNGAsync(CancellationToken token = default(CancellationToken))
        {
            // Setup the PRNG here, and wait for data
            while (!_accumulator.HasEnoughEntropy)
            {
                await Task.Delay(1, token).ConfigureAwait(false);
            }
        }

        public override void GetBytes(byte[] data)
        {
            var timeSinceLastReseed = DateTime.UtcNow - _lastReseedTime;

            if (_accumulator.HasEnoughEntropy && timeSinceLastReseed > OneHundredMilliseconds)
            {
                // Reseed the Generator
                Reseed(_accumulator.GetRandomDataFromPools());
            }

            if (_reseedCount == 0)
            {
                throw new InvalidOperationException("The PRNG has not yet been seeded."); 
            }

            // Any necessary reseeds should have completed - now generate the random data
             _generator.GenerateBytes(data);
        }

        void IReseedableFortunaProvider.Reseed(byte[] seed)
        {
            Reseed(seed);
        }

        private void Reseed(byte[] seed)
        {
            _reseedCount++;
            _generator.Reseed(seed);
            _lastReseedTime = DateTime.UtcNow;
        }

        private bool _isDisposed;
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            _accumulator?.Dispose();

            base.Dispose(disposing);

            _isDisposed = true;
        }
    }
}
