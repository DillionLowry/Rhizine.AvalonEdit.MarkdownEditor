using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Rhizine.AvalonEdit.MarkdownEditor.Sample;

public partial class MainViewModel : ObservableObject
{
    #region Fields

    private readonly ILogger<MainViewModel> _logger;
    private readonly MarkdownService _MarkdownService;

    [ObservableProperty]
    private TextDocument _currentPageContent;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isReadOnly = true;

    [ObservableProperty]
    private ObservableCollection<MarkdownPage> _MarkdownPages;

    [ObservableProperty]
    private MarkdownColorizingTransformer _markdownTransformer;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MarkdownPage> _searchResults;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPageContent))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private MarkdownPage _selectedPage;

    #endregion Fields

    #region Constructors

    public MainViewModel(MarkdownService MarkdownService, ILogger<MainViewModel> logger)
    {
        _MarkdownService = MarkdownService;
        _logger = logger;

        MarkdownPages = new ObservableCollection<MarkdownPage>(_MarkdownService.GetAllPages());
        CurrentPageContent = new TextDocument();
        SearchResults = new ObservableCollection<MarkdownPage>();

        _markdownTransformer = new MarkdownColorizingTransformer();

        SelectedPage = MarkdownPages.FirstOrDefault();

        if (SelectedPage != null)
        {
            LoadSelectedPageContent();
        }
    }

    #endregion Constructors

    #region Methods

    private bool CanSave() => !IsReadOnly && SelectedPage != null;

    [RelayCommand]
    private void Edit()
    {
        IsReadOnly = false;
    }

    private void LoadSelectedPageContent()
    {
        if (SelectedPage != null)
        {
            string content = _MarkdownService.GetPageContent(SelectedPage.Id);
            CurrentPageContent.Text = content;
            IsReadOnly = true;
        }
        else
        {
            _logger.LogInformation("Selected page was null");
        }
    }

    [RelayCommand]
    private void NewPage()
    {
        var newPageTitle = "New Page"; // prompt user
        var newPage = _MarkdownService.CreateNewPage(newPageTitle);

        MarkdownPages.Add(newPage);

        SelectedPage = newPage;
        IsReadOnly = false;
        CurrentPageContent.Text = $"# {newPageTitle}\n\nEnter your content here.";
    }

    partial void OnSelectedPageChanged(MarkdownPage value)
    {
        LoadSelectedPageContent();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        if (SelectedPage != null)
        {
            _MarkdownService.SavePageContent(SelectedPage.Id, CurrentPageContent.Text);
            IsReadOnly = true;
        }
        else
        {
            _logger.LogInformation("SelectedPage is null");
        }
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchResults.Clear();
            return;
        }

        var results = _MarkdownService.SearchPages(SearchQuery);
        SearchResults.Clear();

        foreach (var result in results)
        {
            SearchResults.Add(result);
        }
    }

    [RelayCommand]
    private void SelectPage(object page)
    {
        if (page is MarkdownPage MarkdownPage)

        {
            SelectedPage = MarkdownPage;
        }
        else if (page is MarkdownSubpage MarkdownSubpage)
        {
            SelectedPage = MarkdownPages.FirstOrDefault(p => p.Subpages.Contains(MarkdownSubpage));
        }
    }

    #endregion Methods
}