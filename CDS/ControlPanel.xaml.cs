using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CDS
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class ControlPanel : Window
    {
        public ControlPanel()
        {
            InitializeComponent();
            Loaded += ControlPanel_Loaded;
        }

        private void ControlPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Configuration.ExistConfiguracion())
            {
                OpenConfigurationWindows();
            }
        }

        private void OpenConfigurationWindows()
        {
            Views.InitialConfiguration initialConfiguration = new Views.InitialConfiguration
            {
                Owner = this
            };
            initialConfiguration.Show();
            Hide();
        }
    }
}
