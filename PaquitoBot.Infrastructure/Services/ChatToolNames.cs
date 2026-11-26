namespace PaquitoBot.Infrastructure.Services;

internal static class ChatToolNames
{
    public const string Courses = "get_my_courses";
    public const string Assessments = "get_my_assessments";
    public const string Grades = "get_my_grades";
    public const string Attendance = "get_my_attendance";
    public const string Finances = "get_my_finances";
    public const string Schedule = "get_my_schedule";
    public const string AssessmentPlans = "get_my_assessment_plans";

    public static bool Contains(string name)
    {
        return name is Courses or Assessments or Grades or Attendance or Finances or Schedule or AssessmentPlans;
    }
}
