using System.Net.Http.Json;

public class CarritoService
{
    private readonly HttpClient _http;
    public int? CarritoId { get; private set; }

    public CarritoService(HttpClient http)
    {
        _http = http;
    }

    public async Task InicializarCarritoAsync()
    {
        if (CarritoId == null)
        {
            var response = await _http.PostAsync("http://localhost:5184/carritos", null);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<RespuestaCarrito>();
                CarritoId = data?.Id;
            }
        }
    }

    public async Task<bool> AgregarProducto(int productoId)
    {
        if (CarritoId == null)
            return false;

        var resultado = await _http.PutAsJsonAsync($"http://localhost:5184/carritos/{CarritoId}/{productoId}", new { Cantidad = 1 });
        return resultado.IsSuccessStatusCode;
    }

    private class RespuestaCarrito
    {
        public int Id { get; set; }
    }
}
