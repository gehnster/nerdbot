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
using System.Linq;
using Nerdbot.Utilities.Fortuna.Extensions;
using Xunit;
using Nerdbot.Utilities.Fortuna;

namespace Nerdbot.Tests.Fortuna
{
    [Trait("Type", "Integration")]
    public class PRNGFortunaProviderTests
    {
        private readonly PRNGFortunaProvider _sut;

        public PRNGFortunaProviderTests()
        {
            _sut = PRNGFortunaProviderFactory.Create() as PRNGFortunaProvider;
        }

        [Fact]
        public void ProviderInitializesCorrectly_ReturnsData()
        {
            var data = new byte[1024];

            _sut.GetBytes(data);

            Assert.False(data.SequenceEqual(Enumerable.Repeat(new byte(), 1024)));
        }

        [Fact]
        public void UniformRandomDistributionTest()
        {
            const int numIterations = 100000;
            const int range = 100;
            var distribution = new int[range];

            var data = new byte[sizeof(int)];
            for (int i = 0; i < numIterations; i++)
            {
                _sut.GetBytes(data);

                var value = BitConverter.ToUInt32(data, 0) % range;
                distribution[value]++;
            }

            var expectedValue = numIterations / range;
            var stdDev = 0.1 * expectedValue;

            Assert.True(distribution.All(i => i >= expectedValue - stdDev && i <= expectedValue + stdDev));
        }

        [Fact]
        public void UniformRandomDistribution_WithRandomNumberMethod_Test()
        {
            const int numIterations = 100000;
            const int range = 100;
            var distribution = new int[range];

            for (int i = 0; i < numIterations; i++)
            {
                var value = _sut.RandomNumber(range);
                distribution[value]++;
            }

            var expectedValue = numIterations / range;
            var stdDev = 0.1 * expectedValue;

            Assert.True(distribution.All(i => i >= expectedValue - stdDev && i <= expectedValue + stdDev));
        }

        [Fact]
        public void DisposeProvider_DoesNotThrow()
        {
            _sut.Dispose();
        }

        [Fact]
        public void DisposeProvider_MultipleTimes_DoesNotThrow()
        {
            _sut.Dispose();
            _sut.Dispose();
        }
    }
}
