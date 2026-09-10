# Laboratorio 04 – NeptunoDB (WPF + MVVM + ADO.NET)

Aplicación de escritorio en **WPF (.NET)** que implementa el mantenimiento de Productos,
Categorías, Proveedores y Pedidos sobre la base de datos **NeptunoDB**, usando el patrón
**MVVM** y **ADO.NET en modo conectado** mediante **procedimientos almacenados**.

## Base de datos

- Se utiliza la base de datos **NeptunoDB** proporcionada en clase.
- Los procedimientos almacenados **no forman parte de este repositorio**: el script con su
  creación se entregó por separado junto con la entrega del laboratorio.
- La aplicación solo **invoca** procedimientos almacenados ya existentes; no genera ni
  modifica objetos de la base de datos.

## Tecnologías

- .NET (WPF, `net10.0-windows`)
- `Microsoft.Data.SqlClient`
- Patrón MVVM (sin frameworks externos)
- Sin Entity Framework ni Dapper: ADO.NET puro

## Estructura del proyecto

| Carpeta | Contenido |
|---|---|
| `Models/` | Clases POCO que coinciden con las columnas de cada tabla |
| `Data/` | `ConnectionHelper` y un repositorio por entidad (una `SqlConnection` por método, `CommandType.StoredProcedure`, `SqlParameter`) |
| `MVVM/` | `ViewModelBase` (INotifyPropertyChanged) y `RelayCommand` (ICommand) |
| `ViewModels/` | Un ViewModel por pantalla más el de navegación |
| `Views/` | Un `UserControl` por pantalla |
| `MainWindow.xaml` | Ventana principal con menú de navegación entre las 4 pantallas |

## Procedimientos almacenados utilizados (22)

- **Productos:** `Productos_Listar`, `Productos_ObtenerPorId`, `Productos_Insertar`, `Productos_Actualizar`, `Productos_Eliminar`
- **Categorías:** `Categorias_Listar`, `Categorias_ObtenerPorId`, `Categorias_Insertar`, `Categorias_Actualizar`, `Categorias_Eliminar`
- **Proveedores:** `Proveedores_Listar`, `Proveedores_ObtenerPorId`, `Proveedores_Insertar`, `Proveedores_Actualizar`, `Proveedores_Eliminar`, `Proveedores_BuscarPorContactoCiudad`
- **Pedidos:** `Pedidos_Listar`, `Pedidos_ObtenerPorId`, `Pedidos_Insertar`, `Pedidos_Actualizar`, `Pedidos_Eliminar`, `DetallePedidos_ListarPorFechas`

## Configuración de la conexión

La cadena de conexión se encuentra en `App.config`, en `<connectionStrings>` con
`name="NeptunoDB"`, y se lee en tiempo de ejecución desde la clase estática
`ConnectionHelper`:

```xml
<connectionStrings>
  <add name="NeptunoDB"
       connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=NeptunoDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"
       providerName="Microsoft.Data.SqlClient" />
</connectionStrings>
```

Si el nombre de la instancia de SQL Server es distinto, se ajusta el valor de
`Data Source` en ese archivo.

## Ejecución

```bash
dotnet build
dotnet run
```

O abriendo el proyecto en Visual Studio y presionando F5.
