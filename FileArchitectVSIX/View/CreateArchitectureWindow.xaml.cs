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

namespace FileArchitectVSIX
{
    /// <summary>
    /// Interaction logic for CreateArchitectureWindow.xaml
    /// </summary>
    public partial class CreateArchitectureWindow : Window
    {
        public CreateArchitectureWindow()
        {
            InitializeComponent();
        }

        private void OnGenerateClicked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Generate clicked!");
        }
    }
}
