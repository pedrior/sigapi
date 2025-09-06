namespace Sigapi.Common.Auth;

public interface IUserContext
{
    string Username { get; }

    string Enrollment { get; }
    
    string[] Enrollments { get; }
    
    string SessionId { get; }
}