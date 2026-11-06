using System;
using System.Collections.Generic;

namespace PaquitoBot.Infrastructure.Models;

public partial class Course
{
    public Guid CourseId { get; set; }

    public string FrappeExternalId { get; set; } = null!;

    public string? Code { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? Credits { get; set; }

    public string? Status { get; set; }

    public DateTime? FrappeModifiedAt { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();

    public virtual ICollection<KnowledgeDocument> KnowledgeDocuments { get; set; } = new List<KnowledgeDocument>();

    public virtual ICollection<ProgramCourse> ProgramCourses { get; set; } = new List<ProgramCourse>();
}
