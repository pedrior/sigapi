namespace Sigapi.Features.Faculties.Scraping;

public static class FacultyPages
{
    public const string FacultyListPage = "sigaa/public/centro/lista.jsf";
    
    private const string FacultyPageFormat = "sigaa/public/centro/portal.jsf?lc=pt_BR&id={0}";
    private const string DepartmentsPageFormat = "sigaa/public/centro/lista_departamentos.jsf?lc=pt_BR&id={0}";
    private const string GraduatePageFormat = "sigaa/public/centro/lista_programas.jsf?lc=pt_BR&id={0}";
    private const string UndergraduatePageFormat = "sigaa/public/centro/lista_cursos.jsf?lc=pt_BR&id={0}";
    
    public static string GetFacultyUrl(string id) => string.Format(FacultyPageFormat, id);
    
    public static string GetDepartmentsUrl(string id) => string.Format(DepartmentsPageFormat, id);
    
    public static string GetGraduateProgramsUrl(string id) => string.Format(GraduatePageFormat, id);
    
    public static string GetUndergraduateCoursesUrl(string id) => string.Format(UndergraduatePageFormat, id);
}