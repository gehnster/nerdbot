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
using Nerdbot.Utilities.Fortuna.Generator;
using Xunit;

namespace Nerdbot.Tests.Fortuna.Generator
{
    public class FortunaGeneratorTests
    {
        private FortunaGenerator _sut;

        public FortunaGeneratorTests()
        {
            _sut = new FortunaGenerator();
        }

        [Fact]
        public void GetBytes_ReturnsData()
        {
            var emptyArray = new byte[1024];
            var data = new byte[1024];

            Assert.True(emptyArray.SequenceEqual(data));

            _sut.GenerateBytes(data);

            Assert.False(emptyArray.SequenceEqual(data));
        }

        [Fact]
        public void GetBytes_WithSameSeed_ReturnsSameData()
        {
            var seed = new byte[1];

            _sut = new FortunaGenerator(seed);

            var data = new byte[1024];
            _sut.GenerateBytes(data);

            _sut = new FortunaGenerator(seed);

            var data2 = new byte[1024];
            _sut.GenerateBytes(data2);

            Assert.True(data.SequenceEqual(data2));
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
                _sut.GenerateBytes(data);

                var value = BitConverter.ToUInt32(data, 0) % range;
                distribution[value]++;
            }

            var expectedValue = numIterations/range;
            var stdDev = 0.1*expectedValue;

            Assert.True(distribution.All(i => i >= expectedValue - stdDev && i <= expectedValue + stdDev));
        }
    }
}
