namespace Vuelos;

/// <summary>
/// Resultado de una búsqueda: una secuencia ordenada de vuelos.
/// </summary>
public class Ruta
{
    public IReadOnlyList<Vuelo> Tramos { get; }

    public Ruta(IEnumerable<Vuelo> tramos) => Tramos = tramos.ToList();

    public double Costo => Tramos.Sum(v => v.Precio);
    public int Minutos => Tramos.Sum(v => v.Minutos);
    public int Escalas => Math.Max(0, Tramos.Count - 1);

    /// <summary>Devuelve una nueva ruta con un vuelo más al final.</summary>
    public Ruta Extender(Vuelo vuelo) => new(Tramos.Append(vuelo));
}