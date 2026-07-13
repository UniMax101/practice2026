using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Subject
{
    public string Name {get; set;} = string.Empty;
    public int Grade {get; set;}
}
public class Student
{
    public string FirstName {get; set;} = string.Empty;
    public string LastName {get; set;} = string.Empty;
    public DateTime BirthDate {get; set;}
    public List<Subject> Grades {get; set;} = new();
}
