using SessionCode.Artifacts;

namespace SessionCode.Patterns;

#region Events

public abstract record EventBase(Guid Id, Guid AggregateId, DateTimeOffset Timestamp);

public record StudentCreated(Guid Id, Guid AggregateId, DateTimeOffset Timestamp)
    : EventBase(Id, AggregateId, Timestamp)
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
}

public record LastNameChanged(Guid Id, Guid AggregateId, DateTimeOffset Timestamp, string NewLastName)
    : EventBase(Id, AggregateId, Timestamp);

public record StudentGotMarried(Guid Id, Guid AggregateId, DateTimeOffset Timestamp, DateTimeOffset MarriedAt)
    : EventBase(Id, AggregateId, Timestamp);

public record PassedExam(Guid Id, Guid AggregateId, DateTimeOffset Timestamp, Guid CourseId)
    : EventBase(Id, AggregateId, Timestamp);

public record FailedExam(Guid Id, Guid AggregateId, DateTimeOffset Timestamp, Guid CourseId)
    : EventBase(Id, AggregateId, Timestamp);

#endregion

#region Aggregate

public class Student
{
    private const string ApplyMethod = "Apply";
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateTimeOffset? IsMarried { get; private set; }
    public int TotalExamsTaken { get; private set; }
    public DateTimeOffset LastChanged { get; private set; }

    private List<EventBase> _unconfirmedEvents = [];

    private Student()
    {
    }

    public static Student FromEvents(IReadOnlyCollection<EventBase> events)
    {
        var student = new Student();
        foreach (var @event in events)
        {
            student.ApplyInternal(@event);
        }

        return student;
    }

    public static Student Create(string firstName, string lastName, TimeProvider timeProvider)
    {
        var student = new Student();
        student.RaiseEvent(new StudentCreated(Guid.NewGuid(), Guid.NewGuid(), timeProvider.GetUtcNow()));
        return student;
    }

    public void Married(DateTimeOffset marriedAt, TimeProvider timeProvider) =>
        RaiseEvent(new StudentGotMarried(Guid.NewGuid(), Id, timeProvider.GetUtcNow(), marriedAt));

    public void ChangeName(string name, TimeProvider timeProvider) =>
        RaiseEvent(new LastNameChanged(Guid.NewGuid(), Id, timeProvider.GetUtcNow(), name));

    public void PassedExam(Guid courseId, TimeProvider timeProvider) =>
        RaiseEvent(new PassedExam(Guid.NewGuid(), Id, timeProvider.GetUtcNow(), courseId));

    public void FailedExam(Guid courseId, TimeProvider timeProvider) =>
        RaiseEvent(new FailedExam(Guid.NewGuid(), Id, timeProvider.GetUtcNow(), courseId));

    private void RaiseEvent(EventBase ev)
    {
        ApplyInternal(ev);
        _unconfirmedEvents.Add(ev);
    }

    public void Apply(StudentCreated ev)
    {
        Id = ev.AggregateId;
        FirstName = ev.FirstName;
        LastName = ev.LastName;
        EventPerformed(ev);
    }

    public void Apply(LastNameChanged ev)
    {
        LastName = ev.NewLastName;
        EventPerformed(ev);
    }

    public void Apply(StudentGotMarried ev)
    {
        IsMarried = ev.MarriedAt;
        EventPerformed(ev);
    }

    public void Apply(PassedExam ev)
    {
        TotalExamsTaken++;
        EventPerformed(ev);
    }

    public void Apply(FailedExam ev)
    {
        TotalExamsTaken++;
        EventPerformed(ev);
    }

    private void EventPerformed(EventBase ev)
    {
        LastChanged = ev.Timestamp;
    }

    public IReadOnlyCollection<EventBase> GetUnconfirmedEvents()
    {
        var events = _unconfirmedEvents.ToList();
        _unconfirmedEvents.Clear();
        return events;
    }

    private void ApplyInternal(EventBase ev)
    {
        this.InvokeIfExists(ApplyMethod, ev);
    }
}

#endregion

#region Usage

public class EventSourcing
{
    private readonly Guid Programmieren3 = Guid.NewGuid();
    private readonly Guid EsoterikInDerInformatik = Guid.NewGuid();

    public void DoIt()
    {
        var timeProvider = TimeProvider.System;

        var student = Student.Create("Chuck", "Norris", timeProvider);
        student.PassedExam(Programmieren3, timeProvider);
        student.FailedExam(EsoterikInDerInformatik, timeProvider);
        student.Married(new DateTime(2000, 1, 2), timeProvider);

        var events = student.GetUnconfirmedEvents();

        var hydratedStudent = Student.FromEvents(events);
    }
}

#endregion