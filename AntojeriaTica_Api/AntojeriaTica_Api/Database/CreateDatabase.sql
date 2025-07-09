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
-- TABLA MOVIMIENTOS 28/6
CREATE TABLE MovimientoInventario (
    IdMovimiento INT PRIMARY KEY IDENTITY,
    IdProducto INT NOT NULL,
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    TipoMovimiento VARCHAR(10) NOT NULL, -- 'Entrada' o 'Salida'
    Cantidad INT NOT NULL,
    FOREIGN KEY (IdProducto) REFERENCES Producto(IdProducto)
);


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

-- Drop and recreate sp_ListarRoles safely
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ListarRoles')
BEGIN
    DROP PROCEDURE sp_ListarRoles;
END
GO

-- Crear  el stored procedure para obtener la lista de roles
CREATE PROCEDURE sp_ListarRoles
AS
BEGIN
    SELECT IdRol, NombreRol, Descripcion
    FROM Rol;
END
GO

-- Crear la tabla Producto si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Producto' AND xtype='U')
BEGIN
    CREATE TABLE Producto (
        IdProducto INT PRIMARY KEY IDENTITY(1,1),
        Codigo VARCHAR(50) UNIQUE NOT NULL,
        Nombre VARCHAR(100) NOT NULL,
        Descripcion TEXT NULL,
        PrecioUnitario DECIMAL(18,2) NOT NULL,
        Existencias INT NOT NULL
    );
END
GO

-- Crear la tabla HistorialProducto si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HistorialProducto' AND xtype='U')
BEGIN
    CREATE TABLE HistorialProducto (
        IdHistorial INT PRIMARY KEY IDENTITY(1,1),
        IdProducto INT NOT NULL,
        Fecha DATETIME NOT NULL DEFAULT(GETDATE()),
        Usuario VARCHAR(100) NOT NULL,
        Cambio TEXT NOT NULL,
        FOREIGN KEY (IdProducto) REFERENCES Producto(IdProducto)
    );
END
GO

-- Stored procedures Producto

-- Insertar
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_InsertarProducto')
BEGIN
    DROP PROCEDURE sp_InsertarProducto;
END
GO
CREATE PROCEDURE sp_InsertarProducto
    @Codigo VARCHAR(50),
    @Nombre VARCHAR(100),
    @Descripcion TEXT,
    @PrecioUnitario DECIMAL(18,2),
    @Existencias INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Producto WHERE Codigo = @Codigo)
    BEGIN
        RAISERROR('Ya existe un producto con este código',16,1);
        RETURN;
    END

    INSERT INTO Producto (Codigo, Nombre, Descripcion, PrecioUnitario, Existencias)
    VALUES (@Codigo, @Nombre, @Descripcion, @PrecioUnitario, @Existencias);

    SELECT SCOPE_IDENTITY() AS IdProducto;
END
GO

-- Actualizar
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ActualizarProducto')
BEGIN
    DROP PROCEDURE sp_ActualizarProducto;
END
GO
CREATE PROCEDURE sp_ActualizarProducto
    @IdProducto INT,
    @Nombre VARCHAR(100),
    @Descripcion TEXT,
    @PrecioUnitario DECIMAL(18,2),
    @Existencias INT
AS
BEGIN
    UPDATE Producto
    SET Nombre = @Nombre,
        Descripcion = @Descripcion,
        PrecioUnitario = @PrecioUnitario,
        Existencias = @Existencias
    WHERE IdProducto = @IdProducto;
END
GO

-- Obtener productos
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ObtenerProductos')
BEGIN
    DROP PROCEDURE sp_ObtenerProductos;
END
GO
CREATE PROCEDURE sp_ObtenerProductos
AS
BEGIN
    SELECT * FROM Producto;
END
GO

-- Obtener producto por id
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ObtenerProducto')
BEGIN
    DROP PROCEDURE sp_ObtenerProducto;
END
GO
CREATE PROCEDURE sp_ObtenerProducto
    @IdProducto INT
AS
BEGIN
    SELECT * FROM Producto WHERE IdProducto = @IdProducto;
END
GO

-- Insertar historial
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_InsertarProductoHistorial')
BEGIN
    DROP PROCEDURE sp_InsertarProductoHistorial;
END
GO
CREATE PROCEDURE sp_InsertarProductoHistorial
    @IdProducto INT,
    @Usuario VARCHAR(100),
    @Cambio TEXT
AS
BEGIN
    INSERT INTO HistorialProducto (IdProducto, Usuario, Cambio)
    VALUES (@IdProducto, @Usuario, @Cambio);
END
GO

-- Obtener historial producto
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ObtenerHistorialProducto')
BEGIN
    DROP PROCEDURE sp_ObtenerHistorialProducto;
END
GO
CREATE PROCEDURE sp_ObtenerHistorialProducto
    @IdProducto INT
AS
BEGIN
    SELECT * FROM HistorialProducto WHERE IdProducto = @IdProducto ORDER BY Fecha DESC;
END
GO

PRINT 'Base de datos y stored procedures creados exitosamente';

-- Elimianr productos 28/6

IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ObtenerHistorialProducto')
BEGIN
    DROP PROCEDURE sp_EliminarProducto;
END
GO
CREATE PROCEDURE sp_EliminarProducto
    @IdProducto INT
AS
BEGIN
    
    IF EXISTS (SELECT 1 FROM HistorialProducto WHERE IdProducto = @IdProducto)
    BEGIN
        RAISERROR ('No se puede eliminar este producto porque tiene historial.', 16, 1)
        RETURN
    END

    
    DELETE FROM Producto WHERE IdProducto = @IdProducto
END
-- Regitsrar movimeintos 28/6

IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ObtenerHistorialProducto')
BEGIN
    DROP PROCEDURE sp_RegistrarMovimientoInventarioo;
END
GO
CREATE PROCEDURE sp_RegistrarMovimientoInventario
    @IdProducto INT,
    @TipoMovimiento VARCHAR(10),
    @Cantidad INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Producto WHERE IdProducto = @IdProducto)
        THROW 50000, 'Producto no encontrado.', 1;

    IF @TipoMovimiento = 'Salida'
    BEGIN
        DECLARE @StockActual INT;
        SELECT @StockActual = Existencias FROM Producto WHERE IdProducto = @IdProducto;
        IF @StockActual < @Cantidad
            THROW 50000, 'No hay existencias suficientes.', 1;

        UPDATE Producto SET Existencias = Existencias - @Cantidad WHERE IdProducto = @IdProducto;
    END
    ELSE IF @TipoMovimiento = 'Entrada'
    BEGIN
        UPDATE Producto SET Existencias = Existencias + @Cantidad WHERE IdProducto = @IdProducto;
    END
    ELSE
        THROW 50000, 'Tipo de movimiento inválido.', 1;

    INSERT INTO MovimientoInventario (IdProducto, TipoMovimiento, Cantidad)
    VALUES (@IdProducto, @TipoMovimiento, @Cantidad);
END

-- Productos con estado 

IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ObtenerHistorialProducto')
BEGIN
    DROP PROCEDURE sp_ObtenerProductosConEstado;
END
GO
CREATE PROCEDURE  sp_ObtenerProductosConEstado
AS
BEGIN
    SELECT 
        IdProducto,
        Nombre,
        Descripcion,
        PrecioUnitario,
        Existencias,
        CASE 
            WHEN Existencias = 0 THEN 'Agotado'
            WHEN Existencias <= 5 THEN 'Bajo stock'
            ELSE 'Disponible'
        END AS EstadoStock
    FROM Producto
END

-- Tabla principal de ventas
CREATE TABLE Venta (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    MetodoPago VARCHAR(50) NOT NULL  -- Efectivo, Tarjeta, Sinpe Móvil
);

-- Detalle de cada producto vendido
CREATE TABLE DetalleVenta (
    Id INT PRIMARY KEY IDENTITY(1,1),
    VentaId INT NOT NULL,
    ProductoId INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (VentaId) REFERENCES Venta(Id),
    FOREIGN KEY (ProductoId) REFERENCES Producto(IdProducto)
);

--------------------------------------------------------------Nuevo Agregar
CREATE TYPE TipoDetalleVenta AS TABLE
(
    ProductoId INT,
    Cantidad INT,
    PrecioUnitario DECIMAL(10,2)
);
GO

CREATE PROCEDURE RegistrarVenta
    @MetodoPago VARCHAR(50),
    @DetallesVenta TipoDetalleVenta READONLY 
AS
BEGIN
    SET NOCOUNT ON;

    
    INSERT INTO Venta (Fecha, MetodoPago)
    VALUES (GETDATE(), @MetodoPago);

    DECLARE @VentaId INT = SCOPE_IDENTITY();

    
    INSERT INTO DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario)
    SELECT @VentaId, ProductoId, Cantidad, PrecioUnitario
    FROM @DetallesVenta;

    
    UPDATE P
    SET P.Existencias = P.Existencias - D.Cantidad
    FROM Producto P
    INNER JOIN @DetallesVenta D ON P.IdProducto = D.ProductoId;
END
GO

----------------------------------------Agregar por si quieren hacer pruebas 

INSERT INTO Producto (Codigo, Nombre, Descripcion, PrecioUnitario, Existencias)
VALUES 
('P001', 'Galleta Choco', 'Galleta con chispas', 1500.00, 20),
('P002', 'Refresco Mango', 'Bebida natural de mango', 1200.00, 15);

SELECT * FROM Venta;
SELECT * FROM DetalleVenta;
SELECT * FROM Producto;