using CommunityToolkit.Mvvm.ComponentModel;

namespace Rhizine.AvalonEdit.MarkdownEditor.Sample;

public class MarkdownSubpage : ObservableObject
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
}