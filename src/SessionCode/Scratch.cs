namespace SessionCode;

public class StudentCreated
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public int Age { get; init; }
}

public record StudentRecord(string FirstName, string LastName);

public class Usage
{
    public void DoIt()
    {
        StudentCreated student = new StudentCreated();

        string category = student.Age switch
        {
            12 => "kleines Genie",
            > 20 => "junges Genie",
            _ => "Normal",
        };
    }
}