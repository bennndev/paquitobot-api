using System.Collections;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PaquitoBot.Application.Common.Exceptions;
using PaquitoBot.Application.Repositories;
using PaquitoBot.Infrastructure.Data;

namespace PaquitoBot.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private Hashtable? _repositories;
    private readonly PaquitoBotDbContext _context;
    public IUserStudentRepository UserStudents { get; }
    public IMessageRepository Messages { get; }
    public IAssessmentResultRepository AssessmentResults { get; }
    public IAssessmentPlanRepository AssessmentPlans { get; }
    public IAssessmentRepository Assessments { get; }
    public IAttendanceRecordRepository AttendanceRecords { get; }
    ISyncRunRepository SyncRuns { get; }
    IKnowledgeDocumentRepository KnowledgeDocuments { get; }

    public UnitOfWork(
        PaquitoBotDbContext context,
        IUserStudentRepository userStudents,
        IMessageRepository messages,
        IAssessmentResultRepository assessmentResults,
        IAssessmentPlanRepository assessmentPlans,
        IAssessmentRepository assessments,
        IAttendanceRecordRepository attendanceRecords,
        ISyncRunRepository syncRuns,
        IKnowledgeDocumentRepository knowledgeDocuments)
    {
        _context = context;
        UserStudents = userStudents;
        Messages = messages;
        AssessmentResults = assessmentResults;
        AssessmentPlans = assessmentPlans;
        Assessments = assessments;
        AttendanceRecords = attendanceRecords;
        SyncRuns = syncRuns;
        KnowledgeDocuments = knowledgeDocuments;
    }
    
