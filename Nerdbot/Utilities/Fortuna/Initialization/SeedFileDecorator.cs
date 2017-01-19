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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nerdbot.Utilities.Fortuna.Exceptions;

namespace Nerdbot.Utilities.Fortuna.Initialization
{
    public sealed class SeedFileDecorator : IPRNGFortunaProvider
    {
        private const int SeedFileLength = 64;
        private static readonly TimeSpan ReseedPeriod = TimeSpan.FromMinutes(10);

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
        private readonly Timer _reseedTimer;

        private readonly IReseedableFortunaProvider _innerProvider;
        private readonly Stream _file;

        internal SeedFileDecorator(IReseedableFortunaProvider innerProvider, Stream file)
        {
            if (innerProvider == null) throw new ArgumentNullException(nameof(innerProvider));
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (!file.CanRead || !file.CanWrite || !file.CanSeek) throw new ArgumentException("Provided FileStream must have read/write/seek capabilities.", nameof(file));

            _innerProvider = innerProvider;
            _file = file;

            _reseedTimer = new Timer(ReseedFileCallback, null, Timeout.InfiniteTimeSpan, ReseedPeriod);
        }

        public void InitializePRNG(CancellationToken token = default(CancellationToken))
        {
            _lock.Wait(token);
            try
            {
                var file = _file;
                if (file.Length == SeedFileLength)
                {
                    // Use the seed file to reseed the generator
                    var buffer = new byte[SeedFileLength];
                    file.Read(buffer, 0, SeedFileLength);

                    _innerProvider.Reseed(buffer);
                }
                else
                {
                    // Otherwise, just do the regular initialization, then save a new file
                    _innerProvider.InitializePRNG(token);
                }

                // Update or re-generate the seed file
                var seed = new byte[SeedFileLength];
                _innerProvider.GetBytes(seed);

                UpdateSeedFile(file, seed);

                // Begin the reseed timer
                _reseedTimer.Change(ReseedPeriod, ReseedPeriod);
            }
            catch (IOException ex)
            {
                throw new FortunaException("Unable to initialize Fortuna with a seed file.", ex);
            }
            catch (Exception ex)
            {
                throw new FortunaException("There was an error initializing Fortuna", ex);
            }
            finally
            {
                _lock.Release();
            }

        }

        public async Task InitializePRNGAsync(CancellationToken token = default(CancellationToken))
        {
            await _lock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var file = _file;
                if (file.Length == SeedFileLength)
                {
                    // Use the seed file to reseed the generator
                    var buffer = new byte[SeedFileLength];
                    await file.ReadAsync(buffer, 0, SeedFileLength, token).ConfigureAwait(false);

                    _innerProvider.Reseed(buffer);
                }
                else
                {
                    // Otherwise, just do the regular initialization, then save a new file
                    await _innerProvider.InitializePRNGAsync(token).ConfigureAwait(false);
                }

                // Update or re-generate the seed file
                var seed = new byte[SeedFileLength];
                _innerProvider.GetBytes(seed);

                file.Seek(0, SeekOrigin.Begin);
                await file.WriteAsync(seed, 0, SeedFileLength, token).ConfigureAwait(false);
                await file.FlushAsync(token).ConfigureAwait(false);

                // Begin the reseed timer
                _reseedTimer.Change(ReseedPeriod, ReseedPeriod);

            }
            catch (IOException ex)
            {
                throw new FortunaException("Unable to initialize Fortuna with a seed file.", ex);
            }
            catch (Exception ex)
            {
                throw new FortunaException("There was an error initializing Fortuna", ex);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void GetBytes(byte[] data)
        {
            _lock.Wait();
            try
            {
                _innerProvider.GetBytes(data);
            }
            finally
            {
                _lock.Release();
            }
        }

        private void ReseedFileCallback(object state)
        {
            _lock.Wait();
            try
            {
                var bytes = new byte[64];
                GetBytes(bytes);
                UpdateSeedFile(_file, bytes);
            }
            finally
            {
                _lock.Release();
            }
        }

        private static void UpdateSeedFile(Stream file, byte[] seed)
        {
            file.Seek(0, SeekOrigin.Begin);
            file.Write(seed, 0, SeedFileLength);
            file.Flush();
        }

        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed) return;

            _file?.Dispose();
            _innerProvider?.Dispose();
            _lock?.Dispose();

            _isDisposed = true;
        }
    }
}
