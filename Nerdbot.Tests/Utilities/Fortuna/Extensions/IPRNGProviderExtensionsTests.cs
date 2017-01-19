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
using System.Linq;
using Nerdbot.Utilities.Fortuna;
using NSubstitute;
using Xunit;
using Nerdbot.Utilities.Fortuna.Extensions;

namespace Nerdbot.Tests.Fortuna.Extensions
{
    public class IPRNGProviderExtensionsTests
    {
        private readonly IPRNGFortunaProvider _provider;

        public IPRNGProviderExtensionsTests()
        {
            _provider = Substitute.For<IPRNGFortunaProvider>();
            var rng = new Random();

            _provider.WhenForAnyArgs(c => c.GetBytes(null)).Do(c => rng.NextBytes(c.Arg<byte[]>()));
        }

        [Fact]
        public void RandomNumber_UpperBound0_Returnts0()
        {
            var rand = _provider.RandomNumber(0);

            Assert.Equal(0, rand);
        }

        [Theory]
        [MemberData("UpperBounds")]
        public void RandomNumber_UpperBoundN_ReturnsBetween0AndN(int upperBound)
        {
            var rand = _provider.RandomNumber(upperBound);

            Assert.True(rand >= 0, $"random number should be equal to or greater than 0, but was {rand}");
            Assert.True(rand < upperBound, $"random number should be less than upperBound, but was {rand}");
        }


        public static IEnumerable<object[]> UpperBounds()
        {
            return Enumerable.Range(1, 11).Select(i => new object[] { i });
        }

        [Fact, Trait("Category", "Integration")]
        public void RandomNumber_UniformDistribution()
        {
            const int numIterations = 10000;
            const int range = 10;
            var distribution = new int[range];

            for (int i = 0; i < numIterations; i++)
            {
                var value = _provider.RandomNumber(range);
                distribution[value]++;
            }

            var expectedValue = numIterations / range;
            var stdDev = 0.1 * expectedValue;

            Assert.True(distribution.All(i => i >= expectedValue - stdDev && i <= expectedValue + stdDev));
        }
    }
}
