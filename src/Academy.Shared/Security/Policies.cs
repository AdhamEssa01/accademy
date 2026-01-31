namespace Academy.Shared.Security;

public static class Policies
{
    public const string Admin = "Admin";
    public const string Instructor = "Instructor";
    public const string Parent = "Parent";
    public const string Student = "Student";
    public const string Staff = "Staff";
    public const string AnyAuthenticated = "AnyAuthenticated";
}