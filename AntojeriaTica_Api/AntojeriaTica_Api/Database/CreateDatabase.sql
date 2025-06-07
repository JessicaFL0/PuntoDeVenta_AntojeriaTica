-- Para usar ejecutar en sql server: sqlcmd -S "{nombre_servidor}" -E -i "AntojeriaTica_Api\Database\CreateDatabase.sql"

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AntojeriaTica')
BEGIN
    CREATE DATABASE AntojeriaTica;
END
GO

USE AntojeriaTica;
GO

-- Crear la tabla Rol si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Rol' AND xtype='U')
BEGIN
    CREATE TABLE Rol (
        IdRol INT PRIMARY KEY IDENTITY(1,1),
        NombreRol VARCHAR(50) UNIQUE NOT NULL,
        Descripcion TEXT
    );
END
GO

-- Crear la tabla Usuario si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuario' AND xtype='U')
BEGIN
    CREATE TABLE Usuario (
        IdUsuario INT PRIMARY KEY IDENTITY(1,1),
        NombreCompleto VARCHAR(100) NOT NULL,
        Correo VARCHAR(100) UNIQUE NOT NULL,
        Cedula VARCHAR(20) UNIQUE NOT NULL,
        ContrasenaHash VARCHAR(255) NOT NULL,
        Estado VARCHAR(20) NOT NULL,
        IdRol INT NOT NULL,
        FOREIGN KEY (IdRol) REFERENCES Rol(IdRol)
    );
END
GO

-- Insertar rol por defecto si no existe
IF NOT EXISTS (SELECT 1 FROM Rol WHERE NombreRol = 'Usuario')
BEGIN
    INSERT INTO Rol (NombreRol, Descripcion) VALUES ('Usuario', 'Rol de usuario estándar');
END
GO

--STORED PROCEDURES - TABLE ROL

-- Crear o actualizar el stored procedure para insertar rol
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_InsertarRol')
BEGIN
    DROP PROCEDURE sp_InsertarRol;
END
GO

CREATE PROCEDURE sp_InsertarRol
   @NombreRol VARCHAR(50),
   @Descripcion TEXT
AS
BEGIN
   INSERT INTO Rol (NombreRol, Descripcion)
   VALUES (@NombreRol, @Descripcion);
END
GO

-- Crear o actualizar el stored procedure para actualizar rol
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ActualizarRol')
BEGIN
    DROP PROCEDURE sp_ActualizarRol;
END
GO

CREATE PROCEDURE sp_ActualizarRol
   @IdRol INT,
   @NombreRol VARCHAR(50),
   @Descripcion TEXT
AS
BEGIN
   UPDATE Rol
   SET NombreRol = @NombreRol,
       Descripcion = @Descripcion
   WHERE IdRol = @IdRol;
END
GO

-- Crear o actualizar el stored procedure para eliminar rol
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_EliminarRol')
BEGIN
    DROP PROCEDURE sp_EliminarRol;
END
GO

CREATE PROCEDURE sp_EliminarRol
   @IdRol INT
AS
BEGIN
   DELETE FROM Rol WHERE IdRol = @IdRol;
END
GO

-- Crear o actualizar el stored procedure para obtener roles
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerRoles')
BEGIN
    DROP PROCEDURE sp_ObtenerRoles;
END
GO

CREATE PROCEDURE sp_ObtenerRoles
AS
BEGIN
   SELECT * FROM Rol;
END
GO

--STORED PROCEDURES - TABLE USUARIO

-- Crear o actualizar el stored procedure para insertar usuario
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_InsertarUsuario')
BEGIN
    DROP PROCEDURE sp_InsertarUsuario;
END
GO

CREATE PROCEDURE sp_InsertarUsuario
    @NombreCompleto VARCHAR(100),
    @Correo VARCHAR(100),
    @Cedula VARCHAR(20),
    @ContrasenaHash VARCHAR(255),
    @Estado VARCHAR(20),
    @IdRol INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Verificar si ya existe un usuario con la misma cédula o correo
        IF EXISTS (SELECT 1 FROM Usuario WHERE Cedula = @Cedula)
        BEGIN
            RAISERROR('Ya existe un usuario con esta cédula', 16, 1);
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM Usuario WHERE Correo = @Correo)
        BEGIN
            RAISERROR('Ya existe un usuario con este correo', 16, 1);
            RETURN;
        END

        -- Insertar el nuevo usuario
        INSERT INTO Usuario (NombreCompleto, Correo, Cedula, ContrasenaHash, Estado, IdRol)
        VALUES (@NombreCompleto, @Correo, @Cedula, @ContrasenaHash, @Estado, @IdRol);

        -- Retornar el ID del usuario creado
        SELECT SCOPE_IDENTITY() AS IdUsuario;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- Crear o actualizar el stored procedure para actualizar usuario
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ActualizarUsuario')
BEGIN
    DROP PROCEDURE sp_ActualizarUsuario;
END
GO

CREATE PROCEDURE sp_ActualizarUsuario
   @IdUsuario INT,
   @NombreCompleto VARCHAR(100),
   @Correo VARCHAR(100),
   @Cedula VARCHAR(20),
   @Estado VARCHAR(20),
   @IdRol INT
AS
BEGIN
   UPDATE Usuario
   SET NombreCompleto = @NombreCompleto,
       Correo = @Correo,
       Cedula = @Cedula,
       Estado = @Estado,
       IdRol = @IdRol
   WHERE IdUsuario = @IdUsuario;
END
GO

-- Crear o actualizar el stored procedure para eliminar usuario
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_EliminarUsuario')
BEGIN
    DROP PROCEDURE sp_EliminarUsuario;
END
GO

CREATE PROCEDURE sp_EliminarUsuario
   @IdUsuario INT
AS
BEGIN
   DELETE FROM Usuario WHERE IdUsuario = @IdUsuario;
END
GO

-- Crear o actualizar el stored procedure para obtener usuarios
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerUsuarios')
BEGIN
    DROP PROCEDURE sp_ObtenerUsuarios;
END
GO

CREATE PROCEDURE sp_ObtenerUsuarios
AS
BEGIN
   SELECT u.*, r.NombreRol
   FROM Usuario u
   INNER JOIN Rol r ON u.IdRol = r.IdRol;
END
GO

-- Crear o actualizar el stored procedure para obtener usuario por ID
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerUsuario')
BEGIN
    DROP PROCEDURE sp_ObtenerUsuario;
END
GO

CREATE PROCEDURE sp_ObtenerUsuario
   @IdUsuario INT
AS
BEGIN
   SELECT u.IdUsuario, u.NombreCompleto, u.Correo, u.Cedula, u.Estado, u.IdRol, r.NombreRol
   FROM Usuario u
   INNER JOIN Rol r ON u.IdRol = r.IdRol
   WHERE u.IdUsuario = @IdUsuario;
END
GO

PRINT 'Base de datos y stored procedures creados exitosamente';
