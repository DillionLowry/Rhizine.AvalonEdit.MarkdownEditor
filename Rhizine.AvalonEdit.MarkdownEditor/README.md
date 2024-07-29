 Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.

 Step 1a) Using this custom control in a XAML file that exists in the current project.
 Add this XmlNamespace attribute to the root element of the markup file where it is 
 to be used:

     xmlns:md="clr-namespace:Rhizine.AvalonEdit.MarkdownEditor"


 Step 1b) Using this custom control in a XAML file that exists in a different project.
 Add this XmlNamespace attribute to the root element of the markup file where it is 
 to be used:

     xmlns:md="clr-namespace:Rhizine.AvalonEdit.MarkdownEditor;assembly=Rhizine.AvalonEdit.MarkdownEditor"

 You will also need to add a project reference from the project where the XAML file lives
 to this project and Rebuild to avoid compilation errors:

     Right click on the target project in the Solution Explorer and
     "Add Reference"->"Projects"->[Select this project]


 Step 2)
 Go ahead and use your control in the XAML file.

     <md:MarkdownEditor
    Document="{Binding CurrentPageContent, Mode=TwoWay}"
    IsReadOnly="{Binding IsReadOnly}"
    IsMarkdownHidden="{Binding IsReadOnly}"
    FontFamily="Segoe UI"
    FontSize="14"
    ShowLineNumbers="False"/>

    MarkdownEditor.RegisterCustomHighlighting("MyCustomHighlighting", 
    new string[] { ".md", ".markdown" }, 
    "MyNamespace.MyCustomHighlighting.xshd");

    myMarkdownEditor.HighlightingType = MarkdownHighlightingType.Custom;
myMarkdownEditor.CustomHighlighting = HighlightingManager.Instance.GetDefinition("MyCustomHighlighting");