﻿using CDS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationTests
{
    [TestClass]
    public class SQLiteTest
    {
        // El nombre del archivo de base de datos que se usará en las pruebas

        private readonly string testPath = @"C:\Sistema\PROY_NUEVO";
        private readonly string testFolderPath = @"C:\Sistema\PROY_NUEVO" + @"\CDS";
        private readonly string testDatabaseName = "cds.db";

        [TestInitialize]
        public void TestInitialize()
        {
            Data data = new Data
            {
                RutaProyNuevo = testPath
            };

            Configuration.SaveConfiguration(data);

            string databasePath = Path.Combine(testFolderPath, testDatabaseName);

            // Crear la base de datos si no existe
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            string databasePath = Path.Combine(testFolderPath, testDatabaseName);

            // Limpiar cualquier archivo de base de datos creado después de las pruebas
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }

        [TestMethod]
        public void CreateDatabase_ReturnTrue_WhenItDoesNotExist()
        {
            // Arrange
            var connector = ConnectorSQLite.Instance;

            bool expected = true;

            // Act
            bool actual = connector.CreateDatabase();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CreateDatabase_ReturnFalse_WhenItAlreadyExist()
        {
            // Arrange
            var connector = ConnectorSQLite.Instance;

            connector.CreateDatabase();

            bool expected = false;

            // Act
            bool actual = connector.CreateDatabase();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExecuteNonQuery_CreateTable()
        {
            // Arrange
            var connector = ConnectorSQLite.Instance;

            connector.CreateDatabase();

            int expected = 0;

            // Act
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(Nombre TEXT, Edad  INTEGER)";

            int actual = connector.ExecuteNonQuery(createTableQuery);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExecuteInsertOrStateQuery_InsertOneRowModify()
        {
            // Arrange
            var connector = ConnectorSQLite.Instance;

            connector.CreateDatabase();

            // Crear las tablas si no existen
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(Nombre TEXT, Edad  INTEGER)";

            _ = connector.ExecuteNonQuery(createTableQuery);

            int expected = 1;

            // Act
            int actual = connector.ExecuteNonQuery($"INSERT INTO Usuarios (Nombre, Edad) VALUES ('Carlos', 25)");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExecuteInsertOrStateQuery_DeleteNoModify()
        {
            // Arrange
            var connector = ConnectorSQLite.Instance;

            connector.CreateDatabase();

            // Crear las tablas si no existen
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(id_usuario INTEGER, nombre TEXT, edad  INTEGER, " +
                                      "PRIMARY KEY(ID_Usuario))";

            _ = connector.ExecuteNonQuery(createTableQuery);

            int expected = 0;

            // Act
            int actual = connector.ExecuteNonQuery($"DELETE FROM Usuarios WHERE id_usuario = (1)");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExecuteInsertOrStateQuery_UpdateException()
        {
            // Arrange
            var connector = ConnectorSQLite.Instance;

            connector.CreateDatabase();

            // Crear las tablas si no existen
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(id_usuario INTEGER, nombre TEXT, edad  INTEGER, " +
                                      "PRIMARY KEY(ID_Usuario))";

            _ = connector.ExecuteNonQuery(createTableQuery);

            int expected = -1;

            // Act
            int actual = connector.ExecuteNonQuery($"UPDATE Usuarios SET name = 'Charls', age = 29");

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExecuteSelectQuery_NotNull()
        {
            // Arrange
            var connector = ConnectorSQLite.Instance;

            connector.CreateDatabase();

            // Crear las tablas si no existen
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(id_usuario INTEGER, nombre TEXT, edad  INTEGER, " +
                                      "PRIMARY KEY(ID_Usuario))";

            _ = connector.ExecuteNonQuery(createTableQuery);

            connector.ExecuteNonQuery($"INSERT INTO Usuarios (Nombre, Edad) VALUES ('Carlos', 25)");

            int expected = 0;

            // Act
            DataTable actual = connector.ExecuteSelectQuery("SELECT * FROM Usuarios WHERE id_usuario = (3)");
            int count = actual.Rows.Count;

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, count);
        }

        [TestMethod]
        public void ExecuteSelectQuery_NotNullException()
        {
            // Arrange
            var connector = ConnectorSQLite.Instance;

            connector.CreateDatabase();

            // Crear las tablas si no existen
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Usuarios " +
                                      "(id_usuario INTEGER, nombre TEXT, edad  INTEGER, " +
                                      "PRIMARY KEY(ID_Usuario))";

            _ = connector.ExecuteNonQuery(createTableQuery);

            connector.ExecuteNonQuery($"INSERT INTO Usuarios (Nombre, Edad) VALUES ('Carlos', 25)");

            // Act
            var actual = connector.ExecuteSelectQuery("SELECT * FROM Users");

            // Assert
            Assert.IsNull(actual);
        }
    }
}