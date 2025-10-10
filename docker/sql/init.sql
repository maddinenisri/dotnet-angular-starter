-- Initialize PersonDb database
-- This script creates the database if it doesn't exist

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'PersonDb')
BEGIN
    CREATE DATABASE PersonDb;
END
GO

USE PersonDb;
GO

-- Database is ready
-- EF Core migrations will create tables
PRINT 'PersonDb initialized successfully';
GO
