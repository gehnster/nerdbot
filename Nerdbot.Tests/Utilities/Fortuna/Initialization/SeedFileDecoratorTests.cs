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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nerdbot.Utilities.Fortuna.Accumulator;
using Nerdbot.Utilities.Fortuna.Accumulator.Event;
using Nerdbot.Utilities.Fortuna.Generator;
using Nerdbot.Utilities.Fortuna.Initialization;
using Nerdbot.Utilities.Fortuna;
using NSubstitute;
using Xunit;

namespace Nerdbot.Tests.Fortuna.Initialization
{
    public class SeedFileDecoratorTests
    {
        private readonly IReseedableFortunaProvider _mockProvider;
        private readonly Stream _mockFile;

        public SeedFileDecoratorTests()
        {
            _mockProvider = Substitute.For<IReseedableFortunaProvider>();
            _mockFile = Substitute.For<Stream>();

            _mockFile.CanRead.Returns(true);
            _mockFile.CanWrite.Returns(true);
            _mockFile.CanSeek.Returns(true);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public void CreateDecorator_FileHasIncorrectCapabilities_Throws(bool canRead, bool canWrite)
        {
            _mockFile.CanRead.Returns(canRead);
            _mockFile.CanWrite.Returns(canWrite);

            Assert.Throws<ArgumentException>(() => new SeedFileDecorator(_mockProvider, _mockFile));
        }

        [Fact]
        public void CreateDecorator_FileNotUsable_ReseedsFromEntropy()
        {
            var decorator = new SeedFileDecorator(_mockProvider, _mockFile);
            decorator.InitializePRNG();

            _mockProvider.Received(1).InitializePRNG();
            _mockFile.DidNotReceive().Read(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());
        }

        [Fact]
        public void CreateDecorator_FileHasEntropy_ReseedsFromFile()
        {
            _mockFile.Length.Returns(64);

            var decorator = new SeedFileDecorator(_mockProvider, _mockFile);
            decorator.InitializePRNG();

            _mockProvider.DidNotReceive().InitializePRNG();
            _mockFile.Received().Read(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>());

            // Finally, assert that the new seed was generated and placed in the file
            _mockFile.Received().Seek(0, SeekOrigin.Begin);
            _mockFile.Received().Write(Arg.Any<byte[]>(), 0, 64);
            _mockFile.Received().Flush();
        }

        [Fact]
#pragma warning disable
        public async Task CreateDecoratorAsync_FileHasEntropy_ReseedsFromFile()
        {
            _mockFile.Length.Returns(64);

            var decorator = new SeedFileDecorator(_mockProvider, _mockFile);
            await decorator.InitializePRNGAsync();

            await _mockProvider.DidNotReceive().InitializePRNGAsync();
            await _mockFile.Received().ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());

            // Finally, assert that the new seed was generated and placed in the file
            _mockFile.Received().Seek(0, SeekOrigin.Begin);
            await _mockFile.Received().WriteAsync(Arg.Any<byte[]>(), 0, 64, Arg.Any<CancellationToken>());
            await _mockFile.Received().FlushAsync(Arg.Any<CancellationToken>());
        }

        [Fact, Trait("Type", "Integration")]
        public void CreateDecorator_SeedsFromFile_ReturnsData()
        {
            _mockFile.Length.Returns(64);

            var generator = new FortunaGenerator();
            var accumulator = new FortunaAccumulator(new EntropyEventScheduler(), Enumerable.Empty<IEntropyProvider>());

            var decorator = new SeedFileDecorator(new PRNGFortunaProvider(generator, accumulator), _mockFile);
            decorator.InitializePRNG();

            var data = new byte[1024];
            decorator.GetBytes(data);

            Assert.NotEqual((IEnumerable<byte>) data, new byte[1024]);
        }
    }
}
