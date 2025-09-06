namespace Sigapi.Features.Account.Models;

public sealed class User(string username, Enrollment enrollment, IEnumerable<Enrollment> enrollments)
{
    public User(string username, Enrollment enrollment) : this(username, enrollment, [enrollment])
    {
    }

    public string Username => username;

    public Enrollment Enrollment => enrollment;

    public IEnumerable<Enrollment> Enrollments => enrollments;
}