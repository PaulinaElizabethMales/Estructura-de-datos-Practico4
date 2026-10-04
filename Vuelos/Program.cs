using System.Globalization;
using System.Text;
using Vuelos;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var registrador = new Registrador();
var red = RedVuelos.CargarDesdeArchivo(Path.Combine(AppContext.BaseDirectory, "vuelos.txt"));

if (args.Contains("--demo")) Demostracion();
else Menu();

void Menu()
{
    while (true)
    {
        registrador.Escribir("""

            ----------------------------------------------------------------
            |                         VUELOS BARATOS                       |
            ----------------------------------------------------------------

            (1) Listar aeropuertos y vuelos      (2) Vuelos desde un aeropuerto
            (3) Ruta mas barata                  (4) Ruta con menos escalas
            (5) Mas barata con maximo de escalas (6) Medir tiempos (benchmark)
            (0) Salir
            """);
            
        Console.Write("Opcion: ");
        var opcion = Console.ReadLine()?.Trim();

        switch (opcion)
        {
            case "0": return;
            case "1": registrador.MostrarReporte(red); break;
            case "2":
                var o = Leer("Origen");
                registrador.MostrarVuelos(o, red.VuelosDesde(o));
                break;
            case "3":
                var (o3, d3) = (Leer("Origen"), Leer("Destino"));
                registrador.MostrarRuta("Ruta mas barata", red.RutaMasBarata(o3, d3));
                break;
            case "4":
                var (o4, d4) = (Leer("Origen"), Leer("Destino"));
                registrador.MostrarRuta("Ruta con menos escalas", red.RutaConMenosEscalas(o4, d4));
                break;
            case "5":
                var (o5, d5) = (Leer("Origen"), Leer("Destino"));
                Console.Write("Maximo de escalas: ");
                if (int.TryParse(Console.ReadLine(), out int k) && k >= 0)
                    registrador.MostrarRuta($"Mas barata con maximo {k} escala(s)",
                        red.RutaMasBarataConLimite(o5, d5, k));
                else registrador.Escribir("Numero invalido.");
                break;
            case "6": Benchmark(); break;
            default: registrador.Escribir("Opcion no valida."); break;
        }
    }
}

string Leer(string etiqueta)
{
    Console.Write($"{etiqueta}: ");
    return (Console.ReadLine() ?? "").Trim().ToUpper();
}

void Demostracion()
{
    registrador.MostrarReporte(red);

    var directos = red.VuelosDirectos("UIO", "MAD");
    registrador.Escribir($"\nVuelo directo UIO->MAD: {(directos.Count == 0 ? "no existe" : directos[0].ToString())}");

    var (r1, t1) = registrador.Medir(() => red.RutaMasBarata("UIO", "MAD"), 1000);
    registrador.MostrarRuta($"Mas barata UIO -> MAD (Dijkstra, {t1:F4} ms)", r1);

    var (r2, t2) = registrador.Medir(() => red.RutaMasBarata("UIO", "JFK"), 1000);
    registrador.MostrarRuta($"Mas barata UIO -> JFK (Dijkstra, {t2:F4} ms)", r2);

    var (r3, t3) = registrador.Medir(() => red.RutaConMenosEscalas("UIO", "JFK"), 1000);
    registrador.MostrarRuta($"Menos escalas UIO -> JFK (BFS, {t3:F4} ms)", r3);

    foreach (int k in new[] { 1, 2 })
    {
        var (rk, tk) = registrador.Medir(() => red.RutaMasBarataConLimite("UIO", "JFK", k), 1000);
        registrador.MostrarRuta($"Mas barata UIO -> JFK, max {k} escala(s) ({tk:F4} ms)", rk);
    }
    Benchmark();
}

void Benchmark()
{
    registrador.Escribir("\nBenchmark en grafos aleatorios (Dijkstra, A0 -> ultimo nodo)");
    registrador.Escribir($"{"Vertices",9}{"Aristas",9}{"Tiempo (ms)",13}");
    foreach (var (n, e) in new[] { (100, 500), (1000, 5000), (5000, 25000), (20000, 100000) })
    {
        var grande = RedVuelos.GenerarAleatoria(n, e);
        var (_, ms) = registrador.Medir(() => grande.RutaMasBarata("A0", $"A{n - 1}"), 5);
        registrador.Escribir($"{n,9}{e,9}{ms,13:F3}");
    }
}