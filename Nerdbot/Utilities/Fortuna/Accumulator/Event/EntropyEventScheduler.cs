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
using System.Threading.Tasks;

namespace Nerdbot.Utilities.Fortuna.Accumulator.Event
{
    public class EntropyEventScheduler : IEventScheduler
    {
        public event EntropyAvailableHandler EntropyAvailable;

        public void ScheduleEvent(int source, IScheduledEvent @event)
        {
            // The resolution of our scheduler (Task.Delay) is approximately 15ms (on Windows), which should be sufficient for our purposes
            Task.Delay(@event.ScheduledPeriod)
                .ContinueWith(t => RaiseEvent(source, @event))
                .ContinueWith(t => ScheduleEvent(source, @event), TaskContinuationOptions.ExecuteSynchronously);
        }

        // Including the 'source' value as an argument here is an explicit design decision.
        // Section 9.5.3.1 specifies that this should be passed at the entropy provider level for security reasons, but because all sources are
        // to be used from within the same Application Domain (and hence same shared memory), this is an acceptable risk.
        private void RaiseEvent(int source, IScheduledEvent @event)
        {
            EntropyAvailable?.Invoke(source, @event.EventCallback());
        }
    }
}
