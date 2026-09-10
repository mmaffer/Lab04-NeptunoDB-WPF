using System.Configuration;

namespace WpfApp1.Data
{
    /// <summary>
    /// Clase estática de apoyo. Su única responsabilidad es entregar la
    /// cadena de conexión hacia NeptunoDB, leyéndola desde App.config
    /// (&lt;connectionStrings&gt; con name="NeptunoDB") vía ConfigurationManager.
    /// Así ningún repositorio tiene la cadena "escrita a mano".
    /// </summary>
    public static class ConnectionHelper
    {
        public static string DatabaseConnectionString =>
            ConfigurationManager.ConnectionStrings["NeptunoDB"]?.ConnectionString
            ?? throw new ConfigurationErrorsException(
                "No se encontró la cadena de conexión 'NeptunoDB' en App.config.");
    }
}
