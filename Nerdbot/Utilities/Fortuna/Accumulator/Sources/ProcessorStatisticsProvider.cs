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
using System.Diagnostics;
using System.Linq;

namespace Nerdbot.Utilities.Fortuna.Accumulator.Sources
{
    public class ProcessorStatisticsProvider : EntropyProviderBase, IDisposable
    {
        private readonly Process _currentProcess = Process.GetCurrentProcess();

        public override string SourceName => "Current Processor Time";
        protected override TimeSpan ScheduledPeriod => TimeSpan.FromMilliseconds(10);
        protected override byte[] GetEntropy()
        {
            var ticks = _currentProcess.TotalProcessorTime.Ticks;
            var vMemory = _currentProcess.VirtualMemorySize64;
            var pagedMemory = _currentProcess.PagedMemorySize64;
            var workingMemory = _currentProcess.WorkingSet64;
            IEnumerable<byte> timeBytes = BitConverter.GetBytes(ticks);
            IEnumerable<byte> vMemoryBytes = BitConverter.GetBytes(vMemory);
            IEnumerable<byte> pagedBytes = BitConverter.GetBytes(pagedMemory);
            IEnumerable<byte> workingBytes = BitConverter.GetBytes(workingMemory);

            if (!BitConverter.IsLittleEndian)
            {
                timeBytes = timeBytes.Reverse().Take(2);
                vMemoryBytes = vMemoryBytes.Reverse().Take(2);
                pagedBytes = pagedBytes.Reverse().Take(2);
                workingBytes = workingBytes.Reverse().Take(2);
            }

            return timeBytes
                .Concat(vMemoryBytes)
                .Concat(pagedBytes)
                .Concat(workingBytes)
                .ToArray();
        }

        #region IDisposable Implementation

        private bool _isDisposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            _currentProcess?.Dispose();

            _isDisposed = true;
        }

        #endregion
    }
}
