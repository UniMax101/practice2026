using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace task13;

public static class StudentSerializer
{
    private static readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new DateTimeConverter() }
    };

    public static string Serialize(Student student)
    {
        if (student == null) throw new ArgumentNullException(nameof(student));
        return JsonSerializer.Serialize(student, options);
    }

    public static Student Deserialize(string json)
    {
        if (json == null) throw new ArgumentNullException(nameof(json));

        Student? student = JsonSerializer.Deserialize<Student>(json, options);

        if (student == null)
            throw new Exception("Не удалось десериализовать объект.");

        return student;
    }
    public static void Save(Student student, string path)
    {
        if (student == null) throw new ArgumentNullException(nameof(student));
        if (path == null) throw new ArgumentNullException(nameof(path));

        string json = Serialize(student);
        File.WriteAllText(path, json);
    }
    public static Student Load(string path)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (!File.Exists(path)) throw new FileNotFoundException("Файл не найден", path);

        string json = File.ReadAllText(path);
        return Deserialize(json);
    }
}
public class DateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "dd.MM.yyyy";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return DateTime.ParseExact(value!, Format, CultureInfo.InvariantCulture);
    }
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}
