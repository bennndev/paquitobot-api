using PaquitoBot.Application.Frappe;
using PaquitoBot.Application.Frappe.Dtos;
using PaquitoBot.Application.Tests.Fakes;

namespace PaquitoBot.Application.Tests.Frappe;

public class FakeFrappeClientTests
{
    [Fact]
    public async Task GetStudentsAsync_ReturnsConfiguredStudents()
    {
        var fake = new FakeFrappeClient();
        fake.Students.Add(new FrappeStudentDto("STU-001", "S001", "Ana", "Lopez", "Ana Lopez", null, null, null, null, "Active", null));
        IFrappeClient client = fake;

        var students = await client.GetStudentsAsync();

        var student = Assert.Single(students);
        Assert.Equal("STU-001", student.ExternalId);
    }

    [Fact]
    public async Task GetCoursesAsync_ReturnsSnapshot_NotLiveList()
    {
        var fake = new FakeFrappeClient();
        fake.Courses.Add(new FrappeCourseDto("CRS-001", "C1", "Algorithms", null, 4, "Active", null));

        var courses = await fake.GetCoursesAsync();
        fake.Courses.Clear();

        Assert.Single(courses);
    }
}
