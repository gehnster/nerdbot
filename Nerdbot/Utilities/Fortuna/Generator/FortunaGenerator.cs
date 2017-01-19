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
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Nerdbot.Utilities.Fortuna.Generator
{
    public class FortunaGenerator : IGenerator
    {
        /// <summary>
        /// 256-bit block cipher key size (in bytes)
        /// </summary>
        private const int KeyBlockSize = 32;

        /// <summary>
        /// 128-bit cipher block size (in bytes)
        /// </summary>
        /// <remarks>
        /// This is also the 'counter' size used, because it fits within a block size
        /// </remarks>
        private const int CipherBlockSize = 16;

        private GeneratorState _state;


        public FortunaGenerator(byte[] seed = null)
        {
            InitializeGenerator(seed ?? BitConverter.GetBytes(Environment.TickCount));
        }

        private void InitializeGenerator(byte[] seed)
        {
            _state = new GeneratorState
            {
                Counter = new BigInteger(),
                Key = new byte[KeyBlockSize]
            };
            Reseed(seed);
        }

        public void Reseed(byte[] seed)
        {
            using (var sha = SHA256.Create())
            {
                var newKey = GetNewKey(sha.ComputeHash(_state.Key.Concat(seed).ToArray()));
                var counter = ++_state.Counter;

                _state = new GeneratorState
                {
                    Key = newKey,
                    Counter = counter
                };
            }
        }

        private byte[] GenerateBlocks(int numBlocks)
        {
            const int stringSize = 16 * 1024; // 16KB

            if (_state.Counter == 0)
                throw new InvalidOperationException("Counter must have already been initialized.");

            using (var aes = Aes.Create())
            {
                aes.Key = _state.Key;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;

                var randomString = new byte[stringSize];
                var numBytes = numBlocks * CipherBlockSize;
                for (var i = 0; i < numBytes; i += CipherBlockSize)
                {
                    var counterBytes = _state.CounterBytes;
                    aes.IV = counterBytes;

                    using (var cryptor = aes.CreateEncryptor())
                    {
                        var cryptedBytes = cryptor.TransformFinalBlock(counterBytes, 0, CipherBlockSize);
                        Array.Copy(cryptedBytes, 0, randomString, i, CipherBlockSize);

                        _state.Counter++;
                    }
                }

                return randomString;
            }
        }

        // AKA PseudoRandomData
        public void GenerateBytes(byte[] data)
        {
            const int maxLength = 1048576; // 2^20

            if (data.Length >= maxLength)
                throw new InvalidOperationException($"Maximum number of bytes to generate cannot exceed {maxLength}.");

            // Compute the output (random data)
            var randomData = GenerateBlocks((int) Math.Ceiling(data.Length / 16d));
            Array.Copy(randomData, 0, data, 0, data.Length);

            // Switch to a new key
            _state.Key = GetNewKey(GenerateBlocks(2));
        }

        private byte[] GetNewKey(byte[] bytes)
        {
            Array.Resize(ref bytes, KeyBlockSize);

            return bytes;
        }
    }
}
