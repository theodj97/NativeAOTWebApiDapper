DROP DATABASE IF EXISTS Todo
GO

CREATE DATABASE Todo
GO

USE Todo
GO

CREATE TABLE dbo.Todos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000),
    CreatedBy INT NOT NULL,
    AssignedTo NVARCHAR(255),
    TargetDate DATETIME,
    IsComplete BIT NOT NULL
);

GO

INSERT INTO Todos (Title, Description, CreatedBy, AssignedTo, TargetDate, IsComplete) VALUES
('Buy groceries', 'Milk, Eggs, Bread', 1, '2,3', '2024-08-10 00:00:00', 0),
('Complete project report', 'Finish the final draft', 2, '4', '2024-08-15 00:00:00', 0),
('Plan vacation', 'Decide on the destination and book tickets', 1, NULL, NULL, 0),
('Workout', 'Morning exercise routine', 3, '1,4', '2024-08-04 00:00:00', 1),
('Read a book', 'Finish reading the current novel', 2, NULL, NULL, 0),
('Visit the dentist', 'Regular check-up and cleaning', 3, NULL, '2024-08-20 00:00:00', 0),
('Prepare presentation', 'Slides for the upcoming meeting', 1, '2', '2024-08-05 00:00:00', 0),
('Organize garage', 'Clean and organize tools', 4, '1,3', NULL, 0),
('Write blog post', 'Article about latest tech trends', 2, NULL, '2024-08-12 00:00:00', 0),
('Fix the kitchen sink', 'Repair the leak', 3, '4', NULL, 0),
('Call mom', 'Weekly check-in', 1, NULL, '2024-08-06 00:00:00', 0),
('Team meeting', 'Discuss project milestones', 4, '2,3', '2024-08-07 09:00:00', 0),
('Grocery shopping', 'Buy vegetables and fruits', 1, '3', '2024-08-08 00:00:00', 0),
('Finish online course', 'Complete the final module', 2, NULL, '2024-08-09 00:00:00', 0),
('Car maintenance', 'Oil change and tire rotation', 3, '1,4', '2024-08-14 00:00:00', 0),
('Update software', 'Install latest updates', 4, NULL, '2024-08-11 00:00:00', 0),
('Plan team outing', 'Decide on venue and date', 1, '2,3', NULL, 0),
('Pay bills', 'Electricity and internet bills', 2, NULL, '2024-08-05 00:00:00', 0),
('Attend webinar', 'Topic: AI advancements', 3, NULL, '2024-08-10 15:00:00', 0),
('Bake a cake', 'Chocolate cake for friend’s birthday', 1, '4', '2024-08-13 00:00:00', 0),
('Submit tax forms', 'Annual tax submission', 2, NULL, '2024-08-18 00:00:00', 0);