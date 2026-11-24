# Frappe Education local para PaquitoBot

Este entorno reproduce la instalación usada por el proyecto:

- Frappe `v15.126.0` (`53bae3ef2550ca96320f036f581446783cd11b72`)
- ERPNext `v15.124.5` (`e5cde8c4b9e3f7f567a92ca3e65fd411402b89ce`)
- Education `develop` fijado en `61408d3da51833a4045b5506be61d47b3a0a8d70`
- MariaDB 10.6
- Redis 6.2

La imagen se construye con el Containerfile oficial de Frappe como base de diseño y el stack contiene backend, frontend, WebSocket, scheduler y workers.

## Requisitos

- Windows 10/11 con WSL 2.
- Docker Desktop configurado para contenedores Linux.
- Git.
- Al menos 4 GB de memoria disponibles para Docker; se recomiendan 6 GB.
- Puertos `8000` para Frappe y `5114` para PaquitoBot libres.

## Instalación

Desde la raíz del repositorio, con Docker Desktop iniciado:

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\frappe\setup.ps1
```

La primera ejecución descarga y compila Frappe, ERPNext y Education. Puede tardar entre 10 y 30 minutos dependiendo del equipo y la conexión. Las siguientes ejecuciones reutilizan la imagen y los volúmenes.

Al finalizar:

- Frappe: <http://education.localhost:8000>
- Usuario: `Administrator`
- Contraseña: se muestra al terminar y permanece en `infra/frappe/.env`.
- Las credenciales de API se generan automáticamente y también se guardan en `.env`.

El archivo `.env` contiene secretos locales y está excluido de Git.

## Ejecutar PaquitoBot conectado a Frappe

En otra terminal:

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\frappe\run-api.ps1
```

El script carga `Frappe__BaseUrl`, `Frappe__ApiKey` y `Frappe__ApiSecret` en el proceso de la API. No modifica archivos de configuración con secretos.

Swagger queda disponible en <http://localhost:5114/swagger>.

## Administrar el entorno

```powershell
# Ver estado
.\infra\frappe\frappe.ps1 status

# Iniciar contenedores existentes
.\infra\frappe\frappe.ps1 start

# Detenerlos sin perder datos
.\infra\frappe\frappe.ps1 stop

# Seguir logs
.\infra\frappe\frappe.ps1 logs

# Eliminar todos los datos locales y volver a empezar
.\infra\frappe\frappe.ps1 reset
.\infra\frappe\setup.ps1 -SkipBuild
```

`reset` solicita escribir `RESET` antes de eliminar los volúmenes.

## Qué se comparte en Git

Se versionan:

- Las versiones de las aplicaciones.
- Docker Compose.
- El Containerfile.
- Los scripts de instalación y administración.

No se versionan:

- Contraseñas o API secrets.
- La base de datos.
- Los archivos privados del sitio.
- El clon auxiliar de `frappe_docker`.
- Cookies o sesiones del navegador.

## Solución de problemas

Si Docker no responde, iniciar Docker Desktop y esperar a que indique que el motor está activo.

Si el puerto 8000 está ocupado, cambiar `FRAPPE_HTTP_PORT` en `.env` y volver a ejecutar:

```powershell
.\infra\frappe\frappe.ps1 start
```

Si falla la creación inicial del sitio:

```powershell
.\infra\frappe\frappe.ps1 logs
```

Para reconstruir la imagen después de cambiar versiones:

```powershell
.\infra\frappe\setup.ps1
```

No se debe actualizar una versión de Frappe, ERPNext o Education de forma aislada. Primero se prueba el conjunto completo y luego se actualizan los hashes fijados.
