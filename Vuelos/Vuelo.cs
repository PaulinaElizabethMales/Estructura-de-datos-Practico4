namespace Vuelos;

/// <summary>
/// Representa una arista dirigida y ponderada del grafo: un vuelo
/// de un aeropuerto de origen a uno de destino.
/// </summary>
public class Vuelo
{
    public string Origen { get; }
    public string Destino { get; }
    public double Precio { get; }
    public int Minutos { get; }
    public string Codigo { get; }

    public Vuelo(string origen, string destino, double precio, int minutos, string codigo)
    {
        if (precio < 0)
            throw new ArgumentException("El precio no puede ser negativo (Dijkstra lo requiere).");

        Origen = origen;
        Destino = destino;
        Precio = precio;
        Minutos = minutos;
        Codigo = codigo;
    }

    public override string ToString() =>
        $"{Origen} -> {Destino}  {Codigo,-6} ${Precio:F0}  {Minutos} min";
}