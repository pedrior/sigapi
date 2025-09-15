namespace Sigapi.Common.Security;

public interface IUserContext
{
    string Username { get; }

    string Enrollment { get; }
    
    string[] Enrollments { get; }
    
    string SessionId { get; }
}