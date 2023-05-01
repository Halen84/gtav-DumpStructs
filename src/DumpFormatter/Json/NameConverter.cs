﻿using DumpFormatter.Model;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DumpFormatter.Json;

internal class NameConverter : JsonConverter<Name>
{
    public override Name Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString()!;
        return s.StartsWith("0x") ? Name.FromHash(uint.Parse(s.AsSpan(2), System.Globalization.NumberStyles.HexNumber)) : Name.FromString(s);
    }

    public override void Write(Utf8JsonWriter writer, Name value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
