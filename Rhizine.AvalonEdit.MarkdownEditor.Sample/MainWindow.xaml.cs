using System.Windows;
using System.Windows.Controls;

namespace Rhizine.AvalonEdit.MarkdownEditor.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(App.GetService<MainPage>());
        }

        public Frame GetNavigationFrame() => MainFrame;
    }
}