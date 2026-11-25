# API REST - Gestión de Proyectos y Tareas

API REST desarrollada con **.NET 10**, **Clean Architecture**, **Entity Framework Core**, **PostgreSQL** y **Swagger (OpenAPI)** para la gestión de proyectos y sus tareas asociadas (relación 1:N).

## Arquitectura del proyecto

- `src/Domain`: Entidades (`Project`, `TaskItem`), enums (`TaskItemStatus`, `TaskPriority`) e interfaces de repositorio (`IProjectRepository`, `ITaskRepository`).
- `src/Application`: DTOs con validaciones (`DataAnnotations`), excepciones de negocio (`NotFoundException`, `BusinessRuleException`), interfaces de servicio y lógica de negocio (`ProjectService`, `TaskService`).
- `Infrastructure`: `AppDbContext` (EF Core + Npgsql) e implementaciones de repositorios (`ProjectRepository`, `TaskRepository`).
- `src/Api`: Controladores (`ProjectsController`, `TasksController`), middleware global de excepciones (`ExceptionHandlingMiddleware`), configuración de Swagger y endpoint `/health`.
- `src/Rest.Tests`: Pruebas unitarias con **NUnit** y **Moq**.
- `.github/workflows/ci-cd.yml`: Pipeline de CI/CD con GitHub Actions.

## Endpoints principales

### Proyectos (`/api/projects`)
- `GET /api/projects` — Lista todos los proyectos incluyendo total de tareas, completadas y porcentaje de avance.
- `GET /api/projects/{id}` — Obtiene un proyecto por ID con su porcentaje de avance.
- `GET /api/projects/{id}/tasks` — Lista todas las tareas de un proyecto.
- `POST /api/projects` — Crea un proyecto (valida que `EndDate >= StartDate`).
- `PUT /api/projects/{id}` — Actualiza un proyecto.
- `DELETE /api/projects/{id}` — Elimina un proyecto (impide eliminar si tiene tareas pendientes o en progreso).

### Tareas (`/api/tasks`)
- `GET /api/tasks?status=&priority=` — Lista y filtra tareas por estado (`Pendiente`, `EnProgreso`, `Completada`) y/o prioridad (`Baja`, `Media`, `Alta`).
- `GET /api/tasks/overdue` — Lista tareas vencidas que no estén completadas.
- `GET /api/tasks/{id}` — Obtiene una tarea por ID.
- `POST /api/tasks` — Crea una tarea asociada a un proyecto (valida que `DueDate >= Project.StartDate`).
- `PUT /api/tasks/{id}` — Actualiza una tarea.
- `PATCH /api/tasks/{id}/status` — Cambia el estado de una tarea.
- `DELETE /api/tasks/{id}` — Elimina una tarea.

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
   dotnet run --project src/Api/Api.csproj
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