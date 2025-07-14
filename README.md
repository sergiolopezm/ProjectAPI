# **ESTRUCTURA DE SOFTWARE**

# **SERVICIO PROJECTAPI**

|  |  |
| --- | --- |
| **CAPA** | BACKEND |
| **PLATAFORMA** | SERVER – WINDOWS |
| **TIPO** | .NET |

## 1. DESCRIPCIÓN GENERAL

El servicio ProjectAPI proporciona una interfaz para la gestión de un sistema de publicaciones (posts) y clientes (customers). El sistema incluye funcionalidades de autenticación basada en JWT, gestión de usuarios con diferentes roles, registro de actividades mediante Serilog y operaciones CRUD completas.

La API está diseñada siguiendo principios RESTful y utiliza Entity Framework Core para la comunicación con la base de datos SQL Server. Implementa un sistema de validaciones robusto, middleware personalizado y manejo de errores centralizado.

## 2. REQUISITOS PREVIOS

### 2.1. Estructura de Base de Datos

Para el funcionamiento correcto del sistema, es necesario crear las siguientes tablas en la base de datos:

#### 2.1.1. Tabla Customer
```sql
CREATE TABLE [dbo].[Customer](
    [CustomerId] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([CustomerId] ASC)
);
```

#### 2.1.2. Tabla Post
```sql
CREATE TABLE [dbo].[Post](
    [PostId] [int] IDENTITY(1,1) NOT NULL,
    [Title] [nvarchar](500) NULL,
    [Body] [nvarchar](500) NULL,
    [Type] [int] NOT NULL,
    [Category] [nvarchar](500) NULL,
    [CustomerId] [int] NOT NULL,
PRIMARY KEY CLUSTERED ([PostId] ASC)
);
```

#### 2.1.3. Tabla Logs
```sql
CREATE TABLE [dbo].[Logs](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Message] [nvarchar](max) NULL,
    [MessageTemplate] [nvarchar](max) NULL,
    [Level] [nvarchar](max) NULL,
    [TimeStamp] [datetime] NULL,
    [Exception] [nvarchar](max) NULL,
    [Properties] [nvarchar](max) NULL,
CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ([Id] ASC)
);
```

#### 2.1.4. Tabla Accesos
```sql
CREATE TABLE [dbo].[Accesos](
    [Id] [int] IDENTITY(1,1) PRIMARY KEY,
    [Sitio] [nvarchar](50) NOT NULL,
    [Contraseña] [nvarchar](250) NOT NULL,
    [FechaCreacion] [datetime2] NOT NULL DEFAULT GETDATE(),
    [Activo] [bit] NOT NULL DEFAULT 1
);
```

#### 2.1.5. Tabla Roles
```sql
CREATE TABLE [dbo].[Roles](
    [Id] [int] IDENTITY(1,1) PRIMARY KEY,
    [Nombre] [nvarchar](50) NOT NULL UNIQUE,
    [Descripcion] [nvarchar](200) NULL,
    [FechaCreacion] [datetime2] NOT NULL DEFAULT GETDATE(),
    [Activo] [bit] NOT NULL DEFAULT 1
);
```

#### 2.1.6. Tabla Usuarios
```sql
CREATE TABLE [dbo].[Usuarios](
    [Id] [uniqueidentifier] PRIMARY KEY DEFAULT NEWID(),
    [NombreUsuario] [nvarchar](100) NOT NULL UNIQUE,
    [Contraseña] [nvarchar](250) NOT NULL,
    [Nombre] [nvarchar](100) NOT NULL,
    [Apellido] [nvarchar](100) NOT NULL,
    [Email] [nvarchar](100) NOT NULL UNIQUE,
    [RolId] [int] NOT NULL,
    [Activo] [bit] NOT NULL DEFAULT 1,
    [FechaCreacion] [datetime2] NOT NULL DEFAULT GETDATE(),
    [FechaUltimoAcceso] [datetime2] NULL,
    FOREIGN KEY (RolId) REFERENCES Roles(Id)
);
```

#### 2.1.7. Tabla Tokens
```sql
CREATE TABLE [dbo].[Tokens](
    [Id] [uniqueidentifier] PRIMARY KEY DEFAULT NEWID(),
    [Token] [nvarchar](1000) NOT NULL,
    [UsuarioId] [uniqueidentifier] NOT NULL,
    [Ip] [nvarchar](45) NOT NULL,
    [FechaCreacion] [datetime2] NOT NULL DEFAULT GETDATE(),
    [FechaExpiracion] [datetime2] NOT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
```

### 2.2. Datos Iniciales

Es necesario insertar los siguientes registros iniciales:

```sql
-- Insertar configuración de acceso
INSERT INTO Accesos (Sitio, Contraseña, FechaCreacion, Activo) 
VALUES ('ProjectAPI', 'ProjectAPI2024', GETDATE(), 1);

-- Insertar roles iniciales
INSERT INTO Roles (Nombre, Descripcion, FechaCreacion, Activo) 
VALUES 
('Admin', 'Administrador del sistema', GETDATE(), 1),
('User', 'Usuario estándar', GETDATE(), 1);

-- Insertar Usuario Admin (password: PostLtda2025)
INSERT INTO Usuarios (Id, NombreUsuario, Contraseña, Nombre, Apellido, Email, RolId, Activo, FechaCreacion)
VALUES (NEWID(), 'admin', 'KtGFnRWiPoPsP8fOaYEDdbv/ICJP1BjP/q9FvGtPTkw=', 'Administrador', 'Sistema', 'admin@projectapi.com', 1, 1, GETDATE());

-- Insertar datos de ejemplo
INSERT [dbo].[Customer] ([CustomerId], [Name]) VALUES (4, N'Maria con');
INSERT [dbo].[Customer] ([CustomerId], [Name]) VALUES (1002, N'Laura Sugey');
INSERT [dbo].[Customer] ([CustomerId], [Name]) VALUES (2002, N'Juan P');
INSERT [dbo].[Customer] ([CustomerId], [Name]) VALUES (3002, N'Juan albero');

INSERT [dbo].[Post] ([PostId], [Title], [Body], [Type], [Category], [CustomerId]) VALUES (1, N'Hola Amigos', N'Lorem Ipsum is simply dummy text of the printing and typesetting industry.', 1, N'Futbol', 1002);
INSERT [dbo].[Post] ([PostId], [Title], [Body], [Type], [Category], [CustomerId]) VALUES (2, N'Amigos Perrunos', N'Lorem Ipsum is simply dummy text of the printing and typesetting industry.', 1, N'Futbol', 4);
INSERT [dbo].[Post] ([PostId], [Title], [Body], [Type], [Category], [CustomerId]) VALUES (3, N'Soldado Caido!!!!', N'Lorem Ipsum is simply dummy text of the printing and typesetting industry.', 1, N'Futbol', 1002);
```

## 3. MÉTODOS

### 3.1. Autenticación

#### 3.1.1. Login

Autentica un usuario en el sistema y devuelve un token JWT.

**Acceso:** `api/Auth/login`  
**Formato:** JSON  
**Servicio:** REST / POST  
**Autenticación:** Requiere headers de acceso (Sitio, Clave)

##### 3.1.1.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| nombreUsuario | Nombre de usuario | String | Sí |
| contraseña | Contraseña del usuario | String | Sí |
| ip | Dirección IP del cliente | String | No |

Ejemplo de entrada:
```json
{
  "nombreUsuario": "admin",
  "contraseña": "PostLtda2025",
  "ip": "192.168.1.1"
}
```

##### 3.1.1.2. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Objeto con datos del usuario autenticado | Object |
| resultado.usuario | Datos del usuario | Object |
| resultado.token | Token JWT para autenticación | String |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Login exitoso",
  "detalle": "Usuario autenticado correctamente",
  "resultado": {
    "usuario": {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "nombreUsuario": "admin",
      "nombre": "Administrador",
      "apellido": "Sistema",
      "email": "admin@projectapi.com",
      "rol": "Admin",
      "rolId": 1,
      "activo": true,
      "fechaCreacion": "2023-01-01T00:00:00",
      "fechaUltimoAcceso": "2024-07-14T12:30:45"
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

#### 3.1.2. Registro

Registra un nuevo usuario en el sistema.

**Acceso:** `api/Auth/registro`  
**Formato:** JSON  
**Servicio:** REST / POST  
**Autenticación:** Requiere headers de acceso (Sitio, Clave) y JWT

##### 3.1.2.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| nombreUsuario | Nombre de usuario | String | Sí |
| contraseña | Contraseña del usuario | String | Sí |
| nombre | Nombre real del usuario | String | Sí |
| apellido | Apellido del usuario | String | Sí |
| email | Correo electrónico | String | Sí |
| rolId | ID del rol asignado | Integer | Sí |

#### 3.1.3. Validar Token

Valida un token JWT y devuelve información del usuario.

**Acceso:** `api/Auth/validar-token`  
**Formato:** JSON  
**Servicio:** REST / GET  
**Autenticación:** JWT requerido

#### 3.1.4. Health Check

Verifica el estado de la API de autenticación.

**Acceso:** `api/Auth/health`  
**Formato:** JSON  
**Servicio:** REST / GET  
**Autenticación:** Headers de acceso (Sitio, Clave)

### 3.2. Gestión de Customers

#### 3.2.1. Obtener Todos los Customers

Obtiene la lista de todos los customers.

**Acceso:** `api/Customer`  
**Formato:** JSON  
**Servicio:** REST / GET  
**Autenticación:** JWT requerido

##### 3.2.1.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de customers | Array |
| resultado[].customerId | Identificador único del customer | Integer |
| resultado[].name | Nombre del customer | String |

Ejemplo de salida:
```json
{
  "exito": true,
  "mensaje": "Customers obtenidos",
  "detalle": "Se obtuvieron 4 customers",
  "resultado": [
    {
      "customerId": 4,
      "name": "Maria con"
    },
    {
      "customerId": 1002,
      "name": "Laura Sugey"
    },
    {
      "customerId": 2002,
      "name": "Juan P"
    },
    {
      "customerId": 3002,
      "name": "Juan albero"
    }
  ]
}
```

#### 3.2.2. Obtener Customer por ID

Obtiene un customer específico por su ID.

**Acceso:** `api/Customer/{id}`  
**Formato:** JSON  
**Servicio:** REST / GET  
**Autenticación:** JWT requerido

##### 3.2.2.1. Parámetros de Ruta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| id | ID del customer | Integer | Sí |

#### 3.2.3. Crear Customer

Crea un nuevo customer en el sistema.

**Acceso:** `api/Customer`  
**Formato:** JSON  
**Servicio:** REST / POST  
**Autenticación:** JWT requerido

##### 3.2.3.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| name | Nombre del customer | String | Sí |

Ejemplo de entrada:
```json
{
  "name": "Nuevo Customer"
}
```

**Validaciones:**
- El nombre no puede estar duplicado
- El nombre es requerido y no puede exceder 500 caracteres

#### 3.2.4. Actualizar Customer

Actualiza un customer existente.

**Acceso:** `api/Customer/{id}`  
**Formato:** JSON  
**Servicio:** REST / PUT  
**Autenticación:** JWT requerido

##### 3.2.4.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| customerId | ID del customer | Integer | Sí |
| name | Nombre del customer | String | Sí |

#### 3.2.5. Eliminar Customer

Elimina un customer y todos sus posts asociados.

**Acceso:** `api/Customer/{id}`  
**Formato:** JSON  
**Servicio:** REST / DELETE  
**Autenticación:** JWT requerido

**Nota:** Antes de eliminar el customer, se eliminan automáticamente todos los posts asociados.

#### 3.2.6. Obtener Customers Paginados

Obtiene una lista paginada de customers con búsqueda.

**Acceso:** `api/Customer/paginado`  
**Formato:** JSON  
**Servicio:** REST / GET  
**Autenticación:** JWT requerido

##### 3.2.6.1. Parámetros de Consulta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** | **Valor Predeterminado** |
|------------|-----------------|----------|---------------|--------------------------|
| pagina | Número de página | Integer | No | 1 |
| elementosPorPagina | Cantidad de elementos por página | Integer | No | 10 |
| busqueda | Texto para filtrar resultados | String | No | null |

### 3.3. Gestión de Posts

#### 3.3.1. Obtener Todos los Posts

Obtiene la lista de todos los posts.

**Acceso:** `api/Post`  
**Formato:** JSON  
**Servicio:** REST / GET  
**Autenticación:** JWT requerido

##### 3.3.1.1. Parámetros de Salida

| **Nombre** | **Descripción** | **Tipo** |
|------------|-----------------|-----------|
| exito | Indica si la operación fue exitosa | Boolean |
| mensaje | Mensaje general de la operación | String |
| detalle | Descripción detallada del resultado | String |
| resultado | Lista de posts | Array |
| resultado[].postId | Identificador único del post | Integer |
| resultado[].title | Título del post | String |
| resultado[].body | Contenido del post | String |
| resultado[].type | Tipo del post | Integer |
| resultado[].category | Categoría del post | String |
| resultado[].customerId | ID del customer asociado | Integer |

#### 3.3.2. Obtener Post por ID

Obtiene un post específico por su ID.

**Acceso:** `api/Post/{id}`  
**Formato:** JSON  
**Servicio:** REST / GET  
**Autenticación:** JWT requerido

#### 3.3.3. Crear Post

Crea un nuevo post en el sistema.

**Acceso:** `api/Post`  
**Formato:** JSON  
**Servicio:** REST / POST  
**Autenticación:** JWT requerido

##### 3.3.3.1. Parámetros de Entrada

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| title | Título del post | String | No |
| body | Contenido del post | String | No |
| type | Tipo del post | Integer | Sí |
| category | Categoría del post | String | No |
| customerId | ID del customer asociado | Integer | Sí |

Ejemplo de entrada:
```json
{
  "title": "Mi nuevo post",
  "body": "Este es el contenido de mi post con más de veinte caracteres para probar la funcionalidad de corte automático.",
  "type": 1,
  "category": "",
  "customerId": 1002
}
```

**Reglas de Negocio:**
1. **Validación de Customer:** Debe existir un customer con el ID proporcionado
2. **Corte de Body:** Si el body tiene más de 20 caracteres y excede 97 caracteres, se corta y se añade "..."
3. **Categoría automática según Type:**
   - Type = 1 → Category = "Farándula"
   - Type = 2 → Category = "Política"
   - Type = 3 → Category = "Futbol"
   - Otros valores → Mantiene la categoría del usuario

#### 3.3.4. Actualizar Post

Actualiza un post existente.

**Acceso:** `api/Post/{id}`  
**Formato:** JSON  
**Servicio:** REST / PUT  
**Autenticación:** JWT requerido

#### 3.3.5. Eliminar Post

Elimina un post específico.

**Acceso:** `api/Post/{id}`  
**Formato:** JSON  
**Servicio:** REST / DELETE  
**Autenticación:** JWT requerido

#### 3.3.6. Buscar Posts

Busca posts por título o contenido.

**Acceso:** `api/Post/buscar`  
**Formato:** JSON  
**Servicio:** REST / GET  
**Autenticación:** JWT requerido

##### 3.3.6.1. Parámetros de Consulta

| **Nombre** | **Descripción** | **Tipo** | **Requerido** |
|------------|-----------------|----------|---------------|
| termino | Término de búsqueda | String | Sí |

#### 3.3.7. Obtener Posts por Usuario

Obtiene todos los posts de un customer específico.

**Acceso:** `api/Post/usuario/{userId}`  
**Formato:** JSON  
**Servicio:** REST / GET  
**Autenticación:** JWT requerido

#### 3.3.8. Crear Múltiples Posts

Crea N cantidad de posts al mismo tiempo.

**Acceso:** `api/Post/crear-multiples`  
**Formato:** JSON  
**Servicio:** REST / POST  
**Autenticación:** JWT requerido

##### 3.3.8.1. Parámetros de Entrada

Array de objetos Post con la misma estructura que la creación individual.

Ejemplo de entrada:
```json
[
  {
    "title": "Post 1",
    "body": "Contenido del primer post",
    "type": 1,
    "customerId": 1002
  },
  {
    "title": "Post 2",
    "body": "Contenido del segundo post",
    "type": 2,
    "customerId": 1002
  }
]
```

**Validaciones:**
- Cada post se valida individualmente
- Si algún post falla la validación, se reporta el error específico
- Solo se crean los posts válidos

## 4. SEGURIDAD

### 4.1. Autenticación

La API utiliza un sistema de doble autenticación:

1. **Headers de Acceso:** Todos los endpoints requieren headers básicos
2. **JWT Token:** Los endpoints protegidos requieren token JWT

#### 4.1.1. Headers para Acceso a la API

| **Header** | **Descripción** | **Ejemplo** |
|------------|-----------------|-------------|
| Sitio | Nombre del sitio | `ProjectAPI` |
| Clave | Clave de acceso | `ProjectAPI2024` |

#### 4.1.2. Headers para Autenticación JWT

| **Header** | **Descripción** | **Ejemplo** |
|------------|-----------------|-------------|
| Authorization | Token JWT con formato Bearer | `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...` |

### 4.2. Middleware Personalizado

#### 4.2.1. AccesoAttribute

Valida los headers de acceso básico antes de procesar cualquier request.

#### 4.2.2. JwtAuthorizationAttribute

Valida el token JWT y extrae la información del usuario.

#### 4.2.3. ValidarModeloAttribute

Valida automáticamente los modelos de entrada usando Data Annotations.

#### 4.2.4. LogAttribute

Registra automáticamente todas las acciones del usuario.

#### 4.2.5. ExceptionAttribute

Maneja excepciones no controladas y las registra en el sistema de logs.

### 4.3. Configuración de Seguridad

```json
{
  "JwtSettings": {
    "Key": "MySecretKeyForJWTGeneration2024ProjectAPI!",
    "Issuer": "ProjectAPI",
    "Audience": "ProjectAPI",
    "TiempoExpiracionMinutos": 30,
    "TiempoExpiracionBDMinutos": 60
  },
  "AccessSettings": {
    "DefaultSitio": "ProjectAPI",
    "DefaultClave": "ProjectAPI2024"
  }
}
```

## 5. SISTEMA DE LOGGING

### 5.1. Configuración Serilog

La API utiliza Serilog para el registro de logs con los siguientes destinos:

1. **Consola:** Para desarrollo y depuración
2. **Base de Datos:** Logs persistentes en tabla Logs

### 5.2. Niveles de Log

| **Nivel** | **Descripción** |
|-----------|-----------------|
| Information | Información general de operaciones |
| Warning | Advertencias del sistema |
| Error | Errores controlados |
| Fatal | Errores críticos del sistema |

### 5.3. Configuración de Logs

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "...",
          "tableName": "Logs",
          "autoCreateSqlTable": true
        }
      }
    ]
  }
}
```

## 6. CONFIGURACIÓN CORS

### 6.1. Configuración Predeterminada

```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200", "https://localhost:4200"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true
  }
}
```

## 7. ESTRUCTURA DEL PROYECTO

### 7.1. Capas de la Aplicación

```
ProjectAPI/
├── API/                          # Capa de presentación
│   ├── Controllers/             # Controladores REST
│   ├── Attributes/              # Middleware personalizado
│   └── Program.cs               # Configuración de la aplicación
├── Business/                    # Capa de lógica de negocio
│   ├── Contracts/               # Interfaces
│   ├── BaseService.cs           # Servicio base genérico
│   ├── AuthRepository.cs        # Lógica de autenticación
│   ├── CustomerRepository.cs    # Lógica de customers
│   ├── PostRepository.cs        # Lógica de posts
│   └── TokenRepository.cs       # Gestión de tokens
├── DataAccess/                  # Capa de acceso a datos
│   ├── Data/                    # Entidades del modelo
│   ├── BaseModel.cs             # Modelo base genérico
│   └── JujuTestContext.cs       # Contexto de Entity Framework
├── ProjectAPI.Shared/           # DTOs y clases compartidas
│   ├── AuthDTO/                 # DTOs de autenticación
│   ├── CustomerDTO/             # DTOs de customers
│   ├── PostDTO/                 # DTOs de posts
│   └── GeneralDTO/              # DTOs generales
└── ProjectAPI.Util/             # Utilidades
    ├── JwtHelper.cs             # Utilidades JWT
    └── Mapping.cs               # Utilidades de mapeo
```

### 7.2. Patrones Implementados

1. **Repository Pattern:** Separación de la lógica de acceso a datos
2. **Generic Repository:** Implementación genérica para operaciones CRUD
3. **DTO Pattern:** Transferencia de datos entre capas
4. **Dependency Injection:** Inyección de dependencias nativa de .NET
5. **Middleware Pattern:** Procesamiento de requests mediante middleware

## 8. EJEMPLOS DE USO

### 8.1. Flujo Completo de Autenticación

#### 8.1.1. Login
```bash
curl -X POST "https://localhost:7071/api/Auth/login" \
  -H "Content-Type: application/json" \
  -H "Sitio: ProjectAPI" \
  -H "Clave: ProjectAPI2024" \
  -d '{
    "nombreUsuario": "admin",
    "contraseña": "PostLtda2025",
    "ip": "192.168.1.1"
  }'
```

#### 8.1.2. Uso del Token
```bash
curl -X GET "https://localhost:7071/api/Customer" \
  -H "Sitio: ProjectAPI" \
  -H "Clave: ProjectAPI2024" \
  -H "Authorization: Bearer {token}"
```

### 8.2. Gestión de Customers

#### 8.2.1. Crear Customer
```bash
curl -X POST "https://localhost:7071/api/Customer" \
  -H "Content-Type: application/json" \
  -H "Sitio: ProjectAPI" \
  -H "Clave: ProjectAPI2024" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "name": "Nuevo Customer"
  }'
```

#### 8.2.2. Actualizar Customer
```bash
curl -X PUT "https://localhost:7071/api/Customer/1002" \
  -H "Content-Type: application/json" \
  -H "Sitio: ProjectAPI" \
  -H "Clave: ProjectAPI2024" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "customerId": 1002,
    "name": "Laura Sugey Actualizada"
  }'
```

### 8.3. Gestión de Posts

#### 8.3.1. Crear Post con Reglas de Negocio
```bash
curl -X POST "https://localhost:7071/api/Post" \
  -H "Content-Type: application/json" \
  -H "Sitio: ProjectAPI" \
  -H "Clave: ProjectAPI2024" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "title": "Post de Ejemplo",
    "body": "Este es un contenido muy largo que debería ser cortado automáticamente por el sistema cuando exceda los 97 caracteres permitidos según las reglas de negocio implementadas",
    "type": 1,
    "customerId": 1002
  }'
```

#### 8.3.2. Crear Múltiples Posts
```bash
curl -X POST "https://localhost:7071/api/Post/crear-multiples" \
  -H "Content-Type: application/json" \
  -H "Sitio: ProjectAPI" \
  -H "Clave: ProjectAPI2024" \
  -H "Authorization: Bearer {token}" \
  -d '[
    {
      "title": "Post 1",
      "body": "Contenido del primer post",
      "type": 1,
      "customerId": 1002
    },
    {
      "title": "Post 2",
      "body": "Contenido del segundo post",
      "type": 2,
      "customerId": 1002
    }
  ]'
```

## 9. GESTIÓN DE ERRORES

### 9.1. Estructura de Respuesta Estándar

Todas las respuestas de la API siguen la estructura estándar `RespuestaDto`:

```json
{
  "exito": boolean,
  "mensaje": "string",
  "detalle": "string",
  "resultado": object
}
```

### 9.2. Códigos de Estado HTTP

| **Código** | **Descripción** | **Uso** |
|------------|-----------------|---------|
| 200 | OK | Operación exitosa |
| 201 | Created | Recurso creado exitosamente |
| 400 | Bad Request | Error en la solicitud |
| 401 | Unauthorized | Error de autenticación |
| 404 | Not Found | Recurso no encontrado |
| 500 | Internal Server Error | Error interno del servidor |

### 9.3. Ejemplos de Respuestas de Error

#### 9.3.1. Error de Validación
```json
{
  "exito": false,
  "mensaje": "Datos inválidos",
  "detalle": "El nombre es requerido; El nombre no puede exceder 500 caracteres",
  "resultado": null
}
```

#### 9.3.2. Error de Autenticación
```json
{
  "exito": false,
  "mensaje": "Token inválido",
  "detalle": "El token proporcionado ha expirado o es inválido",
  "resultado": null
}
```

#### 9.3.3. Error de Negocio
```json
{
  "exito": false,
  "mensaje": "Usuario no encontrado",
  "detalle": "No existe un customer con ID 999",
  "resultado": null
}
```

## 10. CONFIGURACIÓN Y DESPLIEGUE

### 10.1. Cadena de Conexión

```json
{
  "ConnectionStrings": {
    "Development": "Data Source=localhost,1433;Initial Catalog=JujuTest;User ID=SA;Password=PortalEmpleo2024;Trust Server Certificate=True;Connect Timeout=30"
  }
}
```

### 10.2. Configuración de Desarrollo

- **Puerto HTTPS:** 7071
- **Puerto HTTP:** 5071
- **Base de datos:** SQL Server
- **Swagger:** Habilitado en desarrollo (ruta raíz)

### 10.3. Variables de Entorno

Las siguientes configuraciones pueden ser sobrescritas mediante variables de entorno:

- `ConnectionStrings__Development`
- `JwtSettings__Key`
- `JwtSettings__TiempoExpiracionMinutos`
- `AccessSettings__DefaultSitio`
- `AccessSettings__DefaultClave`

## 11. BUENAS PRÁCTICAS IMPLEMENTADAS

### 11.1. Código

- **Separación de responsabilidades** en capas bien definidas
- **Inyección de dependencias** para facilitar testing
- **Async/Await** para operaciones asíncronas
- **Generic repositories** para reutilización de código
- **Data annotations** para validaciones automáticas

### 11.2. Seguridad

- **Doble autenticación** (headers + JWT)
- **Hasheo de contraseñas** con SHA256
- **Validación de tokens** con expiración
- **CORS configurado** para orígenes específicos
- **Logging completo** de acciones de usuarios

### 11.3. Base de Datos

- **Entity Framework Core** con Code First
- **Transacciones automáticas** en operaciones complejas
- **Eliminación en cascada** implementada manualmente
- **Índices únicos** en campos críticos
- **Valores por defecto** en tablas

### 11.4. API

- **Documentación Swagger** automática
- **Versionado** mediante rutas
- **Paginación** en endpoints de listado
- **Búsqueda** en endpoints aplicables
- **Respuestas consistentes** con RespuestaDto

## 12. CONSIDERACIONES TÉCNICAS

### 12.1. Rendimiento

- **Consultas asíncronas** para mejor throughput
- **Lazy loading** deshabilitado por defecto
- **Connection pooling** automático de EF Core
- **Logs estructurados** para mejor análisis

### 12.2. Escalabilidad

- **Stateless design** compatible con balanceadores
- **JWT tokens** para autenticación distribuida
- **Configuración externa** mediante appsettings
- **Separación de capas** para microservicios futuros

### 12.3. Mantenibilidad

- **Código autodocumentado** con comentarios XML
- **Patrones consistentes** en toda la aplicación
- **Manejo centralizado** de errores y logs
- **Testing ready** con interfaces y DI

## 13. CONCLUSIÓN

ProjectAPI es una solución robusta y escalable para la gestión de customers y posts, implementando las mejores prácticas de desarrollo .NET y proporcionando una base sólida para futuras expansiones. El sistema de autenticación JWT, el manejo centralizado de errores y la arquitectura en capas facilitan tanto el mantenimiento como la evolución del sistema.

La API está preparada para entornos de producción con logging completo, manejo de errores robusto y documentación exhaustiva que facilita la integración por parte de equipos de desarrollo frontend.

---

*Documentación generada para ProjectAPI v1.0.0*  
*Última actualización: Julio 14, 2025*
