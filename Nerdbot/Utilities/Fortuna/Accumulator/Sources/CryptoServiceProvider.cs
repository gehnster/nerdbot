﻿/* Fortuna
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

namespace Nerdbot.Utilities.Fortuna.Accumulator.Sources
{
    public class CryptoServiceProvider : EntropyProviderBase, IDisposable
    {
        private readonly RandomNumberGenerator _cryptoService = RandomNumberGenerator.Create();

        public override string SourceName => ".NET RNGCryptoServiceProvider";
        protected override TimeSpan ScheduledPeriod => TimeSpan.FromMilliseconds(100);
        protected override byte[] GetEntropy()
        {
            var randomData = new byte[32];
            _cryptoService.GetBytes(randomData);

            return randomData;
        }

        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed) return;

            _cryptoService?.Dispose();

            _isDisposed = true;
        }
    }
}
