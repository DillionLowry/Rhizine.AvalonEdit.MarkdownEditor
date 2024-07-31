# MarkdownEditor Control Usage Guide
The MarkdownEditor is a custom control built on top of AvalonEdit, designed to provide an enhanced editing experience for Markdown documents.

#### "Why should I use this over something like MarkDig?"
    
> You probably shouldn't. I made this control to be a lightweight editor/viewer for Markdown documents with customizable syntax highlighting that doesn't convert the text to HTML, but it's not a full-fledged Markdown parser.
> If you're already using AvalonEdit in your project or you want a way to add syntax highlighting without a ton of overhead, this control might be a good fit for your project.



# Basic Usage

1. Add a reference to the `Rhizine.AvalonEdit.MarkdownEditor` library in your project.
Add the following namespace to your XAML file:

``` xaml
xmlns:md="clr-namespace:Rhizine.AvalonEdit.MarkdownEditor;assembly=Rhizine.AvalonEdit.MarkdownEditor"
```

2. Add the following namespace to your XAML file:
 
``` xaml
<md:MarkdownEditor x:Name="myMarkdownEditor" />
```



You can also create and manipulate the MarkdownEditor in your C# code:
``` csharp
MarkdownEditor editor = new MarkdownEditor(); editor.Text = "# Welcome to MarkdownEditor";
```

# Binding to a Document
The MarkdownEditor has a *`Document`* property that you can bind to a string property in your view model:

``` xaml
     <md:MarkdownEditor
        Document="{Binding CurrentPageContent, Mode=TwoWay}"
        IsReadOnly="{Binding IsReadOnly}"
        IsMarkdownHidden="{Binding IsReadOnly}"
        FontFamily="Segoe UI"
        FontSize="14"
        ShowLineNumbers="False"/>
```

# Features

## Syntax Highlighting

The MarkdownEditor comes with several highlighting options:

- Default: A custom highlighting scheme defined in the control.
- AvalonEdit Markdown: The built-in Markdown highlighting from AvalonEdit.
- AvalonEdit MarkdownWithFontSize: Another built-in option from AvalonEdit.
- Custom: Allows you to define your own highlighting scheme.

To change the highlighting type:

``` csharp
myMarkdownEditor.HighlightingType = MarkdownHighlightingType.AvalonEditMarkdown;
```


## Custom Highlighting
You can provide your own custom highlighting:

``` csharp
myMarkdownEditor.HighlightingType = MarkdownHighlightingType.Custom;
myMarkdownEditor.CustomHighlighting = YourCustomHighlightingDefinition;
```

## Markdown Hiding
The MarkdownEditor can hide Markdown syntax for a cleaner reading experience:

``` csharp
myMarkdownEditor.IsMarkdownHidden = true;
```

## Read-Only Mode
You can set the editor to read-only mode, which applies a different style:

``` csharp
myMarkdownEditor.IsReadOnly = true;
```


# Advanced Usage
### Registering Custom Highlighting
You can register a custom highlighting from an .xshd file:

``` csharp
MarkdownEditor.RegisterCustomHighlighting("MyCustomHighlighting", 
        new string[] { ".md", ".markdown" }, 
        "MyNamespace.MyCustomHighlighting.xshd");
```

Then use it like this:
``` csharp
myMarkdownEditor.HighlightingType = MarkdownHighlightingType.Custom;
myMarkdownEditor.CustomHighlighting = HighlightingManager.Instance.GetDefinition("MyCustomHighlighting");
```

### Accessing the Underlying TextEditor
The MarkdownEditor inherits from AvalonEdit's TextEditor, so you have access to all its properties and methods:

``` csharp
myMarkdownEditor.Options.EnableHyperlinks = true;
myMarkdownEditor.Options.EnableEmailHyperlinks = true;
```

### Styling
The MarkdownEditor uses a default style defined in Themes/Generic.xaml. You can override this style in your application to customize the appearance of the control.