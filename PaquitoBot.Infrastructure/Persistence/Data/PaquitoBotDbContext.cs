using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PaquitoBot.Domain.Models;

namespace PaquitoBot.Infrastructure.Data;

public partial class PaquitoBotDbContext : DbContext
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
        modelBuilder.Entity<AcademicPeriod>(entity =>
        {
            entity.HasKey(e => e.AcademicPeriodId).HasName("academic_periods_pkey");

            entity.ToTable("academic_periods", "paquito");

            entity.HasIndex(e => e.FrappeExternalId, "academic_periods_frappe_external_id_key").IsUnique();

            entity.Property(e => e.AcademicPeriodId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("academic_period_id");
            entity.Property(e => e.Code)
                .HasMaxLength(80)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.Name)
                .HasMaxLength(160)
                .HasColumnName("name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Assessment>(entity =>
        {
            entity.HasKey(e => e.AssessmentId).HasName("assessments_pkey");

            entity.ToTable("assessments", "paquito");

            entity.HasIndex(e => e.FrappeExternalId, "assessments_frappe_external_id_key").IsUnique();

            entity.HasIndex(e => e.SectionId, "ix_assessments_section");

            entity.Property(e => e.AssessmentId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("assessment_id");
            entity.Property(e => e.AssessmentDate).HasColumnName("assessment_date");
            entity.Property(e => e.AssessmentType)
                .HasMaxLength(80)
                .HasColumnName("assessment_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.MaximumScore)
                .HasPrecision(10, 2)
                .HasColumnName("maximum_score");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Weight)
                .HasPrecision(10, 4)
                .HasColumnName("weight");

            entity.HasOne(d => d.Section).WithMany(p => p.Assessments)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("assessments_section_id_fkey");
        });

        modelBuilder.Entity<AssessmentPlan>(entity =>
        {
            entity.HasKey(e => e.AssessmentPlanId).HasName("assessment_plans_pkey");

            entity.ToTable("assessment_plans", "paquito");

            entity.HasIndex(e => e.FrappeExternalId, "assessment_plans_frappe_external_id_key").IsUnique();

            entity.HasIndex(e => e.SectionId, "ix_assessment_plans_section");

            entity.Property(e => e.AssessmentPlanId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("assessment_plan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.MaximumScore)
                .HasPrecision(10, 2)
                .HasColumnName("maximum_score");
            entity.Property(e => e.ScheduledDate).HasColumnName("scheduled_date");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Section).WithMany(p => p.AssessmentPlans)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("assessment_plans_section_id_fkey");
        });

        modelBuilder.Entity<AssessmentResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("assessment_results_pkey");

            entity.ToTable("assessment_results", "paquito");

            entity.HasIndex(e => e.FrappeExternalId, "assessment_results_frappe_external_id_key").IsUnique();

            entity.HasIndex(e => new { e.StudentId, e.AssessmentId }, "ix_assessment_results_student");

            entity.Property(e => e.ResultId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("result_id");
            entity.Property(e => e.AssessmentId).HasColumnName("assessment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.Grade)
                .HasPrecision(10, 2)
                .HasColumnName("grade");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Assessment).WithMany(p => p.AssessmentResults)
                .HasForeignKey(d => d.AssessmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("assessment_results_assessment_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.AssessmentResults)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("assessment_results_student_id_fkey");
        });

        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("attendance_records_pkey");

            entity.ToTable("attendance_records", "paquito");

            entity.HasIndex(e => e.FrappeExternalId, "attendance_records_frappe_external_id_key").IsUnique();

            entity.HasIndex(e => e.SectionId, "ix_attendance_section");

            entity.HasIndex(e => new { e.StudentId, e.AttendanceDate }, "ix_attendance_student_date");

            entity.Property(e => e.AttendanceId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("attendance_id");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Section).WithMany(p => p.AttendanceRecords)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("attendance_records_section_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.AttendanceRecords)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("attendance_records_student_id_fkey");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("conversations_pkey");

            entity.ToTable("conversations", "paquito");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "ix_conversations_user_date");

            entity.Property(e => e.ConversationId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("conversations_user_id_fkey");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("courses_pkey");

            entity.ToTable("courses", "paquito");

            entity.HasIndex(e => e.FrappeExternalId, "courses_frappe_external_id_key").IsUnique();

            entity.HasIndex(e => e.FrappeModifiedAt, "ix_courses_sync");

            entity.Property(e => e.CourseId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("course_id");
            entity.Property(e => e.Code)
                .HasMaxLength(80)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Credits).HasColumnName("credits");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<CourseSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("course_schedules_pkey");

            entity.ToTable("course_schedules", "paquito");

            entity.HasIndex(e => e.FrappeExternalId, "course_schedules_frappe_external_id_key").IsUnique();

            entity.HasIndex(e => new { e.InstructorId, e.ClassDate }, "ix_course_schedules_instructor_date");

            entity.HasIndex(e => new { e.SectionId, e.ClassDate }, "ix_course_schedules_section_date");

            entity.Property(e => e.ScheduleId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("schedule_id");
            entity.Property(e => e.ClassDate).HasColumnName("class_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeliveryMode)
                .HasMaxLength(40)
                .HasColumnName("delivery_mode");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Instructor).WithMany(p => p.CourseSchedules)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("course_schedules_instructor_id_fkey");

            entity.HasOne(d => d.Section).WithMany(p => p.CourseSchedules)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("course_schedules_section_id_fkey");
        });

        modelBuilder.Entity<CourseSection>(entity =>
        {
            entity.HasKey(e => e.SectionId).HasName("course_sections_pkey");

            entity.ToTable("course_sections", "paquito");

            entity.HasIndex(e => e.FrappeExternalId, "course_sections_frappe_external_id_key").IsUnique();

            entity.HasIndex(e => new { e.AcademicPeriodId, e.CourseId }, "ix_course_sections_period");

            entity.Property(e => e.SectionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("section_id");
            entity.Property(e => e.AcademicPeriodId).HasColumnName("academic_period_id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeliveryMode)
                .HasMaxLength(40)
                .HasColumnName("delivery_mode");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.InstructorName)
                .HasMaxLength(160)
                .HasColumnName("instructor_name");
            entity.Property(e => e.SectionCode)
                .HasMaxLength(80)
                .HasColumnName("section_code");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.AcademicPeriod).WithMany(p => p.CourseSections)
                .HasForeignKey(d => d.AcademicPeriodId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("course_sections_academic_period_id_fkey");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSections)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("course_sections_course_id_fkey");
        });

        modelBuilder.Entity<FeeSchedule>(entity =>
        {
            entity.HasKey(e => e.FeeScheduleId).HasName("fee_schedules_pkey");

            entity.ToTable("fee_schedules", "paquito");

            entity.HasIndex(e => e.FrappeExternalId, "fee_schedules_frappe_external_id_key").IsUnique();

            entity.HasIndex(e => new { e.FeeStructureId, e.DueDate }, "ix_fee_schedules_structure_due");

            entity.Property(e => e.FeeScheduleId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("fee_schedule_id");
            entity.Property(e => e.AcademicPeriodId).HasColumnName("academic_period_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.FeeStructureId).HasColumnName("fee_structure_id");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.ScheduledAmount)
                .HasPrecision(14, 2)
                .HasColumnName("scheduled_amount");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.AcademicPeriod).WithMany(p => p.FeeSchedules)
                .HasForeignKey(d => d.AcademicPeriodId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fee_schedules_academic_period_id_fkey");

            entity.HasOne(d => d.FeeStructure).WithMany(p => p.FeeSchedules)
                .HasForeignKey(d => d.FeeStructureId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fee_schedules_fee_structure_id_fkey");
        });

        modelBuilder.Entity<FeeStructure>(entity =>
        {
            entity.HasKey(e => e.FeeStructureId).HasName("fee_structures_pkey");

            entity.ToTable("fee_structures", "paquito");

            entity.HasIndex(e => e.FrappeExternalId, "fee_structures_frappe_external_id_key").IsUnique();

            entity.HasIndex(e => e.ProgramId, "ix_fee_structures_program");

            entity.Property(e => e.FeeStructureId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("fee_structure_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .HasDefaultValueSql("'PEN'::character varying")
                .HasColumnName("currency_code");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(14, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Program).WithMany(p => p.FeeStructures)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fee_structures_program_id_fkey");
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.InstructorId).HasName("instructors_pkey");

            entity.ToTable("instructors", "paquito");

            entity.HasIndex(e => e.FrappeExternalId, "instructors_frappe_external_id_key").IsUnique();

            entity.Property(e => e.InstructorId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("instructor_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(160)
                .HasColumnName("first_name");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.FullName)
                .HasMaxLength(320)
                .HasColumnName("full_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(160)
                .HasColumnName("last_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(40)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<KnowledgeDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("knowledge_documents_pkey");

            entity.ToTable("knowledge_documents", "paquito");

            entity.HasIndex(e => e.CourseId, "ix_knowledge_documents_course");

            entity.Property(e => e.DocumentId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("document_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.ContentHash)
                .HasMaxLength(128)
                .HasColumnName("content_hash");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(60)
                .HasColumnName("document_type");
            entity.Property(e => e.IndexedAt).HasColumnName("indexed_at");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("source");
            entity.Property(e => e.SourceUrl).HasColumnName("source_url");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Course).WithMany(p => p.KnowledgeDocuments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("knowledge_documents_course_id_fkey");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("messages_pkey");

            entity.ToTable("messages", "paquito");

            entity.HasIndex(e => new { e.ConversationId, e.CreatedAt }, "ix_messages_conversation_date");

            entity.Property(e => e.MessageId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("message_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.SenderType)
                .HasMaxLength(40)
                .HasColumnName("sender_type");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("messages_conversation_id_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payments_pkey");

            entity.ToTable("payments", "paquito");

            entity.HasIndex(e => new { e.StudentId, e.PaidOn }, "ix_payments_student_paid");

            entity.HasIndex(e => e.TuitionFeeId, "ix_payments_tuition_fee");

            entity.HasIndex(e => e.FrappeExternalId, "payments_frappe_external_id_key").IsUnique();

            entity.Property(e => e.PaymentId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(14, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .HasDefaultValueSql("'PEN'::character varying")
                .HasColumnName("currency_code");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.PaidOn).HasColumnName("paid_on");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(80)
                .HasColumnName("payment_method");
            entity.Property(e => e.Reference)
                .HasMaxLength(140)
                .HasColumnName("reference");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.TuitionFeeId).HasColumnName("tuition_fee_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Student).WithMany(p => p.Payments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("payments_student_id_fkey");

            entity.HasOne(d => d.TuitionFee).WithMany(p => p.Payments)
                .HasForeignKey(d => d.TuitionFeeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("payments_tuition_fee_id_fkey");
        });

        modelBuilder.Entity<Program>(entity =>
        {
            entity.HasKey(e => e.ProgramId).HasName("programs_pkey");

            entity.ToTable("programs", "paquito");

            entity.HasIndex(e => e.FrappeModifiedAt, "ix_programs_sync");

            entity.HasIndex(e => e.FrappeExternalId, "programs_frappe_external_id_key").IsUnique();

            entity.Property(e => e.ProgramId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("program_id");
            entity.Property(e => e.Code)
                .HasMaxLength(80)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ProgramCourse>(entity =>
        {
            entity.HasKey(e => e.ProgramCourseId).HasName("program_courses_pkey");

            entity.ToTable("program_courses", "paquito");

            entity.HasIndex(e => e.CourseId, "ix_program_courses_course");

            entity.HasIndex(e => new { e.ProgramId, e.CourseId }, "uq_program_courses").IsUnique();

            entity.Property(e => e.ProgramCourseId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("program_course_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CycleNumber).HasColumnName("cycle_number");
            entity.Property(e => e.IsRequired)
                .HasDefaultValue(true)
                .HasColumnName("is_required");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");

            entity.HasOne(d => d.Course).WithMany(p => p.ProgramCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("program_courses_course_id_fkey");

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramCourses)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("program_courses_program_id_fkey");
        });

        modelBuilder.Entity<ProgramEnrollment>(entity =>
        {
            entity.HasKey(e => e.ProgramEnrollmentId).HasName("program_enrollments_pkey");

            entity.ToTable("program_enrollments", "paquito");

            entity.HasIndex(e => e.AcademicPeriodId, "ix_program_enrollments_period");

            entity.HasIndex(e => e.ProgramId, "ix_program_enrollments_program");

            entity.HasIndex(e => e.StudentId, "ix_program_enrollments_student");

            entity.HasIndex(e => e.FrappeExternalId, "program_enrollments_frappe_external_id_key").IsUnique();

            entity.Property(e => e.ProgramEnrollmentId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("program_enrollment_id");
            entity.Property(e => e.AcademicPeriodId).HasColumnName("academic_period_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EnrollmentDate).HasColumnName("enrollment_date");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.AcademicPeriod).WithMany(p => p.ProgramEnrollments)
                .HasForeignKey(d => d.AcademicPeriodId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("program_enrollments_academic_period_id_fkey");

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramEnrollments)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("program_enrollments_program_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.ProgramEnrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("program_enrollments_student_id_fkey");
        });

        modelBuilder.Entity<SectionEnrollment>(entity =>
        {
            entity.HasKey(e => e.SectionEnrollmentId).HasName("section_enrollments_pkey");

            entity.ToTable("section_enrollments", "paquito");

            entity.HasIndex(e => e.SectionId, "ix_section_enrollments_section");

            entity.HasIndex(e => e.FrappeExternalId, "section_enrollments_frappe_external_id_key").IsUnique();

            entity.HasIndex(e => new { e.StudentId, e.SectionId }, "uq_section_enrollments_pair").IsUnique();

            entity.Property(e => e.SectionEnrollmentId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("section_enrollment_id");
            entity.Property(e => e.AttendancePercentage)
                .HasPrecision(7, 2)
                .HasColumnName("attendance_percentage");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EnrollmentDate).HasColumnName("enrollment_date");
            entity.Property(e => e.FinalGrade)
                .HasPrecision(10, 2)
                .HasColumnName("final_grade");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Section).WithMany(p => p.SectionEnrollments)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("section_enrollments_section_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.SectionEnrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("section_enrollments_student_id_fkey");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("students_pkey");

            entity.ToTable("students", "paquito");

            entity.HasIndex(e => e.FrappeModifiedAt, "ix_students_sync");

            entity.HasIndex(e => e.FrappeExternalId, "students_frappe_external_id_key").IsUnique();

            entity.Property(e => e.StudentId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("student_id");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.FirstNames)
                .HasMaxLength(160)
                .HasColumnName("first_names");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.FullName)
                .HasMaxLength(320)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(40)
                .HasColumnName("gender");
            entity.Property(e => e.LastNames)
                .HasMaxLength(160)
                .HasColumnName("last_names");
            entity.Property(e => e.Phone)
                .HasMaxLength(40)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.StudentCode)
                .HasMaxLength(80)
                .HasColumnName("student_code");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<SyncRun>(entity =>
        {
            entity.HasKey(e => e.SyncRunId).HasName("sync_runs_pkey");

            entity.ToTable("sync_runs", "paquito");

            entity.HasIndex(e => new { e.EntityName, e.StartDate }, "ix_sync_runs_entity_date");

            entity.Property(e => e.SyncRunId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("sync_run_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.EntityName)
                .HasMaxLength(140)
                .HasColumnName("entity_name");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.ProcessedRecords).HasColumnName("processed_records");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
        });

        modelBuilder.Entity<TuitionFee>(entity =>
        {
            entity.HasKey(e => e.TuitionFeeId).HasName("tuition_fees_pkey");

            entity.ToTable("tuition_fees", "paquito");

            entity.HasIndex(e => e.FeeScheduleId, "ix_tuition_fees_schedule");

            entity.HasIndex(e => new { e.StudentId, e.DueDate }, "ix_tuition_fees_student_due");

            entity.HasIndex(e => e.FrappeExternalId, "tuition_fees_frappe_external_id_key").IsUnique();

            entity.Property(e => e.TuitionFeeId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("tuition_fee_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .HasDefaultValueSql("'PEN'::character varying")
                .HasColumnName("currency_code");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.FeeScheduleId).HasColumnName("fee_schedule_id");
            entity.Property(e => e.FrappeExternalId)
                .HasMaxLength(140)
                .HasColumnName("frappe_external_id");
            entity.Property(e => e.FrappeModifiedAt).HasColumnName("frappe_modified_at");
            entity.Property(e => e.IssuedOn).HasColumnName("issued_on");
            entity.Property(e => e.OriginalAmount)
                .HasPrecision(14, 2)
                .HasColumnName("original_amount");
            entity.Property(e => e.OutstandingBalance)
                .HasPrecision(14, 2)
                .HasColumnName("outstanding_balance");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.FeeSchedule).WithMany(p => p.TuitionFees)
                .HasForeignKey(d => d.FeeScheduleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tuition_fees_fee_schedule_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.TuitionFees)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("tuition_fees_student_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users", "paquito");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(40)
                .HasDefaultValueSql("'student'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserStudent>(entity =>
        {
            entity.HasKey(e => e.LinkId).HasName("user_students_pkey");

            entity.ToTable("user_students", "paquito");

            entity.HasIndex(e => e.StudentId, "ix_user_students_student");

            entity.HasIndex(e => new { e.UserId, e.StudentId }, "uq_user_students_pair").IsUnique();

            entity.HasIndex(e => e.UserId, "ux_user_students_primary")
                .IsUnique()
                .HasFilter("(is_primary = true)");

            entity.Property(e => e.LinkId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("link_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(true)
                .HasColumnName("is_primary");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Student).WithMany(p => p.UserStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("user_students_student_id_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.UserStudent)
                .HasForeignKey<UserStudent>(d => d.UserId)
                .HasConstraintName("user_students_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
