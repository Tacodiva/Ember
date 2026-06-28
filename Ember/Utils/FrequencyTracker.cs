
using System;
using System.Diagnostics;

namespace Ember.Utils;

public sealed class FrequencyTracker {

    private static readonly Stopwatch _Stopwatch = Stopwatch.StartNew();

    private long _lastSampleTime;

    private int _sampleIndex;
    private bool _samplesFull;
    private readonly long[] _samples;
    private long _samplesSum;

    public float Frequency {
        get {
            int sampleCount = _samplesFull ? _samples.Length : _sampleIndex;
            if (sampleCount == 0) return float.NaN;
            float averageDeltaTicks = _samplesSum / (float)sampleCount;
            return Stopwatch.Frequency / averageDeltaTicks;
        }
    }

    public float AverageDeltaMS {
        get {
            int sampleCount = _samplesFull ? _samples.Length : _sampleIndex;
            if (sampleCount == 0) return float.NaN;
            float averageDeltaTicks = _samplesSum / (float)sampleCount;
            return (averageDeltaTicks * 1000) / Stopwatch.Frequency;
        }
    }

    public FrequencyTracker(int sampleCount = 16) {
        ArgumentOutOfRangeException.ThrowIfLessThan(sampleCount, 1);
        _samples = new long[sampleCount];
        Reset();
    }

    public void Reset() {
        _lastSampleTime = -1;
        _sampleIndex = 0;
        _samplesFull = false;
        _samplesSum = 0;
    }

    public void Sample() {
        long time = _Stopwatch.ElapsedTicks;

        if (_lastSampleTime == -1) {
            _lastSampleTime = time;
            return;
        }

        Sample(time - _lastSampleTime);

        _lastSampleTime = time;
    }

    public void Sample(long elapsedStopwatchTicks) {
        if (_sampleIndex == _samples.Length) _sampleIndex = 0;

        if (_samplesFull) _samplesSum -= _samples[_sampleIndex];

        _samplesSum += _samples[_sampleIndex++] = elapsedStopwatchTicks;

        if (_sampleIndex == _samples.Length) _samplesFull = true;
    }
}