using System.Windows;

namespace Tests.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (sender, args) => { this.Close(); };
        }
        
    }
}