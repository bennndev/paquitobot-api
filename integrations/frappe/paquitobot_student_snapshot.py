# API Server Script: paquitobot_student_snapshot
# The caller cannot supply a student ID. OAuth determines frappe.session.user.
student = frappe.db.get_value("Student", {"user": frappe.session.user}, "name")
if not student:
    frappe.throw("No student is linked to the authenticated Frappe user.")

students = frappe.get_all("Student", filters={"name": student},
    fields=["name", "first_name", "middle_name", "last_name", "student_name",
            "email_address", "student_mobile_number", "date_of_birth", "gender",
            "enabled", "modified"])
enrollments = frappe.get_all("Course Enrollment",
    filters={"student": student, "docstatus": 1},
    fields=["name", "student", "course", "program", "student_batch", "academic_term",
            "enrollment_date", "status", "fee_plan", "modified"])

course_names = []
batch_names = []
program_names = []
term_names = []
for row in enrollments:
    if row.course and row.course not in course_names:
        course_names.append(row.course)
    if row.student_batch and row.student_batch not in batch_names:
        batch_names.append(row.student_batch)
    if row.program and row.program not in program_names:
        program_names.append(row.program)
    if row.academic_term and row.academic_term not in term_names:
        term_names.append(row.academic_term)

courses = []
batches = []
terms = []
programs = []
schedules = []
plans = []
faculty = []
if course_names:
    courses = frappe.get_all("Course", filters={"name": ["in", course_names]},
        fields=["name", "course_abbreviation", "course_credit", "short_description",
                "program", "modified"])
if batch_names:
    batches = frappe.get_all("Student Batch Name", filters={"name": ["in", batch_names]},
        fields=["name", "course", "program", "start_date", "end_date", "max_strength",
                "disabled", "modified"])
    schedules = frappe.get_all("Subject Schedule",
        filters={"student_batch": ["in", batch_names]},
        fields=["name", "student_batch", "faculty", "faculty_name", "course", "subject",
                "schedule_date", "room", "from_time", "to_time", "modified"])
    plans = frappe.get_all("Assessment Plan",
        filters={"student_batch": ["in", batch_names], "docstatus": 1},
        fields=["name", "student_batch", "assessment_name", "assignment_type", "course",
                "subject", "academic_term", "schedule_date", "faculty", "faculty_name",
                "maximum_assessment_score", "modified"])
faculty_names = []
for row in schedules:
    if row.faculty and row.faculty not in faculty_names:
        faculty_names.append(row.faculty)
if faculty_names:
    faculty = frappe.get_all("Faculty", filters={"name": ["in", faculty_names]},
        fields=["name", "first_name", "last_name", "faculty_name", "email_address",
                "phone_number", "status", "modified"])
if program_names:
    programs = frappe.get_all("Program", filters={"name": ["in", program_names]},
        fields=["name", "program_abbreviation", "short_description", "modified"])
if term_names:
    terms = frappe.get_all("Academic Term", filters={"name": ["in", term_names]},
        fields=["name", "term_name", "term_start_date", "term_end_date", "modified"])

results = frappe.get_all("Assessment Result",
    filters={"student": student, "docstatus": 1},
    fields=["name", "student", "assessment_plan", "score", "maximum_score", "comment",
            "student_batch", "modified"])
attendance = frappe.get_all("Student Attendance",
    filters={"student": student, "docstatus": 1},
    fields=["name", "student", "student_batch", "subject_schedule", "date", "status", "modified"])
fee_plans = frappe.get_all("Fee Plan", filters={"student": student, "docstatus": 1},
    fields=["name", "student", "course_enrollment", "total_amount", "outstanding_amount",
            "paid_amount", "modified"])
invoices = frappe.get_all("Sales Invoice", filters={"student": student, "docstatus": 1},
    fields=["name", "student", "fee_plan", "posting_date", "due_date", "grand_total",
            "outstanding_amount", "currency", "status", "modified"])
invoice_names = []
for row in invoices:
    invoice_names.append(row.name)
payments = []
references = []
if invoice_names:
    references = frappe.get_all("Payment Entry Reference",
        filters={"reference_doctype": "Sales Invoice", "reference_name": ["in", invoice_names]},
        fields=["parent", "reference_name", "allocated_amount"])
    payment_names = []
    for row in references:
        if row.parent not in payment_names:
            payment_names.append(row.parent)
    if payment_names:
        payments = frappe.get_all("Payment Entry",
            filters={"name": ["in", payment_names], "docstatus": 1},
            fields=["name", "party", "posting_date", "paid_amount", "received_amount",
                    "reference_no", "status", "modified"])

frappe.flags.snapshot = {
    "student": students[0] if students else None,
    "enrollments": enrollments,
    "courses": courses,
    "batches": batches,
    "terms": terms,
    "programs": programs,
    "schedules": schedules,
    "plans": plans,
    "faculty": faculty,
    "results": results,
    "attendance": attendance,
    "fee_plans": fee_plans,
    "invoices": invoices,
    "payments": payments,
    "payment_references": references
}
