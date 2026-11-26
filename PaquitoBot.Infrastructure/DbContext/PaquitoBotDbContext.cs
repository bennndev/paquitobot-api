using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.DbContext;

public partial class PaquitoBotDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public PaquitoBotDbContext(DbContextOptions<PaquitoBotDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcademicPeriod> AcademicPeriods { get; set; }

    public virtual DbSet<Assessment> Assessments { get; set; }

    public virtual DbSet<AssessmentPlan> AssessmentPlans { get; set; }

    public virtual DbSet<AssessmentResult> AssessmentResults { get; set; }

    public virtual DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseSchedule> CourseSchedules { get; set; }

    public virtual DbSet<CourseSection> CourseSections { get; set; }

    public virtual DbSet<FeeSchedule> FeeSchedules { get; set; }

    public virtual DbSet<FeeStructure> FeeStructures { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<KnowledgeDocument> KnowledgeDocuments { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Program> Programs { get; set; }

    public virtual DbSet<ProgramCourse> ProgramCourses { get; set; }

    public virtual DbSet<ProgramEnrollment> ProgramEnrollments { get; set; }

    public virtual DbSet<SectionEnrollment> SectionEnrollments { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<SyncRun> SyncRuns { get; set; }

    public virtual DbSet<TuitionFee> TuitionFees { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserStudent> UserStudents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaquitoBotDbContext).Assembly);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
