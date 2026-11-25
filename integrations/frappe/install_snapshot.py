"""Install the PaquitoBot student snapshot API in an existing Frappe bench."""

import pathlib
import sys

import frappe


site, script_path = sys.argv[1:3]
frappe.init(site=site)
frappe.connect()
frappe.set_user("Administrator")

try:
    name = "PaquitoBot Student Snapshot"
    source = pathlib.Path(script_path).read_text(encoding="utf-8")
    if frappe.db.exists("Server Script", name):
        document = frappe.get_doc("Server Script", name)
        document.script = source
        document.save(ignore_permissions=True)
    else:
        document = frappe.get_doc({
            "doctype": "Server Script",
            "name": name,
            "script_type": "API",
            "api_method": "paquitobot_student_snapshot",
            "allow_guest": 0,
            "script": source,
        })
        document.insert(ignore_permissions=True)
    frappe.db.commit()
    print(f"Installed {document.api_method} on {site}")
except Exception:
    frappe.db.rollback()
    raise
finally:
    frappe.destroy()
