using System;
using System.IO;
using System.Collections.Generic;
using Xunit;
using task13;

public class StudentSerializerTests
{
    private Student CreateTestStudent()
    {
        return new Student
        {
            FirstName = "Иван",
            LastName = "Иванов",
            BirthDate = new DateTime(2000, 1, 15),
            Grades = new List<Subject>
            {
                new Subject { Name = "Математика", Grade = 5 },
                new Subject { Name = "Физика", Grade = 4 }
            }
        };
    }

    [Fact]
    public void Serialize_ShouldReturnJson()
    {
        var student = CreateTestStudent();
        var json = StudentSerializer.Serialize(student);
        Assert.Contains("Иван", json);
        Assert.Contains("15.01.2000", json);
    }

    [Fact]
    public void Deserialize_ShouldReturnStudent()
    {
        var student = CreateTestStudent();
        var json = StudentSerializer.Serialize(student);
        var result = StudentSerializer.Deserialize(json);
        Assert.Equal("Иван", result.FirstName);
        Assert.Equal(new DateTime(2000, 1, 15), result.BirthDate);
    }

    [Fact]
    public void SaveAndLoad_ShouldReturnSameStudent()
    {
        var student = CreateTestStudent();
        var path = Path.GetTempFileName();

        StudentSerializer.Save(student, path);
        var loaded = StudentSerializer.Load(path);

        Assert.Equal(student.FirstName, loaded.FirstName);
        Assert.Equal(student.BirthDate, loaded.BirthDate);
        Assert.Equal(student.Grades.Count, loaded.Grades.Count);

        File.Delete(path);
    }

    [Fact]
    public void Serialize_ShouldThrowOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => StudentSerializer.Serialize(null!));
    }

    [Fact]
    public void Load_ShouldThrowIfFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() => StudentSerializer.Load("nonexistent.json"));
    }
}
