using System.Text.Json.Serialization;

namespace calories_tracking.domain;

public class PageList<T>
{
    [JsonConstructor]
    public PageList(IEnumerable<T> items, int page, int size)
    {
        Items = items.ToList();
        Page = page;
        Size = size;
    }

    private PageList(List<T> items, int page, int size)
    {
        Items = items;
        Page = page;
        Size = size;
    }

    public List<T> Items { get; }
    public int Page { get; }
    public int Size { get; }

    public static PageList<T> Create(IEnumerable<T> source, int page, int size)
    {
        List<T> items = source.Skip((page - 1) * size).Take(size).ToList();
        return new(items, page, size);
    }
}
