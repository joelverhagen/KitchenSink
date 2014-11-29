using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Knapcode.KitchenSink.Support
{
    public class EventTimer : IDisposable
    {
        private readonly Action<TimeSpan> _onCompletion;
        private readonly Stopwatch _stopwatch;

        private EventTimer(Action<TimeSpan> onCompletion)
        {
            _onCompletion = onCompletion;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _onCompletion(_stopwatch.Elapsed);
        }

        private static EventTimer Enter(Action<TimeSpan> onCompletion)
        {
            return new EventTimer(onCompletion);
        }

        public static TimeSpan Time(Action action)
        {
            TimeSpan timeSpan = TimeSpan.Zero;
            using (Enter(d => timeSpan = d))
            {
                action();
            }
            return timeSpan;
        }

        public static async Task<TimeSpan> TimeAsync(Func<Task> actionAsync)
        {
            TimeSpan timeSpan = TimeSpan.Zero;
            using (Enter(d => timeSpan = d))
            {
                await actionAsync();
            }
            return timeSpan;
        }
    }
}