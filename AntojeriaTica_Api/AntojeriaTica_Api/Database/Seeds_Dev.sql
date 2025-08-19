-- Seeds_Dev.sql - Datos de ejemplo para pruebas locales/DEV
-- Ejecutar contra la BD AntojeriaTica

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @UsuarioPepe INT = (SELECT TOP 1 Id FROM Usuario WHERE Email = 'pepe@pepe');
    IF @UsuarioPepe IS NULL
        SET @UsuarioPepe = (SELECT TOP 1 Id FROM Usuario ORDER BY Id);

    DECLARE @ProdP001 INT = (SELECT TOP 1 Id FROM Producto WHERE Codigo = 'P001');
    DECLARE @ProdP002 INT = (SELECT TOP 1 Id FROM Producto WHERE Codigo = 'P002');
    DECLARE @ProdP003 INT = (SELECT TOP 1 Id FROM Producto WHERE Codigo = 'P003');
    DECLARE @ProdP004 INT = (SELECT TOP 1 Id FROM Producto WHERE Codigo = 'P004');

    -- Normalizar nombre con acento por si quedó mal codificado previamente
    UPDATE Producto SET Nombre = N'Café' WHERE Codigo = 'P004' AND Nombre <> N'Café';

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

    -- Venta de AYER si no existe
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

    -- ============================
    -- Pedidos de ejemplo (si no hay pedidos hoy)
    -- ============================
    IF NOT EXISTS (SELECT 1 FROM Pedido WHERE CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE))
    BEGIN
        DECLARE @Cont INT;
        SELECT @Cont = ISNULL(MAX(CAST(SUBSTRING(NumeroPedido, 4, LEN(NumeroPedido)) AS INT)), 0)
        FROM Pedido
        WHERE NumeroPedido LIKE 'PED%'
          AND ISNUMERIC(SUBSTRING(NumeroPedido, 4, LEN(NumeroPedido))) = 1;

        -- Pedido 1: En preparación (no atrasado, estimado 30 min)
        DECLARE @Ped1 INT; SET @Cont = @Cont + 1;
        DECLARE @Num1 NVARCHAR(20) = 'PED' + RIGHT('00000' + CAST(@Cont AS NVARCHAR), 5);
        INSERT INTO Pedido (NumeroPedido, Fecha, UsuarioId, Cliente, Mesa, TipoPedido, Estado, TiempoEstimado,
                            FechaEstimadaEntrega, Observaciones, Subtotal, Impuesto, Descuento, Total,
                            FechaInicioPreparacion)
        VALUES (@Num1, GETDATE(), @UsuarioPepe, N'Mostrador', '1', 'Mesa', N'En preparación', 30,
                DATEADD(MINUTE, 30, GETDATE()), N'Pedido demo en preparación', 0, 0, 0, 0,
                GETDATE());
        SET @Ped1 = SCOPE_IDENTITY();

        IF @ProdP001 IS NOT NULL
            INSERT INTO DetallePedido (PedidoId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal, ObservacionesItem)
            SELECT @Ped1, @ProdP001, 1, Precio, 0, CASE WHEN ISNULL(Gravado,1)=1 THEN Precio*0.13 ELSE 0 END, Precio, N''
            FROM Producto WHERE Id=@ProdP001;
        IF @ProdP003 IS NOT NULL
            INSERT INTO DetallePedido (PedidoId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal, ObservacionesItem)
            SELECT @Ped1, @ProdP003, 2, Precio, 0, CASE WHEN ISNULL(Gravado,1)=1 THEN Precio*0.13 ELSE 0 END, Precio*2, N'Sin hielo'
            FROM Producto WHERE Id=@ProdP003;

        UPDATE p SET 
            Subtotal = x.Subtotal,
            Impuesto = x.Impuesto,
            Total = x.Subtotal + x.Impuesto
        FROM Pedido p
        CROSS APPLY (
            SELECT 
                SUM(dp.Cantidad*dp.PrecioUnitario) AS Subtotal,
                SUM(CASE WHEN ISNULL(pr.Gravado,1)=1 THEN dp.Cantidad*dp.PrecioUnitario*0.13 ELSE 0 END) AS Impuesto
            FROM DetallePedido dp
            LEFT JOIN Producto pr ON pr.Id = dp.ProductoId
            WHERE dp.PedidoId = p.Id
        ) x
        WHERE p.Id = @Ped1;

        -- Pedido 2: Listo (no entregado)
        DECLARE @Ped2 INT; SET @Cont = @Cont + 1;
        DECLARE @Num2 NVARCHAR(20) = 'PED' + RIGHT('00000' + CAST(@Cont AS NVARCHAR), 5);
        INSERT INTO Pedido (NumeroPedido, Fecha, UsuarioId, Cliente, Mesa, TipoPedido, Estado, TiempoEstimado,
                            FechaEstimadaEntrega, Observaciones, Subtotal, Impuesto, Descuento, Total,
                            FechaInicioPreparacion)
        VALUES (@Num2, GETDATE(), @UsuarioPepe, N'Cliente 2', '2', 'Mesa', N'Listo', 20,
                DATEADD(MINUTE, 20, GETDATE()), N'Pedido listo para servir', 0, 0, 0, 0,
                DATEADD(MINUTE, -15, GETDATE()));
        SET @Ped2 = SCOPE_IDENTITY();
        IF @ProdP002 IS NOT NULL
            INSERT INTO DetallePedido (PedidoId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal)
            SELECT @Ped2, @ProdP002, 1, Precio, 0, CASE WHEN ISNULL(Gravado,1)=1 THEN Precio*0.13 ELSE 0 END, Precio
            FROM Producto WHERE Id=@ProdP002;
        UPDATE p SET 
            Subtotal = x.Subtotal,
            Impuesto = x.Impuesto,
            Total = x.Subtotal + x.Impuesto
        FROM Pedido p
        CROSS APPLY (
            SELECT 
                SUM(dp.Cantidad*dp.PrecioUnitario) AS Subtotal,
                SUM(CASE WHEN ISNULL(pr.Gravado,1)=1 THEN dp.Cantidad*dp.PrecioUnitario*0.13 ELSE 0 END) AS Impuesto
            FROM DetallePedido dp
            LEFT JOIN Producto pr ON pr.Id = dp.ProductoId
            WHERE dp.PedidoId = p.Id
        ) x
        WHERE p.Id = @Ped2;

        -- Pedido 3: Entregado (completado)
        DECLARE @Ped3 INT; SET @Cont = @Cont + 1;
        DECLARE @Num3 NVARCHAR(20) = 'PED' + RIGHT('00000' + CAST(@Cont AS NVARCHAR), 5);
        INSERT INTO Pedido (NumeroPedido, Fecha, UsuarioId, Cliente, Mesa, TipoPedido, Estado, TiempoEstimado,
                            FechaEstimadaEntrega, Observaciones, Subtotal, Impuesto, Descuento, Total,
                            FechaInicioPreparacion, FechaFinalizacion, TiempoPreparacion)
        VALUES (@Num3, GETDATE(), @UsuarioPepe, N'Cliente 3', '3', 'Mesa', N'Entregado', 15,
                DATEADD(MINUTE, 15, GETDATE()), N'Pedido entregado', 0, 0, 0, 0,
                DATEADD(MINUTE, -20, GETDATE()), GETDATE(), 20);
        SET @Ped3 = SCOPE_IDENTITY();
        IF @ProdP004 IS NOT NULL
            INSERT INTO DetallePedido (PedidoId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal)
            SELECT @Ped3, @ProdP004, 2, Precio, 0, CASE WHEN ISNULL(Gravado,1)=1 THEN Precio*0.13 ELSE 0 END, Precio*2
            FROM Producto WHERE Id=@ProdP004;
        UPDATE p SET 
            Subtotal = x.Subtotal,
            Impuesto = x.Impuesto,
            Total = x.Subtotal + x.Impuesto
        FROM Pedido p
        CROSS APPLY (
            SELECT 
                SUM(dp.Cantidad*dp.PrecioUnitario) AS Subtotal,
                SUM(CASE WHEN ISNULL(pr.Gravado,1)=1 THEN dp.Cantidad*dp.PrecioUnitario*0.13 ELSE 0 END) AS Impuesto
            FROM DetallePedido dp
            LEFT JOIN Producto pr ON pr.Id = dp.ProductoId
            WHERE dp.PedidoId = p.Id
        ) x
        WHERE p.Id = @Ped3;

        -- Pedido 4: Cancelado (ayer), requiere autorizacion si ya inició
        IF NOT EXISTS (SELECT 1 FROM Pedido WHERE CAST(Fecha AS DATE) = CAST(DATEADD(DAY,-1,GETDATE()) AS DATE) AND Estado='Cancelado')
        BEGIN
            DECLARE @Ped4 INT; SET @Cont = @Cont + 1;
            DECLARE @Num4 NVARCHAR(20) = 'PED' + RIGHT('00000' + CAST(@Cont AS NVARCHAR), 5);
            INSERT INTO Pedido (NumeroPedido, Fecha, UsuarioId, Cliente, Mesa, TipoPedido, Estado, TiempoEstimado,
                                FechaEstimadaEntrega, Observaciones, Subtotal, Impuesto, Descuento, Total,
                                FechaInicioPreparacion, FechaCancelacion, MotivoCancelacion, UsuarioCancelacion)
            VALUES (@Num4, DATEADD(DAY,-1,GETDATE()), @UsuarioPepe, N'Cliente 4', '4', 'Mesa', N'Cancelado', 25,
                    DATEADD(MINUTE, 25, DATEADD(DAY,-1,GETDATE())), N'Pedido cancelado', 0, 0, 0, 0,
                    DATEADD(DAY,-1,GETDATE()), DATEADD(DAY,-1,GETDATE()), N'Cliente se retiró', @UsuarioPepe);
            SET @Ped4 = SCOPE_IDENTITY();
            IF @ProdP001 IS NOT NULL
                INSERT INTO DetallePedido (PedidoId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal)
                SELECT @Ped4, @ProdP001, 1, Precio, 0, CASE WHEN ISNULL(Gravado,1)=1 THEN Precio*0.13 ELSE 0 END, Precio
                FROM Producto WHERE Id=@ProdP001;
            UPDATE p SET 
                Subtotal = x.Subtotal,
                Impuesto = x.Impuesto,
                Total = x.Subtotal + x.Impuesto
            FROM Pedido p
            CROSS APPLY (
                SELECT 
                    SUM(dp.Cantidad*dp.PrecioUnitario) AS Subtotal,
                    SUM(CASE WHEN ISNULL(pr.Gravado,1)=1 THEN dp.Cantidad*dp.PrecioUnitario*0.13 ELSE 0 END) AS Impuesto
                FROM DetallePedido dp
                LEFT JOIN Producto pr ON pr.Id = dp.ProductoId
                WHERE dp.PedidoId = p.Id
            ) x
            WHERE p.Id = @Ped4;
        END

        -- Marcar uno como atrasado (Ped1) si la hora estimada ya pasó
        UPDATE Pedido SET EsAtrasado = CASE WHEN FechaEstimadaEntrega < GETDATE() AND Estado <> 'Entregado' THEN 1 ELSE 0 END
        WHERE Id IN (@Ped1, @Ped2);
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @msg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR('Seeds_Dev error: %s', 16, 1, @msg);
END CATCH

-- Verificaciones rápidas
DECLARE @f DATE = CAST(GETDATE() AS DATE);
SELECT @f AS FechaHoy, COUNT(*) AS VentasHoy
FROM Venta WHERE CAST(Fecha AS DATE) = @f;

EXEC ReporteVentasDia @Fecha = @f;
EXEC sp_CierreCajaDiario;
