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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nerdbot.Utilities.Fortuna.Accumulator;
using Nerdbot.Utilities.Fortuna.Accumulator.Event;
using Nerdbot.Utilities.Fortuna.Accumulator.Sources;
using Nerdbot.Utilities.Fortuna.Generator;
using Nerdbot.Utilities.Fortuna.Initialization;

namespace Nerdbot.Utilities.Fortuna
{
    public static class PRNGFortunaProviderFactory
    {
        public static IPRNGFortunaProvider Create(CancellationToken token = default(CancellationToken))
        {
            var prng = GetProvider();
            prng.InitializePRNG(token);

            return prng;
        }

        public static async Task<IPRNGFortunaProvider> CreateAsync(CancellationToken token = default(CancellationToken))
        {
            var prng = GetProvider();
            await prng.InitializePRNGAsync(token).ConfigureAwait(false);

            return prng;
        }

        public static IPRNGFortunaProvider CreateWithSeedFile(Stream seedStream,
            CancellationToken token = default(CancellationToken))
        {
            var prng = GetSeedFileDecorator(seedStream);
            prng.InitializePRNG(token);

            return prng;
        }

        public static async Task<IPRNGFortunaProvider> CreateWithSeedFileAsync(Stream seedStream,
            CancellationToken token = default(CancellationToken))
        {
            var prng = GetSeedFileDecorator(seedStream);
            await prng.InitializePRNGAsync(token).ConfigureAwait(false);

            return prng;
        }

        private static PRNGFortunaProvider GetProvider()
        {
            var providers = new IEntropyProvider[]
            {
                new SystemTimeProvider(),
                new GarbageCollectionProvider(),
                new CryptoServiceProvider(),
                new EnvironmentUptimeProvider(),
                new ProcessorStatisticsProvider()
            };

            return new PRNGFortunaProvider(
                new FortunaGenerator(),
                new FortunaAccumulator(new EntropyEventScheduler(), providers));
        }

        private static IPRNGFortunaProvider GetSeedFileDecorator(Stream seedStream)
        {
            return new SeedFileDecorator(GetProvider(), seedStream);
        }
    }
}
