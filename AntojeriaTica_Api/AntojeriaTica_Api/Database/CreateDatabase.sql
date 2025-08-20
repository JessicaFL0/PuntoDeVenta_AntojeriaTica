-- =============================================
-- Base de datos AntojeriaTica - Script Consolidado
-- Incluye todas las funcionalidades: básica + facturación electrónica + pedidos
-- =============================================

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AntojeriaTica')
BEGIN
    CREATE DATABASE AntojeriaTica;
    PRINT 'Base de datos AntojeriaTica creada';
END
ELSE
BEGIN
    PRINT 'Base de datos AntojeriaTica ya existe';
END
GO

-- Asegurar contexto de base de datos desde el inicio
IF DB_ID('AntojeriaTica') IS NOT NULL
BEGIN
    USE AntojeriaTica;
END
GO

-- =============================================
-- TODOs y notas de mantenimiento (Ejemplos)
-- =============================================
-- Este bloque solo contiene comentarios con ejemplos de TODO para guiar trabajos futuros.
-- No impacta la ejecución del script.

/*
Formato sugerido de TODO:
TODO [CATEGORIA][ID opcional]: Título corto y claro
- Contexto: breve explicación del porqué.
- Acción/Enfoque: pasos propuestos o decisión pendiente.
- Riesgos/Dependencias: si aplica.
- Responsable/ETA: si aplica.
*/

-- TODO [DB-PERF][PERF-001]: Índices para acelerar reportes
-- - Contexto: Consultas frecuentes por fecha y joins de ventas/detalles.
-- - Acción: Crear índices no-clustered en Venta(Fecha), DetalleVenta(VentaId), Producto(Codigo) INCLUDE(Nombre,Precio).
--   Ejemplos:
--   CREATE INDEX IX_Venta_Fecha ON Venta(Fecha);
--   CREATE INDEX IX_DetalleVenta_VentaId ON DetalleVenta(VentaId);
--   CREATE INDEX IX_Producto_Codigo ON Producto(Codigo) INCLUDE (Nombre, Precio);

-- TODO [DB-SEC][SEC-001]: Endurecer seguridad de base de datos
-- - Contexto: Separar permisos por roles de app (lectura/escritura/reportes).
-- - Acción: Crear esquemas/roles (app_reader, app_writer, app_report) y aplicar GRANT mínimos a SPs.
-- - Revisar mascarado/datos sensibles (clientes) y políticas de acceso.

-- TODO [DB-OBS][OBS-001]: Telemetría y auditoría
-- - Contexto: Necesitamos trazabilidad de ejecuciones de SPs críticos.
-- - Acción: Tabla de auditoría (ExecLog) + capturar parámetros/tiempos/errores; activar XE o Query Store para SPs clave.

-- TODO [DB-CLEANUP][CLN-001]: Archivado histórico
-- - Contexto: Tablas de ventas crecerán rápidamente.
-- - Acción: Estrategia de particionamiento por Fecha o tablas *_Archive con jobs mensuales.

-- TODO [DB-CI][CI-001]: Versionado de esquema
-- - Contexto: Necesitamos control de versiones e idempotencia en despliegues.
-- - Acción: Tabla __SchemaVersion y convención de scripts (Up/Down); integrar en pipeline CI/CD.

-- TODO [DB-API-CONTRACT][API-001]: Contratos SP ↔ modelos
-- - Contexto: Validar que columnas devueltas por SPs coincidan con modelos de la Web/API.
-- - Acción: Documentar contratos en comentarios sobre cada SP y añadir pruebas de integración básicas.

-- TODO [DB-TEST][TST-001]: Dataset de prueba y validaciones
-- - Contexto: Validar reportes (diarios/anuales) y cierres de caja.
-- - Acción: Script de seed adicional con diferentes métodos de pago/fechas y consultas de verificación.

-- TODO [DB-LOCALE][LOC-001]: Configuración regional/zonas horarias
-- - Contexto: Formateos de mes/nombre de día y TZ.
-- - Acción: Normalizar a UTC en base y presentar en app con TZ; o usar AT TIME ZONE 'Central America Standard Time' cuando aplique.

-- TODO [DB-BACKUP][BKP-001]: Plan de respaldo y restauración
-- - Contexto: Definir RPO/RTO.
-- - Acción: Backups diferenciales/diarios, verificación RESTORE WITH VERIFYONLY y procedimiento de DR.

-- TODO [DB-MAINT][MTN-001]: Mantenimiento de índices y estadísticas
-- - Contexto: Evitar degradación de rendimiento.
-- - Acción: Jobs para reorganize/rebuild y UPDATE STATISTICS según fragmentación/tamaño.

-- TODO [DB-TXN][TXN-001]: Consistencia transaccional
-- - Contexto: SPs con múltiples writes.
-- - Acción: Asegurar TRY/CATCH + TRAN + SET XACT_ABORT ON en SPs críticos.

-- TODO [DB-FE][FE-001]: Facturación electrónica avanzada
-- - Contexto: Integración con Hacienda y colas de envío.
-- - Acción: Tabla de colas (EmailQueue/FEQueue), reintentos y DLQ; validar estados con timeouts.

-- TODO [DB-MONITOR][MON-001]: Monitoreo de salud
-- - Contexto: Alertas proactivas.
-- - Acción: Métricas de bloqueo/lentitud, espacio en disco, tiempos de SPs; exponer a dashboard.

-- TODO [DB-ANON][ANON-001]: Anonimización para entornos de prueba
-- - Contexto: Cumplimiento y privacidad.
-- - Acción: Rutina de scramble para datos de clientes al refrescar ambientes no productivos.

-- TODO [DB-ENV][ENV-001]: Parámetros por entorno
-- - Contexto: Diferencias Dev/QA/Prod (porcentajes, seeds, flags).
-- - Acción: Variables/parametrización y toggles en script o uso de DACPAC con perfiles.

-- TODO [DB-NAMING][STD-001]: Convenciones de nombres
-- - Contexto: Homogeneidad de objetos.
-- - Acción: Documentar prefijos de SP (sp_), índices (IX_), FKs (FK_Tabla_Ref) y aplicar en objetos nuevos.

-- TODO [DB-REPORT][RPT-001]: Mejoras de reportes
-- - Contexto: Dashboard y ventas anuales.
-- - Acción: SPs con filtros por rango/usuario, cacheo temporal y vistas materializadas si procede.

/*
Plantilla rápida para nuevos TODOs
---------------------------------
TODO [CATEGORIA][ID]: <Resumen>
- Contexto:
- Acción:
- Dependencias:
- Responsable/ETA:
*/

-- =============================================
-- DATOS DE EJEMPLO PARA PRUEBAS (SOLO DEV)
-- (Se ejecutará solo si las tablas ya existen)
-- =============================================
IF DB_ID('AntojeriaTica') IS NOT NULL
AND OBJECT_ID('dbo.Usuario','U') IS NOT NULL
AND OBJECT_ID('dbo.Producto','U') IS NOT NULL
AND OBJECT_ID('dbo.Venta','U') IS NOT NULL
AND OBJECT_ID('dbo.DetalleVenta','U') IS NOT NULL
AND OBJECT_ID('dbo.MovimientoDiario','U') IS NOT NULL
AND OBJECT_ID('dbo.CierreCaja','U') IS NOT NULL
BEGIN
-- Notas:
-- - Inserciones idempotentes: solo se agregan si no existen datos para la fecha objetivo.
-- - Crea ventas y detalles para hoy y días recientes, movimientos de caja del día, y un cierre de caja de ayer.

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @UsuarioPepe INT = (SELECT TOP 1 Id FROM Usuario WHERE Email = 'pepe@pepe');
    IF @UsuarioPepe IS NULL
        SET @UsuarioPepe = (SELECT TOP 1 Id FROM Usuario ORDER BY Id);

    DECLARE @ProdP001 INT = (SELECT TOP 1 Id FROM Producto WHERE Codigo = 'P001');
    DECLARE @ProdP002 INT = (SELECT TOP 1 Id FROM Producto WHERE Codigo = 'P002');
    DECLARE @ProdP003 INT = (SELECT TOP 1 Id FROM Producto WHERE Codigo = 'P003');
    DECLARE @ProdP004 INT = (SELECT TOP 1 Id FROM Producto WHERE Codigo = 'P004');

    -- Ventas de HOY si no hay ninguna aún
    IF NOT EXISTS (SELECT 1 FROM Venta WHERE CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE))
    BEGIN
        DECLARE @v1 INT, @v2 INT;

        INSERT INTO Venta (Fecha, UsuarioId, Cliente, Subtotal, Impuesto, Descuento, Total, MetodoPago, Estado, Observaciones)
        VALUES (GETDATE(), @UsuarioPepe, N'Cliente mostrador', 0, 0, 0, 0, N'Efectivo', N'Completada', N'Venta demo Efectivo');
        SET @v1 = SCOPE_IDENTITY();

        IF @ProdP001 IS NOT NULL
        BEGIN
            DECLARE @p1 DECIMAL(10,2) = (SELECT Precio FROM Producto WHERE Id=@ProdP001);
            INSERT INTO DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal)
            VALUES (@v1, @ProdP001, 2, @p1, 0, 0, 2*@p1);
        END
        IF @ProdP003 IS NOT NULL
        BEGIN
            DECLARE @p3 DECIMAL(10,2) = (SELECT Precio FROM Producto WHERE Id=@ProdP003);
            INSERT INTO DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal)
            VALUES (@v1, @ProdP003, 1, @p3, 0, 0, 1*@p3);
        END

        -- Recalcular totales de @v1
        UPDATE v SET 
            Subtotal = x.Subtotal,
            Impuesto = x.Impuesto,
            Total = x.Subtotal + x.Impuesto
        FROM Venta v
        CROSS APPLY (
            SELECT 
                SUM(dv.Cantidad*dv.PrecioUnitario) AS Subtotal,
                SUM(CASE WHEN ISNULL(p.Gravado,1)=1 THEN dv.Cantidad*dv.PrecioUnitario*0.13 ELSE 0 END) AS Impuesto
            FROM DetalleVenta dv
            LEFT JOIN Producto p ON p.Id = dv.ProductoId
            WHERE dv.VentaId = v.Id
        ) x
        WHERE v.Id = @v1;

        -- Segunda venta de hoy con Tarjeta
        INSERT INTO Venta (Fecha, UsuarioId, Cliente, Subtotal, Impuesto, Descuento, Total, MetodoPago, Estado, Observaciones)
        VALUES (GETDATE(), @UsuarioPepe, N'Cliente mostrador', 0, 0, 0, 0, N'Tarjeta', N'Completada', N'Venta demo Tarjeta');
        SET @v2 = SCOPE_IDENTITY();

        IF @ProdP002 IS NOT NULL
        BEGIN
            DECLARE @p2 DECIMAL(10,2) = (SELECT Precio FROM Producto WHERE Id=@ProdP002);
            INSERT INTO DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal)
            VALUES (@v2, @ProdP002, 1, @p2, 0, 0, 1*@p2);
        END
        IF @ProdP004 IS NOT NULL
        BEGIN
            DECLARE @p4 DECIMAL(10,2) = (SELECT Precio FROM Producto WHERE Id=@ProdP004);
            INSERT INTO DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal)
            VALUES (@v2, @ProdP004, 2, @p4, 0, 0, 2*@p4);
        END

        UPDATE v SET 
            Subtotal = x.Subtotal,
            Impuesto = x.Impuesto,
            Total = x.Subtotal + x.Impuesto
        FROM Venta v
        CROSS APPLY (
            SELECT 
                SUM(dv.Cantidad*dv.PrecioUnitario) AS Subtotal,
                SUM(CASE WHEN ISNULL(p.Gravado,1)=1 THEN dv.Cantidad*dv.PrecioUnitario*0.13 ELSE 0 END) AS Impuesto
            FROM DetalleVenta dv
            LEFT JOIN Producto p ON p.Id = dv.ProductoId
            WHERE dv.VentaId = v.Id
        ) x
        WHERE v.Id = @v2;
    END

    -- Venta reciente (ayer) si no existe
    IF NOT EXISTS (SELECT 1 FROM Venta WHERE CAST(Fecha AS DATE) = CAST(DATEADD(DAY,-1,GETDATE()) AS DATE))
    BEGIN
        DECLARE @vAyer INT;
        INSERT INTO Venta (Fecha, UsuarioId, Cliente, Subtotal, Impuesto, Descuento, Total, MetodoPago, Estado, Observaciones)
        VALUES (DATEADD(DAY,-1,GETDATE()), @UsuarioPepe, N'Cliente mostrador', 0, 0, 0, 0, N'Efectivo', N'Completada', N'Venta demo Ayer');
        SET @vAyer = SCOPE_IDENTITY();
        IF @ProdP001 IS NOT NULL
        BEGIN
            DECLARE @p1y DECIMAL(10,2) = (SELECT Precio FROM Producto WHERE Id=@ProdP001);
            INSERT INTO DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal)
            VALUES (@vAyer, @ProdP001, 1, @p1y, 0, 0, 1*@p1y);
        END
        UPDATE v SET 
            Subtotal = x.Subtotal,
            Impuesto = x.Impuesto,
            Total = x.Subtotal + x.Impuesto
        FROM Venta v
        CROSS APPLY (
            SELECT 
                SUM(dv.Cantidad*dv.PrecioUnitario) AS Subtotal,
                SUM(CASE WHEN ISNULL(p.Gravado,1)=1 THEN dv.Cantidad*dv.PrecioUnitario*0.13 ELSE 0 END) AS Impuesto
            FROM DetalleVenta dv
            LEFT JOIN Producto p ON p.Id = dv.ProductoId
            WHERE dv.VentaId = v.Id
        ) x
        WHERE v.Id = @vAyer;
    END

    -- Movimientos de caja para HOY si no hay registros
    IF NOT EXISTS (SELECT 1 FROM MovimientoDiario WHERE CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE))
    BEGIN
        INSERT INTO MovimientoDiario (Fecha, TipoMovimiento, Descripcion, Categoria, Monto, UsuarioId)
        VALUES (GETDATE(), 'Entrada', N'Caja inicial', 'Caja', 50000, @UsuarioPepe);
        INSERT INTO MovimientoDiario (Fecha, TipoMovimiento, Descripcion, Categoria, Monto, UsuarioId)
        VALUES (GETDATE(), 'Venta', N'Ingresos de ventas del día', 'Ingresos', 30000, @UsuarioPepe);
        INSERT INTO MovimientoDiario (Fecha, TipoMovimiento, Descripcion, Categoria, Monto, UsuarioId)
        VALUES (GETDATE(), 'Gasto', N'Compra de insumos', 'Gastos', 10000, @UsuarioPepe);
    END

    -- Cierre de caja de AYER para listado si no existe
    IF NOT EXISTS (SELECT 1 FROM CierreCaja WHERE CAST(Fecha AS DATE) = CAST(DATEADD(DAY,-1,GETDATE()) AS DATE))
    BEGIN
        INSERT INTO CierreCaja (
            Fecha, UsuarioId, MontoInicial, TotalVentas, TotalEfectivo, TotalTarjeta, TotalOtros,
            MontoFinal, Diferencia, Observaciones, Estado, FechaCierre)
        VALUES (
            CAST(DATEADD(DAY,-1,GETDATE()) AS DATE), @UsuarioPepe, 50000, 65000, 45000, 15000, 5000,
            114000, 0, N'Cierre demo de ayer', 'Cerrado', DATEADD(DAY,-1,GETDATE())
        );
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    -- Registrar el error si fuese necesario
END CATCH

END -- FIN IF de datos DEV (solo si tablas existen)


-- (Contexto ya establecido arriba; repetimos por seguridad)
USE AntojeriaTica;
GO

-- =============================================
-- TABLAS BÁSICAS
-- =============================================

-- 1. Tabla Rol
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Rol' AND xtype='U')
BEGIN
    CREATE TABLE Rol (
        Id int IDENTITY(1,1) NOT NULL,
        Nombre nvarchar(50) NOT NULL,
        Descripcion nvarchar(200) NULL,
        Activo bit NOT NULL DEFAULT 1,
        FechaCreacion datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_Rol PRIMARY KEY (Id)
    );

    -- Insertar roles por defecto
    INSERT INTO Rol (Nombre, Descripcion) VALUES
    ('Admin', 'Administrador del sistema'),
    ('Vendedor', 'Personal de ventas'),
    ('Contador', 'Personal contable');

    PRINT 'Tabla Rol creada con datos iniciales';
END
ELSE
BEGIN
    PRINT 'Tabla Rol ya existe';
END
GO

-- 2. Tabla Usuario
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuario' AND xtype='U')
BEGIN
    CREATE TABLE Usuario (
        Id int IDENTITY(1,1) NOT NULL,
        Nombre nvarchar(100) NOT NULL,
        Email nvarchar(255) NOT NULL,
        Password nvarchar(255) NOT NULL,
        RolId int NOT NULL,
        Activo bit NOT NULL DEFAULT 1,
        FechaCreacion datetime NOT NULL DEFAULT GETDATE(),
        UltimoAcceso datetime NULL,
        CONSTRAINT PK_Usuario PRIMARY KEY (Id),
        CONSTRAINT FK_Usuario_Rol FOREIGN KEY (RolId) REFERENCES Rol(Id),
        CONSTRAINT UQ_Usuario_Email UNIQUE (Email)
    );
    PRINT 'Tabla Usuario creada';
END
ELSE
BEGIN
    PRINT 'Tabla Usuario ya existe';
END
GO

-- 3. Tabla Producto
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Producto' AND xtype='U')
BEGIN
    CREATE TABLE Producto (
        Id int IDENTITY(1,1) NOT NULL,
        Codigo nvarchar(50) NOT NULL,
        Nombre nvarchar(200) NOT NULL,
        Descripcion nvarchar(500) NULL,
        Precio decimal(10,2) NOT NULL,
        Categoria nvarchar(100) NULL,
        Stock int NOT NULL DEFAULT 0,
        StockMinimo int NOT NULL DEFAULT 5,
        Gravado bit NOT NULL DEFAULT 1, -- Para facturación electrónica
        Activo bit NOT NULL DEFAULT 1,
        FechaCreacion datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_Producto PRIMARY KEY (Id),
        CONSTRAINT UQ_Producto_Codigo UNIQUE (Codigo)
    );
    PRINT 'Tabla Producto creada con campo Gravado';
END
ELSE
BEGIN
    -- Agregar columna Gravado si no existe
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Producto') AND name = 'Gravado')
    BEGIN
        ALTER TABLE Producto ADD Gravado BIT NOT NULL DEFAULT 1;
        -- Marcar algunos productos como exentos
        UPDATE Producto SET Gravado = 0 WHERE Nombre LIKE '%Medicina%' OR Nombre LIKE '%Leche%' OR Nombre LIKE '%Arroz%';
        PRINT 'Columna Gravado agregada a tabla Producto';
    END
    PRINT 'Tabla Producto ya existe';
END
GO

-- 4. Tabla MetodoPago
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MetodoPago' AND xtype='U')
BEGIN
    CREATE TABLE MetodoPago (
        Id int IDENTITY(1,1) NOT NULL,
        Nombre nvarchar(50) NOT NULL,
        Descripcion nvarchar(200) NULL,
        RequiereCambio bit NOT NULL DEFAULT 0,
        Activo bit NOT NULL DEFAULT 1,
        FechaCreacion datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_MetodoPago PRIMARY KEY (Id)
    );

    -- Insertar métodos de pago por defecto
    INSERT INTO MetodoPago (Nombre, Descripcion, RequiereCambio) VALUES
    ('Efectivo', 'Pago en efectivo', 1),
    ('Tarjeta', 'Tarjeta de crédito/débito', 0),
    ('Transferencia', 'Transferencia bancaria', 0),
    ('SINPE Móvil', 'SINPE Móvil', 0);

    PRINT 'Tabla MetodoPago creada con datos iniciales';
END
ELSE
BEGIN
    PRINT 'Tabla MetodoPago ya existe';
END
GO

-- 5. Tabla Impuesto
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Impuesto' AND xtype='U')
BEGIN
    CREATE TABLE Impuesto (
        Id int IDENTITY(1,1) NOT NULL,
        Nombre nvarchar(50) NOT NULL,
        -- Ampliado para compatibilidad con capa Web/API
        Tipo nvarchar(50) NULL, -- IVA, ISC, etc.
        Porcentaje decimal(5,2) NOT NULL,
        AplicaEnRestaurante bit NOT NULL DEFAULT 0,
        EsExonerado bit NOT NULL DEFAULT 0,
        Activo bit NOT NULL DEFAULT 1,
        FechaCreacion datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_Impuesto PRIMARY KEY (Id)
    );

    -- Insertar impuestos por defecto
    INSERT INTO Impuesto (Nombre, Tipo, Porcentaje, AplicaEnRestaurante, EsExonerado) VALUES
    ('IVA', 'IVA', 13.00, 0, 0),
    ('Exento', 'IVA', 0.00, 0, 1);

    PRINT 'Tabla Impuesto creada con datos iniciales';
END
ELSE
BEGIN
    PRINT 'Tabla Impuesto ya existe';
    -- Agregar columnas faltantes si no existen para compatibilidad con la Web
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Impuesto') AND name = 'Tipo')
    BEGIN
        ALTER TABLE Impuesto ADD Tipo nvarchar(50) NULL;
        -- Rellenar valor por defecto basado en Nombre cuando sea posible
        UPDATE Impuesto SET Tipo = CASE WHEN Nombre LIKE '%IVA%' THEN 'IVA' ELSE 'IVA' END WHERE Tipo IS NULL;
        PRINT 'Columna Tipo agregada a Impuesto';
    END
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Impuesto') AND name = 'AplicaEnRestaurante')
    BEGIN
        ALTER TABLE Impuesto ADD AplicaEnRestaurante bit NOT NULL DEFAULT 0;
        PRINT 'Columna AplicaEnRestaurante agregada a Impuesto';
    END
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Impuesto') AND name = 'EsExonerado')
    BEGIN
        ALTER TABLE Impuesto ADD EsExonerado bit NOT NULL DEFAULT 0;
        PRINT 'Columna EsExonerado agregada a Impuesto';
    END
END
GO

-- 6. Tabla Descuento
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Descuento' AND xtype='U')
BEGIN
    CREATE TABLE Descuento (
        Id int IDENTITY(1,1) NOT NULL,
        Nombre nvarchar(100) NOT NULL,
        TipoDescuento nvarchar(20) NOT NULL, -- Porcentaje o Monto
        ValorDescuento decimal(10,2) NOT NULL,
        FechaInicio datetime NOT NULL,
        FechaFin datetime NOT NULL,
        Activo bit NOT NULL DEFAULT 1,
        FechaCreacion datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_Descuento PRIMARY KEY (Id)
    );
    PRINT 'Tabla Descuento creada';
END
ELSE
BEGIN
    PRINT 'Tabla Descuento ya existe';
END
GO

-- 7. Tabla Venta
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Venta' AND xtype='U')
BEGIN
    CREATE TABLE Venta (
        Id int IDENTITY(1,1) NOT NULL,
        Fecha datetime NOT NULL DEFAULT GETDATE(),
        UsuarioId int NOT NULL,
        Cliente nvarchar(255) NULL,
        Subtotal decimal(10,2) NOT NULL DEFAULT 0,
        Impuesto decimal(10,2) NOT NULL DEFAULT 0,
        Descuento decimal(10,2) NOT NULL DEFAULT 0,
        Total decimal(10,2) NOT NULL DEFAULT 0,
        MetodoPago nvarchar(50) NOT NULL,
        Estado nvarchar(20) NOT NULL DEFAULT 'Completada',
        Observaciones nvarchar(500) NULL,
        CONSTRAINT PK_Venta PRIMARY KEY (Id),
        CONSTRAINT FK_Venta_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id)
    );
    PRINT 'Tabla Venta creada';
END
ELSE
BEGIN
    PRINT 'Tabla Venta ya existe';
END
GO

-- Agregar columna PedidoId a Venta si no existe y crear FK a Pedido
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Venta') AND name = 'PedidoId')
BEGIN
    ALTER TABLE Venta ADD PedidoId int NULL;
    PRINT 'Columna PedidoId agregada a Venta';
END
-- Nota: La FK depende de la existencia de la tabla Pedido; se crea más adelante tras crear Pedido
IF OBJECT_ID('Pedido','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Venta_Pedido')
BEGIN
    ALTER TABLE Venta WITH CHECK ADD CONSTRAINT FK_Venta_Pedido FOREIGN KEY (PedidoId) REFERENCES Pedido(Id);
    PRINT 'FK_Venta_Pedido creada';
END
GO

-- 8. Tabla DetalleVenta
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DetalleVenta' AND xtype='U')
BEGIN
    CREATE TABLE DetalleVenta (
        Id int IDENTITY(1,1) NOT NULL,
        VentaId int NOT NULL,
        ProductoId int NOT NULL,
        Cantidad int NOT NULL,
        PrecioUnitario decimal(10,2) NOT NULL,
        Descuento decimal(10,2) NOT NULL DEFAULT 0,
        Impuesto decimal(10,2) NOT NULL DEFAULT 0,
        Subtotal decimal(10,2) NOT NULL DEFAULT 0,
        CONSTRAINT PK_DetalleVenta PRIMARY KEY (Id),
        CONSTRAINT FK_DetalleVenta_Venta FOREIGN KEY (VentaId) REFERENCES Venta(Id),
        CONSTRAINT FK_DetalleVenta_Producto FOREIGN KEY (ProductoId) REFERENCES Producto(Id)
    );
    PRINT 'Tabla DetalleVenta creada';
END
ELSE
BEGIN
    PRINT 'Tabla DetalleVenta ya existe';
END
GO

-- Crear FK Venta(PedidoId) -> Pedido(Id) después de asegurar que Pedido existe
IF OBJECT_ID('dbo.Pedido','U') IS NOT NULL AND OBJECT_ID('dbo.Venta','U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Venta_Pedido')
BEGIN
    ALTER TABLE Venta WITH CHECK ADD CONSTRAINT FK_Venta_Pedido FOREIGN KEY (PedidoId) REFERENCES Pedido(Id);
    PRINT 'FK_Venta_Pedido creada';
END
GO

-- 9. Tabla MovimientoDiario
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MovimientoDiario' AND xtype='U')
BEGIN
    CREATE TABLE MovimientoDiario (
        Id int IDENTITY(1,1) NOT NULL,
        Fecha datetime NOT NULL DEFAULT GETDATE(),
        TipoMovimiento nvarchar(50) NOT NULL, -- Entrada, Salida, Venta, Devolucion
        Descripcion nvarchar(500) NOT NULL,
        -- Compatibilidad: algunas capas esperan una columna 'Categoria'
        Categoria nvarchar(50) NOT NULL DEFAULT 'Otros',
        Monto decimal(10,2) NOT NULL,
        UsuarioId int NOT NULL,
        VentaId int NULL,
        CONSTRAINT PK_MovimientoDiario PRIMARY KEY (Id),
        CONSTRAINT FK_MovimientoDiario_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id),
        CONSTRAINT FK_MovimientoDiario_Venta FOREIGN KEY (VentaId) REFERENCES Venta(Id)
    );
    PRINT 'Tabla MovimientoDiario creada';
END
ELSE
BEGIN
    PRINT 'Tabla MovimientoDiario ya existe';
    -- Agregar columna Categoria si no existe, para compatibilidad con la API/Web
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MovimientoDiario') AND name = 'Categoria')
    BEGIN
        ALTER TABLE MovimientoDiario ADD Categoria nvarchar(50) NOT NULL DEFAULT 'Otros';
        PRINT 'Columna Categoria agregada a MovimientoDiario';
    END
END
GO

-- =============================================
-- SPs de MovimientoDiario necesarios por la API/Web
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='InsertarMovimientoDiario')
    DROP PROCEDURE InsertarMovimientoDiario;
GO
CREATE PROCEDURE InsertarMovimientoDiario
    @TipoMovimiento nvarchar(50),
    @Categoria nvarchar(50),
    @Monto decimal(10,2),
    @Descripcion nvarchar(500),
    @IdUsuario int
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO MovimientoDiario (Fecha, TipoMovimiento, Categoria, Monto, Descripcion, UsuarioId)
    VALUES (GETDATE(), @TipoMovimiento, @Categoria, @Monto, @Descripcion, @IdUsuario);
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='ActualizarMovimientoDiario')
    DROP PROCEDURE ActualizarMovimientoDiario;
GO
CREATE PROCEDURE ActualizarMovimientoDiario
    @IdMovimiento int,
    @TipoMovimiento nvarchar(50),
    @Categoria nvarchar(50),
    @Monto decimal(10,2),
    @Descripcion nvarchar(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MovimientoDiario
    SET TipoMovimiento = @TipoMovimiento,
        Categoria = @Categoria,
        Monto = @Monto,
        Descripcion = @Descripcion
    WHERE Id = @IdMovimiento;
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='EliminarMovimientoDiario')
    DROP PROCEDURE EliminarMovimientoDiario;
GO
CREATE PROCEDURE EliminarMovimientoDiario
    @IdMovimiento int
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM MovimientoDiario WHERE Id = @IdMovimiento;
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ListarMovimientosConNombre')
    DROP PROCEDURE sp_ListarMovimientosConNombre;
GO
CREATE PROCEDURE sp_ListarMovimientosConNombre
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        md.Id            AS IdMovimiento,
        md.Fecha         AS FechaHora,
        md.TipoMovimiento,
        md.Categoria,
        md.Monto,
        md.Descripcion,
        md.UsuarioId     AS IdUsuario,
        u.Nombre         AS NombreUsuario
    FROM MovimientoDiario md
    INNER JOIN Usuario u ON u.Id = md.UsuarioId
    ORDER BY md.Fecha DESC;
END
GO

-- 10. Tabla MovimientoInventario
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MovimientoInventario' AND xtype='U')
BEGIN
    CREATE TABLE MovimientoInventario (
        Id int IDENTITY(1,1) NOT NULL,
        ProductoId int NOT NULL,
        TipoMovimiento nvarchar(50) NOT NULL, -- Entrada, Salida, Ajuste
        Cantidad int NOT NULL,
        StockAnterior int NOT NULL,
        StockActual int NOT NULL,
        Motivo nvarchar(500) NULL,
        Fecha datetime NOT NULL DEFAULT GETDATE(),
        UsuarioId int NOT NULL,
        VentaId int NULL,
        CONSTRAINT PK_MovimientoInventario PRIMARY KEY (Id),
        CONSTRAINT FK_MovimientoInventario_Producto FOREIGN KEY (ProductoId) REFERENCES Producto(Id),
        CONSTRAINT FK_MovimientoInventario_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id),
        CONSTRAINT FK_MovimientoInventario_Venta FOREIGN KEY (VentaId) REFERENCES Venta(Id)
    );
    PRINT 'Tabla MovimientoInventario creada';
END
ELSE
BEGIN
    PRINT 'Tabla MovimientoInventario ya existe';
END
GO

-- 11. Tabla CierreCaja
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CierreCaja' AND xtype='U')
BEGIN
    CREATE TABLE CierreCaja (
        Id int IDENTITY(1,1) NOT NULL,
        Fecha datetime NOT NULL DEFAULT GETDATE(),
        UsuarioId int NOT NULL,
        MontoInicial decimal(10,2) NOT NULL DEFAULT 0,
        TotalVentas decimal(10,2) NOT NULL DEFAULT 0,
        TotalEfectivo decimal(10,2) NOT NULL DEFAULT 0,
        TotalTarjeta decimal(10,2) NOT NULL DEFAULT 0,
        TotalOtros decimal(10,2) NOT NULL DEFAULT 0,
        MontoFinal decimal(10,2) NOT NULL DEFAULT 0,
        Diferencia decimal(10,2) NOT NULL DEFAULT 0,
        Observaciones nvarchar(500) NULL,
        Estado nvarchar(20) NOT NULL DEFAULT 'Abierto',
        FechaCierre datetime NULL,
        CONSTRAINT PK_CierreCaja PRIMARY KEY (Id),
        CONSTRAINT FK_CierreCaja_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id)
    );
    PRINT 'Tabla CierreCaja creada';
END
ELSE
BEGIN
    PRINT 'Tabla CierreCaja ya existe';
END
GO

-- Totales de caja del día actual (compatibilidad con API)
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_CierreCajaDiario')
    DROP PROCEDURE sp_CierreCajaDiario;
GO
CREATE PROCEDURE sp_CierreCajaDiario
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
    ISNULL(SUM(CASE WHEN md.TipoMovimiento IN ('Entrada','Venta') THEN md.Monto ELSE 0 END), 0) AS TotalIngresos,
    ISNULL(SUM(CASE WHEN md.TipoMovimiento IN ('Salida','Devolucion','Gasto') THEN md.Monto ELSE 0 END), 0) AS TotalEgresos
    FROM MovimientoDiario md
    WHERE CAST(md.Fecha AS DATE) = CAST(GETDATE() AS DATE);
END
GO

-- Listado de cierres de caja (compatibilidad con vistas Web)
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='sp_ListarCierresDeCaja')
    DROP PROCEDURE sp_ListarCierresDeCaja;
GO
CREATE PROCEDURE sp_ListarCierresDeCaja
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        cc.Id                              AS IdMovimiento,
        cc.Fecha                           AS FechaHora,
        (cc.TotalEfectivo + cc.TotalTarjeta + cc.TotalOtros) AS TotalIngresos,
        CAST(0 AS decimal(10,2))           AS TotalEgresos,
        cc.MontoFinal                      AS MontoFisico,
        cc.Observaciones                   AS NotaJustificativa,
        u.Nombre                           AS NombreUsuario
    FROM CierreCaja cc
    INNER JOIN Usuario u ON u.Id = cc.UsuarioId
    ORDER BY cc.Fecha DESC;
END
GO

-- 12. Tabla HistorialMetodoPago
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HistorialMetodoPago' AND xtype='U')
BEGIN
    CREATE TABLE HistorialMetodoPago (
        Id int IDENTITY(1,1) NOT NULL,
        VentaId int NOT NULL,
        MetodoPagoId int NOT NULL,
        Monto decimal(10,2) NOT NULL,
        FechaPago datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_HistorialMetodoPago PRIMARY KEY (Id),
        CONSTRAINT FK_HistorialMetodoPago_Venta FOREIGN KEY (VentaId) REFERENCES Venta(Id),
        CONSTRAINT FK_HistorialMetodoPago_MetodoPago FOREIGN KEY (MetodoPagoId) REFERENCES MetodoPago(Id)
    );
    PRINT 'Tabla HistorialMetodoPago creada';
END
ELSE
BEGIN
    PRINT 'Tabla HistorialMetodoPago ya existe';
END
GO

-- =============================================
-- TABLAS DE PEDIDOS
-- =============================================

-- 13. Tabla Pedido
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Pedido' AND xtype='U')
BEGIN
    CREATE TABLE Pedido (
        Id int IDENTITY(1,1) NOT NULL,
        NumeroPedido nvarchar(20) NOT NULL,
        Fecha datetime NOT NULL DEFAULT GETDATE(),
        UsuarioId int NOT NULL,
        Cliente nvarchar(255) NULL,
        Mesa nvarchar(20) NULL,
        TipoPedido nvarchar(50) NOT NULL, -- Mesa, Telefono, App
        Estado nvarchar(50) NOT NULL DEFAULT 'En preparación', -- En preparación, Listo, Entregado, Cancelado
        TiempoEstimado int NULL, -- Tiempo estimado en minutos
        TiempoPreparacion int NULL, -- Tiempo real de preparación en minutos
        FechaEstimadaEntrega datetime NULL, -- Fecha y hora estimada de entrega
        EsAtrasado bit NOT NULL DEFAULT 0, -- Indica si el pedido está atrasado
        Subtotal decimal(10,2) NOT NULL DEFAULT 0,
        Impuesto decimal(10,2) NOT NULL DEFAULT 0,
        Descuento decimal(10,2) NOT NULL DEFAULT 0,
        Total decimal(10,2) NOT NULL DEFAULT 0,
        Observaciones nvarchar(500) NULL,
        FechaCreacion datetime NOT NULL DEFAULT GETDATE(),
        FechaActualizacion datetime NULL,
        FechaInicioPreparacion datetime NULL, -- Cuando empezó la preparación
        FechaFinalizacion datetime NULL, -- Cuando se completó el pedido
        -- Columnas para PED-004: Cancelación de pedidos
        FechaCancelacion datetime NULL,
        MotivoCancelacion nvarchar(500) NULL,
        UsuarioCancelacion int NULL,
        AutorizadoPor int NULL,
        CONSTRAINT PK_Pedido PRIMARY KEY (Id),
        CONSTRAINT FK_Pedido_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id),
        CONSTRAINT FK_Pedido_UsuarioCancelacion FOREIGN KEY (UsuarioCancelacion) REFERENCES Usuario(Id),
        CONSTRAINT FK_Pedido_AutorizadoPor FOREIGN KEY (AutorizadoPor) REFERENCES Usuario(Id),
        CONSTRAINT UQ_Pedido_Numero UNIQUE (NumeroPedido)
    );
    PRINT 'Tabla Pedido creada';
END
ELSE
BEGIN
    -- Agregar nuevas columnas si no existen
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedido') AND name = 'TiempoEstimado')
    BEGIN
        ALTER TABLE Pedido ADD TiempoEstimado int NULL;
        PRINT 'Columna TiempoEstimado agregada';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedido') AND name = 'TiempoPreparacion')
    BEGIN
        ALTER TABLE Pedido ADD TiempoPreparacion int NULL;
        PRINT 'Columna TiempoPreparacion agregada';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedido') AND name = 'FechaEstimadaEntrega')
    BEGIN
        ALTER TABLE Pedido ADD FechaEstimadaEntrega datetime NULL;
        PRINT 'Columna FechaEstimadaEntrega agregada';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedido') AND name = 'EsAtrasado')
    BEGIN
        ALTER TABLE Pedido ADD EsAtrasado bit NOT NULL DEFAULT 0;
        PRINT 'Columna EsAtrasado agregada';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedido') AND name = 'FechaInicioPreparacion')
    BEGIN
        ALTER TABLE Pedido ADD FechaInicioPreparacion datetime NULL;
        PRINT 'Columna FechaInicioPreparacion agregada';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedido') AND name = 'FechaFinalizacion')
    BEGIN
        ALTER TABLE Pedido ADD FechaFinalizacion datetime NULL;
        PRINT 'Columna FechaFinalizacion agregada';
    END

    -- Columnas para PED-004: Cancelación de pedidos
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedido') AND name = 'FechaCancelacion')
    BEGIN
        ALTER TABLE Pedido ADD FechaCancelacion datetime NULL;
        PRINT 'Columna FechaCancelacion agregada';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedido') AND name = 'MotivoCancelacion')
    BEGIN
        ALTER TABLE Pedido ADD MotivoCancelacion nvarchar(500) NULL;
        PRINT 'Columna MotivoCancelacion agregada';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedido') AND name = 'UsuarioCancelacion')
    BEGIN
        ALTER TABLE Pedido ADD UsuarioCancelacion int NULL;
        PRINT 'Columna UsuarioCancelacion agregada';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Pedido') AND name = 'AutorizadoPor')
    BEGIN
        ALTER TABLE Pedido ADD AutorizadoPor int NULL;
        PRINT 'Columna AutorizadoPor agregada';
    END

    -- Agregar foreign keys para las nuevas columnas si no existen
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Pedido_UsuarioCancelacion')
    BEGIN
        ALTER TABLE Pedido ADD CONSTRAINT FK_Pedido_UsuarioCancelacion 
            FOREIGN KEY (UsuarioCancelacion) REFERENCES Usuario(Id);
        PRINT 'Foreign key FK_Pedido_UsuarioCancelacion agregada';
    END

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Pedido_AutorizadoPor')
    BEGIN
        ALTER TABLE Pedido ADD CONSTRAINT FK_Pedido_AutorizadoPor 
            FOREIGN KEY (AutorizadoPor) REFERENCES Usuario(Id);
        PRINT 'Foreign key FK_Pedido_AutorizadoPor agregada';
    END

    PRINT 'Tabla Pedido ya existe - columnas actualizadas';
END
GO

-- 14. Tabla DetallePedido
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DetallePedido' AND xtype='U')
BEGIN
    CREATE TABLE DetallePedido (
        Id int IDENTITY(1,1) NOT NULL,
        PedidoId int NOT NULL,
        ProductoId int NOT NULL,
        Cantidad int NOT NULL,
        PrecioUnitario decimal(10,2) NOT NULL,
        Descuento decimal(10,2) NOT NULL DEFAULT 0,
        Impuesto decimal(10,2) NOT NULL DEFAULT 0,
        Subtotal decimal(10,2) NOT NULL DEFAULT 0,
        ObservacionesItem nvarchar(200) NULL, -- Para especificaciones como "sin cebolla", "extra picante", etc.
        CONSTRAINT PK_DetallePedido PRIMARY KEY (Id),
        CONSTRAINT FK_DetallePedido_Pedido FOREIGN KEY (PedidoId) REFERENCES Pedido(Id),
        CONSTRAINT FK_DetallePedido_Producto FOREIGN KEY (ProductoId) REFERENCES Producto(Id)
    );
    PRINT 'Tabla DetallePedido creada';
END
ELSE
BEGIN
    PRINT 'Tabla DetallePedido ya existe';
END
GO

-- 15. Tabla NotificacionPedido
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NotificacionPedido' AND xtype='U')
BEGIN
    CREATE TABLE NotificacionPedido (
        Id int IDENTITY(1,1) NOT NULL,
        PedidoId int NOT NULL,
        UsuarioId int NOT NULL, -- Usuario que recibe la notificación
        TipoNotificacion nvarchar(50) NOT NULL, -- Listo, Atrasado, Cancelado
        Mensaje nvarchar(500) NOT NULL,
        Leida bit NOT NULL DEFAULT 0,
        FechaCreacion datetime NOT NULL DEFAULT GETDATE(),
        FechaLectura datetime NULL,
        CONSTRAINT PK_NotificacionPedido PRIMARY KEY (Id),
        CONSTRAINT FK_NotificacionPedido_Pedido FOREIGN KEY (PedidoId) REFERENCES Pedido(Id),
        CONSTRAINT FK_NotificacionPedido_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id)
    );
    PRINT 'Tabla NotificacionPedido creada';
END
ELSE
BEGIN
    PRINT 'Tabla NotificacionPedido ya existe';
END
GO

-- 13. Tabla FacturaElectronica - Estructura completa Hacienda
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FacturaElectronica' AND xtype='U')
BEGIN
    CREATE TABLE FacturaElectronica (
        Id int IDENTITY(1,1) NOT NULL,
        IdFactura AS Id, -- Columna calculada para compatibilidad
        VentaId int NOT NULL,
        NumeroFactura nvarchar(50) NOT NULL,
        ClaveNumerica nvarchar(50) NOT NULL,
        FechaGeneracion datetime NOT NULL DEFAULT GETDATE(),
        FechaEmision AS FechaGeneracion, -- Columna calculada para compatibilidad

        -- Información del documento
        TipoDocumento nvarchar(10) NOT NULL DEFAULT '01', -- 01 = Factura Electrónica
        CodigoSeguridadComprobante nvarchar(8) NULL,
        Estado nvarchar(20) NOT NULL DEFAULT 'Activo',

        -- Datos del cliente
        TipoIdentificacionCliente nvarchar(10) NOT NULL DEFAULT '01', -- 01=Cédula física, 02=Cédula jurídica, 03=DIMEX, 04=NITE
        IdentificacionCliente nvarchar(20) NULL,
        NombreCliente nvarchar(255) NOT NULL,
        ClienteNombre AS NombreCliente, -- Columna calculada para compatibilidad
        CorreoCliente nvarchar(255) NULL,
        ClienteEmail AS CorreoCliente, -- Columna calculada para compatibilidad
        TelefonoCliente nvarchar(20) NULL,
        ClienteTelefono AS TelefonoCliente, -- Columna calculada para compatibilidad

        -- Totales Hacienda
        SubtotalMercanciasGravadas decimal(18,5) NOT NULL DEFAULT 0,
        SubtotalMercanciasExentas decimal(18,5) NOT NULL DEFAULT 0,
        MontoTotalMercanciasGravadas decimal(18,5) NOT NULL DEFAULT 0,
        MontoTotalMercanciasExentas decimal(18,5) NOT NULL DEFAULT 0,
        MontoTotalImpuesto decimal(18,5) NOT NULL DEFAULT 0,
        TotalComprobante decimal(18,5) NOT NULL DEFAULT 0,

        -- Campos de compatibilidad
        SubTotal AS SubtotalMercanciasGravadas + SubtotalMercanciasExentas,
        MontoImpuesto AS MontoTotalImpuesto,
        MontoTotal AS TotalComprobante,

        -- Estado en Hacienda
        EstadoHacienda nvarchar(50) NOT NULL DEFAULT 'Pendiente',
        MensajeHacienda nvarchar(max) NULL,
        FechaRespuestaHacienda datetime NULL,
        ConsecutivoHacienda nvarchar(20) NULL,

        -- Email
        EmailEnviado bit NOT NULL DEFAULT 0,
        FechaEnvioEmail datetime NULL,

        -- Auditoria
        CreadoPor nvarchar(100) NOT NULL DEFAULT 'Sistema',
        FechaCreacion datetime NOT NULL DEFAULT GETDATE(),
        ModificadoPor nvarchar(100) NULL,
        FechaModificacion datetime NULL,

        CONSTRAINT PK_FacturaElectronica PRIMARY KEY (Id),
        CONSTRAINT FK_FacturaElectronica_Venta FOREIGN KEY (VentaId) REFERENCES Venta(Id),
        CONSTRAINT UQ_FacturaElectronica_ClaveNumerica UNIQUE (ClaveNumerica),
        CONSTRAINT UQ_FacturaElectronica_NumeroFactura UNIQUE (NumeroFactura)
    );
    PRINT 'Tabla FacturaElectronica creada correctamente con estructura Hacienda';
END
ELSE
BEGIN
    PRINT 'Tabla FacturaElectronica ya existe';
END
GO

-- 14. Tabla para historial de eventos de facturación
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HistorialFacturacionElectronica' AND xtype='U')
BEGIN
    CREATE TABLE HistorialFacturacionElectronica (
        Id int IDENTITY(1,1) NOT NULL,
        IdFactura int NOT NULL,
        TipoEvento nvarchar(50) NOT NULL, -- Email, Hacienda, PDF, etc.
        Detalle nvarchar(500) NULL,
        Estado nvarchar(50) NOT NULL, -- Pendiente, Exitoso, Error
        Mensaje nvarchar(max) NULL,
        FechaEvento datetime NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_HistorialFacturacionElectronica PRIMARY KEY (Id),
        CONSTRAINT FK_HistorialFacturacion_Factura FOREIGN KEY (IdFactura) REFERENCES FacturaElectronica(Id)
    );
    PRINT 'Tabla HistorialFacturacionElectronica creada correctamente';
END
ELSE
BEGIN
    PRINT 'Tabla HistorialFacturacionElectronica ya existe';
END
GO

-- =============================================
-- STORED PROCEDURES BÁSICOS
-- =============================================

-- Productos con estado de stock para pantalla de inventario
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerProductosConEstado')
    DROP PROCEDURE sp_ObtenerProductosConEstado;
GO
CREATE PROCEDURE sp_ObtenerProductosConEstado
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        p.Id              AS IdProducto,
        p.Nombre,
        p.Descripcion,
        p.Precio          AS PrecioUnitario,
        p.Stock           AS Existencias,
        CASE 
            WHEN p.Stock <= 0 THEN 'Agotado'
            WHEN p.Stock <= p.StockMinimo THEN 'Bajo stock'
            ELSE 'En stock'
        END               AS EstadoStock
    FROM Producto p
    WHERE p.Activo = 1
    ORDER BY p.Nombre;
END
GO

-- Reporte de ventas por día (resumen, por método y por producto)
IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='ReporteVentasDia')
    DROP PROCEDURE ReporteVentasDia;
GO
CREATE PROCEDURE ReporteVentasDia
    @Fecha DATE
AS
BEGIN
    SET NOCOUNT ON;
    -- Totales del día (usar totales de la venta para estabilidad)
    SELECT 
        COUNT(*) AS TotalVentas,
        ISNULL(SUM(v.Total), 0) AS MontoTotal
    FROM Venta v
    WHERE CAST(v.Fecha AS DATE) = @Fecha;

    -- Ventas por método de pago (usar totales de la venta)
    SELECT 
        v.MetodoPago,
        COUNT(*) AS CantidadVentas,
        ISNULL(SUM(v.Total), 0) AS MontoTotal
    FROM Venta v
    WHERE CAST(v.Fecha AS DATE) = @Fecha
    GROUP BY v.MetodoPago
    ORDER BY MontoTotal DESC;

    -- Productos vendidos
    SELECT 
        p.Codigo AS ProductoCodigo,
        p.Nombre AS ProductoNombre,
        ISNULL(SUM(dv.Cantidad),0) AS CantidadVendida,
        ISNULL(SUM(dv.Cantidad * dv.PrecioUnitario),0) AS MontoTotal
    FROM Venta v
    INNER JOIN DetalleVenta dv ON v.Id = dv.VentaId
    INNER JOIN Producto p ON dv.ProductoId = p.Id
    WHERE CAST(v.Fecha AS DATE) = @Fecha
    GROUP BY p.Codigo, p.Nombre
    ORDER BY CantidadVendida DESC;
END
GO

-- SP: Insertar Usuario - CORREGIDO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_InsertarUsuario')
    DROP PROCEDURE sp_InsertarUsuario;
GO

CREATE PROCEDURE sp_InsertarUsuario
    @NombreCompleto NVARCHAR(100),
    @Correo NVARCHAR(255),
    @Cedula NVARCHAR(50) = NULL, -- Este parámetro se ignora ya que no existe en la tabla
    @ContrasenaHash NVARCHAR(255),
    @Estado NVARCHAR(20) = 'Activo',
    @IdRol INT = 2
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Verificar si el email ya existe
        IF EXISTS (SELECT 1 FROM Usuario WHERE Email = @Correo)
        BEGIN
            RAISERROR('El email ya está registrado', 16, 1);
            RETURN;
        END

        -- Insertar usuario (ignoramos @Cedula ya que no existe esa columna)
        INSERT INTO Usuario (Nombre, Email, Password, RolId, Activo)
        VALUES (@NombreCompleto, @Correo, @ContrasenaHash, @IdRol,
                CASE WHEN @Estado = 'Activo' THEN 1 ELSE 0 END);

        -- Retornar el ID del usuario creado
        SELECT SCOPE_IDENTITY() as IdUsuario;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- SP: Obtener Usuario por ID - CORREGIDO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerUsuario')
    DROP PROCEDURE sp_ObtenerUsuario;
GO

CREATE PROCEDURE sp_ObtenerUsuario
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.Id as IdUsuario,
        u.Nombre as NombreCompleto,
        u.Email as Correo,
        '' as Cedula, -- Campo no existe en la tabla actual, retornar vacío
        CASE WHEN u.Activo = 1 THEN 'Activo' ELSE 'Inactivo' END as Estado,
        u.RolId as IdRol,
        r.Nombre as NombreRol,
        u.Password as ContrasenaHash
    FROM Usuario u
    INNER JOIN Rol r ON u.RolId = r.Id
    WHERE u.Id = @IdUsuario
        AND u.Activo = 1;
END
GO

-- SP: Obtener Todos los Usuarios - CORREGIDO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerUsuarios')
    DROP PROCEDURE sp_ObtenerUsuarios;
GO

CREATE PROCEDURE sp_ObtenerUsuarios
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.Id as IdUsuario,
        u.Nombre as NombreCompleto,
        u.Email as Correo,
        '' as Cedula, -- Campo no existe en la tabla actual, retornar vacío
        CASE WHEN u.Activo = 1 THEN 'Activo' ELSE 'Inactivo' END as Estado,
        u.RolId as IdRol,
        r.Nombre as NombreRol,
        u.FechaCreacion
    FROM Usuario u
    INNER JOIN Rol r ON u.RolId = r.Id
    ORDER BY u.FechaCreacion DESC;
END
GO

-- SP: Insertar Rol
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_InsertarRol')
    DROP PROCEDURE sp_InsertarRol;
GO

CREATE PROCEDURE sp_InsertarRol
    @NombreRol NVARCHAR(50),
    @Descripcion NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Verificar si el rol ya existe
        IF EXISTS (SELECT 1 FROM Rol WHERE Nombre = @NombreRol)
        BEGIN
            RAISERROR('El rol ya existe', 16, 1);
            RETURN;
        END

        -- Insertar rol
        INSERT INTO Rol (Nombre, Descripcion, Activo)
        VALUES (@NombreRol, @Descripcion, 1);

        SELECT SCOPE_IDENTITY() as IdRol;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- SP: Listar Roles
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ListarRoles')
    DROP PROCEDURE sp_ListarRoles;
GO

CREATE PROCEDURE sp_ListarRoles
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id as IdRol,
        Nombre as NombreRol,
        Descripcion,
        Activo,
        FechaCreacion
    FROM Rol
    WHERE Activo = 1
    ORDER BY Nombre;
END
GO

-- SP: Actualizar Usuario - NUEVO (requerido por AccountController)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ActualizarUsuario')
    DROP PROCEDURE sp_ActualizarUsuario;
GO

CREATE PROCEDURE sp_ActualizarUsuario
    @IdUsuario INT,
    @NombreCompleto NVARCHAR(100),
    @Correo NVARCHAR(255),
    @Cedula NVARCHAR(50) = NULL, -- Este parámetro se ignora ya que no existe en la tabla
    @Estado NVARCHAR(20) = 'Activo',
    @IdRol INT = 2
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Verificar que el usuario existe
        IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Id = @IdUsuario)
        BEGIN
            RAISERROR('El usuario no existe', 16, 1);
            RETURN;
        END

        -- Verificar si el email ya existe para otro usuario
        IF EXISTS (SELECT 1 FROM Usuario WHERE Email = @Correo AND Id != @IdUsuario)
        BEGIN
            RAISERROR('El email ya está registrado por otro usuario', 16, 1);
            RETURN;
        END

        -- Actualizar usuario
        UPDATE Usuario 
        SET 
            Nombre = @NombreCompleto,
            Email = @Correo,
            RolId = @IdRol,
            Activo = CASE WHEN @Estado = 'Activo' THEN 1 ELSE 0 END
        WHERE Id = @IdUsuario;

        -- Retornar número de filas afectadas
        SELECT @@ROWCOUNT as FilasAfectadas;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- SP: Eliminar Usuario - NUEVO (requerido por AccountController)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_EliminarUsuario')
    DROP PROCEDURE sp_EliminarUsuario;
GO

CREATE PROCEDURE sp_EliminarUsuario
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Verificar que el usuario existe
        IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Id = @IdUsuario)
        BEGIN
            RAISERROR('El usuario no existe', 16, 1);
            RETURN;
        END

        -- En lugar de eliminar físicamente, marcar como inactivo
        UPDATE Usuario 
        SET Activo = 0
        WHERE Id = @IdUsuario;

        -- Retornar número de filas afectadas
        SELECT @@ROWCOUNT as FilasAfectadas;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- SP: Eliminar Rol - NUEVO (requerido por AccountController)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_EliminarRol')
    DROP PROCEDURE sp_EliminarRol;
GO

CREATE PROCEDURE sp_EliminarRol
    @IdRol INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Verificar que el rol existe
        IF NOT EXISTS (SELECT 1 FROM Rol WHERE Id = @IdRol)
        BEGIN
            RAISERROR('El rol no existe', 16, 1);
            RETURN;
        END

        -- Verificar que no hay usuarios usando este rol
        IF EXISTS (SELECT 1 FROM Usuario WHERE RolId = @IdRol)
        BEGIN
            RAISERROR('No se puede eliminar el rol porque está en uso por usuarios', 16, 1);
            RETURN;
        END

        -- Marcar rol como inactivo en lugar de eliminarlo
        UPDATE Rol 
        SET Activo = 0
        WHERE Id = @IdRol;

        -- Retornar número de filas afectadas
        SELECT @@ROWCOUNT as FilasAfectadas;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- SP: Obtener productos con stock bajo
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ObtenerProductosStockBajo')
    DROP PROCEDURE ObtenerProductosStockBajo;
GO

CREATE PROCEDURE ObtenerProductosStockBajo
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id,
        p.Codigo,
        p.Nombre,
        p.Stock,
        p.StockMinimo,
        p.Precio,
        p.Categoria
    FROM Producto p
    WHERE p.Stock <= p.StockMinimo
        AND p.Activo = 1
    ORDER BY p.Stock ASC, p.Nombre;
END
GO

-- SP: Registrar movimiento de inventario
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'RegistrarMovimientoInventario')
    DROP PROCEDURE RegistrarMovimientoInventario;
GO

CREATE PROCEDURE RegistrarMovimientoInventario
    @ProductoId INT,
    @TipoMovimiento NVARCHAR(50),
    @Cantidad INT,
    @Motivo NVARCHAR(500) = NULL,
    @UsuarioId INT,
    @VentaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StockAnterior INT;
    DECLARE @StockActual INT;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Obtener stock actual
        SELECT @StockAnterior = Stock FROM Producto WHERE Id = @ProductoId;

        -- Calcular nuevo stock
        IF @TipoMovimiento = 'Entrada'
            SET @StockActual = @StockAnterior + @Cantidad;
        ELSE IF @TipoMovimiento = 'Salida'
            SET @StockActual = @StockAnterior - @Cantidad;
        ELSE
            SET @StockActual = @Cantidad; -- Para ajustes

        -- Validar stock negativo
        IF @StockActual < 0
        BEGIN
            RAISERROR('Stock insuficiente', 16, 1);
            RETURN;
        END

        -- Actualizar stock del producto
        UPDATE Producto SET Stock = @StockActual WHERE Id = @ProductoId;

        -- Registrar movimiento
        INSERT INTO MovimientoInventario (
            ProductoId, TipoMovimiento, Cantidad, StockAnterior,
            StockActual, Motivo, UsuarioId, VentaId
        )
        VALUES (
            @ProductoId, @TipoMovimiento, @Cantidad, @StockAnterior,
            @StockActual, @Motivo, @UsuarioId, @VentaId
        );

        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- STORED PROCEDURES DE FACTURACIÓN ELECTRÓNICA
-- =============================================

-- SP: GenerarFacturaElectronica mejorado
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GenerarFacturaElectronica')
    DROP PROCEDURE GenerarFacturaElectronica;
GO

CREATE PROCEDURE GenerarFacturaElectronica
    @VentaId INT,
    @ClienteNombre NVARCHAR(255),
    @ClienteEmail NVARCHAR(255) = NULL,
    @ClienteTelefono NVARCHAR(20) = NULL,
    @TipoIdentificacionCliente NVARCHAR(10) = '01',
    @IdentificacionCliente NVARCHAR(20) = NULL,
    @CreadoPor NVARCHAR(100) = 'Sistema'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FacturaId INT
    DECLARE @NumeroFactura NVARCHAR(50)
    DECLARE @ClaveNumerica NVARCHAR(50)
    DECLARE @SubtotalGravado DECIMAL(18,5) = 0
    DECLARE @SubtotalExento DECIMAL(18,5) = 0
    DECLARE @MontoImpuesto DECIMAL(18,5) = 0
    DECLARE @TotalVenta DECIMAL(18,5) = 0
    DECLARE @Intentos INT = 0
    DECLARE @MaxIntentos INT = 10

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Verificar que la venta existe
        IF NOT EXISTS (SELECT 1 FROM Venta WHERE Id = @VentaId)
        BEGIN
            RAISERROR('La venta especificada no existe', 16, 1);
            RETURN;
        END

        -- Calcular totales separando gravados y exentos
        SELECT
            @SubtotalGravado = ISNULL(SUM(CASE WHEN ISNULL(p.Gravado, 1) = 1 THEN dv.Cantidad * dv.PrecioUnitario ELSE 0 END), 0),
            @SubtotalExento = ISNULL(SUM(CASE WHEN ISNULL(p.Gravado, 1) = 0 THEN dv.Cantidad * dv.PrecioUnitario ELSE 0 END), 0)
        FROM DetalleVenta dv
        LEFT JOIN Producto p ON dv.ProductoId = p.Id
        WHERE dv.VentaId = @VentaId;

        -- Calcular impuesto solo sobre productos gravados
        SET @MontoImpuesto = @SubtotalGravado * 0.13;
        SET @TotalVenta = @SubtotalGravado + @SubtotalExento + @MontoImpuesto;

        -- Generar número de factura secuencial
        DECLARE @Contador INT;
        SELECT @Contador = ISNULL(MAX(CAST(SUBSTRING(NumeroFactura, 4, LEN(NumeroFactura)-3) AS INT)), 0) + 1
        FROM FacturaElectronica
        WHERE NumeroFactura LIKE 'FAC%';

        SET @NumeroFactura = 'FAC' + RIGHT('000000' + CAST(@Contador AS VARCHAR), 6);

        -- Generar clave numérica única con reintentos
        WHILE @Intentos < @MaxIntentos
        BEGIN
            BEGIN TRY
                -- Generar clave numérica con GUID para garantizar unicidad
                SET @ClaveNumerica =
                    FORMAT(GETDATE(), 'ddMMyyyy') +
                    FORMAT(GETDATE(), 'HHmmss') +
                    RIGHT('000' + CAST(ABS(CHECKSUM(NEWID()) % 1000) AS VARCHAR), 3) +
                    RIGHT('00000' + CAST(ABS(CHECKSUM(NEWID()) % 100000) AS VARCHAR), 5);

                -- Insertar la factura con estructura Hacienda
                INSERT INTO FacturaElectronica (
                    VentaId, NumeroFactura, ClaveNumerica, FechaGeneracion,
                    TipoIdentificacionCliente, IdentificacionCliente, NombreCliente,
                    CorreoCliente, TelefonoCliente,
                    SubtotalMercanciasGravadas, SubtotalMercanciasExentas,
                    MontoTotalMercanciasGravadas, MontoTotalMercanciasExentas,
                    MontoTotalImpuesto, TotalComprobante,
                    EstadoHacienda, CreadoPor, FechaCreacion
                )
                VALUES (
                    @VentaId, @NumeroFactura, @ClaveNumerica, GETDATE(),
                    @TipoIdentificacionCliente, @IdentificacionCliente, @ClienteNombre,
                    @ClienteEmail, @ClienteTelefono,
                    @SubtotalGravado, @SubtotalExento,
                    @SubtotalGravado, @SubtotalExento,
                    @MontoImpuesto, @TotalVenta,
                    'Generada', @CreadoPor, GETDATE()
                );

                SET @FacturaId = SCOPE_IDENTITY();

                -- Si llegamos aquí, la inserción fue exitosa
                BREAK;

            END TRY
            BEGIN CATCH
                -- Si es error de clave duplicada, intentar de nuevo
                IF ERROR_NUMBER() = 2627 -- Violation of UNIQUE KEY constraint
                BEGIN
                    SET @Intentos = @Intentos + 1;
                    WAITFOR DELAY '00:00:01'; -- Esperar 1 segundo antes del siguiente intento
                    CONTINUE;
                END
                ELSE
                BEGIN
                    -- Si es otro tipo de error, lanzarlo
                    THROW;
                END
            END CATCH
        END

        IF @Intentos >= @MaxIntentos
        BEGIN
            RAISERROR('No se pudo generar una clave numérica única después de varios intentos', 16, 1);
            RETURN;
        END

        -- Registro de evento de creación
        INSERT INTO HistorialFacturacionElectronica (IdFactura, TipoEvento, Estado, Mensaje, FechaEvento)
        VALUES (@FacturaId, 'Creacion', 'Exitoso', 'Factura electrónica generada correctamente', GETDATE());

        -- Registro de actividad para el email si se proporcionó
        IF @ClienteEmail IS NOT NULL
        BEGIN
            INSERT INTO HistorialFacturacionElectronica (IdFactura, TipoEvento, Detalle, Estado, Mensaje, FechaEvento)
            VALUES (@FacturaId, 'Email', @ClienteEmail, 'Pendiente', 'Email programado para envío', GETDATE());
        END

        COMMIT TRANSACTION;

        -- Retornar información de la factura creada
        SELECT
            @FacturaId as Id,
            @FacturaId as IdFactura,
            @NumeroFactura as NumeroFactura,
            @ClaveNumerica as ClaveNumerica,
            @ClienteNombre as ClienteNombre,
            @ClienteNombre as NombreCliente,
            @ClienteEmail as ClienteEmail,
            @ClienteEmail as CorreoCliente,
            @SubtotalGravado + @SubtotalExento as SubTotal,
            @MontoImpuesto as MontoImpuesto,
            @TotalVenta as MontoTotal,
            @TotalVenta as TotalComprobante,
            'Generada' as EstadoHacienda,
            GETDATE() as FechaEmision,
            GETDATE() as FechaGeneracion;

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- SP: Buscar Facturas Electrónicas
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'BuscarFacturasElectronicas')
    DROP PROCEDURE BuscarFacturasElectronicas;
GO

CREATE PROCEDURE BuscarFacturasElectronicas
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL,
    @NumeroFactura NVARCHAR(50) = NULL,
    @ClienteNombre NVARCHAR(255) = NULL,
    @IdentificacionCliente NVARCHAR(20) = NULL,
    @EstadoHacienda NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        fe.Id,
        fe.Id as IdFactura,
        fe.VentaId,
        fe.NumeroFactura,
        fe.ClaveNumerica,
        fe.FechaGeneracion,
        fe.FechaGeneracion as FechaEmision,
        fe.NombreCliente,
        fe.NombreCliente as ClienteNombre,
        fe.CorreoCliente,
        fe.CorreoCliente as ClienteEmail,
        fe.TelefonoCliente,
        fe.TelefonoCliente as ClienteTelefono,
        fe.IdentificacionCliente,
        fe.SubtotalMercanciasGravadas + fe.SubtotalMercanciasExentas as SubTotal,
        fe.MontoTotalImpuesto as MontoImpuesto,
        fe.TotalComprobante,
        fe.TotalComprobante as MontoTotal,
        fe.EstadoHacienda,
        fe.MensajeHacienda,
        fe.EmailEnviado,
        fe.FechaEnvioEmail
    FROM FacturaElectronica fe
    WHERE fe.Estado = 'Activo'
        AND (@FechaInicio IS NULL OR fe.FechaGeneracion >= @FechaInicio)
        AND (@FechaFin IS NULL OR fe.FechaGeneracion <= @FechaFin)
        AND (@NumeroFactura IS NULL OR fe.NumeroFactura LIKE '%' + @NumeroFactura + '%')
        AND (@ClienteNombre IS NULL OR fe.NombreCliente LIKE '%' + @ClienteNombre + '%')
        AND (@IdentificacionCliente IS NULL OR fe.IdentificacionCliente LIKE '%' + @IdentificacionCliente + '%')
        AND (@EstadoHacienda IS NULL OR fe.EstadoHacienda = @EstadoHacienda)
    ORDER BY fe.FechaGeneracion DESC;
END
GO

-- SP: Obtener Detalle Completo de Factura
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ObtenerDetalleFacturaElectronica')
    DROP PROCEDURE ObtenerDetalleFacturaElectronica;
GO

CREATE PROCEDURE ObtenerDetalleFacturaElectronica
    @FacturaId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        -- Datos principales de la factura
        fe.Id,
        fe.Id as IdFactura,
        fe.VentaId,
        fe.NumeroFactura,
        fe.ClaveNumerica,
        fe.FechaGeneracion,
        fe.FechaGeneracion as FechaEmision,
        fe.TipoDocumento,
        fe.EstadoHacienda,
        fe.MensajeHacienda,

        -- Datos del cliente
        fe.TipoIdentificacionCliente,
        fe.IdentificacionCliente,
        fe.NombreCliente,
        fe.NombreCliente as ClienteNombre,
        fe.CorreoCliente,
        fe.CorreoCliente as ClienteEmail,
        fe.TelefonoCliente,
        fe.TelefonoCliente as ClienteTelefono,

        -- Datos de la venta
        v.Fecha as FechaVenta,
        v.MetodoPago as MetodoPagoVenta,

        -- Totales Hacienda
        fe.SubtotalMercanciasGravadas,
        fe.SubtotalMercanciasExentas,
        fe.MontoTotalMercanciasGravadas,
        fe.MontoTotalMercanciasExentas,
        fe.MontoTotalImpuesto,
        fe.TotalComprobante,

        -- Totales de compatibilidad
        fe.SubtotalMercanciasGravadas + fe.SubtotalMercanciasExentas as SubTotal,
        fe.MontoTotalImpuesto as MontoImpuesto,
        fe.TotalComprobante as MontoTotal,

        -- Email
        fe.EmailEnviado,
        fe.FechaEnvioEmail

    FROM FacturaElectronica fe
    INNER JOIN Venta v ON fe.VentaId = v.Id
    WHERE fe.Id = @FacturaId
        AND fe.Estado = 'Activo';

    -- También devolver los detalles de productos
    SELECT
        p.Id as ProductoId,
        p.Codigo as ProductoCodigo,
        p.Nombre as ProductoDescripcion,
        dv.Cantidad,
        dv.PrecioUnitario,
        (dv.Cantidad * dv.PrecioUnitario) as MontoTotal,
        (dv.Cantidad * dv.PrecioUnitario) as BaseImponible,
        CASE WHEN ISNULL(p.Gravado, 1) = 1 THEN 13.0 ELSE 0.0 END as TarifaImpuesto,
        CASE WHEN ISNULL(p.Gravado, 1) = 1 THEN (dv.Cantidad * dv.PrecioUnitario * 0.13) ELSE 0.0 END as MontoImpuestoDetalle,
        ISNULL(p.Gravado, 1) as Gravado
    FROM FacturaElectronica fe
    INNER JOIN Venta v ON fe.VentaId = v.Id
    INNER JOIN DetalleVenta dv ON v.Id = dv.VentaId
    INNER JOIN Producto p ON dv.ProductoId = p.Id
    WHERE fe.Id = @FacturaId
        AND fe.Estado = 'Activo'
    ORDER BY p.Nombre;
END
GO

-- =============================================
-- STORED PROCEDURES DE PEDIDOS
-- =============================================

-- =============================================
-- SPs de soporte para Facturación Electrónica (faltantes)
-- =============================================

-- Actualizar estado en Hacienda para una factura electrónica
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ActualizarEstadoFacturaHacienda')
    DROP PROCEDURE ActualizarEstadoFacturaHacienda;
GO
CREATE PROCEDURE ActualizarEstadoFacturaHacienda
    @IdFactura INT,
    @EstadoHacienda NVARCHAR(50),
    @MensajeHacienda NVARCHAR(MAX) = NULL,
    @XMLGenerado NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Actualizar encabezado de factura
    UPDATE FacturaElectronica
    SET 
        EstadoHacienda = @EstadoHacienda,
        MensajeHacienda = COALESCE(@MensajeHacienda, MensajeHacienda),
        FechaRespuestaHacienda = GETDATE(),
        ModificadoPor = 'Sistema',
        FechaModificacion = GETDATE()
    WHERE Id = @IdFactura;

    -- Registrar historial del cambio
    INSERT INTO HistorialFacturacionElectronica (IdFactura, TipoEvento, Detalle, Estado, Mensaje, FechaEvento)
    VALUES (@IdFactura, 'Hacienda', 'Actualización de estado', @EstadoHacienda, ISNULL(@MensajeHacienda, 'Actualización automática'), GETDATE());
END
GO

-- Registrar evento de envío (Email u otros) asociado a la factura
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'RegistrarEnvioFactura')
    DROP PROCEDURE RegistrarEnvioFactura;
GO
CREATE PROCEDURE RegistrarEnvioFactura
    @IdFactura INT,
    @TipoEnvio NVARCHAR(50),
    @Destinatario NVARCHAR(255) = NULL,
    @EstadoEnvio NVARCHAR(50) = 'Enviado',
    @MensajeRespuesta NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Marcar envío de email si corresponde
    IF @TipoEnvio = 'Email'
    BEGIN
        UPDATE FacturaElectronica
        SET EmailEnviado = CASE WHEN @EstadoEnvio IN ('Enviado','Exitoso') THEN 1 ELSE EmailEnviado END,
            FechaEnvioEmail = GETDATE(),
            ModificadoPor = 'Sistema',
            FechaModificacion = GETDATE()
        WHERE Id = @IdFactura;
    END

    -- Insertar en historial
    INSERT INTO HistorialFacturacionElectronica (IdFactura, TipoEvento, Detalle, Estado, Mensaje, FechaEvento)
    VALUES (@IdFactura, @TipoEnvio, @Destinatario, @EstadoEnvio, @MensajeRespuesta, GETDATE());
END
GO

-- =============================================
-- TIPOS Y STORED PROCEDURES DE VENTAS (FALTANTES)
-- =============================================

-- Tipo de tabla para registrar detalles de venta
IF NOT EXISTS (
    SELECT 1 FROM sys.types st
    JOIN sys.schemas ss ON st.schema_id = ss.schema_id
    WHERE st.name = 'TipoDetalleVenta' AND ss.name = 'dbo'
)
BEGIN
    CREATE TYPE dbo.TipoDetalleVenta AS TABLE (
        ProductoId INT NOT NULL,
        Cantidad   INT NOT NULL,
        PrecioUnitario DECIMAL(10,2) NOT NULL
    );
    PRINT 'Tipo TipoDetalleVenta creado';
END
GO

-- Registrar venta a partir de TVP
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'RegistrarVenta') AND type = 'P')
    DROP PROCEDURE RegistrarVenta;
GO
CREATE PROCEDURE RegistrarVenta
    @MetodoPago NVARCHAR(50),
    @DetallesVenta dbo.TipoDetalleVenta READONLY,
    @PedidoId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @VentaId INT;
    DECLARE @UsuarioId INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Obtener un UsuarioId válido (fallback a cualquier usuario activo)
        SELECT TOP 1 @UsuarioId = u.Id
        FROM Usuario u
        WHERE u.Activo = 1
        ORDER BY CASE WHEN u.Email = 'pepe@pepe' THEN 0 ELSE 1 END, u.Id;

        IF @UsuarioId IS NULL
        BEGIN
            -- Si no hay usuarios activos, tomar cualquier usuario disponible
            SELECT TOP 1 @UsuarioId = u.Id FROM Usuario u ORDER BY u.Id;
        END

        -- Crear encabezado de venta con totales en 0
        INSERT INTO Venta (Fecha, UsuarioId, Cliente, Subtotal, Impuesto, Descuento, Total, MetodoPago, Estado, Observaciones, PedidoId)
        VALUES (GETDATE(), @UsuarioId, NULL, 0, 0, 0, 0, @MetodoPago, N'Completada', NULL, @PedidoId);
        SET @VentaId = SCOPE_IDENTITY();

        -- Insertar detalles
        INSERT INTO DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal)
        SELECT 
            @VentaId,
            dv.ProductoId,
            dv.Cantidad,
            dv.PrecioUnitario,
            0 AS Descuento,
            CASE WHEN ISNULL(p.Gravado,1)=1 THEN (dv.Cantidad * dv.PrecioUnitario * 0.13) ELSE 0 END AS Impuesto,
            (dv.Cantidad * dv.PrecioUnitario) AS Subtotal
        FROM @DetallesVenta dv
        INNER JOIN Producto p ON p.Id = dv.ProductoId;

        -- Recalcular totales
        UPDATE v SET 
            Subtotal = x.Subtotal,
            Impuesto = x.Impuesto,
            Total    = x.Subtotal + x.Impuesto
        FROM Venta v
        CROSS APPLY (
            SELECT 
                SUM(dv.Cantidad*dv.PrecioUnitario) AS Subtotal,
                SUM(CASE WHEN ISNULL(p.Gravado,1)=1 THEN dv.Cantidad*dv.PrecioUnitario*0.13 ELSE 0 END) AS Impuesto
            FROM DetalleVenta dv
            INNER JOIN Producto p ON p.Id = dv.ProductoId
            WHERE dv.VentaId = v.Id
        ) x
        WHERE v.Id = @VentaId;

        -- Si la venta corresponde a un pedido, marcarlo como Entregado
        IF @PedidoId IS NOT NULL
        BEGIN
            UPDATE Pedido
            SET Estado = 'Entregado',
                FechaActualizacion = GETDATE(),
                FechaFinalizacion = COALESCE(FechaFinalizacion, GETDATE())
            WHERE Id = @PedidoId;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Buscar ventas (filtros por fecha, método, id)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'BuscarVentas') AND type = 'P')
    DROP PROCEDURE BuscarVentas;
GO
CREATE PROCEDURE BuscarVentas
    @FechaInicio DATETIME = NULL,
    @FechaFin    DATETIME = NULL,
    @MetodoPago  NVARCHAR(50) = NULL,
    @VentaId     INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
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
    ORDER BY v.Fecha DESC, v.Id DESC;
END
GO

-- Obtener detalle completo de una venta
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'ObtenerDetalleVenta') AND type = 'P')
    DROP PROCEDURE ObtenerDetalleVenta;
GO
CREATE PROCEDURE ObtenerDetalleVenta
    @VentaId INT
AS
BEGIN
    SET NOCOUNT ON;
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
    INNER JOIN Producto p ON dv.ProductoId = p.Id
    WHERE v.Id = @VentaId
    ORDER BY dv.ProductoId;
END
GO

-- Crear tipo de tabla para detalles de pedido
IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = 'TipoDetallePedido' AND ss.name = 'dbo')
BEGIN
    CREATE TYPE TipoDetallePedido AS TABLE (
        ProductoId INT,
        Cantidad INT,
        PrecioUnitario DECIMAL(10,2),
        ObservacionesItem NVARCHAR(200)
    );
    PRINT 'Tipo TipoDetallePedido creado';
END
GO

-- Stored procedure para registrar pedidos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'RegistrarPedido') AND type = 'P')
    DROP PROCEDURE RegistrarPedido;
GO

CREATE PROCEDURE RegistrarPedido
    @UsuarioId INT,
    @Cliente NVARCHAR(255) = NULL,
    @Mesa NVARCHAR(20) = NULL,
    @TipoPedido NVARCHAR(50),
    @TiempoEstimado INT = 30, -- Tiempo estimado en minutos (por defecto 30 min)
    @Observaciones NVARCHAR(500) = NULL,
    @DetallesPedido TipoDetallePedido READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PedidoId INT;
    DECLARE @NumeroPedido NVARCHAR(20);
    DECLARE @Subtotal DECIMAL(10,2) = 0;
    DECLARE @Impuesto DECIMAL(10,2) = 0;
    DECLARE @Total DECIMAL(10,2) = 0;
    DECLARE @FechaEstimadaEntrega DATETIME;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Calcular fecha estimada de entrega
        SET @FechaEstimadaEntrega = DATEADD(MINUTE, @TiempoEstimado, GETDATE());

        -- Generar número de pedido único
        DECLARE @Contador INT;
        SELECT @Contador = ISNULL(MAX(CAST(SUBSTRING(NumeroPedido, 4, LEN(NumeroPedido)) AS INT)), 0) + 1
        FROM Pedido
        WHERE NumeroPedido LIKE 'PED%' AND ISNUMERIC(SUBSTRING(NumeroPedido, 4, LEN(NumeroPedido))) = 1;

        SET @NumeroPedido = 'PED' + RIGHT('00000' + CAST(@Contador AS NVARCHAR), 5);

        -- Insertar pedido
        INSERT INTO Pedido (NumeroPedido, UsuarioId, Cliente, Mesa, TipoPedido, Estado, TiempoEstimado,
                           FechaEstimadaEntrega, Observaciones, FechaInicioPreparacion)
        VALUES (@NumeroPedido, @UsuarioId, @Cliente, @Mesa, @TipoPedido, 'En preparación', @TiempoEstimado,
                @FechaEstimadaEntrega, @Observaciones, GETDATE());

        SET @PedidoId = SCOPE_IDENTITY();

        -- Insertar detalles del pedido y calcular totales
        DECLARE @ProductoId INT, @Cantidad INT, @PrecioUnitario DECIMAL(10,2), @ObservacionesItem NVARCHAR(200);
        DECLARE @SubtotalItem DECIMAL(10,2), @ImpuestoItem DECIMAL(10,2);
        DECLARE @EsGravado BIT;

        DECLARE detalle_cursor CURSOR FOR
        SELECT ProductoId, Cantidad, PrecioUnitario, ObservacionesItem
        FROM @DetallesPedido;

        OPEN detalle_cursor;
        FETCH NEXT FROM detalle_cursor INTO @ProductoId, @Cantidad, @PrecioUnitario, @ObservacionesItem;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Verificar si el producto está gravado
            SELECT @EsGravado = ISNULL(Gravado, 1) FROM Producto WHERE Id = @ProductoId;

            SET @SubtotalItem = @Cantidad * @PrecioUnitario;
            SET @ImpuestoItem = CASE WHEN @EsGravado = 1 THEN @SubtotalItem * 0.13 ELSE 0 END;

            -- Insertar detalle del pedido
            INSERT INTO DetallePedido (PedidoId, ProductoId, Cantidad, PrecioUnitario, Impuesto, Subtotal, ObservacionesItem)
            VALUES (@PedidoId, @ProductoId, @Cantidad, @PrecioUnitario, @ImpuestoItem, @SubtotalItem, @ObservacionesItem);

            -- Acumular totales
            SET @Subtotal = @Subtotal + @SubtotalItem;
            SET @Impuesto = @Impuesto + @ImpuestoItem;

            FETCH NEXT FROM detalle_cursor INTO @ProductoId, @Cantidad, @PrecioUnitario, @ObservacionesItem;
        END

        CLOSE detalle_cursor;
        DEALLOCATE detalle_cursor;

        SET @Total = @Subtotal + @Impuesto;

        -- Actualizar totales del pedido
        UPDATE Pedido
        SET Subtotal = @Subtotal, Impuesto = @Impuesto, Total = @Total, FechaActualizacion = GETDATE()
        WHERE Id = @PedidoId;

        COMMIT TRANSACTION;

        -- Retornar información del pedido creado
        SELECT @PedidoId as PedidoId, @NumeroPedido as NumeroPedido, @FechaEstimadaEntrega as FechaEstimadaEntrega,
               'Pedido registrado correctamente' as Mensaje;

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Stored procedure para actualizar estado de pedido con notificaciones
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'ActualizarEstadoPedido') AND type = 'P')
    DROP PROCEDURE ActualizarEstadoPedido;
GO

CREATE PROCEDURE ActualizarEstadoPedido
    @PedidoId INT,
    @NuevoEstado NVARCHAR(50),
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EstadoAnterior NVARCHAR(50);
    DECLARE @UsuarioMesero INT;
    DECLARE @NumeroPedido NVARCHAR(20);
    DECLARE @TiempoPreparacion INT;

    BEGIN TRY
        -- Validar que el pedido existe
        IF NOT EXISTS (SELECT 1 FROM Pedido WHERE Id = @PedidoId)
        BEGIN
            RAISERROR('El pedido no existe', 16, 1);
            RETURN;
        END

        -- Validar que no se intente usar este procedimiento para cancelar
        IF @NuevoEstado = 'Cancelado'
        BEGIN
            RAISERROR('Para cancelar pedidos use el procedimiento CancelarPedido', 16, 1);
            RETURN;
        END

        -- Obtener estado anterior y datos del pedido
        SELECT @EstadoAnterior = Estado, @UsuarioMesero = UsuarioId, @NumeroPedido = NumeroPedido
        FROM Pedido
        WHERE Id = @PedidoId;

        -- Validar que el pedido no esté cancelado
        IF @EstadoAnterior = 'Cancelado'
        BEGIN
            RAISERROR('No se puede cambiar el estado de un pedido cancelado', 16, 1);
            RETURN;
        END

        -- Si está cambiando a "Listo", calcular tiempo de preparación
        IF @NuevoEstado = 'Listo'
        BEGIN
            SELECT @TiempoPreparacion = DATEDIFF(MINUTE, FechaInicioPreparacion, GETDATE())
            FROM Pedido
            WHERE Id = @PedidoId;

            -- Actualizar estado con tiempo de preparación y fecha de finalización
            UPDATE Pedido
            SET Estado = @NuevoEstado,
                FechaActualizacion = GETDATE(),
                FechaFinalizacion = GETDATE(),
                TiempoPreparacion = @TiempoPreparacion
            WHERE Id = @PedidoId;

            -- Crear notificación para el mesero
            INSERT INTO NotificacionPedido (PedidoId, UsuarioId, TipoNotificacion, Mensaje)
            VALUES (@PedidoId, @UsuarioMesero, 'Listo',
                    'El pedido ' + @NumeroPedido + ' está listo para servir');
        END
        ELSE IF @NuevoEstado = 'Entregado'
        BEGIN
            -- Actualizar estado a entregado
            UPDATE Pedido
            SET Estado = @NuevoEstado, FechaActualizacion = GETDATE()
            WHERE Id = @PedidoId;
        END
        ELSE
        BEGIN
            -- Actualizar estado normal
            UPDATE Pedido
            SET Estado = @NuevoEstado, FechaActualizacion = GETDATE()
            WHERE Id = @PedidoId;
        END

        SELECT 'Estado actualizado correctamente' as Mensaje, @TiempoPreparacion as TiempoPreparacion;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Stored procedure para buscar pedidos
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'BuscarPedidos') AND type = 'P')
    DROP PROCEDURE BuscarPedidos;
GO

CREATE PROCEDURE BuscarPedidos
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL,
    @Estado NVARCHAR(50) = NULL,
    @TipoPedido NVARCHAR(50) = NULL,
    @PedidoId INT = NULL,
    @UsuarioId INT = NULL, -- Para filtrar pedidos de un mesero específico
    @SoloAtrasados BIT = 0 -- Para obtener solo pedidos atrasados
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id,
        p.NumeroPedido,
        p.Fecha,
        p.Cliente,
        p.Mesa,
        p.TipoPedido,
        p.Estado,
        p.Total,
        p.Observaciones,
        p.TiempoEstimado,
        p.TiempoPreparacion,
        p.FechaEstimadaEntrega,
        p.EsAtrasado,
        p.FechaInicioPreparacion,
        p.FechaFinalizacion,
        p.FechaCancelacion,
        p.MotivoCancelacion,
        u.Nombre as Usuario,
        uc.Nombre as UsuarioCancelacion,
        ua.Nombre as AutorizadoPor,
        COUNT(dp.Id) as CantidadItems,
        -- Calcular tiempo transcurrido y estado de tiempo
        CASE
            WHEN p.Estado = 'Entregado' AND p.FechaFinalizacion IS NOT NULL THEN
                DATEDIFF(MINUTE, p.FechaInicioPreparacion, p.FechaFinalizacion)
            WHEN p.Estado IN ('En preparación', 'Listo') THEN
                DATEDIFF(MINUTE, p.FechaInicioPreparacion, GETDATE())
            ELSE 0
        END as TiempoTranscurrido,
        CASE
            WHEN p.Estado = 'Entregado' THEN 'Completado'
            WHEN p.EsAtrasado = 1 THEN 'Atrasado'
            WHEN p.FechaEstimadaEntrega > GETDATE() THEN 'A tiempo'
            WHEN p.FechaEstimadaEntrega <= GETDATE() AND p.Estado != 'Entregado' THEN 'Atrasado'
            ELSE 'A tiempo'
        END as EstadoTiempo,
        -- Calcular minutos de atraso o tiempo restante
        CASE
            WHEN p.FechaEstimadaEntrega <= GETDATE() AND p.Estado != 'Entregado' THEN
                DATEDIFF(MINUTE, p.FechaEstimadaEntrega, GETDATE())
            WHEN p.FechaEstimadaEntrega > GETDATE() THEN
                DATEDIFF(MINUTE, GETDATE(), p.FechaEstimadaEntrega)
            ELSE 0
        END as MinutosDiferencia
    FROM Pedido p
    LEFT JOIN Usuario u ON p.UsuarioId = u.Id
    LEFT JOIN Usuario uc ON p.UsuarioCancelacion = uc.Id
    LEFT JOIN Usuario ua ON p.AutorizadoPor = ua.Id
    LEFT JOIN DetallePedido dp ON p.Id = dp.PedidoId
    WHERE
        (@FechaInicio IS NULL OR p.Fecha >= @FechaInicio)
        AND (@FechaFin IS NULL OR p.Fecha <= @FechaFin)
        AND (
            @Estado IS NULL 
            OR p.Estado = @Estado 
            -- Compatibilidad por datos con codificación inválida ('En preparaciÃ³n')
            OR (@Estado = N'En preparación' AND p.Estado = N'En preparaciÃ³n')
        )
        AND (@TipoPedido IS NULL OR p.TipoPedido = @TipoPedido)
        AND (@PedidoId IS NULL OR p.Id = @PedidoId)
        AND (@UsuarioId IS NULL OR p.UsuarioId = @UsuarioId)
        AND (@SoloAtrasados = 0 OR p.EsAtrasado = 1)
    GROUP BY p.Id, p.NumeroPedido, p.Fecha, p.Cliente, p.Mesa, p.TipoPedido, p.Estado, p.Total,
             p.Observaciones, p.TiempoEstimado, p.TiempoPreparacion, p.FechaEstimadaEntrega,
             p.EsAtrasado, p.FechaInicioPreparacion, p.FechaFinalizacion, p.FechaCancelacion, 
             p.MotivoCancelacion, u.Nombre, uc.Nombre, ua.Nombre
    ORDER BY
        CASE WHEN p.EsAtrasado = 1 THEN 0 ELSE 1 END, -- Atrasados primero
        p.FechaEstimadaEntrega ASC, -- Luego por tiempo estimado
        p.Fecha DESC;
END
GO

-- Stored procedure para detectar y notificar pedidos atrasados
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'DetectarPedidosAtrasados') AND type = 'P')
    DROP PROCEDURE DetectarPedidosAtrasados;
GO

CREATE PROCEDURE DetectarPedidosAtrasados
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PedidoId INT, @UsuarioMesero INT, @NumeroPedido NVARCHAR(20), @MinutosAtraso INT;

    -- Cursor para pedidos atrasados
    DECLARE pedidos_atrasados CURSOR FOR
    SELECT Id, UsuarioId, NumeroPedido,
           DATEDIFF(MINUTE, FechaEstimadaEntrega, GETDATE()) as MinutosAtraso
    FROM Pedido
    WHERE Estado IN (N'En preparación', N'En preparaciÃ³n')
      AND FechaEstimadaEntrega < GETDATE()
      AND EsAtrasado = 0; -- Solo los que no han sido marcados como atrasados

    OPEN pedidos_atrasados;
    FETCH NEXT FROM pedidos_atrasados INTO @PedidoId, @UsuarioMesero, @NumeroPedido, @MinutosAtraso;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Marcar como atrasado
        UPDATE Pedido
        SET EsAtrasado = 1, FechaActualizacion = GETDATE()
        WHERE Id = @PedidoId;

        -- Crear notificación para el mesero
        INSERT INTO NotificacionPedido (PedidoId, UsuarioId, TipoNotificacion, Mensaje)
        VALUES (@PedidoId, @UsuarioMesero, 'Atrasado',
                'ALERTA: El pedido ' + @NumeroPedido + ' está atrasado por ' + CAST(@MinutosAtraso AS NVARCHAR) + ' minutos');

        -- Crear notificación para jefes de cocina (usuarios con rol Admin)
        INSERT INTO NotificacionPedido (PedidoId, UsuarioId, TipoNotificacion, Mensaje)
        SELECT @PedidoId, u.Id, 'Atrasado',
               'ALERTA COCINA: Pedido ' + @NumeroPedido + ' atrasado ' + CAST(@MinutosAtraso AS NVARCHAR) + ' min'
        FROM Usuario u
        INNER JOIN Rol r ON u.RolId = r.Id
        WHERE r.Nombre = 'Admin';

        FETCH NEXT FROM pedidos_atrasados INTO @PedidoId, @UsuarioMesero, @NumeroPedido, @MinutosAtraso;
    END

    CLOSE pedidos_atrasados;
    DEALLOCATE pedidos_atrasados;

    -- Retornar cantidad de pedidos atrasados detectados
    SELECT @@ROWCOUNT as PedidosAtrasadosDetectados;
END
GO

-- Stored procedure para obtener notificaciones de un usuario
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'ObtenerNotificacionesUsuario') AND type = 'P')
    DROP PROCEDURE ObtenerNotificacionesUsuario;
GO

CREATE PROCEDURE ObtenerNotificacionesUsuario
    @UsuarioId INT,
    @SoloNoLeidas BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        n.Id,
        n.PedidoId,
        p.NumeroPedido,
        n.TipoNotificacion,
        n.Mensaje,
        n.Leida,
        n.FechaCreacion,
        n.FechaLectura,
        p.Estado as EstadoPedido,
        p.Mesa,
        p.Cliente
    FROM NotificacionPedido n
    INNER JOIN Pedido p ON n.PedidoId = p.Id
    WHERE n.UsuarioId = @UsuarioId
      AND (@SoloNoLeidas = 0 OR n.Leida = 0)
    ORDER BY n.FechaCreacion DESC;
END
GO

-- Stored procedure para marcar notificación como leída
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'MarcarNotificacionLeida') AND type = 'P')
    DROP PROCEDURE MarcarNotificacionLeida;
GO

CREATE PROCEDURE MarcarNotificacionLeida
    @NotificacionId INT,
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE NotificacionPedido
    SET Leida = 1, FechaLectura = GETDATE()
    WHERE Id = @NotificacionId AND UsuarioId = @UsuarioId;

    SELECT 'Notificación marcada como leída' as Mensaje;
END
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'ObtenerDetallePedido') AND type = 'P')
    DROP PROCEDURE ObtenerDetallePedido;
GO

CREATE PROCEDURE ObtenerDetallePedido
    @PedidoId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Información del pedido
    SELECT
        p.Id,
        p.NumeroPedido,
        p.Fecha,
        p.Cliente,
        p.Mesa,
        p.TipoPedido,
        p.Estado,
        p.Subtotal,
        p.Impuesto,
        p.Total,
        p.Observaciones,
        p.FechaCancelacion,
        p.MotivoCancelacion,
        u.Nombre as Usuario,
        uc.Nombre as UsuarioCancelacion,
        ua.Nombre as AutorizadoPor
    FROM Pedido p
    LEFT JOIN Usuario u ON p.UsuarioId = u.Id
    LEFT JOIN Usuario uc ON p.UsuarioCancelacion = uc.Id
    LEFT JOIN Usuario ua ON p.AutorizadoPor = ua.Id
    WHERE p.Id = @PedidoId;

    -- Detalles del pedido
    SELECT
        dp.Id,
        dp.ProductoId,
        p.Codigo as ProductoCodigo,
        p.Nombre as ProductoNombre,
        dp.Cantidad,
        dp.PrecioUnitario,
        dp.Subtotal,
        dp.Impuesto,
        dp.ObservacionesItem
    FROM DetallePedido dp
    INNER JOIN Producto p ON dp.ProductoId = p.Id
    WHERE dp.PedidoId = @PedidoId
    ORDER BY p.Nombre;
END
GO

-- =============================================
-- STORED PROCEDURES PED-004: CANCELACIÓN DE PEDIDOS
-- =============================================

-- Stored procedure para cancelar pedidos con lógica de autorización
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'CancelarPedido') AND type = 'P')
    DROP PROCEDURE CancelarPedido;
GO

CREATE PROCEDURE CancelarPedido
    @PedidoId INT,
    @UsuarioId INT, -- Usuario que solicita la cancelación
    @MotivoCancelacion NVARCHAR(500),
    @UsuarioAutorizacion INT = NULL -- Usuario que autoriza (requerido si ya inició preparación)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EstadoActual NVARCHAR(50);
    DECLARE @FechaInicioPreparacion DATETIME;
    DECLARE @NumeroPedido NVARCHAR(20);
    DECLARE @UsuarioMesero INT;
    DECLARE @RequiereAutorizacion BIT = 0;
    DECLARE @RolAutorizador NVARCHAR(50);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validar que el pedido existe
        IF NOT EXISTS (SELECT 1 FROM Pedido WHERE Id = @PedidoId)
        BEGIN
            RAISERROR('El pedido especificado no existe', 16, 1);
            RETURN;
        END

        -- Obtener información del pedido
        SELECT 
            @EstadoActual = Estado,
            @FechaInicioPreparacion = FechaInicioPreparacion,
            @NumeroPedido = NumeroPedido,
            @UsuarioMesero = UsuarioId
        FROM Pedido 
        WHERE Id = @PedidoId;

        -- Validar que el pedido no esté ya cancelado o completado
        IF @EstadoActual IN ('Cancelado', 'Entregado')
        BEGIN
            RAISERROR('No se puede cancelar un pedido que ya está %s', 16, 1, @EstadoActual);
            RETURN;
        END

        -- Determinar si requiere autorización
        -- Escenario 1: Sin preparación iniciada = No requiere autorización
        -- Escenario 2: Con preparación iniciada = Requiere autorización de Admin
        IF @FechaInicioPreparacion IS NOT NULL
        BEGIN
            SET @RequiereAutorizacion = 1;
            
            -- Validar que se proporcionó usuario de autorización
            IF @UsuarioAutorizacion IS NULL
            BEGIN
                RAISERROR('Este pedido ya inició preparación y requiere autorización de un administrador para ser cancelado', 16, 1);
                RETURN;
            END

            -- Validar que el usuario autorizador existe y tiene rol de Admin
            SELECT @RolAutorizador = r.Nombre
            FROM Usuario u
            INNER JOIN Rol r ON u.RolId = r.Id
            WHERE u.Id = @UsuarioAutorizacion AND u.Activo = 1;

            IF @RolAutorizador IS NULL
            BEGIN
                RAISERROR('El usuario autorizador no existe o está inactivo', 16, 1);
                RETURN;
            END

            IF @RolAutorizador != 'Admin'
            BEGIN
                RAISERROR('Solo los administradores pueden autorizar la cancelación de pedidos en preparación', 16, 1);
                RETURN;
            END
        END

        -- Realizar la cancelación
        UPDATE Pedido
        SET 
            Estado = 'Cancelado',
            FechaCancelacion = GETDATE(),
            MotivoCancelacion = @MotivoCancelacion,
            UsuarioCancelacion = @UsuarioId,
            AutorizadoPor = @UsuarioAutorizacion,
            FechaActualizacion = GETDATE(),
            FechaFinalizacion = GETDATE()
        WHERE Id = @PedidoId;

        -- Crear notificación para el mesero original
        INSERT INTO NotificacionPedido (PedidoId, UsuarioId, TipoNotificacion, Mensaje)
        VALUES (@PedidoId, @UsuarioMesero, 'Cancelado',
                'El pedido ' + @NumeroPedido + ' ha sido cancelado. Motivo: ' + @MotivoCancelacion);

        -- Crear notificación para cocina (usuarios Admin) si ya había iniciado preparación
        IF @RequiereAutorizacion = 1
        BEGIN
            INSERT INTO NotificacionPedido (PedidoId, UsuarioId, TipoNotificacion, Mensaje)
            SELECT @PedidoId, u.Id, 'Cancelado',
                   'CANCELACIÓN AUTORIZADA: Pedido ' + @NumeroPedido + ' cancelado durante preparación'
            FROM Usuario u
            INNER JOIN Rol r ON u.RolId = r.Id
            WHERE r.Nombre = 'Admin' AND u.Id != @UsuarioAutorizacion;
        END
        ELSE
        BEGIN
            -- Notificar a cocina sobre cancelación temprana
            INSERT INTO NotificacionPedido (PedidoId, UsuarioId, TipoNotificacion, Mensaje)
            SELECT @PedidoId, u.Id, 'Cancelado',
                   'Pedido ' + @NumeroPedido + ' cancelado antes de iniciar preparación'
            FROM Usuario u
            INNER JOIN Rol r ON u.RolId = r.Id
            WHERE r.Nombre = 'Admin';
        END

        COMMIT TRANSACTION;

        -- Retornar resultado exitoso
        SELECT 
            'Pedido cancelado correctamente' as Mensaje,
            @RequiereAutorizacion as RequirioAutorizacion,
            CASE WHEN @RequiereAutorizacion = 1 THEN 'Cancelación autorizada por administrador' 
                 ELSE 'Cancelación sin autorización requerida' END as TipoCancelacion,
            @NumeroPedido as NumeroPedido,
            GETDATE() as FechaCancelacion;

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- Stored procedure para verificar si un pedido puede ser cancelado
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'VerificarCancelacionPedido') AND type = 'P')
    DROP PROCEDURE VerificarCancelacionPedido;
GO

CREATE PROCEDURE VerificarCancelacionPedido
    @PedidoId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EstadoActual NVARCHAR(50);
    DECLARE @FechaInicioPreparacion DATETIME;
    DECLARE @NumeroPedido NVARCHAR(20);

    -- Validar que el pedido existe
    IF NOT EXISTS (SELECT 1 FROM Pedido WHERE Id = @PedidoId)
    BEGIN
        SELECT 
            0 as PuedeCancelarse,
            'El pedido no existe' as Mensaje,
            NULL as RequiereAutorizacion,
            NULL as EstadoActual,
            NULL as NumeroPedido;
        RETURN;
    END

    -- Obtener información del pedido
    SELECT 
        @EstadoActual = Estado,
        @FechaInicioPreparacion = FechaInicioPreparacion,
        @NumeroPedido = NumeroPedido
    FROM Pedido 
    WHERE Id = @PedidoId;

    -- Verificar si se puede cancelar
    IF @EstadoActual IN ('Cancelado', 'Entregado')
    BEGIN
        SELECT 
            0 as PuedeCancelarse,
            'El pedido ya está ' + @EstadoActual as Mensaje,
            NULL as RequiereAutorizacion,
            @EstadoActual as EstadoActual,
            @NumeroPedido as NumeroPedido;
    END
    ELSE
    BEGIN
        SELECT 
            1 as PuedeCancelarse,
            CASE WHEN @FechaInicioPreparacion IS NULL 
                 THEN 'El pedido puede cancelarse sin autorización'
                 ELSE 'El pedido puede cancelarse con autorización de administrador' END as Mensaje,
            CASE WHEN @FechaInicioPreparacion IS NULL THEN 0 ELSE 1 END as RequiereAutorizacion,
            @EstadoActual as EstadoActual,
            @NumeroPedido as NumeroPedido;
    END
END
GO

-- =============================================
-- DATOS INICIALES
-- =============================================

-- Hotfix: normalizar posibles estados mal codificados por problemas de página de códigos
-- Esto corrige filas antiguas donde 'En preparación' quedó como 'En preparaciÃ³n'
UPDATE Pedido SET Estado = N'En preparación' WHERE Estado = N'En preparaciÃ³n';

-- Insertar usuario administrador por defecto si no existe
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'admin@antojeria.com')
BEGIN
    INSERT INTO Usuario (Nombre, Email, Password, RolId, Activo)
    VALUES ('Administrador', 'admin@antojeria.com', '$2a$12$tSK4PGHN.MJWqn3QB9hSm.r47IyY2q7212SdPsa4c3z1Kf/LE9bDa', 1, 1);
    PRINT 'Usuario administrador creado con contraseña: admin123';
END

-- Insertar usuario de prueba para login si no existe
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'pepe@pepe')
BEGIN
    -- Generar hash BCrypt para la contraseña 'pepe123'
    -- Para pruebas, usaremos un hash simple, pero en producción debe ser BCrypt
    INSERT INTO Usuario (Nombre, Email, Password, RolId, Activo)
    VALUES ('Pepe Usuario', 'pepe@pepe', '$2a$12$qtb6J12SO7AG32l9HZjhs.OtT.oV8C6sWt4H1E.0vJ/5kgazCmsSu', 2, 1);
    PRINT 'Usuario de prueba pepe@pepe creado con contraseña: pepe123';
END

-- Insertar algunos productos de ejemplo si la tabla está vacía
IF NOT EXISTS (SELECT 1 FROM Producto)
BEGIN
    INSERT INTO Producto (Codigo, Nombre, Descripcion, Precio, Categoria, Stock, StockMinimo, Gravado) VALUES
    ('P001', 'Gallo Pinto', 'Plato tradicional costarricense', 2500.00, 'Comida', 50, 5, 1),
    ('P002', 'Casado', 'Casado completo', 3500.00, 'Comida', 30, 5, 1),
    ('P003', 'Refresco Natural', 'Refresco de frutas naturales', 800.00, 'Bebidas', 100, 10, 1),
    ('P004', 'Café', 'Café costarricense', 600.00, 'Bebidas', 80, 10, 1),
    ('P005', 'Arroz con Leche', 'Postre tradicional', 1200.00, 'Postres', 25, 5, 0), -- Exento
    ('P006', 'Leche', 'Leche pasteurizada', 1000.00, 'Básicos', 40, 10, 0); -- Exento

    PRINT 'Productos de ejemplo insertados';
END

-- =============================================
-- STORED PROCEDURE PARA SEGUIMIENTO AVANZADO - PED-002
-- =============================================

-- Eliminar y recrear ObtenerPedidosConSeguimiento si existe
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'ObtenerPedidosConSeguimiento') AND type = 'P')
    DROP PROCEDURE ObtenerPedidosConSeguimiento;
GO

CREATE PROCEDURE ObtenerPedidosConSeguimiento
    @UsuarioId INT = NULL,
    @SoloAtrasados BIT = 0
AS
BEGIN
    SELECT 
        p.Id,
        p.NumeroPedido,
        p.Fecha,
        p.Cliente,
        p.Mesa,
        p.TipoPedido,
        p.Estado,
        p.TiempoEstimado,
        p.FechaEstimadaEntrega,
        p.EsAtrasado,
        p.Total,
        p.Observaciones,
        p.FechaInicioPreparacion,
        p.FechaFinalizacion,
        u.Nombre AS Usuario,
        p.UsuarioId,
        -- Cálculo del tiempo transcurrido en minutos
        CASE 
            WHEN p.Estado = 'Entregado' AND p.FechaFinalizacion IS NOT NULL 
            THEN DATEDIFF(MINUTE, p.FechaInicioPreparacion, p.FechaFinalizacion)
            WHEN p.FechaInicioPreparacion IS NOT NULL 
            THEN DATEDIFF(MINUTE, p.FechaInicioPreparacion, GETDATE())
            ELSE DATEDIFF(MINUTE, p.Fecha, GETDATE())
        END AS TiempoTranscurrido,
        
        -- Cantidad de items en el pedido
        ISNULL((SELECT COUNT(*) FROM DetallePedido dp WHERE dp.PedidoId = p.Id), 0) AS CantidadItems,
        
        -- Estado del tiempo (A tiempo, Atrasado, Completado)
        CASE 
            WHEN p.Estado = 'Entregado' THEN 'Completado'
            WHEN p.Estado = 'Cancelado' THEN 'Cancelado'
            WHEN p.TiempoEstimado IS NULL THEN 'Sin estimación'
            WHEN p.FechaInicioPreparacion IS NULL THEN 'No iniciado'
            WHEN DATEDIFF(MINUTE, p.FechaInicioPreparacion, GETDATE()) > p.TiempoEstimado THEN 'Atrasado'
            ELSE 'A tiempo'
        END AS EstadoTiempo,
        
        -- Diferencia en minutos (positivo = atrasado, negativo = a tiempo)
        CASE 
            WHEN p.TiempoEstimado IS NOT NULL AND p.FechaInicioPreparacion IS NOT NULL
            THEN DATEDIFF(MINUTE, p.FechaInicioPreparacion, GETDATE()) - p.TiempoEstimado
            ELSE 0
        END AS MinutosDiferencia

    FROM Pedido p
    INNER JOIN Usuario u ON p.UsuarioId = u.Id
    WHERE 
        p.Estado IN ('En preparación', 'Listo') -- Solo pedidos activos
        AND (@UsuarioId IS NULL OR p.UsuarioId = @UsuarioId)
        AND (@SoloAtrasados = 0 OR p.EsAtrasado = 1)
    ORDER BY 
        p.EsAtrasado DESC, -- Atrasados primero
        p.Fecha ASC -- Más antiguos primero
END
GO

PRINT 'Stored procedure ObtenerPedidosConSeguimiento creado para PED-002';

-- =============================================
-- VERIFICACIÓN FINAL DE STORED PROCEDURES
-- =============================================

-- Verificar que todos los stored procedures requeridos existen
PRINT '=============================================';
PRINT 'VERIFICANDO STORED PROCEDURES REQUERIDOS:';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_InsertarUsuario')
    PRINT '✓ sp_InsertarUsuario existe';
ELSE
    PRINT '✗ sp_InsertarUsuario NO existe';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerUsuario')
    PRINT '✓ sp_ObtenerUsuario existe';
ELSE
    PRINT '✗ sp_ObtenerUsuario NO existe';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ObtenerUsuarios')
    PRINT '✓ sp_ObtenerUsuarios existe';
ELSE
    PRINT '✗ sp_ObtenerUsuarios NO existe';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ActualizarUsuario')
    PRINT '✓ sp_ActualizarUsuario existe';
ELSE
    PRINT '✗ sp_ActualizarUsuario NO existe';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_EliminarUsuario')
    PRINT '✓ sp_EliminarUsuario existe';
ELSE
    PRINT '✗ sp_EliminarUsuario NO existe';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ListarRoles')
    PRINT '✓ sp_ListarRoles existe';
ELSE
    PRINT '✗ sp_ListarRoles NO existe';

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_EliminarRol')
    PRINT '✓ sp_EliminarRol existe';
ELSE
    PRINT '✗ sp_EliminarRol NO existe';

-- Mostrar estructura actual de la tabla Usuario para confirmar
PRINT '=============================================';
PRINT 'ESTRUCTURA DE LA TABLA USUARIO:';
SELECT 
    COLUMN_NAME as 'Columna',
    DATA_TYPE as 'Tipo',
    IS_NULLABLE as 'Permite NULL',
    COLUMN_DEFAULT as 'Valor por defecto'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Usuario' 
ORDER BY ORDINAL_POSITION;

PRINT '=============================================';
PRINT 'USUARIOS DISPONIBLES PARA PRUEBAS:';
SELECT u.Email, u.Nombre, 
       CASE WHEN u.Activo = 1 THEN 'Activo' ELSE 'Inactivo' END as Estado,
       r.Nombre as Rol
FROM Usuario u
INNER JOIN Rol r ON u.RolId = r.Id
ORDER BY u.Email;

PRINT '=============================================';

-- =============================================
-- MENSAJE FINAL
-- =============================================
PRINT '=========================================';
PRINT 'Base de datos AntojeriaTica configurada completamente';
PRINT 'Incluye:';
PRINT '- Tablas básicas del sistema';
PRINT '- Sistema de pedidos completo con seguimiento PED-002';
PRINT '- Sistema de cancelación de pedidos PED-004';
PRINT '- Facturación electrónica completa';
PRINT '- Stored procedures optimizados';
PRINT '- Datos iniciales';
PRINT '=========================================';



--- MR--001

GO
CREATE OR ALTER PROCEDURE sp_ReporteVentasAnual
    @Anio INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        MONTH(Fecha) AS Mes,
        DATENAME(MONTH, Fecha) AS NombreMes,
        SUM(Total) AS TotalVentas,
        COUNT(*) AS CantidadVentas
    FROM Venta
    WHERE YEAR(Fecha) = @Anio
    GROUP BY MONTH(Fecha), DATENAME(MONTH, Fecha)
    ORDER BY Mes;
END

----INCLUIR DATOS EN EL AÑO 2024

INSERT INTO Venta (Fecha, UsuarioId, Cliente, Subtotal, Impuesto, Descuento, Total, MetodoPago, Estado, Observaciones) VALUES
('2024-01-15', 2, N'Cliente mostrador', 20000, 2600, 0, 22600, N'Efectivo',     N'Completada', N'Venta de refrescos y casados'),
('2024-02-10', 2, N'Cliente mostrador', 18000, 2340, 0, 20340, N'Tarjeta',      N'Completada', N'Venta de desayunos'),
('2024-03-12', 2, N'Cliente mostrador', 25000, 3250, 0, 28250, N'Transferencia',N'Completada', N'Venta de almuerzos'),
('2024-04-08', 2, N'Cliente mostrador', 22000, 2860, 0, 24860, N'SINPE Móvil',  N'Completada', N'Venta variada'),
('2024-05-22', 2, N'Cliente mostrador', 18000, 2340, 0, 20340, N'Efectivo',     N'Completada', N'Postres y café'),
('2024-06-05', 2, N'Cliente mostrador', 30000, 3900, 0, 33900, N'Tarjeta',      N'Completada', N'Cenas'),
('2024-07-18', 2, N'Cliente mostrador', 21000, 2730, 0, 23730, N'Efectivo',     N'Completada', N'Merienda del día'),
('2024-08-09', 2, N'Cliente mostrador', 26000, 3380, 0, 29380, N'Transferencia',N'Completada', N'Almuerzos'),
('2024-09-14', 2, N'Cliente mostrador', 19500, 2535, 0, 22035, N'SINPE Móvil',  N'Completada', N'Desayunos'),
('2024-10-03', 2, N'Cliente mostrador', 28000, 3640, 0, 31640, N'Efectivo',     N'Completada', N'Venta general'),
('2024-11-18', 2, N'Cliente mostrador', 22000, 2860, 0, 24860, N'Tarjeta',      N'Completada', N'Café y postres'),
('2024-12-22', 2, N'Cliente mostrador', 35000, 4550, 0, 39550, N'Efectivo',     N'Completada', N'Temporada alta');
GO

-- Verificar ventas de Pepe en 2024
SELECT * 
FROM Venta
WHERE UsuarioId = 2 AND YEAR(Fecha) = 2024
ORDER BY Fecha;


--- MR--002 

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_DashboardVentas')
BEGIN
    DROP PROCEDURE sp_DashboardVentas;
END
GO

CREATE PROCEDURE sp_DashboardVentas
AS
BEGIN
    SET NOCOUNT ON;

    
    SELECT 
        ISNULL(SUM(Total), 0) AS TotalVentasHoy,
        COUNT(*) AS CantidadPedidosHoy,
        (
            SELECT ISNULL(SUM(Total),0)
            FROM Venta
            WHERE Fecha >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))
        ) AS TendenciaSemana
    FROM Venta
    WHERE CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE);


    SELECT 
        CONVERT(VARCHAR(10), CAST(Fecha AS DATE), 103) AS Dia,
        ISNULL(SUM(Total), 0) AS Total
    FROM Venta
    WHERE Fecha >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))
    GROUP BY CAST(Fecha AS DATE)
    ORDER BY CAST(Fecha AS DATE);
END
GO