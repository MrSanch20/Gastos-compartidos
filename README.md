# Gastos Compartidos API

API REST en **.NET** para gestionar gastos compartidos entre grupos de personas (viajes, pisos compartidos, cenas...). Registra quién ha pagado qué y calcula automáticamente **quién debe a quién**, proponiendo la forma de saldar las deudas con el **menor número de pagos posible**.

> 🚧 Proyecto en desarrollo.

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

## 📁 Estructura prevista

```
gastos-compartidos/
├── src/
│   └── GastosCompartidos.Api/
│       ├── Controllers/
│       ├── Models/          # Entidades
│       ├── DTOs/            # Objetos de entrada/salida
│       ├── Services/        # Lógica de negocio (cálculo de saldos)
│       ├── Data/            # DbContext y migraciones
│       └── Validators/
├── tests/
│   └── GastosCompartidos.Tests/
├── docker-compose.yml
└── README.md
```

---

## 🚀 Cómo ejecutarlo

_Pendiente — se completará cuando el proyecto esté montado._

---

## ✅ Hoja de ruta

- [x] Crear solución y proyectos
- [x] Modelo de datos y base de datos
- [ ] Registro / login con JWT
- [ ] CRUD de grupos y miembros
- [ ] CRUD de gastos
- [ ] Cálculo de saldos
- [ ] Algoritmo de "saldar cuentas"
- [ ] Tests
- [ ] Docker + GitHub Actions
- [ ] README final con capturas y ejemplos

---

## 👤 Autor

**Daniel Sancho Goñi** — Desarrollador .NET
