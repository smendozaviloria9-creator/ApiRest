# API REST - Gestión de Proyectos y Tareas

API REST desarrollada con **.NET 10**, **Clean Architecture**, **Entity Framework Core**, **PostgreSQL** y **Swagger (OpenAPI)** para la gestión de proyectos y sus tareas asociadas (relación 1:N).

## Arquitectura del proyecto

- `src/Rest.Domain`: Entidades (`Project`, `TaskItem`), enums (`TaskItemStatus`, `TaskPriority`) e interfaces de repositorio (`IProjectRepository`, `ITaskRepository`).
- `src/Rest.Application`: DTOs con validaciones (`DataAnnotations`), excepciones de negocio (`NotFoundException`, `BusinessRuleException`), interfaces de servicio y lógica de negocio (`ProjectService`, `TaskService`).
- `src/Rest.Infrastructure`: `AppDbContext` (EF Core + Npgsql) e implementaciones de repositorios (`ProjectRepository`, `TaskRepository`).
- `src/Rest.Api`: Controladores (`ProjectsController`, `TasksController`), middleware global de excepciones (`ExceptionHandlingMiddleware`), configuración de Swagger y endpoint `/health`.
- `tests/Rest.Tests`: Pruebas unitarias con **NUnit** y **Moq**.
- `.github/workflows/ci-cd.yml`: Pipeline de CI/CD con GitHub Actions.

## Endpoints principales

### Proyectos (`/api/v1/projects`)
- `GET /api/v1/projects` — Lista todos los proyectos incluyendo total de tareas, completadas y porcentaje de avance.
- `GET /api/v1/projects/{id}` — Obtiene un proyecto por ID con su porcentaje de avance.
- `GET /api/v1/projects/{id}/tasks` — Lista todas las tareas de un proyecto.
- `POST /api/v1/projects` — Crea un proyecto (valida que `EndDate >= StartDate`).
- `PUT /api/v1/projects/{id}` — Actualiza un proyecto.
- `DELETE /api/v1/projects/{id}` — Elimina un proyecto (impide eliminar si tiene tareas pendientes o en progreso).

### Tareas (`/api/v1/tasks`)
- `GET /api/v1/tasks?status=&priority=` — Lista y filtra tareas por estado (`Pendiente`, `EnProgreso`, `Completada`) y/o prioridad (`Baja`, `Media`, `Alta`).
- `GET /api/v1/tasks/overdue` — Lista tareas vencidas que no estén completadas.
- `GET /api/v1/tasks/{id}` — Obtiene una tarea por ID.
- `POST /api/v1/tasks` — Crea una tarea asociada a un proyecto (valida que `DueDate >= Project.StartDate`).
- `PUT /api/v1/tasks/{id}` — Actualiza una tarea.
- `PATCH /api/v1/tasks/{id}/status` — Cambia el estado de una tarea.
- `DELETE /api/v1/tasks/{id}` — Elimina una tarea.

### Documentación y Salud
- `/` — Interfaz interactiva de **Swagger UI**.
- `/health` — Estado de salud del servicio.

## Ejecución local

1. Asegúrate de tener una base de datos PostgreSQL activa y actualiza `ConnectionStrings:DefaultConnection` en `src/Api/appsettings.json` (o usa variables de entorno).
2. Compilar y ejecutar pruebas:
   ```bash
   dotnet build
   dotnet test
   ```
3. Ejecutar la API:
   ```bash
   dotnet run --project src/Rest.Api/Rest.Api.csproj
   ```

## Despliegue en Azure y CI/CD

1. Crea una base de datos **Azure Database for PostgreSQL (Flexible Server)** y obtén su cadena de conexión.
2. Crea un **Azure App Service (Web App)** con runtime **.NET 10 (Linux)**.
3. En la configuración del App Service (**Environment variables -> Connection strings**), agrega `DefaultConnection` (tipo `PostgreSQL` o `Custom`) con la cadena de conexión de Azure PostgreSQL.
4. Descarga el **Publish Profile** del App Service desde el portal de Azure.
5. En tu repositorio de GitHub (**Settings -> Secrets and variables -> Actions**):
   - **Secret**: `AZURE_WEBAPP_PUBLISH_PROFILE` = contenido del archivo Publish Profile.
   - **Variable**: `AZURE_WEBAPP_NAME` = nombre de tu App Service en Azure.
6. Al hacer `push` a `main`, el workflow `.github/workflows/ci-cd.yml` compilará, ejecutará las pruebas unitarias y desplegará automáticamente en Azure.