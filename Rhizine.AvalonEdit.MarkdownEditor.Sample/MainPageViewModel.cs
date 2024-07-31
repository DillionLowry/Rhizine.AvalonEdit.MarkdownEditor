using CommunityToolkit.Mvvm.ComponentModel;

using CommunityToolkit.Mvvm.Input;

using ICSharpCode.AvalonEdit.Document;

using ICSharpCode.AvalonEdit.Highlighting;

using ICSharpCode.AvalonEdit.Highlighting.Xshd;

using Microsoft.Extensions.Logging;

using System.Collections.ObjectModel;

using System.IO;

using System.Windows;

using System.Xml;

namespace Rhizine.AvalonEdit.MarkdownEditor.Sample

{
    //public class IssueResolution

    //{
    //    public string Issue { get; set; }

    //    public string Resolution { get; set; }

    //}

    //public class FAQ

    //{
    //    public string Question { get; set; }

    //    public string Answer { get; set; }

    //}

    //public class TableOfContentsItem

    //{
    //    public string Id { get; set; }

    //    public string Title { get; set; }

    //    public int Level { get; set; }

    //    public ObservableCollection<TableOfContentsItem> Children { get; set; }

    //    public TableOfContentsItem()

    //    {
    //        Children = new ObservableCollection<TableOfContentsItem>();

    //    }

    //}

    //public class PageHistoryEntry

    //{
    //    public string Version { get; set; }

    //    public DateTime Timestamp { get; set; }

    //    public string Author { get; set; }

    //    public string Comment { get; set; }

    //}

    //    private string GenerateId(string text)

    //    {
    //        return Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9\s-]", "")

    //            .Trim()

    //            .Replace(" ", "-");

    //    }

    //}

    public partial class MainPageViewModel : ObservableObject

    {
        private readonly ILogger<MainPageViewModel> _logger;
        private readonly MarkdownService _MarkdownService;
        private readonly string HighlightingUri = "pack://application:,,,/Rhizine.AvalonEdit.MarkdownEditor.Sample;component/Resources/WikiHighlighting.xshd";

        [ObservableProperty]
        private MarkdownColorizingTransformer _markdownTransformer;

        [ObservableProperty]
        private IHighlightingDefinition _wikiHighlighting;

        [ObservableProperty]
        private ObservableCollection<MarkdownPage> _MarkdownPages;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentPageContent))]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private MarkdownPage _selectedPage;

        [ObservableProperty]
        private TextDocument _currentPageContent;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private bool _isReadOnly = true;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private ObservableCollection<MarkdownPage> _searchResults;

        public MainPageViewModel(MarkdownService MarkdownService, ILogger<MainPageViewModel> logger)

        {
            _MarkdownService = MarkdownService;

            _logger = logger;
            _logger.LogInformation("Creating MainPageViewModel");
            MarkdownPages = new ObservableCollection<MarkdownPage>(_MarkdownService.GetAllPages());

            CurrentPageContent = new TextDocument();

            SearchResults = new ObservableCollection<MarkdownPage>();

            _markdownTransformer = new MarkdownColorizingTransformer();

            // Set default landing page

            SelectedPage = MarkdownPages.FirstOrDefault(p => p.Title == "Welcome");

            if (SelectedPage != null)

            {
                LoadSelectedPageContent();
            }
        }

        public void LoadSyntaxHighlighting()

        {
            try

            {
                using (Stream rs = Application.GetResourceStream(new Uri(HighlightingUri)).Stream)

                {
                    if (rs == null) return;

                    using XmlReader reader = new XmlTextReader(rs);

                    WikiHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }

                HighlightingManager.Instance.RegisterHighlighting("Wiki Highlighting", new string[] { ".md" }, WikiHighlighting);
            }
            catch (Exception ex)

            {
                _logger.LogError(ex, "Error loading syntax highlighting");
            }
        }

        private partial void OnSelectedPageChanged(MarkdownPage value)

        {
            LoadSelectedPageContent();
        }

        private void LoadSelectedPageContent()

        {
            if (SelectedPage != null)

            {
                string content = _MarkdownService.GetPageContent(SelectedPage.Id);

                _logger.LogDebug("Page content: {Content}", content);

                CurrentPageContent.Text = content;

                IsReadOnly = true;
            }
            else

            {
                _logger.LogInformation("Selected page was null");
            }
        }

        [RelayCommand]
        private void Edit()

        {
            IsReadOnly = false;

            //_markdownTransformer.HideMarkdown = false;

            //MarkdownTransformer = null;

            //MarkdownTransformer = _markdownTransformer;  // Trigger update
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()

        {
            if (SelectedPage != null)

            {
                _MarkdownService.SavePageContent(SelectedPage.Id, CurrentPageContent.Text);

                IsReadOnly = true;

                //_markdownTransformer.HideMarkdown = true;

                //MarkdownTransformer = null;

                //MarkdownTransformer = _markdownTransformer;  // Trigger update
            }
            else

            {
                _logger.LogInformation("SelectedPage is null");
            }
        }

        private bool CanSave() => !IsReadOnly && SelectedPage != null;

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

        [RelayCommand]
        private void NewPage()

        {
            var newPageTitle = "New Page"; //TODO: prompt user

            var newPage = _MarkdownService.CreateNewPage(newPageTitle);

            MarkdownPages.Add(newPage);

            SelectedPage = newPage;

            IsReadOnly = false;

            CurrentPageContent.Text = $"# {newPageTitle}\n\nEnter your content here.";
        }
    }
}