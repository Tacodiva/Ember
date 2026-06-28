using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Json.Schema;

namespace Ember.Utils;

public static class JsonSchemaExt {
    public static JsonDocument ValidateFile(this JsonSchema schema, string path, JsonDocumentOptions options = default) {
        return ValidateDocument(schema, JsonDocument.Parse(File.ReadAllText(path), options), path);
    }

    public static async Task<JsonDocument> ValidateFileAsync(this JsonSchema schema, string path, JsonDocumentOptions options = default) {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        return ValidateDocument(schema, await JsonDocument.ParseAsync(stream, options), path);
    }

    public static JsonDocument ValidateString(this JsonSchema schema, string content, string path = "", JsonDocumentOptions options = default) {
        return ValidateDocument(schema, JsonDocument.Parse(content, options), path);
    }

    public static JsonDocument ValidateDocument(this JsonSchema schema, JsonDocument doc, string path = "") {
        EvaluationResults results = schema.Evaluate(doc, new EvaluationOptions() {
            ValidateAgainstMetaSchema = false,
            OutputFormat = OutputFormat.List
        });
        if (!results.IsValid)
            throw new InvalidJsonException(results, path);
        return doc;
    }
}