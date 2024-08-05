# Pruebas con SQL Server en Docker

Para realizar estas pruebas, he creado un contenedor de SQL Server en mi entorno local utilizando Docker y una base de datos con la siguiente estructura y datos de prueba.

## Instrucciones

### 1. Crear el contenedor de SQL Server

Ejecuta el siguiente comando para crear un contenedor de SQL Server:

```sh
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Your_password123" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2019-latest
```

### 2. Crear la tabla 'dbo.Todos'

Conéctate al contenedor de SQL Server y ejecuta el siguiente comando para crear la tabla 'dbo.Todos':

```sql
CREATE TABLE dbo.Todos (
    Id INT PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000),
    CreatedBy INT NOT NULL,
    AssignedTo NVARCHAR(255),
    TargetDate DATETIME,
    IsComplete BIT NOT NULL
);
```

### 3. Insertar datos de prueba

Ejecuta el siguiente comando para insertar datos de prueba en la tabla 'dbo.Todos':

```sql
INSERT INTO Todos (Id, Title, Description, CreatedBy, AssignedTo, TargetDate, IsComplete) VALUES
(1, 'Buy groceries', 'Milk, Eggs, Bread', 1, '2,3', '2024-08-10 00:00:00', 0),
(2, 'Complete project report', 'Finish the final draft', 2, '4', '2024-08-15 00:00:00', 0),
(3, 'Plan vacation', 'Decide on the destination and book tickets', 1, NULL, NULL, 0),
(4, 'Workout', 'Morning exercise routine', 3, '1,4', '2024-08-04 00:00:00', 1),
(5, 'Read a book', 'Finish reading the current novel', 2, NULL, NULL, 0),
(6, 'Visit the dentist', 'Regular check-up and cleaning', 3, NULL, '2024-08-20 00:00:00', 0),
(7, 'Prepare presentation', 'Slides for the upcoming meeting', 1, '2', '2024-08-05 00:00:00', 0),
(8, 'Organize garage', 'Clean and organize tools', 4, '1,3', NULL, 0),
(9, 'Write blog post', 'Article about latest tech trends', 2, NULL, '2024-08-12 00:00:00', 0),
(10, 'Fix the kitchen sink', 'Repair the leak', 3, '4', NULL, 0),
(11, 'Call mom', 'Weekly check-in', 1, NULL, '2024-08-06 00:00:00', 0),
(12, 'Team meeting', 'Discuss project milestones', 4, '2,3', '2024-08-07 09:00:00', 0),
(13, 'Grocery shopping', 'Buy vegetables and fruits', 1, '3', '2024-08-08 00:00:00', 0),
(14, 'Finish online course', 'Complete the final module', 2, NULL, '2024-08-09 00:00:00', 0),
(15, 'Car maintenance', 'Oil change and tire rotation', 3, '1,4', '2024-08-14 00:00:00', 0),
(16, 'Update software', 'Install latest updates', 4, NULL, '2024-08-11 00:00:00', 0),
(17, 'Plan team outing', 'Decide on venue and date', 1, '2,3', NULL, 0),
(18, 'Pay bills', 'Electricity and internet bills', 2, NULL, '2024-08-05 00:00:00', 0),
(19, 'Attend webinar', 'Topic: AI advancements', 3, NULL, '2024-08-10 15:00:00', 0),
(20, 'Bake a cake', 'Chocolate cake for friend’s birthday', 1, '4', '2024-08-13 00:00:00', 0),
(21, 'Submit tax forms', 'Annual tax submission', 2, NULL, '2024-08-18 00:00:00', 0);
```
