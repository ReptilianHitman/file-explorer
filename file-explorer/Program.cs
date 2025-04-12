Console.Clear();
Console.CursorVisible = false;

var selectionIndex = 0;
List<FileSystemItem> items;
try
{
    items = new DirectoryItem(args[0], args[0]).GetChildren();
}
catch
{
    items = new DirectoryItem("/", "/").GetChildren();
}

while (true)
{
    foreach (var item in items)
    {
        if (item != items[selectionIndex])
        {
            Console.ForegroundColor = item is DirectoryItem ? ConsoleColor.Blue : ConsoleColor.White;
        }
        else
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
        }

        
        Console.WriteLine(item.ItemName);
        Console.ResetColor();
    }

    int userInput = Console.ReadKey().KeyChar;

    switch (userInput)
    {
        case 'q':
            Console.ResetColor();
            Console.CursorVisible = true;
            Console.Clear();
            Environment.Exit(0);
            break;
        case '\r':
            items = items[selectionIndex].Open();
            selectionIndex = 0;
            break;
        case 'j':
            selectionIndex += selectionIndex < items.Count - 1 ? 1 : 0;
            break;
        case 'k':
            selectionIndex -= selectionIndex > 0 ? 1 : 0;
            break;
    }

    Console.Clear();
}

internal abstract class FileSystemItem
{
    public string ItemName { get; }
    protected readonly string ItemPath;

    protected FileSystemItem(string itemName, string itemPath)
    {
        ItemName = itemName;
        ItemPath = itemPath;
    }

    public abstract long GetSize();
    public abstract List<FileSystemItem> GetChildren();

    public abstract List<FileSystemItem> Open();
}

internal class DirectoryItem : FileSystemItem
{
    private List<FileSystemItem> _children = new List<FileSystemItem>();

    public DirectoryItem(string itemName, string itemPath) : base(itemName, itemPath)
    {
    }

    public override long GetSize()
    {
        return 0;
    }

    public override List<FileSystemItem> GetChildren()
    {
        var tmpItems = new List<FileSystemItem>();
        var parent = Directory.GetParent(ItemPath);

        if (parent != null)
            tmpItems.Add(new DirectoryItem("..", parent.FullName));

        tmpItems.AddRange(Directory.GetDirectories(ItemPath)
            .Select(file => new DirectoryItem(Path.GetRelativePath(ItemPath, file), file))
            .OrderBy(item => item.ItemName));
        tmpItems.AddRange(Directory.GetFiles(ItemPath)
            .Select(file => new FileItem(Path.GetRelativePath(ItemPath, file), file, 0))
            .OrderBy(item => item.ItemName));

        return tmpItems;
    }

    public override List<FileSystemItem> Open()
    {
        return GetChildren();
    }
}

internal class FileItem : FileSystemItem
{
    private long _size;

    public FileItem(string itemName, string itemPath, long size) : base(itemName, itemPath)
    {
        _size = size;
    }

    public override long GetSize()
    {
        return _size;
    }

    public override List<FileSystemItem> GetChildren()
    {
        Console.ResetColor();
        Console.CursorVisible = true;
        throw new NotImplementedException();
    }

    public override List<FileSystemItem> Open()
    {
        var fileLines = File.ReadLines(ItemPath).ToList();

        Console.Clear();
        //Console.CursorVisible = true;
    
        foreach (var line in fileLines)
        {
            Console.WriteLine(line);
        }

        Console.ReadKey();

        //Console.CursorVisible = false;
        var parent = Directory.GetParent(ItemPath)!;
        return new DirectoryItem(parent.FullName, parent.FullName).GetChildren();
    }
}