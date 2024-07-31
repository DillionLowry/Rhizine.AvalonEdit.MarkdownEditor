using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;

namespace Rhizine.AvalonEdit.MarkdownEditor.Sample;

public class MarkdownService

{
    #region Fields

    private readonly string _contentDirectory = "WikiContent";
    private readonly ILogger<MarkdownService> _logger;

    private readonly List<MarkdownPage> _pages;
    private int _nextId;

    #endregion Fields

    #region Constructors

    public MarkdownService(ILogger<MarkdownService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Creating MarkdownService");

        _pages = new List<MarkdownPage>
            {
                new MarkdownPage { Id = 1, Title = "README", IsSystemPage = true },
            };

        _nextId = _pages.Max(p => p.Id) + 1;

        Directory.CreateDirectory(_contentDirectory);

        LoadUserPages();
    }

    #endregion Constructors

    #region Methods

    public MarkdownPage CreateNewPage(string title)
    {
        var newPage = new MarkdownPage { Id = _nextId++, Title = title, IsSystemPage = false };
        _pages.Add(newPage);

        SavePageContent(newPage.Id, $"# {title}\n\nAdd your content here.");
        return newPage;
    }

    public List<MarkdownPage> GetAllPages()
    {
        return _pages;
    }

    public string GetPageContent(int pageId)
    {
        var page = _pages.Find(p => p.Id == pageId);
        if (page is null) return "Page not found.";

        if (page.IsSystemPage)
        {
            return GetEmbeddedResourceContent($"Rhizine.AvalonEdit.MarkdownEditor.Sample.{page.Title.Replace(" ", "_")}.md");
        }
        else
        {
            var filePath = Path.Combine(_contentDirectory, $"{pageId}.md");
            return File.Exists(filePath) ? File.ReadAllText(filePath) : "Page content not found.";
        }
    }

    public void SavePageContent(int pageId, string content)
    {
        var page = _pages.Find(p => p.Id == pageId);
        if (page is null) return;

        if (!page.IsSystemPage)
        {
            var filePath = Path.Combine(_contentDirectory, $"{pageId}.md");

            File.WriteAllText(filePath, content);

            page.Content = content;
        }
    }

    public List<MarkdownPage> SearchPages(string query)
    {
        return _pages.Where(p =>

            p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Content.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Subpages.Any(s => s.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    private string GetEmbeddedResourceContent(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is not null)
        {
            using var reader = new System.IO.StreamReader(stream);
            return reader.ReadToEnd();
        }

        _logger.LogError("Failed to find resource with name {Name}", resourceName);
        return "Resource not found.";
    }

    private void LoadUserPages()
    {
        foreach (var file in Directory.GetFiles(_contentDirectory, "*.md"))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);

            if (int.TryParse(fileName, out int id) && _pages.TrueForAll(p => p.Id != id))
            {
                var content = File.ReadAllText(file);
                var title = content.Split('\n').FirstOrDefault()?.TrimStart('#', ' ') ?? "Untitled";

                _pages.Add(new MarkdownPage { Id = id, Title = title, Content = content, IsSystemPage = false });
                _nextId = Math.Max(_nextId, id + 1);
            }
        }
    }

    #endregion Methods
}