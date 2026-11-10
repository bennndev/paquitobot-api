namespace PaquitoBot.Domain.Models;

public partial class ProgramCourse
{
    public Guid ProgramCourseId { get; set; }

    public Guid ProgramId { get; set; }

    public Guid CourseId { get; set; }

    public int? CycleNumber { get; set; }

    public bool IsRequired { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Program Program { get; set; } = null!;
}
