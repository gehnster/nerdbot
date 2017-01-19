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
using Nerdbot.Utilities.Fortuna;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Nerdbot.Tests.Fortuna
{
    [Trait("Type", "Integration")]
    public class PRNGFortunaProviderFactoryTests
    {
        [Fact]
        public void Create_Cancelled_ThrowsCancellationException()
        {
            // Create an immediate cancellation
            var source = new CancellationTokenSource();
            source.Cancel();

            Assert.Throws<OperationCanceledException>(() => PRNGFortunaProviderFactory.Create(token: source.Token));
        }

        [Fact]
#pragma warning disable
        public async Task CreateAsync_Cancelled_ThrowsCancellationException()
        {
            // Create an immediate cancellation
            var source = new CancellationTokenSource();
            source.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await PRNGFortunaProviderFactory.CreateAsync(token: source.Token));
        }

        [Fact, Trait("Category", "Integration")]
        public void Create_WithSeedFile_DoesNotThrow()
        {
            var memoryStream = new MemoryStream(new byte[64]);
            PRNGFortunaProviderFactory.CreateWithSeedFile(memoryStream);
        }
    }
}
