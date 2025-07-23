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
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MovimientoInventario' AND xtype='U')
BEGIN
    CREATE TABLE MovimientoInventario (
        IdMovimiento INT PRIMARY KEY IDENTITY,
        IdProducto INT NOT NULL,
        Fecha DATETIME NOT NULL DEFAULT GETDATE(),
        TipoMovimiento VARCHAR(10) NOT NULL, -- 'Entrada' o 'Salida'
        Cantidad INT NOT NULL,
        FOREIGN KEY (IdProducto) REFERENCES Producto(IdProducto)
    );
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

-- Eliminar productos 28/6
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_EliminarProducto')
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
GO
-- Registrar movimientos 28/6
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_RegistrarMovimientoInventario')
BEGIN
    DROP PROCEDURE sp_RegistrarMovimientoInventario;
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
GO

-- Productos con estado
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ObtenerProductosConEstado')
BEGIN
    DROP PROCEDURE sp_ObtenerProductosConEstado;
END
GO
CREATE PROCEDURE sp_ObtenerProductosConEstado
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
GO

-- Tabla principal de ventas
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Venta' AND xtype='U')
BEGIN
    CREATE TABLE Venta (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Fecha DATETIME NOT NULL DEFAULT GETDATE(),
        MetodoPago VARCHAR(50) NOT NULL  -- Efectivo, Tarjeta, Sinpe Móvil
    );
END
GO

-- Detalle de cada producto vendido
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DetalleVenta' AND xtype='U')
BEGIN
    CREATE TABLE DetalleVenta (
        Id INT PRIMARY KEY IDENTITY(1,1),
        VentaId INT NOT NULL,
        ProductoId INT NOT NULL,
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(10,2) NOT NULL,
        FOREIGN KEY (VentaId) REFERENCES Venta(Id),
        FOREIGN KEY (ProductoId) REFERENCES Producto(IdProducto)
    );
END
GO

--------------------------------------------------------------Nuevo Agregar
IF TYPE_ID(N'TipoDetalleVenta') IS NULL
BEGIN
    CREATE TYPE TipoDetalleVenta AS TABLE
    (
        ProductoId INT,
        Cantidad INT,
        PrecioUnitario DECIMAL(10,2)
    );
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='RegistrarVenta')
BEGIN
    DROP PROCEDURE RegistrarVenta;
END
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

-- Solo insertar productos de prueba si no existen
IF NOT EXISTS (SELECT 1 FROM Producto WHERE Codigo = 'P001')
BEGIN
    INSERT INTO Producto (Codigo, Nombre, Descripcion, PrecioUnitario, Existencias)
    VALUES ('P001', 'Galleta Choco', 'Galleta con chispas', 1500.00, 20);
END

IF NOT EXISTS (SELECT 1 FROM Producto WHERE Codigo = 'P002')
BEGIN
    INSERT INTO Producto (Codigo, Nombre, Descripcion, PrecioUnitario, Existencias)
    VALUES ('P002', 'Refresco Mango', 'Bebida natural de mango', 1200.00, 15);
END

SELECT * FROM Venta;
SELECT * FROM DetalleVenta;
SELECT * FROM Producto;

------------------------------------------------------------NUEVO 4 SPRINT

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MetodoPago' AND xtype='U')
BEGIN
    CREATE TABLE MetodoPago (
        IdMetodoPago INT PRIMARY KEY IDENTITY(1,1),
        Nombre NVARCHAR(50) NOT NULL,
        EstaActivo BIT NOT NULL DEFAULT 1
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HistorialMetodoPago' AND xtype='U')
BEGIN
    CREATE TABLE HistorialMetodoPago (
        IdHistorial INT PRIMARY KEY IDENTITY(1,1),
        IdMetodoPago INT NOT NULL,
        FechaModificacion DATETIME NOT NULL DEFAULT GETDATE(),
        Accion NVARCHAR(50) NOT NULL,
        UsuarioModificador NVARCHAR(100) NOT NULL,
        FOREIGN KEY (IdMetodoPago) REFERENCES MetodoPago(IdMetodoPago)
    );
END
GO

IF OBJECT_ID('TR_MetodoPago_Historial', 'TR') IS NOT NULL
    DROP TRIGGER TR_MetodoPago_Historial;
GO

CREATE TRIGGER TR_MetodoPago_Historial
ON MetodoPago
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Accion NVARCHAR(50);

    -- inserción o una actualización
    IF EXISTS (SELECT * FROM inserted EXCEPT SELECT * FROM deleted)
        SET @Accion = 'INSERT';
    ELSE
        SET @Accion = 'UPDATE';

    -- Insertar en el historial
    INSERT INTO HistorialMetodoPago (IdMetodoPago, FechaModificacion, Accion, UsuarioModificador)
    SELECT
        i.IdMetodoPago,
        GETDATE(),
        @Accion,
        SYSTEM_USER
    FROM inserted i;
END;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Descuento' AND xtype='U')
BEGIN
    CREATE TABLE Descuento (
        IdDescuento INT PRIMARY KEY IDENTITY(1,1),
        Nombre NVARCHAR(100) NOT NULL,
        Tipo NVARCHAR(20) NOT NULL, -- 'Porcentaje', 'MontoFijo', 'Cupon'
        Valor DECIMAL(10,2) NOT NULL,
        CodigoCupon NVARCHAR(50) NULL,
        FechaInicio DATETIME NOT NULL,
        FechaFin DATETIME NOT NULL,
        Estado NVARCHAR(20) NOT NULL, -- 'Activo', 'Inactivo', 'Vencido'
        Restricciones NVARCHAR(MAX) NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Impuesto' AND xtype='U')
BEGIN
    CREATE TABLE Impuesto (
        IdImpuesto INT PRIMARY KEY IDENTITY(1,1),
        Nombre NVARCHAR(100),
        Tipo NVARCHAR(50), -- IVA o ISC
        Porcentaje DECIMAL(5,2),
        AplicaEnRestaurante BIT,
        EsExonerado BIT,
        Estado BIT -- 1: Activo, 0: Inactivo
    );
END
GO

-- Verificar si la columna ya existe antes de agregarla
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Producto') AND name = 'IdImpuesto')
BEGIN
    ALTER TABLE Producto
    ADD IdImpuesto INT FOREIGN KEY REFERENCES Impuesto(IdImpuesto);
END
GO

----movimientos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MovimientoDiario' AND xtype='U')
BEGIN
    CREATE TABLE MovimientoDiario (
        IdMovimiento INT IDENTITY(1,1) PRIMARY KEY,
        FechaHora DATETIME NOT NULL DEFAULT GETDATE(),
        TipoMovimiento VARCHAR(20) NOT NULL, -- 'Ingreso' o 'Egreso'
        Categoria VARCHAR(50) NOT NULL, -- 'Ventas', 'Compras', 'Gastos Operativos', etc.
        Monto DECIMAL(10,2) NOT NULL,
        Descripcion NVARCHAR(255),
        IdUsuario INT NOT NULL,
        FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
    );
END
GO

------
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='InsertarMovimientoDiario')
BEGIN
    DROP PROCEDURE InsertarMovimientoDiario;
END
GO
CREATE PROCEDURE InsertarMovimientoDiario
    @TipoMovimiento VARCHAR(20),
    @Categoria VARCHAR(50),
    @Monto DECIMAL(10,2),
    @Descripcion NVARCHAR(255),
    @IdUsuario INT
AS
BEGIN
    INSERT INTO MovimientoDiario (TipoMovimiento, Categoria, Monto, Descripcion, IdUsuario)
    VALUES (@TipoMovimiento, @Categoria, @Monto, @Descripcion, @IdUsuario);
END
GO
----

IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ListarMovimientosConNombre')
BEGIN
    DROP PROCEDURE sp_ListarMovimientosConNombre;
END
GO
CREATE PROCEDURE sp_ListarMovimientosConNombre
AS
BEGIN
    SELECT
        md.IdMovimiento,
        md.FechaHora,
        md.TipoMovimiento,
        md.Categoria,
        md.Monto,
        md.Descripcion,
        md.IdUsuario,
        u.NombreCompleto AS NombreUsuario
    FROM MovimientoDiario md
    INNER JOIN Usuario u ON md.IdUsuario = u.IdUsuario
    ORDER BY md.FechaHora DESC
END
GO

-----

IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='EliminarMovimientoDiario')
BEGIN
    DROP PROCEDURE EliminarMovimientoDiario;
END
GO
CREATE PROCEDURE EliminarMovimientoDiario
    @IdMovimiento INT
AS
BEGIN
    DELETE FROM MovimientoDiario WHERE IdMovimiento = @IdMovimiento;
END
GO

-------
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='ActualizarMovimientoDiario')
BEGIN
    DROP PROCEDURE ActualizarMovimientoDiario;
END
GO
CREATE PROCEDURE ActualizarMovimientoDiario
    @IdMovimiento INT,
    @TipoMovimiento VARCHAR(20),
    @Categoria VARCHAR(50),
    @Monto DECIMAL(10,2),
    @Descripcion NVARCHAR(255)
AS
BEGIN
    UPDATE MovimientoDiario
    SET TipoMovimiento = @TipoMovimiento,
        Categoria = @Categoria,
        Monto = @Monto,
        Descripcion = @Descripcion
    WHERE IdMovimiento = @IdMovimiento;
END
GO




------- CIERRE DE CAJA

-- Crear tabla CierreCaja si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CierreCaja' AND xtype='U')
BEGIN
    CREATE TABLE CierreCaja (
        IdMovimiento INT PRIMARY KEY IDENTITY(1,1),
        FechaHora DATETIME NOT NULL DEFAULT GETDATE(),
        TotalIngresos DECIMAL(10,2) NOT NULL,
        TotalEgresos DECIMAL(10,2) NOT NULL,
        MontoFisico DECIMAL(10,2),
        NotaJustificativa NVARCHAR(255),
        NombreUsuario NVARCHAR(100)
    );
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_CierreCajaDiario')
BEGIN
    DROP PROCEDURE sp_CierreCajaDiario;
END
GO
CREATE PROCEDURE sp_CierreCajaDiario
AS
BEGIN
    SELECT
        SUM(CASE WHEN TipoMovimiento = 'Ingreso' THEN Monto ELSE 0 END) AS TotalIngresos,
        SUM(CASE WHEN TipoMovimiento = 'Egreso' THEN Monto ELSE 0 END) AS TotalEgresos
    FROM MovimientoDiario
    WHERE CAST(FechaHora AS DATE) = CAST(GETDATE() AS DATE)
END
GO

---------
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ListarCierresDeCaja')
BEGIN
    DROP PROCEDURE sp_ListarCierresDeCaja;
END
GO
CREATE PROCEDURE sp_ListarCierresDeCaja
AS
BEGIN
    SELECT
        IdMovimiento,
        FechaHora,
        TotalIngresos,
        TotalEgresos,
        MontoFisico,
        NotaJustificativa,
        NombreUsuario
    FROM CierreCaja
    ORDER BY FechaHora DESC
END
GO

-- ====================================================
-- STORED PROCEDURES PARA BÚSQUEDA Y FILTRADO DE VENTAS
-- ====================================================

-- Procedimiento para buscar ventas con filtros
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='BuscarVentas')
BEGIN
    DROP PROCEDURE BuscarVentas;
END
GO
CREATE PROCEDURE BuscarVentas
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL,
    @MetodoPago VARCHAR(50) = NULL,
    @VentaId INT = NULL
AS
BEGIN
    SELECT 
        v.Id,
        v.Fecha,
        v.MetodoPago,
        SUM(dv.Cantidad * dv.PrecioUnitario) AS Total,
        SUM(dv.Cantidad) AS CantidadProductos
    FROM Venta v
    INNER JOIN DetalleVenta dv ON v.Id = dv.VentaId
    WHERE 
        (@FechaInicio IS NULL OR v.Fecha >= @FechaInicio)
        AND (@FechaFin IS NULL OR v.Fecha <= @FechaFin)
        AND (@MetodoPago IS NULL OR v.MetodoPago = @MetodoPago)
        AND (@VentaId IS NULL OR v.Id = @VentaId)
    GROUP BY v.Id, v.Fecha, v.MetodoPago
    ORDER BY v.Fecha DESC, v.Id DESC
END
GO

-- Procedimiento para obtener detalle completo de una venta
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='ObtenerDetalleVenta')
BEGIN
    DROP PROCEDURE ObtenerDetalleVenta;
END
GO
CREATE PROCEDURE ObtenerDetalleVenta
    @VentaId INT
AS
BEGIN
    SELECT 
        v.Id AS VentaId,
        v.Fecha,
        v.MetodoPago,
        SUM(dv.Cantidad * dv.PrecioUnitario) OVER(PARTITION BY v.Id) AS TotalVenta,
        dv.ProductoId,
        p.Nombre AS ProductoNombre,
        p.Codigo AS ProductoCodigo,
        dv.Cantidad,
        dv.PrecioUnitario,
        (dv.Cantidad * dv.PrecioUnitario) AS Subtotal
    FROM Venta v
    INNER JOIN DetalleVenta dv ON v.Id = dv.VentaId
    INNER JOIN Producto p ON dv.ProductoId = p.IdProducto
    WHERE v.Id = @VentaId
    ORDER BY dv.ProductoId
END
GO

-- Procedimiento para reporte de ventas por día
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='ReporteVentasDia')
BEGIN
    DROP PROCEDURE ReporteVentasDia;
END
GO
CREATE PROCEDURE ReporteVentasDia
    @Fecha DATE
AS
BEGIN
    -- Totales del día
    SELECT 
        COUNT(DISTINCT v.Id) AS TotalVentas,
        SUM(dv.Cantidad * dv.PrecioUnitario) AS MontoTotal
    FROM Venta v
    INNER JOIN DetalleVenta dv ON v.Id = dv.VentaId
    WHERE CAST(v.Fecha AS DATE) = @Fecha;

    -- Ventas por método de pago
    SELECT 
        v.MetodoPago,
        COUNT(DISTINCT v.Id) AS CantidadVentas,
        SUM(dv.Cantidad * dv.PrecioUnitario) AS MontoTotal
    FROM Venta v
    INNER JOIN DetalleVenta dv ON v.Id = dv.VentaId
    WHERE CAST(v.Fecha AS DATE) = @Fecha
    GROUP BY v.MetodoPago
    ORDER BY MontoTotal DESC;

    -- Productos vendidos
    SELECT 
        p.Codigo AS ProductoCodigo,
        p.Nombre AS ProductoNombre,
        SUM(dv.Cantidad) AS CantidadVendida,
        SUM(dv.Cantidad * dv.PrecioUnitario) AS MontoTotal
    FROM Venta v
    INNER JOIN DetalleVenta dv ON v.Id = dv.VentaId
    INNER JOIN Producto p ON dv.ProductoId = p.IdProducto
    WHERE CAST(v.Fecha AS DATE) = @Fecha
    GROUP BY p.Codigo, p.Nombre
    ORDER BY CantidadVendida DESC;
END
GO

-- ====================================================
-- TABLAS Y PROCEDIMIENTOS PARA DEVOLUCIONES Y REEMBOLSOS
-- ====================================================

-- Tabla para registrar devoluciones
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Devolucion' AND xtype='U')
BEGIN
    CREATE TABLE Devolucion (
        Id INT PRIMARY KEY IDENTITY(1,1),
        VentaOriginalId INT NOT NULL,
        Fecha DATETIME NOT NULL DEFAULT GETDATE(),
        TipoDevolucion VARCHAR(20) NOT NULL, -- 'Total', 'Parcial'
        TipoReembolso VARCHAR(20) NOT NULL, -- 'Efectivo', 'Tarjeta', 'Credito'
        MetodoPagoOriginal VARCHAR(50) NOT NULL,
        MontoTotal DECIMAL(10,2) NOT NULL,
        MontoDevuelto DECIMAL(10,2) NOT NULL,
        Motivo VARCHAR(500),
        Estado VARCHAR(20) NOT NULL DEFAULT 'Procesada', -- 'Procesada', 'Cancelada'
        UsuarioId INT, -- Para futuras implementaciones
        NumeroComprobante VARCHAR(50),
        FOREIGN KEY (VentaOriginalId) REFERENCES Venta(Id)
    );
END
GO

-- Tabla para detalles de productos devueltos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DetalleDevolucion' AND xtype='U')
BEGIN
    CREATE TABLE DetalleDevolucion (
        Id INT PRIMARY KEY IDENTITY(1,1),
        DevolucionId INT NOT NULL,
        ProductoId INT NOT NULL,
        CantidadDevuelta INT NOT NULL,
        PrecioUnitario DECIMAL(10,2) NOT NULL,
        SubtotalDevolucion DECIMAL(10,2) NOT NULL,
        FOREIGN KEY (DevolucionId) REFERENCES Devolucion(Id),
        FOREIGN KEY (ProductoId) REFERENCES Producto(IdProducto)
    );
END
GO

-- Tabla para créditos en cuenta de cliente
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CreditoCliente' AND xtype='U')
BEGIN
    CREATE TABLE CreditoCliente (
        Id INT PRIMARY KEY IDENTITY(1,1),
        DevolucionId INT NOT NULL,
        NumeroIdentificacion VARCHAR(50) NOT NULL, -- Cédula o identificación del cliente
        NombreCliente VARCHAR(200) NOT NULL,
        MontoCredito DECIMAL(10,2) NOT NULL,
        MontoUtilizado DECIMAL(10,2) NOT NULL DEFAULT 0.00,
        SaldoDisponible AS (MontoCredito - MontoUtilizado),
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaVencimiento DATETIME NOT NULL, -- Los créditos pueden vencer
        Estado VARCHAR(20) NOT NULL DEFAULT 'Activo', -- 'Activo', 'Utilizado', 'Vencido'
        FOREIGN KEY (DevolucionId) REFERENCES Devolucion(Id)
    );
END
GO

-- Stored Procedure para procesar devolución total
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='ProcesarDevolucionTotal')
BEGIN
    DROP PROCEDURE ProcesarDevolucionTotal;
END
GO

CREATE PROCEDURE ProcesarDevolucionTotal
    @VentaId INT,
    @TipoReembolso VARCHAR(20), -- 'Efectivo', 'Tarjeta', 'Credito'
    @Motivo VARCHAR(500) = NULL,
    @NumeroIdentificacion VARCHAR(50) = NULL, -- Solo para crédito
    @NombreCliente VARCHAR(200) = NULL, -- Solo para crédito
    @DiasVencimientoCredito INT = 90 -- Por defecto 90 días
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Verificar que la venta existe
        DECLARE @MetodoPagoOriginal VARCHAR(50);
        DECLARE @MontoTotal DECIMAL(10,2);
        
        SELECT 
            @MetodoPagoOriginal = v.MetodoPago,
            @MontoTotal = SUM(dv.Cantidad * dv.PrecioUnitario)
        FROM Venta v
        INNER JOIN DetalleVenta dv ON v.Id = dv.VentaId
        WHERE v.Id = @VentaId
        GROUP BY v.MetodoPago;
        
        IF @MetodoPagoOriginal IS NULL
        BEGIN
            RAISERROR('La venta especificada no existe', 16, 1);
            RETURN;
        END
        
        -- Verificar que no se haya devuelto ya
        IF EXISTS (SELECT 1 FROM Devolucion WHERE VentaOriginalId = @VentaId AND Estado = 'Procesada')
        BEGIN
            RAISERROR('Esta venta ya ha sido devuelta anteriormente', 16, 1);
            RETURN;
        END
        
        -- Generar número de comprobante
        DECLARE @NumeroComprobante VARCHAR(50) = 'DEV' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + CAST(@VentaId AS VARCHAR(10));
        
        -- Registrar la devolución
        INSERT INTO Devolucion (VentaOriginalId, TipoDevolucion, TipoReembolso, MetodoPagoOriginal, 
                               MontoTotal, MontoDevuelto, Motivo, NumeroComprobante)
        VALUES (@VentaId, 'Total', @TipoReembolso, @MetodoPagoOriginal, @MontoTotal, @MontoTotal, @Motivo, @NumeroComprobante);
        
        DECLARE @DevolucionId INT = SCOPE_IDENTITY();
        
        -- Registrar detalles de productos devueltos
        INSERT INTO DetalleDevolucion (DevolucionId, ProductoId, CantidadDevuelta, PrecioUnitario, SubtotalDevolucion)
        SELECT 
            @DevolucionId,
            dv.ProductoId,
            dv.Cantidad,
            dv.PrecioUnitario,
            dv.Cantidad * dv.PrecioUnitario
        FROM DetalleVenta dv
        WHERE dv.VentaId = @VentaId;
        
        -- Restaurar inventario
        UPDATE p
        SET p.Existencias = p.Existencias + dv.Cantidad
        FROM Producto p
        INNER JOIN DetalleVenta dv ON p.IdProducto = dv.ProductoId
        WHERE dv.VentaId = @VentaId;
        
        -- Si es crédito, crear registro de crédito
        IF @TipoReembolso = 'Credito'
        BEGIN
            IF @NumeroIdentificacion IS NULL OR @NombreCliente IS NULL
            BEGIN
                RAISERROR('Para crédito en cuenta se requiere número de identificación y nombre del cliente', 16, 1);
                RETURN;
            END
            
            INSERT INTO CreditoCliente (DevolucionId, NumeroIdentificacion, NombreCliente, MontoCredito, FechaVencimiento)
            VALUES (@DevolucionId, @NumeroIdentificacion, @NombreCliente, @MontoTotal, 
                   DATEADD(DAY, @DiasVencimientoCredito, GETDATE()));
        END
        
        -- Devolver información de la devolución procesada
        SELECT 
            d.Id,
            d.NumeroComprobante,
            d.MontoDevuelto,
            d.TipoReembolso,
            d.Fecha,
            CASE WHEN d.TipoReembolso = 'Credito' THEN cc.Id ELSE NULL END AS CreditoId
        FROM Devolucion d
        LEFT JOIN CreditoCliente cc ON d.Id = cc.DevolucionId
        WHERE d.Id = @DevolucionId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Tipo de tabla para productos a devolver en devolución parcial
IF TYPE_ID(N'TipoProductosDevolucion') IS NULL
BEGIN
    CREATE TYPE TipoProductosDevolucion AS TABLE
    (
        ProductoId INT,
        CantidadDevolver INT
    );
END
GO

-- Stored Procedure para procesar devolución parcial
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='ProcesarDevolucionParcial')
BEGIN
    DROP PROCEDURE ProcesarDevolucionParcial;
END
GO

CREATE PROCEDURE ProcesarDevolucionParcial
    @VentaId INT,
    @ProductosDevolver TipoProductosDevolucion READONLY,
    @TipoReembolso VARCHAR(20), -- 'Efectivo', 'Tarjeta', 'Credito'
    @Motivo VARCHAR(500) = NULL,
    @NumeroIdentificacion VARCHAR(50) = NULL,
    @NombreCliente VARCHAR(200) = NULL,
    @DiasVencimientoCredito INT = 90
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Verificar que la venta existe
        DECLARE @MetodoPagoOriginal VARCHAR(50);
        DECLARE @MontoTotalVenta DECIMAL(10,2);
        
        SELECT 
            @MetodoPagoOriginal = v.MetodoPago,
            @MontoTotalVenta = SUM(dv.Cantidad * dv.PrecioUnitario)
        FROM Venta v
        INNER JOIN DetalleVenta dv ON v.Id = dv.VentaId
        WHERE v.Id = @VentaId
        GROUP BY v.MetodoPago;
        
        IF @MetodoPagoOriginal IS NULL
        BEGIN
            RAISERROR('La venta especificada no existe', 16, 1);
            RETURN;
        END
        
        -- Calcular monto a devolver
        DECLARE @MontoDevolver DECIMAL(10,2);
        SELECT @MontoDevolver = SUM(pd.CantidadDevolver * dv.PrecioUnitario)
        FROM @ProductosDevolver pd
        INNER JOIN DetalleVenta dv ON pd.ProductoId = dv.ProductoId AND dv.VentaId = @VentaId;
        
        -- Validar que no se devuelvan más productos de los vendidos
        IF EXISTS (
            SELECT 1 
            FROM @ProductosDevolver pd
            INNER JOIN DetalleVenta dv ON pd.ProductoId = dv.ProductoId AND dv.VentaId = @VentaId
            WHERE pd.CantidadDevolver > dv.Cantidad
        )
        BEGIN
            RAISERROR('No se puede devolver más cantidad de la que se vendió originalmente', 16, 1);
            RETURN;
        END
        
        -- Generar número de comprobante
        DECLARE @NumeroComprobante VARCHAR(50) = 'DEV-P' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + CAST(@VentaId AS VARCHAR(10));
        
        -- Registrar la devolución
        INSERT INTO Devolucion (VentaOriginalId, TipoDevolucion, TipoReembolso, MetodoPagoOriginal, 
                               MontoTotal, MontoDevuelto, Motivo, NumeroComprobante)
        VALUES (@VentaId, 'Parcial', @TipoReembolso, @MetodoPagoOriginal, @MontoTotalVenta, @MontoDevolver, @Motivo, @NumeroComprobante);
        
        DECLARE @DevolucionId INT = SCOPE_IDENTITY();
        
        -- Registrar detalles de productos devueltos
        INSERT INTO DetalleDevolucion (DevolucionId, ProductoId, CantidadDevuelta, PrecioUnitario, SubtotalDevolucion)
        SELECT 
            @DevolucionId,
            pd.ProductoId,
            pd.CantidadDevolver,
            dv.PrecioUnitario,
            pd.CantidadDevolver * dv.PrecioUnitario
        FROM @ProductosDevolver pd
        INNER JOIN DetalleVenta dv ON pd.ProductoId = dv.ProductoId AND dv.VentaId = @VentaId;
        
        -- Restaurar inventario
        UPDATE p
        SET p.Existencias = p.Existencias + pd.CantidadDevolver
        FROM Producto p
        INNER JOIN @ProductosDevolver pd ON p.IdProducto = pd.ProductoId;
        
        -- Si es crédito, crear registro de crédito
        IF @TipoReembolso = 'Credito'
        BEGIN
            IF @NumeroIdentificacion IS NULL OR @NombreCliente IS NULL
            BEGIN
                RAISERROR('Para crédito en cuenta se requiere número de identificación y nombre del cliente', 16, 1);
                RETURN;
            END
            
            INSERT INTO CreditoCliente (DevolucionId, NumeroIdentificacion, NombreCliente, MontoCredito, FechaVencimiento)
            VALUES (@DevolucionId, @NumeroIdentificacion, @NombreCliente, @MontoDevolver, 
                   DATEADD(DAY, @DiasVencimientoCredito, GETDATE()));
        END
        
        -- Devolver información de la devolución procesada
        SELECT 
            d.Id,
            d.NumeroComprobante,
            d.MontoDevuelto,
            d.TipoReembolso,
            d.Fecha,
            CASE WHEN d.TipoReembolso = 'Credito' THEN cc.Id ELSE NULL END AS CreditoId
        FROM Devolucion d
        LEFT JOIN CreditoCliente cc ON d.Id = cc.DevolucionId
        WHERE d.Id = @DevolucionId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Stored Procedure para buscar créditos de cliente
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='BuscarCreditosCliente')
BEGIN
    DROP PROCEDURE BuscarCreditosCliente;
END
GO

CREATE PROCEDURE BuscarCreditosCliente
    @NumeroIdentificacion VARCHAR(50)
AS
BEGIN
    SELECT 
        cc.Id,
        cc.NumeroIdentificacion,
        cc.NombreCliente,
        cc.MontoCredito,
        cc.MontoUtilizado,
        cc.SaldoDisponible,
        cc.FechaCreacion,
        cc.FechaVencimiento,
        cc.Estado,
        d.NumeroComprobante AS ComprobanteDevolucion,
        d.VentaOriginalId
    FROM CreditoCliente cc
    INNER JOIN Devolucion d ON cc.DevolucionId = d.Id
    WHERE cc.NumeroIdentificacion = @NumeroIdentificacion
      AND cc.Estado = 'Activo'
      AND cc.SaldoDisponible > 0
      AND cc.FechaVencimiento > GETDATE()
    ORDER BY cc.FechaCreacion DESC;
END
GO

-- Stored Procedure para aplicar crédito a una venta
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='AplicarCreditoAVenta')
BEGIN
    DROP PROCEDURE AplicarCreditoAVenta;
END
GO

CREATE PROCEDURE AplicarCreditoAVenta
    @CreditoId INT,
    @VentaId INT,
    @MontoAplicar DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Verificar que el crédito existe y está disponible
        DECLARE @SaldoDisponible DECIMAL(10,2);
        SELECT @SaldoDisponible = SaldoDisponible
        FROM CreditoCliente
        WHERE Id = @CreditoId AND Estado = 'Activo' AND FechaVencimiento > GETDATE();
        
        IF @SaldoDisponible IS NULL
        BEGIN
            RAISERROR('El crédito especificado no existe o no está disponible', 16, 1);
            RETURN;
        END
        
        IF @MontoAplicar > @SaldoDisponible
        BEGIN
            RAISERROR('El monto a aplicar excede el saldo disponible del crédito', 16, 1);
            RETURN;
        END
        
        -- Actualizar el crédito
        UPDATE CreditoCliente
        SET MontoUtilizado = MontoUtilizado + @MontoAplicar,
            Estado = CASE WHEN (MontoCredito - (MontoUtilizado + @MontoAplicar)) <= 0 THEN 'Utilizado' ELSE 'Activo' END
        WHERE Id = @CreditoId;
        
        -- Aquí se podría registrar la aplicación del crédito a la venta
        -- Por ahora solo devolvemos confirmación
        SELECT 
            'Crédito aplicado exitosamente' AS Mensaje,
            @MontoAplicar AS MontoAplicado,
            (SELECT SaldoDisponible FROM CreditoCliente WHERE Id = @CreditoId) AS SaldoRestante;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Stored Procedure para consultar historial de devoluciones
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='ConsultarHistorialDevoluciones')
BEGIN
    DROP PROCEDURE ConsultarHistorialDevoluciones;
END
GO

CREATE PROCEDURE ConsultarHistorialDevoluciones
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL,
    @TipoDevolucion VARCHAR(20) = NULL,
    @TipoReembolso VARCHAR(20) = NULL
AS
BEGIN
    SELECT 
        d.Id,
        d.NumeroComprobante,
        d.VentaOriginalId,
        d.Fecha,
        d.TipoDevolucion,
        d.TipoReembolso,
        d.MetodoPagoOriginal,
        d.MontoTotal,
        d.MontoDevuelto,
        d.Motivo,
        d.Estado,
        v.Fecha AS FechaVentaOriginal,
        COUNT(dd.Id) AS CantidadProductosDevueltos,
        CASE WHEN d.TipoReembolso = 'Credito' THEN cc.NombreCliente ELSE NULL END AS ClienteCredito,
        CASE WHEN d.TipoReembolso = 'Credito' THEN cc.NumeroIdentificacion ELSE NULL END AS IdentificacionCliente
    FROM Devolucion d
    INNER JOIN Venta v ON d.VentaOriginalId = v.Id
    LEFT JOIN DetalleDevolucion dd ON d.Id = dd.DevolucionId
    LEFT JOIN CreditoCliente cc ON d.Id = cc.DevolucionId
    WHERE 
        (@FechaInicio IS NULL OR d.Fecha >= @FechaInicio)
        AND (@FechaFin IS NULL OR d.Fecha <= @FechaFin)
        AND (@TipoDevolucion IS NULL OR d.TipoDevolucion = @TipoDevolucion)
        AND (@TipoReembolso IS NULL OR d.TipoReembolso = @TipoReembolso)
    GROUP BY d.Id, d.NumeroComprobante, d.VentaOriginalId, d.Fecha, d.TipoDevolucion, 
             d.TipoReembolso, d.MetodoPagoOriginal, d.MontoTotal, d.MontoDevuelto, 
             d.Motivo, d.Estado, v.Fecha, cc.NombreCliente, cc.NumeroIdentificacion
    ORDER BY d.Fecha DESC;
END
GO

-- TABLA DE FACTURAS ELECTRONICAS
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FacturaElectronica' AND xtype='U')
BEGIN
    CREATE TABLE FacturaElectronica (
        IdFactura INT PRIMARY KEY IDENTITY(1,1),
        VentaId INT NOT NULL,
        NumeroFactura VARCHAR(50) NOT NULL,
        ClaveNumerica VARCHAR(50) UNIQUE NOT NULL,
        ClienteNombre VARCHAR(200) NOT NULL,
        ClienteEmail VARCHAR(200) NULL,
        ClienteTelefono VARCHAR(20) NULL,
        FechaEmision DATETIME NOT NULL DEFAULT GETDATE(),
        SubTotal DECIMAL(18,2) NOT NULL,
        MontoImpuesto DECIMAL(18,2) NOT NULL,
        MontoTotal DECIMAL(18,2) NOT NULL,
        EstadoHacienda VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
        PDFGenerado BIT NOT NULL DEFAULT 0,
        EmailEnviado BIT NOT NULL DEFAULT 0,
        FechaGeneracionPDF DATETIME NULL,
        FechaEnvioEmail DATETIME NULL,
        FOREIGN KEY (VentaId) REFERENCES Venta(Id)
    );
END
GO

-- TABLA DE HISTORIAL DE ENVIO DE FACTURAS
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HistorialEnvioFacturas' AND xtype='U')
BEGIN
    CREATE TABLE HistorialEnvioFacturas (
        Id INT PRIMARY KEY IDENTITY(1,1),
        IdFactura INT NOT NULL,
        TipoEnvio VARCHAR(20) NOT NULL, -- 'Email', 'SMS', etc.
        Destinatario VARCHAR(200) NOT NULL,
        EstadoEnvio VARCHAR(20) NOT NULL, -- 'Enviado', 'Error', 'Pendiente'
        MensajeRespuesta TEXT NULL,
        FechaEnvio DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (IdFactura) REFERENCES FacturaElectronica(IdFactura)
    );
END
GO

-- ACTUALIZAR PROCEDIMIENTO PARA GENERAR FACTURA ELECTRONICA
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GenerarFacturaElectronica')
    DROP PROCEDURE GenerarFacturaElectronica
GO

CREATE PROCEDURE GenerarFacturaElectronica
    @VentaId INT,
    @ClienteNombre VARCHAR(200),
    @ClienteEmail VARCHAR(200) = NULL,
    @ClienteTelefono VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @IdFactura INT
    DECLARE @NumeroFactura VARCHAR(50)
    DECLARE @ClaveNumerica VARCHAR(50)
    DECLARE @SubTotal DECIMAL(18,2)
    DECLARE @MontoImpuesto DECIMAL(18,2)
    DECLARE @MontoTotal DECIMAL(18,2)
    DECLARE @Intentos INT = 0
    DECLARE @MaxIntentos INT = 10
    
    -- Obtener datos de la venta
    SELECT 
        @SubTotal = v.SubTotal,
        @MontoImpuesto = v.MontoImpuesto,
        @MontoTotal = v.MontoTotal
    FROM Venta v
    WHERE v.Id = @VentaId
    
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Venta no encontrada', 16, 1)
        RETURN
    END
    
    -- Generar número de factura secuencial
    DECLARE @Contador INT
    SELECT @Contador = ISNULL(MAX(CAST(SUBSTRING(NumeroFactura, 4, LEN(NumeroFactura)-3) AS INT)), 0) + 1
    FROM FacturaElectronica
    WHERE NumeroFactura LIKE 'FAC%'
    
    SET @NumeroFactura = 'FAC' + RIGHT('000000' + CAST(@Contador AS VARCHAR), 6)
    
    -- Generar clave numérica única con reintentos
    WHILE @Intentos < @MaxIntentos
    BEGIN
        BEGIN TRY
            -- Generar clave numérica con GUID para garantizar unicidad
            SET @ClaveNumerica = 
                FORMAT(GETDATE(), 'ddMMyyyy') + 
                FORMAT(GETDATE(), 'HHmmss') + 
                RIGHT('000' + CAST(ABS(CHECKSUM(NEWID()) % 1000) AS VARCHAR), 3) +
                RIGHT('00000' + CAST(ABS(CHECKSUM(NEWID()) % 100000) AS VARCHAR), 5)
            
            -- Insertar la factura
            INSERT INTO FacturaElectronica (
                VentaId, NumeroFactura, ClaveNumerica, ClienteNombre, 
                ClienteEmail, ClienteTelefono, FechaEmision, 
                SubTotal, MontoImpuesto, MontoTotal, EstadoHacienda
            )
            VALUES (
                @VentaId, @NumeroFactura, @ClaveNumerica, @ClienteNombre,
                @ClienteEmail, @ClienteTelefono, GETDATE(),
                @SubTotal, @MontoImpuesto, @MontoTotal, 'Generada'
            )
            
            SET @IdFactura = SCOPE_IDENTITY()
            
            -- Si llegamos aquí, la inserción fue exitosa
            BREAK
            
        END TRY
        BEGIN CATCH
            -- Si es error de clave duplicada, intentar de nuevo
            IF ERROR_NUMBER() = 2627 -- Violation of UNIQUE KEY constraint
            BEGIN
                SET @Intentos = @Intentos + 1
                WAITFOR DELAY '00:00:01' -- Esperar 1 segundo antes del siguiente intento
                CONTINUE
            END
            ELSE
            BEGIN
                -- Si es otro tipo de error, lanzarlo
                THROW
            END
        END CATCH
    END
    
    IF @Intentos >= @MaxIntentos
    BEGIN
        RAISERROR('No se pudo generar una clave numérica única después de varios intentos', 16, 1)
        RETURN
    END
    
    -- Devolver los datos de la factura generada
    SELECT 
        IdFactura,
        NumeroFactura,
        ClaveNumerica,
        ClienteNombre,
        ClienteEmail,
        ClienteTelefono,
        FechaEmision,
        SubTotal,
        MontoImpuesto,
        MontoTotal,
        EstadoHacienda
    FROM FacturaElectronica
    WHERE IdFactura = @IdFactura
END
GO
