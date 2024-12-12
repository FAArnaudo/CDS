using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using ContextMenu = System.Windows.Forms.ContextMenu;
using Image = System.Windows.Controls.Image;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;

namespace CDS
{
    /// <summary>
    /// Lógica de interacción para ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : Window
    {
        private NotifyIcon notifyIcon;
        public ControlPanel()
        {
            InitializeComponent();
            Loaded += ControlPanel_Loaded;
        }

        private void ControlPanel_Loaded(object sender, RoutedEventArgs e)
        {
            Icon = new BitmapImage(new Uri("pack://application:,,,/CDS;component/Images/LogoSurtidor.ico"));
            SetupNotifyIcon();

            if (!Configuration.ExistConfiguracion())
            {
                OpenConfigurationWindows();
            }
            else
            {
                Init();
            }
        }

        private void InitialConfiguration_Closed(object sender, EventArgs e)
        {
            Show();
            Init();
        }

        private void Init()
        {
            ConfigureExpander(Configuration.GetConfiguration().StationFlag, Configuration.GetConfiguration().Controller);
        }

        private void OpenConfigurationWindows()
        {
            Views.InitialConfiguration initialConfiguration = new Views.InitialConfiguration
            {
                Owner = this
            };
            initialConfiguration.Show();
            initialConfiguration.Closed += InitialConfiguration_Closed;
            Hide();
        }

        #region EVENTOS

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            // Mostrar un cuadro de diálogo de confirmación
            MessageBoxResult result = MessageBox.Show("¿Está seguro de que desea realizar esta acción?\n      El programa dejará de funcionar.",
                                                      "Confirmación",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);
            // Verificar la respuesta del usuario
            if (result == MessageBoxResult.Yes)
            {
                //Controlador.StopProcess();
                notifyIcon.Dispose();
                base.OnClosed(e);
                Close();
            }
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnCambiarConfig_Click(object sender, RoutedEventArgs e)
        {
            OpenConfigurationWindows();
        }

        private void BtnVerDespachos_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnVerConfigEstacion_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnVerTanques_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnVerProductos_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnVerCierres_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCierreAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                _ = MessageBox.Show($"¡{clickedButton.Content} presionado!");
            }
        }

        private void BtnCIO_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                _ = MessageBox.Show($"¡{clickedButton.Content} presionado!");
            }
        }
        #endregion
        #region PROCESO EN SEGUNDO PLANO
        private void SetupNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon("LogoSurtidor.ico"),
                Visible = false,
                Text = "Controlador De Surtidores"
            };

            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            ContextMenu contextMenu = new ContextMenu();
            _ = contextMenu.MenuItems.Add("Restaurar", (s, e) => RestoreFromTray());
            _ = contextMenu.MenuItems.Add("Salir", (s, e) => ExitApplication());

            notifyIcon.ContextMenu = contextMenu;
        }
        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            RestoreFromTray();
        }
        private void RestoreFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            notifyIcon.Visible = false;
        }
        private void ExitApplication()
        {
            notifyIcon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        }
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            notifyIcon.Dispose();
            base.OnClosed(e);
        }
        #endregion
        #region EXPANDERS
        private void ConfigureExpander(string flagStation, string controller)
        {
            // Crear el Expander
            Expander expander = new Expander
            {
                Header = controller,
                IsExpanded = true,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#013220")),
                BorderThickness = new Thickness(2)
            };

            // Crear un StackPanel para apilar los controles
            StackPanel stackPanel = new StackPanel { };

            // Crear un Label
            Label label = new Label
            {
                Content = flagStation,
                Width = 176,
                Height = 35,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 20,
                Foreground = Brushes.White,
                Margin = new Thickness(2)
            };

            if (flagStation.Equals("YPF"))
            {
                label.Background = Brushes.CornflowerBlue;

                // Crear un TextBlock
                TextBlock textBlock = new TextBlock
                {
                    Text = "Datos CIO",
                    Height = 16,
                    Width = 56,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2)
                };

                // Crear el primer botón
                Button btnCIO = new Button
                {
                    Width = 110,
                    Height = 35,
                    Background = Brushes.CornflowerBlue,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Style = (Style)Resources["Styles"],
                    Margin = new Thickness(2)
                };

                btnCIO.Click += BtnCIO_Click;

                // Crear un objeto Image
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Images/Data.ico", UriKind.RelativeOrAbsolute)) // Establece la ruta de la imagen
                };

                btnCIO.Content = image;

                // Crear el segundo botón
                Button btnCierreAnterior = new Button
                {
                    Content = "Cierre Anterior",
                    Foreground = Brushes.White,
                    FontSize = 15,
                    Width = 110,
                    Height = 35,
                    Background = Brushes.CornflowerBlue,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Style = (Style)Resources["Styles"],
                    Margin = new Thickness(2)
                };

                btnCierreAnterior.Click += BtnCierreAnterior_Click;

                _ = stackPanel.Children.Add(label);
                _ = stackPanel.Children.Add(textBlock);
                _ = stackPanel.Children.Add(btnCIO);
                _ = stackPanel.Children.Add(btnCierreAnterior);
            }
            else if (flagStation.Equals("AXION"))
            {
                label.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B008B"));
                expander.IsEnabled = false;
                _ = stackPanel.Children.Add(label);
            }
            else if (flagStation.Equals("PUMA"))
            {
                label.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A50000"));
                expander.IsEnabled = false;
                _ = stackPanel.Children.Add(label);
            }

            expander.Content = stackPanel;

            _ = SPExpander.Children.Add(expander);
        }
        #endregion
    }
}
