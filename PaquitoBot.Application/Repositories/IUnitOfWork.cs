namespace PaquitoBot.Application.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    IUserStudentRepository UserStudents { get; }
    IMessageRepository Messages { get; }
    IAssessmentResultRepository AssessmentResults { get; }
    IAssessmentPlanRepository AssessmentPlans { get; }
    IAssessmentRepository Assessments { get; }
    IAttendanceRecordRepository AttendanceRecords { get; }
}
}
