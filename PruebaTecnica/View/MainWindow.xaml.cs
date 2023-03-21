using PruebaTecnica.ViewModel;

using System.Windows;

namespace PruebaTecnica {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private readonly MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel viewModel) {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
