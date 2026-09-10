using System.Windows.Controls;

namespace WpfApp1.Views
{
    /// <summary>
    /// Vista (UserControl) de Productos. No tiene lógica: todo está en el ViewModel.
    /// El DataContext lo asigna automáticamente el DataTemplate de MainWindow.
    /// </summary>
    public partial class ProductosView : UserControl
    {
        public ProductosView()
        {
            InitializeComponent();
        }
    }
}
