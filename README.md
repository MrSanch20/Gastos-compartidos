# Gastos Compartidos API

API REST en **.NET** para gestionar gastos compartidos entre grupos de personas (viajes, pisos compartidos, cenas...). Registra quién ha pagado qué y calcula automáticamente **quién debe a quién**, proponiendo la forma de saldar las deudas con el **menor número de pagos posible**.

> ✅ Proyecto de portfolio completo: autenticación, CRUD de grupos y gastos, cálculo de saldos
> con el algoritmo de liquidación mínima, tests (unitarios + integración), Docker y CI.

---

## ✨ Funcionalidades

- Registro e inicio de sesión de usuarios (JWT).
- Crear grupos y añadir miembros.
- Añadir, editar y borrar gastos.
- Reparto **a partes iguales** o con **cantidades personalizadas**.
- Consultar el **saldo** de cada miembro del grupo.
- Calcular **cómo saldar las cuentas** con el mínimo de transferencias.
- Registrar pagos (liquidaciones) entre miembros.

---

## 🧩 Modelo de datos

| Entidad | Descripción | Campos principales |
|---|---|---|
| **Usuario** | Persona registrada en la app | Id, Nombre, Email, PasswordHash |
| **Grupo** | Conjunto de personas que comparten gastos | Id, Nombre, FechaCreacion, Miembros |
| **MiembroGrupo** | Relación usuario ↔ grupo | GrupoId, UsuarioId, Rol (admin / miembro) |
| **Gasto** | Algo que alguien pagó para el grupo | Id, GrupoId, PagadoPorId, Importe, Concepto, Fecha |
| **ParticipacionGasto** | Parte del gasto que corresponde a cada miembro | GastoId, UsuarioId, Importe |
| **Liquidacion** | Pago de un miembro a otro para saldar deudas | Id, GrupoId, DeId, ParaId, Importe, Fecha |

---

## 🔌 Endpoints previstos

### Autenticación
| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/registro` | Crear cuenta |
| POST | `/api/auth/login` | Iniciar sesión y obtener token JWT |

### Grupos
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/grupos` | Listar mis grupos |
| POST | `/api/grupos` | Crear un grupo |
| GET | `/api/grupos/{id}` | Ver detalle de un grupo |
| POST | `/api/grupos/{id}/miembros` | Añadir un miembro |
| DELETE | `/api/grupos/{id}/miembros/{usuarioId}` | Quitar un miembro |

### Gastos
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/grupos/{id}/gastos` | Listar gastos (con paginación y filtros) |
| POST | `/api/grupos/{id}/gastos` | Añadir un gasto |
| PUT | `/api/grupos/{id}/gastos/{gastoId}` | Editar un gasto |
| DELETE | `/api/grupos/{id}/gastos/{gastoId}` | Borrar un gasto |

### Saldos y liquidaciones
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/grupos/{id}/saldos` | Saldo de cada miembro |
| GET | `/api/grupos/{id}/saldar` | Propuesta de pagos para quedar en paz ⭐ |
| POST | `/api/grupos/{id}/liquidaciones` | Registrar un pago entre miembros |

---

## 📏 Reglas de negocio

- Solo los **miembros de un grupo** pueden ver y modificar sus gastos.
- Solo el **admin del grupo** puede añadir o quitar miembros.
- La suma de las participaciones de un gasto debe ser **igual al importe total**.
- Al repartir a partes iguales, los **céntimos sobrantes** se asignan de forma justa (10 € entre 3 → 3,34 + 3,33 + 3,33).
- Los importes se manejan con `decimal`, nunca con `float`/`double`.
- No se puede quitar del grupo a un miembro con saldo pendiente.

---

## 🛠️ Tecnologías

- **ASP.NET Core** (Web API)
- **Entity Framework Core**
- **SQLite** (desarrollo) / **PostgreSQL** (producción)
- **JWT** para autenticación
- **FluentValidation** para validaciones
- **xUnit** para tests unitarios y de integración
- **Swagger / OpenAPI** para documentación
- **Docker** y **GitHub Actions** (CI)

---

## 📁 Estructura

```
gastos-compartidos/
├── src/
│   └── GastosCompartidos.Api/
│       ├── Controllers/      # Reciben la petición y llaman a un servicio
│       ├── Models/           # Entidades de EF Core
│       ├── DTOs/              # Objetos de entrada/salida (nunca se exponen entidades)
│       ├── Services/         # Lógica de negocio (saldos, reparto de céntimos, JWT...)
│       ├── Data/              # DbContext, configuraciones Fluent API y migraciones
│       ├── Validators/       # Reglas de FluentValidation para los DTOs de entrada
│       ├── Excepciones/      # Excepciones de dominio (404, 409, 403, 401, 400)
│       └── Middleware/       # Traduce las excepciones a ProblemDetails
├── tests/
│   └── GastosCompartidos.Tests/   # Tests unitarios y de integración (xUnit)
├── .github/workflows/ci.yml       # Compila y ejecuta los tests en cada push/PR
├── Dockerfile
├── docker-compose.yml
└── README.md
```

---

## 🚀 Cómo ejecutarlo

### Opción A: con Docker (la más rápida)

Necesitas Docker y Docker Compose. La clave de firma de los JWT nunca está en el repositorio,
así que hay que pasarla como variable de entorno:

```bash
export JWT_CLAVE="una-cadena-larga-y-aleatoria-de-al-menos-32-caracteres"
docker compose up --build
```

La API queda escuchando en `http://localhost:8080` (usa SQLite dentro del contenedor y aplica
las migraciones automáticamente al arrancar). Swagger está disponible solo en entornos de
desarrollo, así que para probar la API en Docker usa los ejemplos de `curl` más abajo.

Si prefieres PostgreSQL en vez de SQLite, el `docker-compose.yml` ya trae un servicio `db`:
cambia en `api.environment` las variables `BaseDeDatos__Proveedor` a `PostgreSql` y
`ConnectionStrings__DefaultConnection` al valor comentado en el propio archivo.

### Opción B: en local con el SDK de .NET 10

```bash
# 1. Clona el repo y restaura dependencias
dotnet restore

# 2. Configura la clave de JWT en secretos de usuario (nunca en appsettings.json)
cd src/GastosCompartidos.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Clave" "una-cadena-larga-y-aleatoria-de-al-menos-32-caracteres"

# 3. Crea la base de datos SQLite aplicando las migraciones
dotnet ef database update

# 4. Arranca la API
dotnet run
```

La API queda en `https://localhost:7xxx` (el puerto exacto lo indica la consola) con Swagger UI
en `/swagger`, donde puedes pegar el token JWT con el botón "Authorize" para probar los
endpoints protegidos.

### Ejecutar los tests

```bash
dotnet test
```

Los tests de integración no dependen de `user-secrets` ni de ninguna base de datos externa:
usan su propia SQLite en memoria y su propia configuración de JWT, así que funcionan igual en
local y en el pipeline de CI.

### Ejemplos de peticiones

Flujo completo: registrar dos personas, crear un grupo, añadir gastos y consultar quién debe
qué a quién.

```bash
# Registrar al primer usuario
curl -X POST http://localhost:8080/api/auth/registro \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Ana","email":"ana@test.com","password":"ContrasenaSegura123"}'
# -> 200 OK
# {"token":"eyJhbGciOi...","expiraEn":"2026-10-05T23:01:26Z","usuarioId":1,"nombre":"Ana","email":"ana@test.com"}

# Registrar al segundo usuario
curl -X POST http://localhost:8080/api/auth/registro \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Bea","email":"bea@test.com","password":"ContrasenaSegura123"}'

# Guarda el token de Ana en una variable para las siguientes peticiones
TOKEN_ANA="eyJhbGciOi..."

# Crear un grupo (Ana queda como admin automáticamente)
curl -X POST http://localhost:8080/api/grupos \
  -H "Authorization: Bearer $TOKEN_ANA" -H "Content-Type: application/json" \
  -d '{"nombre":"Viaje a Roma"}'
# -> 201 Created
# {"id":1,"nombre":"Viaje a Roma","fechaCreacion":"2026-10-05T..."}

# Añadir a Bea al grupo (solo el admin puede hacerlo)
curl -X POST http://localhost:8080/api/grupos/1/miembros \
  -H "Authorization: Bearer $TOKEN_ANA" -H "Content-Type: application/json" \
  -d '{"email":"bea@test.com"}'
# -> 204 No Content

# Ana paga una cena de 10 € a repartir a partes iguales entre las dos
curl -X POST http://localhost:8080/api/grupos/1/gastos \
  -H "Authorization: Bearer $TOKEN_ANA" -H "Content-Type: application/json" \
  -d '{
        "concepto": "Cena",
        "importe": 10,
        "fecha": "2026-10-05T20:00:00Z",
        "pagadoPorId": 1,
        "tipoReparto": "PartesIguales",
        "participaciones": [{"usuarioId": 1}, {"usuarioId": 2}]
      }'
# -> 201 Created, con las participaciones ya calculadas (5.00 € cada una)

# Consultar los saldos del grupo
curl -H "Authorization: Bearer $TOKEN_ANA" http://localhost:8080/api/grupos/1/saldos
# -> [{"usuarioId":1,"nombre":"Ana","saldo":5.0},{"usuarioId":2,"nombre":"Bea","saldo":-5.0}]

# Pedir la propuesta de pagos para quedar en paz
curl -H "Authorization: Bearer $TOKEN_ANA" http://localhost:8080/api/grupos/1/saldar
# -> [{"deId":2,"deNombre":"Bea","paraId":1,"paraNombre":"Ana","importe":5.0}]

# Registrar que Bea ya le ha pagado a Ana esos 5 €
curl -X POST http://localhost:8080/api/grupos/1/liquidaciones \
  -H "Authorization: Bearer $TOKEN_ANA" -H "Content-Type: application/json" \
  -d '{"deId": 2, "paraId": 1, "importe": 5}'
# -> 200 OK; a partir de aquí /saldos devuelve 0.0 para ambas y /saldar no propone ya ningún pago
```

---

## ✅ Hoja de ruta

- [x] Crear solución y proyectos
- [x] Modelo de datos y base de datos
- [x] Registro / login con JWT
- [x] CRUD de grupos y miembros
- [x] CRUD de gastos
- [x] Cálculo de saldos
- [x] Algoritmo de "saldar cuentas"
- [x] Tests
- [x] Docker + GitHub Actions
- [x] README final con ejemplos

---

## 👤 Autor

**Daniel Sancho Goñi** — Desarrollador .NET
