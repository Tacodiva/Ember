
using System;
using System.Text.Json;

namespace Ember.Utils;

public static class JsonElementExt {

    public static string GetRequiredString(this JsonElement element) {
        string? s = element.GetString();
        if (s == null) throw new JsonException($"Element contains a null value. Expected a string.");
        return s;
    }

    public static string? GetString(this JsonElement element, ReadOnlySpan<char> property) {
        if (element.TryGetProperty(property, out JsonElement value)) return value.GetString();
        return null;
    }

    public static string GetRequiredString(this JsonElement element, ReadOnlySpan<char> property) {
        return element.GetProperty(property).GetRequiredString();
    }

    public static int GetInt32(this JsonElement element, ReadOnlySpan<char> property) {
        return element.GetProperty(property).GetInt32();
    }

    public static bool GetBoolean(this JsonElement element, ReadOnlySpan<char> property) {
        return element.GetProperty(property).GetBoolean();
    }

    public static double GetDouble(this JsonElement element, ReadOnlySpan<char> property) {
        return element.GetProperty(property).GetDouble();
    }

    public static decimal GetDecimal(this JsonElement element, ReadOnlySpan<char> property) {
        return element.GetProperty(property).GetDecimal();
    }

    public static DateTime GetDateTime(this JsonElement element, ReadOnlySpan<char> property) {
        return element.GetProperty(property).GetDateTime();
    }

}