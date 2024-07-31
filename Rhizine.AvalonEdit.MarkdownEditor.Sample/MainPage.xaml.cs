using System.Windows.Controls;

namespace Rhizine.AvalonEdit.MarkdownEditor.Sample;

public partial class MainPage : Page
{
    public MainPage(MainViewModel viewmodel)

    {
        InitializeComponent();
        DataContext = viewmodel;
    }
}