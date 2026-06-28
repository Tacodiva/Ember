
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ember.Utils;

public struct AwaitableEvent {
    public readonly Action<ManualResetEventSlim> FinalizeAction;

    public readonly ManualResetEventSlim? Event {
        get {
            if (_finalizeRun) return default;
            return _event;
        }
    }

    private readonly ManualResetEventSlim _event;
    private AtomicBool _finalizeRun;

    public readonly bool IsNull => _event == null;

    public AwaitableEvent(ManualResetEventSlim @event, Action<ManualResetEventSlim>? finalize = null) {
        _event = @event;

        finalize ??= @event => @event.Dispose();
        FinalizeAction = finalize;
    }

    public readonly EventAwaiter GetAwaiter() => new(this);

    public readonly struct EventAwaiter : ICriticalNotifyCompletion {
        public readonly AwaitableEvent AwaitableEvent;

        public bool IsCompleted => AwaitableEvent._finalizeRun || AwaitableEvent._event.IsSet;

        public EventAwaiter(AwaitableEvent awaitableEvent) {
            AwaitableEvent = awaitableEvent;
        }

        private void Wait() {
            if (AwaitableEvent._finalizeRun) return;
            AwaitableEvent._event.Wait();
            if (AwaitableEvent._finalizeRun.FalseToTrue())
                AwaitableEvent.FinalizeAction(AwaitableEvent._event);
        }

        public void GetResult() => Wait();

        public void OnCompleted(Action continuation) {
            Wait();
            continuation();
        }

        public void UnsafeOnCompleted(Action continuation) {
            Wait();
            continuation();
        }
    }
}
