Para realizar estas pruebas, en mi docker local, he creado un contenedor de SQL Server y una base de datos con alimentada con los siguientes comandos:

docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Your_password123" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2019-latest


CREATE TABLE dbo.Todos (
    Id INT PRIMARY KEY,
    Title NVARCHAR(255) NULL,
    DueBy DATE NULL,
    IsComplete BIT NOT NULL DEFAULT 0
);

INSERT INTO dbo.Todos (Id, Title, DueBy, IsComplete)
VALUES
    (1, 'Buy groceries', '2024-08-02', 0),
    (2, 'Complete project report', '2024-08-05', 0),
    (3, 'Schedule doctor appointment', NULL, 0),
    (4, 'Read a book', '2024-08-10', 1),
    (5, 'Exercise', NULL, 0);



# Pruebas con SQL Server en Docker

Para realizar estas pruebas, he creado un contenedor de SQL Server en mi entorno local utilizando Docker y una base de datos con la siguiente estructura y datos de prueba.

## Instrucciones

### 1. Crear el contenedor de SQL Server

Ejecuta el siguiente comando para crear un contenedor de SQL Server:

```sh
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Your_password123" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2019-latest
```

### 2. Crear la tabla 'dbo.Todos'

Conéctate al contenedor de SQL Server y ejecuta el siguiente comando para crear la tabla dbo.Todos:

```sql
CREATE TABLE dbo.Todos (
    Id INT PRIMARY KEY,
    Title NVARCHAR(255) NULL,
    DueBy DATE NULL,
    IsComplete BIT NOT NULL DEFAULT 0
);
```

### 3. Insertar datos de prueba

Ejecuta el siguiente comando para insertar datos de prueba en la tabla dbo.Todos:

```sql
INSERT INTO dbo.Todos (Id, Title, DueBy, IsComplete)
VALUES
    (1, 'Buy groceries', '2024-08-02', 0),
    (2, 'Complete project report', '2024-08-05', 0),
    (3, 'Schedule doctor appointment', NULL, 0),
    (4, 'Read a book', '2024-08-10', 1),
    (5, 'Exercise', NULL, 0);
```

### Datos de Prueba

| Id  | Title                       | DueBy       | IsComplete |
| --- | --------------------------- | ----------- | ---------- |
| 1   | Buy groceries               | 2024-08-02  | 0          |
| 2   | Complete project report     | 2024-08-05  | 0          |
| 3   | Schedule doctor appointment | NULL        | 0          |
| 4   | Read a book                 | 2024-08-10  | 1          |
| 5   | Exercise                    | NULL        | 0          |

