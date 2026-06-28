
using System;
using System.Text;

namespace Ember.Logging;

public record struct LogMessage(Logger? Source, string Text, LogLevel Level) {
    public override string ToString() {
        StringBuilder sb = Log.ThreadStringBuilder;

        sb.Append('[');
        sb.Append(Level.GetShortName());
        sb.Append(']');
        if (Source != null) {
            sb.Append(Source.Prefix);
        }
        sb.Append(' ');
        sb.Append(Text);

        string stringified = sb.ToString();
        sb.Clear();
        
        return stringified;
    }
}