using System.Diagnostics;

namespace Vuelos;

/// <summary>
/// Se encarga de la salida: reportes en consola, registro opcional en archivo
/// y medición de tiempos de ejecución.
/// </summary>
public class Registrador
{
    private readonly string? _archivo;

    public Registrador(string? archivoRegistro = null) => _archivo = archivoRegistro;

    public void Escribir(string texto = "")
    {
        Console.WriteLine(texto);
        if (_archivo != null)
            File.AppendAllText(_archivo, texto + Environment.NewLine);
    }

    public void MostrarReporte(RedVuelos red)
    {
        Escribir($"Aeropuertos (vertices): {red.CantidadAeropuertos}   Vuelos (aristas): {red.CantidadVuelos}");
        Escribir($"{"Origen",-8}Vuelos (destino $precio)");
        foreach (var a in red.Aeropuertos())
        {
            var salidas = red.VuelosDesde(a).Select(v => $"{v.Destino} ${v.Precio:F0}");
            var texto = string.Join(", ", salidas);
            Escribir($"{a,-8}{(texto.Length == 0 ? "-" : texto)}");
        }
    }

    public void MostrarVuelos(string origen, List<Vuelo> vuelos)
    {
        Escribir(vuelos.Count == 0
            ? $"No hay vuelos desde {origen}."
            : $"Vuelos desde {origen} (ordenados por precio):");
        foreach (var v in vuelos) Escribir("  " + v);
    }

    public void MostrarRuta(string titulo, Ruta? ruta)
    {
        Escribir();
        Escribir(titulo);
        if (ruta == null || ruta.Tramos.Count == 0)
        {
            Escribir("  No se encontro ruta.");
            return;
        }
        foreach (var v in ruta.Tramos) Escribir("  " + v);
        Escribir($"  Total: ${ruta.Costo:F0}, {ruta.Minutos / 60}h{ruta.Minutos % 60:D2}m, {ruta.Escalas} escala(s)");
    }

    /// <summary>
    /// Ejecuta la función varias veces (tras una ejecución de calentamiento para el JIT)
    /// y devuelve el resultado y el tiempo promedio en milisegundos.
    /// </summary>
    public (T Resultado, double Milisegundos) Medir<T>(Func<T> funcion, int repeticiones = 1)
    {
        T resultado = funcion(); // calentamiento
        var reloj = Stopwatch.StartNew();
        for (int i = 0; i < repeticiones; i++) resultado = funcion();
        reloj.Stop();
        return (resultado, reloj.Elapsed.TotalMilliseconds / repeticiones);
    }
}