var builder = WebApplication.CreateBuilder(args);

// Agregar servicios CORS para permitir solicitudes desde el cliente
builder.Services.AddCors(options => {
    options.AddPolicy("AllowClientApp", policy => {
        policy.WithOrigins("http://localhost:5177", "https://localhost:7221")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Agregar controladores si es necesario
builder.Services.AddControllers();

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}

// Usar CORS con la política definida
app.UseCors("AllowClientApp");

// Mapear rutas básicas
app.MapGet("/", () => "Servidor API está en funcionamiento");

// Ejemplo de endpoint de API
app.MapGet("/api/datos", () => new { Mensaje = "Datos desde el servidor", Fecha = DateTime.Now });

// Endpoint busqueda con query
app.MapGet("/productos", async (TiendaContext db, string? buscar) =>
{
    var query = db.Productos.AsQueryable();

    if (!string.IsNullOrEmpty(buscar))
    {
        query = query.Where(p => p.Nombre.Contains(buscar) || p.Descripcion.Contains(buscar));
    }

    var productos = await query.ToListAsync();
    return Results.Ok(productos);
});

// Endpoint para crear un nuevo carrito de compras
app.MapPost("/carritos", async (TiendaContext db) =>
{
    var nuevaCompra = new Compra
    {
        Fecha = DateTime.now,
        Total = 0,
        NombreCliente = "",
        ApellidoCliente = "",
        EmailCliente = "",
        Items = new List<ItemCompra>()
    };

    db.Compras.Add(nuevaCompra);
    await db.SaveChangesAsync();

    return Results.Created($"/carritos/{nuevaCompra.Id}", new { nuevaCompra.Id });
});

// Endpoint para obtener los ítems de un carrito específico
app.MapGet("/carritos/{carritoId:int}", async (int carritoId, TiendaContext db) =>
{
    var compra = await db.Compras
        .Include(c => c.Items)
        .ThenInclude(item => item.Producto)
        .FirstOrDefaultAsync(c => c.Id == carritoId);

    if (compra == null) return Results.NotFound("Carrito no encontrado.");

    var respuesta = compra.Items.Select(item => new
    {
        ProductoId = item.ProductoId,
        NombreProducto = item.Producto.Nombre,
        PrecioUnitario = item.PrecioUnitario,
        Cantidad = item.Cantidad,
        Subtotal = item.Cantidad * item.PrecioUnitario
    });

    return Results.Ok(respuesta);
});

// Endpoint para vaciar un carrito de compras, por ID
app.MapDelete("/carritos/{carritoId:int}", async (int carritoId, TiendaContext db) =>
{
    var compra = await db.Compras
        .Include(c => c.Items)
        .FirstOrDefaultAsync(c => c.Id == carritoId);

    if (compra == null) return Results.NotFound("Carrito no encontrado.");

    db.ItemsCompra.RemoveRange(compra.Items);
    await db.SaveChangesAsync();

    return Results.Ok("Carrito vaciado exitosamente.");
});

app.Run();

class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; } = null!;

    public List<ItemCompra> ItmsCompra { get; set; } = new(); // Relación uno a muchos
}

class Compra
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }

    public string NombreCliente { get; set; } = null!;
    public string ApellidoCliente { get; set; } = null!;
    public string EmailCliente { get; set; } = null!;

    public List<ItemCompra> Items { get; set; } = new(); // Relación uno a muchos
}

class ItemCompra
{
    public int Id { get; set; }

    public int ProductoId { get; set; }
    public Producto producto { get; set; } = null!;

    public int CompraId { get; set; }
    public Compra compra { get; set; } = null!;

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

class TiendaContext : DbContext
{
    public TiendaContext(DbContextOptions<TiendaContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<ItemCompra> ItemsCompra => Set<ItemCompra>();
}

record ClienteDTO(string Nombre, string Apellido, string Email);

record ItemCantidadDTO(int Cantidad);