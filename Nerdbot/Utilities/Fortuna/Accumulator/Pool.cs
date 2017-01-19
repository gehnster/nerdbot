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
using System.Security.Cryptography;
using System.Threading;

namespace Nerdbot.Utilities.Fortuna.Accumulator
{
    public sealed class Pool : IDisposable
    {
        private readonly SHA256 _sha256 = SHA256.Create();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private long _runningSize;
        public long Size => Interlocked.Read(ref _runningSize);
        
        private byte[] _hash;
        public byte[] Hash
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _hash;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            private set
            {
                // We guarantee this is only ever called from AddEventData, 
                // so we're protected by the outer write lock scope
                _hash = value;
            }
        }

        public void AddEventData(int source, byte[] data)
        {
            if (data.Length > 32) throw new ArgumentOutOfRangeException(nameof(data), "Length must not exceed 32 bytes.");
            if (source < 0 || source > 255) throw new ArgumentOutOfRangeException(nameof(source), "Source identifier must be between 0 and 255 inclusive.");

            _lock.EnterWriteLock();
            try
            {
                var hashData = new[] { (byte) source, (byte) data.Length }.Concat(data).ToArray();
                Hash = _sha256.ComputeHash(hashData);
                Interlocked.Add(ref _runningSize, hashData.Length);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public byte[] ReadFromPool()
        {
            _lock.EnterReadLock();
            try
            {
                Interlocked.Exchange(ref _runningSize, 0);
                return Hash;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        #region IDisposable Implementation

        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed) return;

            _sha256?.Dispose();
            _lock?.Dispose();

            _isDisposed = true;
        }

        #endregion
    }
}
