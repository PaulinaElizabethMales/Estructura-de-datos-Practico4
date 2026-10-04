namespace Vuelos;

/// <summary>
/// Grafo dirigido y ponderado de vuelos, implementado con listas de adyacencia:
/// Dictionary&lt;aeropuerto, List&lt;Vuelo&gt;&gt;.
/// </summary>
public class RedVuelos
{
    private readonly Dictionary<string, List<Vuelo>> _adyacencia = new();

    public int CantidadAeropuertos => _adyacencia.Count;
    public int CantidadVuelos => _adyacencia.Values.Sum(l => l.Count);

    // ---------------- Construcción ----------------

    public void AgregarAeropuerto(string codigo)
    {
        if (!_adyacencia.ContainsKey(codigo))
            _adyacencia[codigo] = new List<Vuelo>();
    }

    public void AgregarVuelo(Vuelo vuelo)
    {
        AgregarAeropuerto(vuelo.Origen);
        AgregarAeropuerto(vuelo.Destino);
        _adyacencia[vuelo.Origen].Add(vuelo);
    }

    /// <summary>Carga la "base de datos" desde un archivo de texto CSV.</summary>
    public static RedVuelos CargarDesdeArchivo(string ruta)
    {
        var red = new RedVuelos();
        foreach (var linea in File.ReadLines(ruta))
        {
            var texto = linea.Trim();
            if (texto.Length == 0 || texto.StartsWith('#')) continue;

            var p = texto.Split(',');
            red.AgregarVuelo(new Vuelo(
                p[0].Trim().ToUpper(),
                p[1].Trim().ToUpper(),
                double.Parse(p[2], System.Globalization.CultureInfo.InvariantCulture),
                int.Parse(p[3]),
                p[4].Trim()));
        }
        return red;
    }

    /// <summary>Genera un grafo aleatorio conectado, útil para medir tiempos.</summary>
    public static RedVuelos GenerarAleatoria(int n, int e, int semilla = 1)
    {
        var rnd = new Random(semilla);
        var red = new RedVuelos();
        for (int i = 0; i < n; i++) red.AgregarAeropuerto($"A{i}");
        for (int i = 0; i < n - 1; i++)
            red.AgregarVuelo(new Vuelo($"A{i}", $"A{i + 1}", rnd.Next(50, 501), 100, $"B{i}"));
        for (int k = 0; k < e - (n - 1); k++)
        {
            int a = rnd.Next(n), b = rnd.Next(n);
            if (a != b)
                red.AgregarVuelo(new Vuelo($"A{a}", $"A{b}", rnd.Next(50, 501), 100, $"R{k}"));
        }
        return red;
    }

    // ---------------- Consultas ----------------

    public bool Existe(string aeropuerto) => _adyacencia.ContainsKey(aeropuerto);

    public IEnumerable<string> Aeropuertos() =>
        _adyacencia.Keys.OrderBy(k => k, StringComparer.Ordinal);

    public List<Vuelo> VuelosDesde(string origen) =>
        _adyacencia.TryGetValue(origen, out var lista)
            ? lista.OrderBy(v => v.Precio).ToList()
            : new List<Vuelo>();

    public List<Vuelo> VuelosDirectos(string origen, string destino) =>
        VuelosDesde(origen).Where(v => v.Destino == destino).ToList();

    // ---------------- Algoritmos ----------------

    /// <summary>Ruta más barata. Dijkstra con cola de prioridad: O((V+E) log V).</summary>
    public Ruta? RutaMasBarata(string origen, string destino)
    {
        if (!Existe(origen) || !Existe(destino)) return null;
        if (origen == destino) return new Ruta(Array.Empty<Vuelo>());

        var dist = new Dictionary<string, double> { [origen] = 0 };
        var previo = new Dictionary<string, Vuelo>();
        var cola = new PriorityQueue<string, double>();
        cola.Enqueue(origen, 0);

        while (cola.TryDequeue(out var u, out var costo))
        {
            if (u == destino) break;
            if (costo > dist[u]) continue; // entrada obsoleta de la cola

            foreach (var v in _adyacencia[u])
            {
                double nuevo = costo + v.Precio;
                if (!dist.TryGetValue(v.Destino, out var actual) || nuevo < actual)
                {
                    dist[v.Destino] = nuevo;
                    previo[v.Destino] = v;
                    cola.Enqueue(v.Destino, nuevo);
                }
            }
        }
        return dist.ContainsKey(destino) ? Reconstruir(previo, origen, destino) : null;
    }

    /// <summary>Ruta con menos escalas. Búsqueda en anchura (BFS): O(V+E).</summary>
    public Ruta? RutaConMenosEscalas(string origen, string destino)
    {
        if (!Existe(origen) || !Existe(destino)) return null;
        if (origen == destino) return new Ruta(Array.Empty<Vuelo>());

        var visitados = new HashSet<string> { origen };
        var previo = new Dictionary<string, Vuelo>();
        var cola = new Queue<string>();
        cola.Enqueue(origen);

        while (cola.Count > 0)
        {
            var u = cola.Dequeue();
            if (u == destino) break;
            foreach (var v in _adyacencia[u])
            {
                if (visitados.Add(v.Destino))
                {
                    previo[v.Destino] = v;
                    cola.Enqueue(v.Destino);
                }
            }
        }
        return visitados.Contains(destino) ? Reconstruir(previo, origen, destino) : null;
    }

    /// <summary>
    /// Ruta más barata con a lo sumo <paramref name="maxEscalas"/> escalas.
    /// Relajación por capas (estilo Bellman-Ford): O((maxEscalas+1) * E).
    /// </summary>
    public Ruta? RutaMasBarataConLimite(string origen, string destino, int maxEscalas)
    {
        if (!Existe(origen) || !Existe(destino) || origen == destino) return null;

        var mejor = new Dictionary<string, Ruta> { [origen] = new Ruta(Array.Empty<Vuelo>()) };

        for (int capa = 0; capa <= maxEscalas; capa++)
        {
            var siguiente = new Dictionary<string, Ruta>(mejor);
            foreach (var (u, ruta) in mejor)
            {
                foreach (var v in _adyacencia[u])
                {
                    double nuevo = ruta.Costo + v.Precio;
                    if (!siguiente.TryGetValue(v.Destino, out var actual) || nuevo < actual.Costo)
                        siguiente[v.Destino] = ruta.Extender(v);
                }
            }
            mejor = siguiente;
        }
        return mejor.TryGetValue(destino, out var r) ? r : null;
    }

    private static Ruta Reconstruir(Dictionary<string, Vuelo> previo, string origen, string destino)
    {
        var tramos = new List<Vuelo>();
        var nodo = destino;
        while (nodo != origen)
        {
            var vuelo = previo[nodo];
            tramos.Add(vuelo);
            nodo = vuelo.Origen;
        }
        tramos.Reverse();
        return new Ruta(tramos);
    }
}