# Student snapshot for PaquitoBot

The OAuth callback requests `/api/method/paquitobot_student_snapshot` with the student's Frappe access token. The API script derives the student from `frappe.session.user`; callers cannot choose another student ID. Shared course data is limited to that student's enrollments. Results, attendance, invoices, and payments are filtered to that student.

On the local Docker installation, install or update the script with:

```powershell
docker exec -w /home/frappe/frappe-bench paquitobot-frappe-backend-1 bench set-config -g server_script_enabled true
docker cp integrations/frappe/paquitobot_student_snapshot.py paquitobot-frappe-backend-1:/tmp/paquitobot_student_snapshot.py
docker cp integrations/frappe/install_snapshot.py paquitobot-frappe-backend-1:/tmp/install_snapshot.py
docker exec -w /home/frappe/frappe-bench/sites paquitobot-frappe-backend-1 /home/frappe/frappe-bench/env/bin/python /tmp/install_snapshot.py education.localhost /tmp/paquitobot_student_snapshot.py
```

Set `FrappeOAuth:BaseUrl`, `ClientId`, `ClientSecret`, and `RedirectUri` in the API's .NET User Secrets. The redirect URI must match the Frappe OAuth Client exactly.

This first sync populates the student and its link to the existing PaquitoBot user, academic term, program, courses, program/course links, course batches and enrollments, faculty, class schedules, assessment plans, assessments, results, attendance, invoices as tuition fees, payments, and a sync run. Frappe Fee Structure and Fee Schedule are excluded until the local Education schema mismatch is resolved. Conversations, messages, knowledge documents, and app users are owned by PaquitoBot rather than imported from Frappe.
