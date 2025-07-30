using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliMarket.Models.Entities;
using BCrypt.Net;

namespace PoliMarket.Components.Infrastructure.Data;

/// <summary>
/// Database seeder for PoliMarket system
/// Seeds realistic test data for complete business workflow
/// </summary>
public class DatabaseSeeder
{
    private readonly PoliMarketDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(PoliMarketDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the database with comprehensive test data
    /// Always runs on startup and seeds missing components (production-safe)
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Always check and seed missing components individually
            // This ensures the system can start even if some components are missing

            // Seed core entities first (no dependencies)
            await SafeSeedAsync("HR Employees", SeedHREmployeesAsync);
            await SafeSeedAsync("Sellers", SeedSellersAsync);
            await SafeSeedAsync("Users", SeedUsersAsync);
            await SafeSeedAsync("Customers", SeedCustomersAsync);
            await SafeSeedAsync("Products", SeedProductsAsync);

            // Save changes after core entities to ensure foreign keys exist
            await _context.SaveChangesAsync();
            _logger.LogInformation("Core entities seeded successfully");

            // Now seed dependent entities (require foreign keys)
            await SafeSeedAsync("Sales", SeedSalesAsync);
            await SafeSeedAsync("Inventory Movements", SeedInventoryMovementsAsync);
            await SafeSeedAsync("Suppliers", SeedSuppliersAsync);
            await SafeSeedAsync("Deliveries", SeedDeliveriesAsync);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Dependent entities seeded successfully");

            // Verify critical components are present
            await VerifySeededDataAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database seeding");
            // Don't throw - allow the application to start even if seeding fails
            _logger.LogWarning("Application will continue without complete seed data");
        }
    }

    /// <summary>
    /// Verifies that critical seed data is present after seeding
    /// </summary>
    private async Task VerifySeededDataAsync()
    {
        try
        {
            var hrCount = await _context.EmpleadosRH.CountAsync();
            var sellerCount = await _context.Vendedores.CountAsync();
            var authorizedSellerCount = await _context.Vendedores.CountAsync(v => v.Autorizado);
            var customerCount = await _context.Clientes.CountAsync();
            var productCount = await _context.Productos.CountAsync();
            var salesCount = await _context.Ventas.CountAsync();
            var hasDemoSeller = await _context.Vendedores.AnyAsync(v => v.CodigoVendedor == "DEMO");

            _logger.LogInformation($"Seed data verification: HR={hrCount}, Sellers={sellerCount}, Authorized={authorizedSellerCount}, Customers={customerCount}, Products={productCount}, Sales={salesCount}, Demo={hasDemoSeller}");

            // Log warnings for missing critical data
            if (hrCount == 0) _logger.LogWarning("No HR employees found - authorization may not work");
            if (authorizedSellerCount == 0) _logger.LogWarning("No authorized sellers found - sales may not work");
            if (customerCount == 0) _logger.LogWarning("No customers found - sales may not work");
            if (productCount == 0) _logger.LogWarning("No products found - sales may not work");
            if (!hasDemoSeller) _logger.LogWarning("DEMO seller not found - testing may be difficult");

            // Ensure minimum required data exists
            if (hrCount > 0 && authorizedSellerCount > 0 && customerCount > 0 && productCount > 0)
            {
                _logger.LogInformation("✅ Database seeding verification passed - all critical components present");
            }
            else
            {
                _logger.LogWarning("⚠️ Database seeding verification incomplete - some components may be missing");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verifying seeded data");
        }
    }

    /// <summary>
    /// Safely executes a seeding method with error handling
    /// </summary>
    private async Task SafeSeedAsync(string componentName, Func<Task> seedMethod)
    {
        try
        {
            _logger.LogInformation($"Starting to seed {componentName}...");
            await seedMethod();
            _logger.LogInformation($"Successfully seeded {componentName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error seeding {componentName}: {ex.Message}");
            // TEMPORARILY: Re-throw to see what's failing
            throw;
        }
    }



    private async Task SeedHREmployeesAsync()
    {
        _logger.LogInformation("Seeding HR employees...");

        // Check if specific HR employee exists (our key indicator for proper seeding)
        if (await _context.EmpleadosRH.AnyAsync(e => e.Id == "HR001"))
        {
            _logger.LogInformation("HR001 employee exists. Skipping HR seeding.");
            return;
        }

        var hrEmployees = new List<EmpleadoRH>
        {
            new EmpleadoRH
            {
                Id = "HR001",
                Nombre = "Ana García Rodríguez",
                Cargo = "Gerente de Recursos Humanos",
                Departamento = "Recursos Humanos",
                Email = "ana.garcia@polimarket.com",
                Telefono = "+57 300 123 4567",
                FechaIngreso = DateTime.UtcNow.AddYears(-3),
                Activo = true
            },
            new EmpleadoRH
            {
                Id = "HR002",
                Nombre = "Carlos López Martínez",
                Cargo = "Analista de Recursos Humanos",
                Departamento = "Recursos Humanos",
                Email = "carlos.lopez@polimarket.com",
                Telefono = "+57 300 234 5678",
                FechaIngreso = DateTime.UtcNow.AddYears(-2),
                Activo = true
            },
            new EmpleadoRH
            {
                Id = "HR003",
                Nombre = "María Elena Vargas",
                Cargo = "Coordinadora de Selección",
                Departamento = "Recursos Humanos",
                Email = "maria.vargas@polimarket.com",
                Telefono = "+57 300 345 6789",
                FechaIngreso = DateTime.UtcNow.AddYears(-1),
                Activo = true
            },
            new EmpleadoRH
            {
                Id = "HR004",
                Nombre = "Jorge Andrés Ruiz",
                Cargo = "Especialista en Capacitación",
                Departamento = "Recursos Humanos",
                Email = "jorge.ruiz@polimarket.com",
                Telefono = "+57 300 456 7890",
                FechaIngreso = DateTime.UtcNow.AddMonths(-8),
                Activo = true
            },
            new EmpleadoRH
            {
                Id = "HR005",
                Nombre = "Laura Patricia Sánchez",
                Cargo = "Asistente de RH",
                Departamento = "Recursos Humanos",
                Email = "laura.sanchez@polimarket.com",
                Telefono = "+57 300 567 8901",
                FechaIngreso = DateTime.UtcNow.AddMonths(-6),
                Activo = true
            }
        };

        await _context.EmpleadosRH.AddRangeAsync(hrEmployees);
        _logger.LogInformation($"Added {hrEmployees.Count} HR employees");
    }

    private async Task SeedSellersAsync()
    {
        _logger.LogInformation("Seeding sellers...");

        // Check if DEMO seller exists (our key indicator for proper seeding)
        if (await _context.Vendedores.AnyAsync(v => v.CodigoVendedor == "DEMO"))
        {
            _logger.LogInformation("DEMO seller exists. Skipping sellers seeding.");
            return;
        }

        var sellers = new List<Vendedor>
        {
            // Authorized Sellers
            new Vendedor
            {
                CodigoVendedor = "V001",
                Nombre = "Juan Carlos Pérez",
                Territorio = "Bogotá Norte",
                Comision = 5.5,
                Autorizado = true,
                FechaAutorizacion = DateTime.UtcNow.AddMonths(-2),
                EmpleadoRHAutorizo = "HR001",
                Activo = true
            },
            new Vendedor
            {
                CodigoVendedor = "V002",
                Nombre = "Sandra Milena Torres",
                Territorio = "Bogotá Sur",
                Comision = 6.0,
                Autorizado = true,
                FechaAutorizacion = DateTime.UtcNow.AddMonths(-2),
                EmpleadoRHAutorizo = "HR001",
                Activo = true
            },
            new Vendedor
            {
                CodigoVendedor = "V003",
                Nombre = "Miguel Ángel Ramírez",
                Territorio = "Medellín Centro",
                Comision = 5.8,
                Autorizado = true,
                FechaAutorizacion = DateTime.UtcNow.AddMonths(-1),
                EmpleadoRHAutorizo = "HR002",
                Activo = true
            },
            new Vendedor
            {
                CodigoVendedor = "V004",
                Nombre = "Diana Carolina Herrera",
                Territorio = "Cali Valle",
                Comision = 6.2,
                Autorizado = true,
                FechaAutorizacion = DateTime.UtcNow.AddMonths(-1),
                EmpleadoRHAutorizo = "HR002",
                Activo = true
            },
            new Vendedor
            {
                CodigoVendedor = "V005",
                Nombre = "Andrés Felipe Morales",
                Territorio = "Barranquilla Atlántico",
                Comision = 5.9,
                Autorizado = true,
                FechaAutorizacion = DateTime.UtcNow.AddDays(-21), // 3 weeks ago
                EmpleadoRHAutorizo = "HR003",
                Activo = true
            },
            // Pending Authorization
            new Vendedor
            {
                CodigoVendedor = "V006",
                Nombre = "Claudia Patricia Jiménez",
                Territorio = "Cartagena Bolívar",
                Comision = 5.7,
                Autorizado = false,
                FechaAutorizacion = DateTime.MinValue,
                EmpleadoRHAutorizo = "",
                Activo = true
            },
            new Vendedor
            {
                CodigoVendedor = "V007",
                Nombre = "Roberto Carlos Mendoza",
                Territorio = "Bucaramanga Santander",
                Comision = 6.1,
                Autorizado = false,
                FechaAutorizacion = DateTime.MinValue,
                EmpleadoRHAutorizo = "",
                Activo = true
            },
            new Vendedor
            {
                CodigoVendedor = "V008",
                Nombre = "Paola Andrea Castillo",
                Territorio = "Pereira Risaralda",
                Comision = 5.6,
                Autorizado = false,
                FechaAutorizacion = DateTime.MinValue,
                EmpleadoRHAutorizo = "",
                Activo = true
            },
            // Demo seller for immediate testing
            new Vendedor
            {
                CodigoVendedor = "DEMO",
                Nombre = "Vendedor Demo",
                Territorio = "Nacional",
                Comision = 5.0,
                Autorizado = true,
                FechaAutorizacion = DateTime.UtcNow,
                EmpleadoRHAutorizo = "HR001",
                Activo = true
            }
        };

        await _context.Vendedores.AddRangeAsync(sellers);
        _logger.LogInformation($"Added {sellers.Count} sellers");
    }

    private async Task SeedUsersAsync()
    {
        _logger.LogInformation("Seeding users...");

        // Check if specific test users already exist (instead of any users)
        var existingTestUsers = await _context.Usuarios
            .Where(u => u.Username == "juan.perez" || u.Username == "sandra.torres" ||
                       u.Username == "ana.garcia" || u.Username == "carlos.lopez")
            .CountAsync();

        if (existingTestUsers >= 4)
        {
            _logger.LogInformation("Test users already exist. Skipping additional users seeding.");
            return;
        }

        var users = new List<Usuario>
        {
            // HR Users
            new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Username = "ana.garcia",
                Email = "ana.garcia@polimarket.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Nombre = "Ana García",
                Apellido = "Rodríguez",
                Rol = UserRole.HRManager,
                EmpleadoRHId = "HR001",
                Activo = true
            },
            new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Username = "carlos.lopez",
                Email = "carlos.lopez@polimarket.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Nombre = "Carlos López",
                Apellido = "Martínez",
                Rol = UserRole.HRManager,
                EmpleadoRHId = "HR002",
                Activo = true
            },
            // Seller Users (only for authorized sellers)
            new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Username = "juan.perez",
                Email = "juan.perez@polimarket.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Nombre = "Juan Carlos",
                Apellido = "Pérez",
                Rol = UserRole.SalesRep,
                VendedorId = "V001",
                Activo = true
            },
            new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Username = "sandra.torres",
                Email = "sandra.torres@polimarket.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Nombre = "Sandra Milena",
                Apellido = "Torres",
                Rol = UserRole.SalesRep,
                VendedorId = "V002",
                Activo = true
            }
        };

        await _context.Usuarios.AddRangeAsync(users);
        _logger.LogInformation($"Added {users.Count} users");
    }

    private async Task SeedCustomersAsync()
    {
        _logger.LogInformation("Seeding customers...");

        // Check if specific customer exists (our key indicator for proper seeding)
        if (await _context.Clientes.AnyAsync(c => c.Id == "C001"))
        {
            _logger.LogInformation("C001 customer exists. Skipping customers seeding.");
            return;
        }

        var customers = new List<Cliente>
        {
            new Cliente
            {
                Id = "C001",
                Nombre = "Supermercados La Economía S.A.S",
                Direccion = "Calle 45 #23-67, Bogotá",
                Telefono = "+57 1 234 5678",
                Email = "compras@laeconomia.com",
                TipoCliente = "Corporativo",
                LimiteCredito = 50000000,
                Activo = true
            },
            new Cliente
            {
                Id = "C002",
                Nombre = "Distribuidora El Mayorista Ltda",
                Direccion = "Carrera 15 #78-90, Medellín",
                Telefono = "+57 4 345 6789",
                Email = "pedidos@elmayorista.com",
                TipoCliente = "Mayorista",
                LimiteCredito = 30000000,
                Activo = true
            },
            new Cliente
            {
                Id = "C003",
                Nombre = "Tienda Don Pepe",
                Direccion = "Calle 12 #34-56, Cali",
                Telefono = "+57 2 456 7890",
                Email = "donpepe@gmail.com",
                TipoCliente = "Minorista",
                LimiteCredito = 5000000,
                Activo = true
            },
            new Cliente
            {
                Id = "C004",
                Nombre = "Almacenes Éxito Regional",
                Direccion = "Avenida 80 #45-23, Barranquilla",
                Telefono = "+57 5 567 8901",
                Email = "compras.regional@exito.com",
                TipoCliente = "Corporativo",
                LimiteCredito = 80000000,
                Activo = true
            },
            new Cliente
            {
                Id = "C005",
                Nombre = "Mercado Central de Abastos",
                Direccion = "Plaza de Mercado, Bucaramanga",
                Telefono = "+57 7 678 9012",
                Email = "administracion@mercadocentral.com",
                TipoCliente = "Mayorista",
                LimiteCredito = 25000000,
                Activo = true
            }
        };

        await _context.Clientes.AddRangeAsync(customers);
        _logger.LogInformation($"Added {customers.Count} customers");
    }

    private async Task SeedProductsAsync()
    {
        _logger.LogInformation("Seeding products...");

        // Check if specific product exists (our key indicator for proper seeding)
        if (await _context.Productos.AnyAsync(p => p.Id == "P001"))
        {
            _logger.LogInformation("P001 product exists. Skipping products seeding.");
            return;
        }

        var products = new List<Producto>
        {
            // Alimentos y Bebidas
            new Producto
            {
                Id = "P001",
                Nombre = "Arroz Diana Premium 500g",
                Descripcion = "Arroz blanco de alta calidad, grano largo",
                Precio = 3500,
                Categoria = "Alimentos Básicos",
                Stock = 150,
                StockMinimo = 20,
                StockMaximo = 500,
                UnidadMedida = "Paquete",
                Estado = true
            },
            new Producto
            {
                Id = "P002",
                Nombre = "Aceite Girasol 1L",
                Descripcion = "Aceite de girasol refinado para cocina",
                Precio = 8900,
                Categoria = "Alimentos Básicos",
                Stock = 80,
                StockMinimo = 15,
                StockMaximo = 200,
                UnidadMedida = "Botella",
                Estado = true
            },
            new Producto
            {
                Id = "P003",
                Nombre = "Leche Entera Alpina 1L",
                Descripcion = "Leche entera pasteurizada",
                Precio = 4200,
                Categoria = "Lácteos",
                Stock = 120,
                StockMinimo = 25,
                StockMaximo = 300,
                UnidadMedida = "Tetrapack",
                Estado = true
            },
            new Producto
            {
                Id = "P004",
                Nombre = "Pan Tajado Bimbo 450g",
                Descripcion = "Pan de molde tajado integral",
                Precio = 5600,
                Categoria = "Panadería",
                Stock = 60,
                StockMinimo = 10,
                StockMaximo = 150,
                UnidadMedida = "Paquete",
                Estado = true
            },
            new Producto
            {
                Id = "P005",
                Nombre = "Coca Cola 2L",
                Descripcion = "Bebida gaseosa sabor cola",
                Precio = 6800,
                Categoria = "Bebidas",
                Stock = 200,
                StockMinimo = 30,
                StockMaximo = 400,
                UnidadMedida = "Botella",
                Estado = true
            },
            // Productos de Limpieza
            new Producto
            {
                Id = "P006",
                Nombre = "Detergente Ariel 1kg",
                Descripcion = "Detergente en polvo para ropa",
                Precio = 12500,
                Categoria = "Limpieza",
                Stock = 90,
                StockMinimo = 15,
                StockMaximo = 200,
                UnidadMedida = "Caja",
                Estado = true
            },
            new Producto
            {
                Id = "P007",
                Nombre = "Jabón Rey 300g",
                Descripcion = "Jabón de tocador antibacterial",
                Precio = 2800,
                Categoria = "Aseo Personal",
                Stock = 180,
                StockMinimo = 25,
                StockMaximo = 350,
                UnidadMedida = "Barra",
                Estado = true
            },
            new Producto
            {
                Id = "P008",
                Nombre = "Papel Higiénico Scott 4 rollos",
                Descripcion = "Papel higiénico doble hoja",
                Precio = 8900,
                Categoria = "Aseo Personal",
                Stock = 75,
                StockMinimo = 12,
                StockMaximo = 180,
                UnidadMedida = "Paquete",
                Estado = true
            }
        };

        await _context.Productos.AddRangeAsync(products);
        _logger.LogInformation($"Added {products.Count} products");
    }

    private async Task SeedSalesAsync()
    {
        _logger.LogInformation("Seeding sales...");

        // Check if sales already exist
        if (await _context.Ventas.AnyAsync())
        {
            _logger.LogInformation("Sales already exist. Skipping sales seeding.");
            return;
        }

        // Verify that required entities exist before creating sales
        var customerCount = await _context.Clientes.CountAsync();
        var sellerCount = await _context.Vendedores.CountAsync(v => v.Autorizado);
        var productCount = await _context.Productos.CountAsync();

        if (customerCount == 0 || sellerCount == 0 || productCount == 0)
        {
            _logger.LogWarning($"Cannot seed sales - missing dependencies: Customers={customerCount}, Sellers={sellerCount}, Products={productCount}");
            return;
        }

        var sales = new List<Venta>
        {
            new Venta
            {
                Id = "S001",
                Fecha = DateTime.UtcNow.AddDays(-15),
                IdCliente = "C001",
                IdVendedor = "V001",
                Total = 125000,
                Estado = "Procesada",
                Observaciones = "Pedido mensual supermercado",
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta { IdProducto = "P001", Cantidad = 20, Precio = 3500, Descuento = 0 },
                    new DetalleVenta { IdProducto = "P002", Cantidad = 5, Precio = 8900, Descuento = 200 },
                    new DetalleVenta { IdProducto = "P003", Cantidad = 10, Precio = 4200, Descuento = 0 }
                }
            },
            new Venta
            {
                Id = "S002",
                Fecha = DateTime.UtcNow.AddDays(-12),
                IdCliente = "C002",
                IdVendedor = "V002",
                Total = 89500,
                Estado = "Procesada",
                Observaciones = "Reposición semanal",
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta { IdProducto = "P004", Cantidad = 8, Precio = 5600, Descuento = 0 },
                    new DetalleVenta { IdProducto = "P005", Cantidad = 6, Precio = 6800, Descuento = 300 },
                    new DetalleVenta { IdProducto = "P006", Cantidad = 2, Precio = 12500, Descuento = 0 }
                }
            },
            new Venta
            {
                Id = "S003",
                Fecha = DateTime.UtcNow.AddDays(-8),
                IdCliente = "C003",
                IdVendedor = "V003",
                Total = 45600,
                Estado = "Procesada",
                Observaciones = "Pedido especial fin de semana",
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta { IdProducto = "P007", Cantidad = 12, Precio = 2800, Descuento = 0 },
                    new DetalleVenta { IdProducto = "P008", Cantidad = 2, Precio = 8900, Descuento = 100 }
                }
            },
            new Venta
            {
                Id = "S004",
                Fecha = DateTime.UtcNow.AddDays(-5),
                IdCliente = "C004",
                IdVendedor = "V004",
                Total = 256000,
                Estado = "Procesada",
                Observaciones = "Pedido corporativo mensual",
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta { IdProducto = "P001", Cantidad = 30, Precio = 3500, Descuento = 500 },
                    new DetalleVenta { IdProducto = "P003", Cantidad = 25, Precio = 4200, Descuento = 0 },
                    new DetalleVenta { IdProducto = "P005", Cantidad = 15, Precio = 6800, Descuento = 200 }
                }
            },
            new Venta
            {
                Id = "S005",
                Fecha = DateTime.UtcNow.AddDays(-2),
                IdCliente = "C005",
                IdVendedor = "V005",
                Total = 78900,
                Estado = "Procesada",
                Observaciones = "Reposición urgente",
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta { IdProducto = "P002", Cantidad = 4, Precio = 8900, Descuento = 0 },
                    new DetalleVenta { IdProducto = "P006", Cantidad = 3, Precio = 12500, Descuento = 400 }
                }
            },
            // Demo sales for immediate testing
            new Venta
            {
                Id = "DEMO001",
                Fecha = DateTime.UtcNow.AddDays(-1),
                IdCliente = "C001",
                IdVendedor = "DEMO",
                Total = 125000,
                Estado = "Procesada",
                Observaciones = "Venta demo - Productos básicos",
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta { IdProducto = "P001", Cantidad = 10, Precio = 3500, Descuento = 0 },
                    new DetalleVenta { IdProducto = "P003", Cantidad = 15, Precio = 4200, Descuento = 0 },
                    new DetalleVenta { IdProducto = "P005", Cantidad = 5, Precio = 6800, Descuento = 0 }
                }
            },
            new Venta
            {
                Id = "DEMO002",
                Fecha = DateTime.UtcNow.AddHours(-6),
                IdCliente = "C002",
                IdVendedor = "DEMO",
                Total = 89600,
                Estado = "Procesada",
                Observaciones = "Venta demo - Productos de limpieza",
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta { IdProducto = "P006", Cantidad = 5, Precio = 12500, Descuento = 0 },
                    new DetalleVenta { IdProducto = "P007", Cantidad = 8, Precio = 2800, Descuento = 0 },
                    new DetalleVenta { IdProducto = "P008", Cantidad = 2, Precio = 8900, Descuento = 500 }
                }
            }
        };

        await _context.Ventas.AddRangeAsync(sales);
        _logger.LogInformation($"Added {sales.Count} sales");
    }

    private async Task SeedInventoryMovementsAsync()
    {
        _logger.LogInformation("Seeding inventory movements...");

        // Check if inventory movements already exist
        if (await _context.MovimientosInventario.AnyAsync())
        {
            _logger.LogInformation("Inventory movements already exist. Skipping inventory movements seeding.");
            return;
        }

        // Verify that products exist before creating inventory movements
        var productCount = await _context.Productos.CountAsync();
        if (productCount == 0)
        {
            _logger.LogWarning("Cannot seed inventory movements - no products found");
            return;
        }

        var movements = new List<MovimientoInventario>
        {
            // Initial stock entries
            new MovimientoInventario
            {
                IdProducto = "P001",
                TipoMovimiento = "Entrada",
                Cantidad = 200,
                StockAnterior = 0,
                StockNuevo = 200,
                Motivo = "Stock inicial",
                FechaMovimiento = DateTime.UtcNow.AddMonths(-1),
                UsuarioResponsable = "SYSTEM"
            },
            new MovimientoInventario
            {
                IdProducto = "P001",
                TipoMovimiento = "Salida",
                Cantidad = 20,
                StockAnterior = 200,
                StockNuevo = 180,
                Motivo = "Venta S001",
                DocumentoReferencia = "S001",
                FechaMovimiento = DateTime.UtcNow.AddDays(-15),
                UsuarioResponsable = "V001"
            },
            new MovimientoInventario
            {
                IdProducto = "P001",
                TipoMovimiento = "Salida",
                Cantidad = 30,
                StockAnterior = 180,
                StockNuevo = 150,
                Motivo = "Venta S004",
                DocumentoReferencia = "S004",
                FechaMovimiento = DateTime.UtcNow.AddDays(-5),
                UsuarioResponsable = "V004"
            }
        };

        await _context.MovimientosInventario.AddRangeAsync(movements);
        _logger.LogInformation($"Added {movements.Count} inventory movements");
    }

    private async Task SeedSuppliersAsync()
    {
        _logger.LogInformation("Seeding suppliers...");

        // Check if suppliers already exist
        if (await _context.Proveedores.AnyAsync())
        {
            _logger.LogInformation("Suppliers already exist. Skipping suppliers seeding.");
            return;
        }

        var suppliers = new List<Proveedor>
        {
            new Proveedor
            {
                Id = "PROV001",
                Nombre = "Distribuidora Nacional de Alimentos S.A.",
                Contacto = "María Fernanda Gómez",
                Telefono = "+57 1 789 0123",
                Email = "ventas@dnalimentos.com",
                Direccion = "Zona Industrial Bogotá, Calle 13 #45-67",
                TipoProveedor = "Alimentos",
                Calificacion = 4.5,
                Activo = true
            },
            new Proveedor
            {
                Id = "PROV002",
                Nombre = "Productos de Limpieza El Aseo Ltda",
                Contacto = "Carlos Alberto Ruiz",
                Telefono = "+57 4 890 1234",
                Email = "pedidos@elaseo.com",
                Direccion = "Medellín, Carrera 50 #23-45",
                TipoProveedor = "Limpieza",
                Calificacion = 4.2,
                Activo = true
            },
            new Proveedor
            {
                Id = "PROV003",
                Nombre = "Tecnología y Equipos Industriales S.A.S",
                Contacto = "Ana Patricia López",
                Telefono = "+57 2 567 8901",
                Email = "comercial@tecequipos.com",
                Direccion = "Cali, Avenida 6N #28-45",
                TipoProveedor = "Tecnología",
                Calificacion = 4.8,
                Activo = true
            },
            new Proveedor
            {
                Id = "PROV004",
                Nombre = "Textiles y Confecciones del Norte",
                Contacto = "Roberto Martínez",
                Telefono = "+57 5 234 5678",
                Email = "ventas@textilesnorte.com",
                Direccion = "Barranquilla, Carrera 43 #76-123",
                TipoProveedor = "Textiles",
                Calificacion = 4.1,
                Activo = true
            },
            new Proveedor
            {
                Id = "PROV005",
                Nombre = "Farmacéuticos Unidos de Colombia",
                Contacto = "Dr. Luis Fernando Herrera",
                Telefono = "+57 1 345 6789",
                Email = "pedidos@farmaunidos.com",
                Direccion = "Bogotá, Zona Rosa, Calle 85 #15-32",
                TipoProveedor = "Farmacéuticos",
                Calificacion = 4.7,
                Activo = true
            },
            new Proveedor
            {
                Id = "PROV006",
                Nombre = "Construcción y Materiales del Pacífico",
                Contacto = "Ingeniero Miguel Torres",
                Telefono = "+57 2 678 9012",
                Email = "construccion@matpacifico.com",
                Direccion = "Buenaventura, Zona Industrial, Km 5",
                TipoProveedor = "Construcción",
                Calificacion = 4.3,
                Activo = true
            },
            new Proveedor
            {
                Id = "PROV007",
                Nombre = "Automotriz Central Ltda",
                Contacto = "Sandra Milena Castro",
                Telefono = "+57 4 789 0123",
                Email = "repuestos@autocentral.com",
                Direccion = "Medellín Centro, Carrera 52 #45-67",
                TipoProveedor = "Automotriz",
                Calificacion = 4.0,
                Activo = false
            },
            new Proveedor
            {
                Id = "PROV008",
                Nombre = "Bebidas y Licores Premium S.A.",
                Contacto = "Juan Carlos Pérez",
                Telefono = "+57 1 890 1234",
                Email = "distribución@licorpremium.com",
                Direccion = "Bogotá Norte, Calle 127 #45-89",
                TipoProveedor = "Bebidas",
                Calificacion = 4.6,
                Activo = true
            }
        };

        await _context.Proveedores.AddRangeAsync(suppliers);
        _logger.LogInformation($"Added {suppliers.Count} suppliers");
    }

    private async Task SeedDeliveriesAsync()
    {
        _logger.LogInformation("Seeding deliveries...");

        // Check if deliveries already exist
        if (await _context.Entregas.AnyAsync())
        {
            _logger.LogInformation("Deliveries already exist. Skipping deliveries seeding.");
            return;
        }

        var deliveries = new List<Entrega>
        {
            new Entrega
            {
                Id = "E001",
                IdVenta = "S001",
                Direccion = "Calle 45 #23-67, Bogotá",
                Cliente = "Empresa ABC S.A.S",
                FechaProgramada = DateTime.UtcNow.AddDays(-10),
                FechaEntrega = DateTime.UtcNow.AddDays(-10),
                Estado = "Entregada",
                Transportista = "TransExpress S.A.S",
                Observaciones = "Entrega exitosa, recibido por almacén"
            },
            new Entrega
            {
                Id = "E002",
                IdVenta = "S002",
                Direccion = "Carrera 15 #78-90, Medellín",
                Cliente = "Corporación XYZ Ltda",
                FechaProgramada = DateTime.UtcNow.AddDays(-7),
                FechaEntrega = DateTime.UtcNow.AddDays(-7),
                Estado = "Entregada",
                Transportista = "Logística Nacional",
                Observaciones = "Entrega completada"
            },
            new Entrega
            {
                Id = "E003",
                IdVenta = "S003",
                Direccion = "Calle 12 #34-56, Cali",
                Cliente = "Distribuidora Valle",
                FechaProgramada = DateTime.UtcNow.AddDays(-3),
                Estado = "En Tránsito",
                Transportista = "Envíos del Valle",
                Observaciones = "En ruta de entrega"
            }
        };

        await _context.Entregas.AddRangeAsync(deliveries);
        _logger.LogInformation($"Added {deliveries.Count} deliveries");
    }
}
