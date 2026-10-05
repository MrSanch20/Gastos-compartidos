# Prompt para Claude Code — Gastos Compartidos API

Copia todo lo que hay debajo de la línea y pégalo en Claude Code, abierto en la carpeta `gastos-compartidos`.

---

Vas a construir una API REST en .NET llamada **Gastos Compartidos**. El diseño completo está en `README.md` (funcionalidades, modelo de datos, endpoints, reglas de negocio, tecnologías y estructura de carpetas). **Léelo antes de empezar y síguelo.** Si algo del README no cuadra o falta, pregúntame antes de inventarlo.

## Contexto
- Es un proyecto de **portfolio** para buscar trabajo de desarrollador backend .NET junior. El código será público en GitHub y lo leerán reclutadores, así que prima la **claridad y las buenas prácticas** sobre la complejidad.
- Todo el código (clases, propiedades, métodos, rutas) en **español**, como en el README. Comentarios breves y en español, solo donde aporten.

## Stack
- .NET 10 (LTS), ASP.NET Core Web API con **controllers** (no minimal APIs).
- Entity Framework Core con **SQLite** en desarrollo (dejar preparado para cambiar a PostgreSQL por configuración).
- Autenticación JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`), contraseñas con hash (BCrypt o `PasswordHasher` de ASP.NET Identity).
- FluentValidation para validar los DTOs.
- Swagger/OpenAPI con soporte para meter el token JWT.
- xUnit para tests (unitarios + integración con `WebApplicationFactory`).

## Convenciones
- Estructura de carpetas exactamente como en el README (`src/GastosCompartidos.Api`, `tests/GastosCompartidos.Tests`) y una solución `GastosCompartidos.sln` en la raíz.
- Los controllers **solo** reciben la petición y llaman a un servicio. Toda la lógica va en `Services/` detrás de interfaces (`IGrupoService`, etc.) registradas con inyección de dependencias.
- **Nunca** devolver entidades de EF directamente: siempre DTOs de entrada y salida.
- Dinero siempre en `decimal`, con 2 decimales.
- Métodos async con `CancellationToken`.
- Errores con un middleware global que devuelva `ProblemDetails` (400 validación, 401, 403, 404, 409).
- Secretos (clave JWT) en `appsettings.Development.json` / user-secrets, nunca hardcodeados en el código.
- Nullable reference types activado y sin warnings.

## Plan de trabajo (por fases)
Trabaja **una fase cada vez**. Al terminar cada fase: compila, ejecuta los tests, marca la casilla correspondiente en la "Hoja de ruta" del README, haz un commit con un mensaje claro en español y **para para que yo lo revise** antes de seguir.

1. **Estructura**: `git init`, `.gitignore` de .NET, solución, proyecto API, proyecto de tests y referencias entre ellos. Swagger funcionando con un endpoint de prueba.
2. **Modelo y base de datos**: entidades, `DbContext`, configuraciones (claves, relaciones, precisión de `decimal`) y primera migración.
3. **Autenticación**: registro y login con JWT. Tests de registro duplicado y login incorrecto.
4. **Grupos y miembros**: CRUD con las reglas de permisos (solo miembros ven el grupo, solo el admin añade/quita miembros).
5. **Gastos**: CRUD con reparto a partes iguales o personalizado. Regla de que las participaciones suman el total y reparto justo de céntimos. Paginación y filtro por fechas en el listado.
6. **Saldos y "saldar cuentas"**: cálculo de saldos (incluyendo liquidaciones ya hechas) y algoritmo que proponga el mínimo de pagos. Endpoint para registrar liquidaciones. **Esta es la parte estrella: tests unitarios exhaustivos** (casos con 2, 3 y 5 personas, céntimos, todos en paz, etc.).
7. **Calidad final**: `Dockerfile`, `docker-compose.yml`, workflow de GitHub Actions que compile y pase los tests, y completar en el README la sección "Cómo ejecutarlo" con ejemplos de peticiones.

## Empieza
Empieza por la **fase 1**. Antes de escribir código, dime en pocas líneas qué vas a hacer en esta fase.
