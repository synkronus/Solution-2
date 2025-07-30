using Microsoft.EntityFrameworkCore;
using PoliMarket.Models.Entities;

namespace PoliMarket.Components.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for PoliMarket system
/// Implements SQLite database with component-based design
/// </summary>
public class PoliMarketDbContext : DbContext
{
    public PoliMarketDbContext(DbContextOptions<PoliMarketDbContext> options) : base(options)
    {
    }

    #region DbSets - Entity Collections

    // Users & Authentication
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }

    // Authorization & HR
    public DbSet<Vendedor> Vendedores { get; set; }
    public DbSet<EmpleadoRH> EmpleadosRH { get; set; }

    // Sales & Customers
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<DetalleVenta> DetallesVenta { get; set; }
    public DbSet<Cliente> Clientes { get; set; }

    // Inventory & Products
    public DbSet<Producto> Productos { get; set; }
    public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
    public DbSet<AlertaStock> AlertasStock { get; set; }

    // Suppliers & Purchase Orders
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<OrdenCompra> OrdenesCompra { get; set; }
    public DbSet<DetalleOrdenCompra> DetallesOrdenCompra { get; set; }

    // Delivery & Logistics
    public DbSet<Entrega> Entregas { get; set; }
    public DbSet<SeguimientoEntrega> SeguimientosEntrega { get; set; }
    public DbSet<RutaEntrega> RutasEntrega { get; set; }
    public DbSet<Transportista> Transportistas { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserEntities(modelBuilder);
        ConfigureAuthorizationEntities(modelBuilder);
        ConfigureSalesEntities(modelBuilder);
        ConfigureInventoryEntities(modelBuilder);
        ConfigureSupplierEntities(modelBuilder);
        ConfigureDeliveryEntities(modelBuilder);

        SeedInitialData(modelBuilder);
    }

    #region Entity Configuration

    private void ConfigureUserEntities(ModelBuilder modelBuilder)
    {
        // Usuario configuration
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Apellido).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Rol).HasConversion<int>();
            entity.Property(e => e.VendedorId).HasMaxLength(20);
            entity.Property(e => e.EmpleadoRHId).HasMaxLength(20);

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            // Relationships
            entity.HasOne(e => e.Vendedor)
                  .WithMany()
                  .HasForeignKey(e => e.VendedorId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.EmpleadoRH)
                  .WithMany()
                  .HasForeignKey(e => e.EmpleadoRHId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // UserSession configuration
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.Property(e => e.IPAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureAuthorizationEntities(ModelBuilder modelBuilder)
    {
        // Vendedor configuration
        modelBuilder.Entity<Vendedor>(entity =>
        {
            entity.HasKey(e => e.CodigoVendedor);
            entity.Property(e => e.CodigoVendedor).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Territorio).HasMaxLength(50);
            entity.Property(e => e.EmpleadoRHAutorizo).HasMaxLength(20);
            
            entity.HasIndex(e => e.CodigoVendedor).IsUnique();
        });

        // EmpleadoRH configuration
        modelBuilder.Entity<EmpleadoRH>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Cargo).HasMaxLength(50);
            entity.Property(e => e.Departamento).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });
    }

    private void ConfigureSalesEntities(ModelBuilder modelBuilder)
    {
        // Venta configuration
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IdCliente).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IdVendedor).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Pendiente");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreadoPor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ActualizadoPor).HasMaxLength(50);

            entity.HasMany(e => e.Detalles)
                  .WithOne(d => d.Venta)
                  .HasForeignKey(d => d.IdVenta)
                  .OnDelete(DeleteBehavior.Cascade);

            // Add foreign key relationships
            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Ventas)
                  .HasForeignKey(e => e.IdCliente)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vendedor)
                  .WithMany()
                  .HasForeignKey(e => e.IdVendedor)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // DetalleVenta configuration
        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IdVenta).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IdProducto).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Descuento).HasColumnType("decimal(18,2)");

            // Add foreign key relationship to Producto
            entity.HasOne(e => e.Producto)
                  .WithMany()
                  .HasForeignKey(e => e.IdProducto)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Cliente configuration
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.TipoCliente).HasMaxLength(20).HasDefaultValue("Regular");
            entity.Property(e => e.LimiteCredito).HasColumnType("decimal(18,2)");
        });
    }

    private void ConfigureInventoryEntities(ModelBuilder modelBuilder)
    {
        // Producto configuration
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Categoria).HasMaxLength(50);
            entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UnidadMedida).HasMaxLength(20).HasDefaultValue("Unidad");

            entity.HasMany(e => e.Movimientos)
                  .WithOne(m => m.Producto)
                  .HasForeignKey(m => m.IdProducto)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // MovimientoInventario configuration
        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IdProducto).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TipoMovimiento).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Motivo).HasMaxLength(200);
            entity.Property(e => e.DocumentoReferencia).HasMaxLength(50);
            entity.Property(e => e.UsuarioResponsable).HasMaxLength(50);
        });

        // AlertaStock configuration
        modelBuilder.Entity<AlertaStock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IdProducto).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NombreProducto).HasMaxLength(100);
            entity.Property(e => e.TipoAlerta).HasMaxLength(20);
            entity.Property(e => e.Mensaje).HasMaxLength(200);
        });
    }

    private void ConfigureSupplierEntities(ModelBuilder modelBuilder)
    {
        // Proveedor configuration
        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Contacto).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.TipoProveedor).HasMaxLength(20).HasDefaultValue("Regular");
            entity.Property(e => e.Calificacion).HasColumnType("decimal(3,2)");

            entity.HasMany(e => e.OrdenesCompra)
                  .WithOne(o => o.Proveedor)
                  .HasForeignKey(o => o.IdProveedor)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // OrdenCompra configuration
        modelBuilder.Entity<OrdenCompra>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IdProveedor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Pendiente");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UsuarioCreador).HasMaxLength(50);
            entity.Property(e => e.UsuarioAprobador).HasMaxLength(50);

            entity.HasMany(e => e.Detalles)
                  .WithOne(d => d.OrdenCompra)
                  .HasForeignKey(d => d.IdOrdenCompra)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // DetalleOrdenCompra configuration
        modelBuilder.Entity<DetalleOrdenCompra>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IdOrdenCompra).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IdProducto).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
        });
    }

    private void ConfigureDeliveryEntities(ModelBuilder modelBuilder)
    {
        // Entrega configuration
        modelBuilder.Entity<Entrega>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IdVenta).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.Cliente).HasMaxLength(100);
            entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Programada");
            entity.Property(e => e.Transportista).HasMaxLength(50);
            entity.Property(e => e.CreadoPor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ActualizadoPor).HasMaxLength(50);

            entity.HasMany(e => e.Seguimientos)
                  .WithOne(s => s.Entrega)
                  .HasForeignKey(s => s.IdEntrega)
                  .OnDelete(DeleteBehavior.Cascade);

            // Add foreign key relationship to Venta
            entity.HasOne(e => e.Venta)
                  .WithMany()
                  .HasForeignKey(e => e.IdVenta)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SeguimientoEntrega configuration
        modelBuilder.Entity<SeguimientoEntrega>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IdEntrega).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.Ubicacion).HasMaxLength(200);
            entity.Property(e => e.Comentarios).HasMaxLength(500);
            entity.Property(e => e.CreadoPor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ActualizadoPor).HasMaxLength(50);
        });

        // Transportista configuration
        modelBuilder.Entity<Transportista>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.LicenciaConducir).HasMaxLength(20);
            entity.Property(e => e.VehiculoPlaca).HasMaxLength(10);
            entity.Property(e => e.VehiculoTipo).HasMaxLength(50);
        });
    }

    #endregion

    #region Seed Data

    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Seed HR Employees
        modelBuilder.Entity<EmpleadoRH>().HasData(
            new EmpleadoRH
            {
                Id = "RH001",
                Nombre = "Ana García",
                Cargo = "Gerente RH",
                Departamento = "Recursos Humanos",
                Email = "ana.garcia@polimarket.com",
                Telefono = "+57 300 123 4567",
                FechaIngreso = DateTime.UtcNow.AddYears(-3),
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            },
            new EmpleadoRH
            {
                Id = "RH002",
                Nombre = "Carlos López",
                Cargo = "Analista RH",
                Departamento = "Recursos Humanos",
                Email = "carlos.lopez@polimarket.com",
                Telefono = "+57 300 765 4321",
                FechaIngreso = DateTime.UtcNow.AddYears(-1),
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            }
        );

        // Seed Sample Products
        modelBuilder.Entity<Producto>().HasData(
            new Producto
            {
                Id = "PROD001",
                Nombre = "Laptop Dell Inspiron",
                Descripcion = "Laptop Dell Inspiron 15 3000 Series",
                Precio = 2500000,
                Categoria = "Tecnología",
                Stock = 50,
                StockMinimo = 10,
                StockMaximo = 100,
                Estado = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Producto
            {
                Id = "PROD002",
                Nombre = "Mouse Inalámbrico",
                Descripcion = "Mouse inalámbrico ergonómico",
                Precio = 45000,
                Categoria = "Accesorios",
                Stock = 200,
                StockMinimo = 20,
                StockMaximo = 500,
                Estado = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Producto
            {
                Id = "PROD003",
                Nombre = "Monitor Samsung 24\"",
                Descripcion = "Monitor LED Full HD 24 pulgadas",
                Precio = 800000,
                Categoria = "Tecnología",
                Stock = 25,
                StockMinimo = 5,
                StockMaximo = 50,
                Estado = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Producto
            {
                Id = "PROD004",
                Nombre = "Teclado Mecánico RGB",
                Descripcion = "Teclado mecánico con retroiluminación RGB",
                Precio = 350000,
                Categoria = "Accesorios",
                Stock = 75,
                StockMinimo = 15,
                StockMaximo = 150,
                Estado = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Producto
            {
                Id = "PROD005",
                Nombre = "Smartphone Samsung Galaxy",
                Descripcion = "Smartphone Samsung Galaxy A54 128GB",
                Precio = 1200000,
                Categoria = "Móviles",
                Stock = 30,
                StockMinimo = 8,
                StockMaximo = 80,
                Estado = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Producto
            {
                Id = "PROD006",
                Nombre = "Auriculares Bluetooth",
                Descripcion = "Auriculares inalámbricos con cancelación de ruido",
                Precio = 250000,
                Categoria = "Audio",
                Stock = 100,
                StockMinimo = 25,
                StockMaximo = 200,
                Estado = true,
                FechaCreacion = DateTime.UtcNow
            }
        );

        // Seed Sample Customers
        modelBuilder.Entity<Cliente>().HasData(
            new Cliente
            {
                Id = "CLI001",
                Nombre = "Empresa ABC S.A.S",
                Direccion = "Calle 123 #45-67, Bogotá",
                Telefono = "+57 1 234 5678",
                Email = "contacto@empresaabc.com",
                TipoCliente = "Corporativo",
                LimiteCredito = 10000000,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            },
            new Cliente
            {
                Id = "CLI002",
                Nombre = "TechSolutions Ltda",
                Direccion = "Carrera 15 #32-18, Medellín",
                Telefono = "+57 4 567 8901",
                Email = "ventas@techsolutions.com",
                TipoCliente = "Corporativo",
                LimiteCredito = 5000000,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            },
            new Cliente
            {
                Id = "CLI003",
                Nombre = "María Fernández",
                Direccion = "Avenida 68 #25-40, Bogotá",
                Telefono = "+57 300 123 4567",
                Email = "maria.fernandez@email.com",
                TipoCliente = "Regular",
                LimiteCredito = 2000000,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            },
            new Cliente
            {
                Id = "CLI004",
                Nombre = "Distribuidora Norte S.A",
                Direccion = "Calle 85 #12-34, Barranquilla",
                Telefono = "+57 5 345 6789",
                Email = "compras@distribuidoranorte.com",
                TipoCliente = "Mayorista",
                LimiteCredito = 15000000,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            }
        );

        // Seed Test Users (Password: "password123" hashed)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = "USR001",
                Username = "admin",
                Email = "admin@polimarket.com",
                PasswordHash = passwordHash,
                Nombre = "Administrador",
                Apellido = "Sistema",
                Rol = UserRole.Admin,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Usuario
            {
                Id = "USR002",
                Username = "hr.manager",
                Email = "hr@polimarket.com",
                PasswordHash = passwordHash,
                Nombre = "María",
                Apellido = "González",
                Rol = UserRole.HRManager,
                EmpleadoRHId = "RH001",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Usuario
            {
                Id = "USR003",
                Username = "sales.rep",
                Email = "sales@polimarket.com",
                PasswordHash = passwordHash,
                Nombre = "Juan",
                Apellido = "Pérez",
                Rol = UserRole.SalesRep,
                VendedorId = "V001",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Usuario
            {
                Id = "USR004",
                Username = "inventory.manager",
                Email = "inventory@polimarket.com",
                PasswordHash = passwordHash,
                Nombre = "Ana",
                Apellido = "López",
                Rol = UserRole.InventoryManager,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            },
            new Usuario
            {
                Id = "USR005",
                Username = "delivery.manager",
                Email = "delivery@polimarket.com",
                PasswordHash = passwordHash,
                Nombre = "Carlos",
                Apellido = "Rodríguez",
                Rol = UserRole.DeliveryManager,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            }
        );
    }

    #endregion
}
