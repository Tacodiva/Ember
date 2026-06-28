
using System;
using System.Runtime.Serialization;
using System.Text;
using Json.Schema;

namespace Ember.Utils;

public class InvalidJsonException : Exception {

    private static string CreateMessage(EvaluationResults results, string? path = null) {
        StringBuilder sb = new($"Invalid JSON{(path == null ? "" : $" {path}")}:");
        foreach (EvaluationResults subresult in results.Details)
            if (subresult.Errors != null)
                foreach (var result in subresult.Errors)
                    sb.AppendLine(path + subresult.InstanceLocation + " : " + result.Value);
        return sb.ToString();
    }

    public readonly EvaluationResults? Results;
    public readonly string Path;

    public InvalidJsonException(EvaluationResults results, string? path = null) : this(CreateMessage(results, path)) {
        Results = results;
        Path = path ?? "";
    }

    public InvalidJsonException(string message) : base(message) {
        Path = "";
    }

}