using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace Rhizine.AvalonEdit.MarkdownEditor;

public enum MarkdownHighlightingType
{
    Custom,
    Default,
    AvalonEditMarkdown,
    AvalonEditMarkdownWithFontSize
}

[TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
public class MarkdownEditor : TextEditor
{
    #region Fields

    public static readonly DependencyProperty CustomHighlightingProperty =
        DependencyProperty.Register(nameof(CustomHighlighting), typeof(IHighlightingDefinition), typeof(MarkdownEditor),
            new PropertyMetadata(null, OnCustomHighlightingChanged));

    public static readonly DependencyProperty HighlightingTypeProperty =
            DependencyProperty.Register(nameof(HighlightingType), typeof(MarkdownHighlightingType), typeof(MarkdownEditor),
                new PropertyMetadata(MarkdownHighlightingType.Default, OnHighlightingTypeChanged));

    public static readonly DependencyProperty IsMarkdownHiddenProperty =
        DependencyProperty.Register(nameof(IsMarkdownHidden), typeof(bool), typeof(MarkdownEditor),
            new PropertyMetadata(true, OnIsMarkdownHiddenChanged));

    public static readonly DependencyProperty TransformerProperty =
        DependencyProperty.Register(nameof(Transformer), typeof(MarkdownColorizingTransformer), typeof(MarkdownEditor),
            new PropertyMetadata(new MarkdownColorizingTransformer(), OnTransformerChanged));

    #endregion Fields

    #region Constructors

    static MarkdownEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MarkdownEditor),
            new FrameworkPropertyMetadata(typeof(MarkdownEditor)));
    }
    public MarkdownEditor() : base()
    {
        HighlightingManager.Instance.RegisterHighlighting("DefaultMarkdown",
            [".md", ".markdown"],
            CreateDefaultCustomHighlighting());

        HighlightingType = MarkdownHighlightingType.Default;
        UpdateHighlighting();
    }

    #endregion Constructors

    #region Properties

    public IHighlightingDefinition CustomHighlighting
    {
        get { return (IHighlightingDefinition)GetValue(CustomHighlightingProperty); }
        set { SetValue(CustomHighlightingProperty, value); }
    }

    public MarkdownHighlightingType HighlightingType
    {
        get { return (MarkdownHighlightingType)GetValue(HighlightingTypeProperty); }
        set { SetValue(HighlightingTypeProperty, value); }
    }

    public bool IsMarkdownHidden
    {
        get { return (bool)GetValue(IsMarkdownHiddenProperty); }
        set { SetValue(IsMarkdownHiddenProperty, value); }
    }
    public MarkdownColorizingTransformer Transformer
    {
        get { return (MarkdownColorizingTransformer)GetValue(TransformerProperty); }
        set { SetValue(TransformerProperty, value); }
    }

    #endregion Properties

    #region Methods

    public static void RegisterCustomHighlighting(string name, string[] extensions, string resourceName)
    {
        IHighlightingDefinition customHighlighting;
        using Stream s = typeof(MarkdownEditor).Assembly.GetManifestResourceStream(resourceName)
                            ?? throw new InvalidOperationException("Could not find resource " + resourceName);

        using XmlReader reader = new XmlTextReader(s);
        customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
        HighlightingLoader.Load(reader, HighlightingManager.Instance);

        HighlightingManager.Instance.RegisterHighlighting(name, extensions, customHighlighting);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        TextArea.TextView.LineTransformers.Add(Transformer);
    }

    private static void OnCustomHighlightingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (MarkdownEditor)d;
        if (editor.HighlightingType == MarkdownHighlightingType.Custom)
        {
            editor.UpdateHighlighting();
        }
    }

    private static void OnHighlightingTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (MarkdownEditor)d;
        editor.UpdateHighlighting();
    }
    private static void OnIsMarkdownHiddenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (MarkdownEditor)d;
        if (editor.Transformer != null)
        {
            editor.Transformer.HideMarkdown = (bool)e.NewValue;
            editor.TextArea.TextView.Redraw();
        }
    }

    private static void OnTransformerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editor = (MarkdownEditor)d;
        if (e.OldValue is MarkdownColorizingTransformer oldTransformer)
        {
            editor.TextArea.TextView.LineTransformers.Remove(oldTransformer);
        }
        if (e.NewValue is MarkdownColorizingTransformer newTransformer)
        {
            newTransformer.HideMarkdown = editor.IsMarkdownHidden;
            newTransformer.IsEditMode = !editor.IsReadOnly;
            editor.TextArea.TextView.LineTransformers.Add(newTransformer);
        }
        editor.TextArea.TextView.Redraw();
    }
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsReadOnlyProperty)
        {
            UpdateTransformerEditMode();
        }
    }

    private void UpdateTransformerEditMode()
    {
        if (Transformer != null)
        {
            Transformer.IsEditMode = !IsReadOnly;
            TextArea.TextView.Redraw();
        }
    }

    private IHighlightingDefinition CreateDefaultCustomHighlighting()
    {
        const string resourceName = "Rhizine.AvalonEdit.MarkdownEditor.Themes.DefaultMarkdown.xshd";
        using Stream s = typeof(MarkdownEditor).Assembly.GetManifestResourceStream(resourceName)
                            ?? throw new InvalidOperationException("Could not find resource " + resourceName);

        using XmlReader reader = new XmlTextReader(s);
        return ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    private void UpdateHighlighting()
    {
        switch (HighlightingType)
        {
            case MarkdownHighlightingType.Custom:
                SyntaxHighlighting = CustomHighlighting ?? HighlightingManager.Instance.GetDefinition("DefaultMarkdown");
                break;

            case MarkdownHighlightingType.Default:
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("DefaultMarkdown");
                break;

            case MarkdownHighlightingType.AvalonEditMarkdown:
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Markdown");
                break;

            case MarkdownHighlightingType.AvalonEditMarkdownWithFontSize:
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("MarkdownWithFontSize");
                break;
        }
    }

    #endregion Methods
}