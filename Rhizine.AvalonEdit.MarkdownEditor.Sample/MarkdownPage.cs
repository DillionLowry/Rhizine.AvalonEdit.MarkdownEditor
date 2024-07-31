using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Rhizine.AvalonEdit.MarkdownEditor.Sample;

public class MarkdownPage : ObservableObject
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public bool IsSystemPage { get; set; }

    public ObservableCollection<MarkdownSubpage> Subpages { get; set; }

    public MarkdownPage()
    {
        Subpages = new ObservableCollection<MarkdownSubpage>();
    }
}