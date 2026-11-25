# Issue #41 — Centralización financiera de estudiantes

## Estado del alcance

Implementado en `feature/student-financial-centralization`:

- GET `/api/student-finances/{studentId}/statement?asOf=yyyy-MM-dd`: obligaciones, pagado, saldo, vencido, fecha próxima, moneda y conciliación.
- GET `/api/student-finances/{studentId}/payments?status=&from=&to=`: pagos filtrados, control de acceso por usuario vinculado o administrador.
- GET `/api/student-finances/{studentId}/schedules?programId=&academicPeriodId=`: matrículas, programas, estructuras, cronogramas y obligaciones individuales.
- POST `/api/payments`: sólo administradores, pago confirmado con clave de idempotencia externa, transacción serializable y protección contra sobrepagos.
- POST `/api/student-finances/{studentId}/reconcile`: auditoría de consistencia **local**, administrador, registro en SyncRun.
- POST `/api/student-finances/{studentId}/frappe/preview?source=Fees`: consulta Frappe **sólo lectura**, administrador, registra en SyncRun.
- POST `/api/student-finances/{studentId}/frappe/preview?source=Sales%20Invoice`: alternativa para entornos Education que generan facturas; requiere campos personalizados de Education `student` y `fee_schedule`.

La prevalidación en Frappe devuelve `safeToApply=false`: **no crea ni modifica** pensiones, pagos o cronogramas. La única escritura del preview es `SyncRun`. Los documentos pueden ser draft/submitted/cancelled.

## Por qué no importamos automáticamente pagos

ERPNext `Payment Entry` puede distribuir un pago entre varias facturas mediante `Payment Entry Reference` (cada referencia tiene `allocated_amount`). Importar `paid_amount` como un pago aplicado a una sola `Fees` o `Sales Invoice` duplicaría importes. El esquema de Frappe debe validarse con la instancia real y su configuración de Education. Además, las facturas `Sales Invoice` pueden ser la fuente del crédito en lugar de `Fees`.

**Criterio de cierre técnico de #41:** todos los casos solicitados de consulta y registro seguro están implementados; existe conciliación con SyncRun y un punto de integración preparado mediante el contrato de #38. La escritura remota automática no estaba requerida en #41 y queda deliberadamente deshabilitada. Una integración productiva de pagos exige mapeo adicional y verificación contable con Frappe.

## Comprobación offline (sin Frappe ni Neon)

```powershell
 dotnet build
 dotnet test PaquitoBot.sln
 git diff --check
```

## Comprobación de API y datos en Neon TEST

Usar la base de datos de prueba con datos ficticios y credenciales JWT en variables de entorno, nunca en Git.

```powershell
 powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\pruebas-financieras\smoke-read.ps1
 powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\pruebas-financieras\smoke-payment-access.ps1
 powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\pruebas-financieras\smoke-payment-write.ps1
 powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\pruebas-financieras\smoke-payment-immutability.ps1
 powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\pruebas-financieras\smoke-financial-reconciliation.ps1 -StudentId "GUID"
 powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\pruebas-financieras\smoke-financial-schedules.ps1 -StudentId "GUID"
```

Los scripts antiguos pueden requerir parámetros adicionales o variables que figuran en sus encabezados. Verificar el script antes de ejecutarlo. El fixture de cronogramas **escribe registros QA** y sólo debe ejecutarse con `-ConfirmTestDatabase` en Neon TEST.

## Comprobación Frappe real (pendiente de instancia y credenciales)

La base del PR #44 exige `Frappe:BaseUrl`, `Frappe:ApiKey` y `Frappe:ApiSecret` válidas al iniciar. Configurarlas con .NET User Secrets o entorno de ejecución, sin publicar secretos. La API habilita `GET /health/frappe` sólo con autenticación JWT.

```powershell
 powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\pruebas-financieras\smoke-frappe-financial-preview.ps1 -StudentId "GUID" -Source "Fees"
 powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\pruebas-financieras\smoke-frappe-financial-preview.ps1 -StudentId "GUID" -Source "Sales Invoice"
```

Sin conexión Frappe los tests unitarios funcionan, pero estas pruebas autenticadas reales no pueden aprobarse. No confundir `SKIP` con `PASS`.

## Revisión de equipo

- Confirmar contratos ya fusionados del PR #44 antes de modificar otra integración.
- Revisar seguridad/privacidad de controladores globales de Students, FeeStructures, FeeSchedules y TuitionFees (fuera del scope del PR #41), y los endpoints que permiten borrar el historial SyncRun.
- Revisar migraciones, datos y credenciales antes de llevar a producción.
- Las pruebas HTTP previas se realizaron en una instancia Neon TEST. Conservar evidencias y resultados de la rama correcta.
