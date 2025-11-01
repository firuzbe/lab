using System;
using System.Collections.Generic;
using System.Linq;

public interface IBorrowable
{
    void IssueBook(Book book, Reader reader);
    void AcceptReturn(Book book, Reader reader);
}

public enum GenreType
{
    Fiction,
    Science,
    History,
    Fantasy,
    Rare
}

public abstract class Book
{
    public string Title { get; set; }
    public string Author { get; set; }
    public int Year { get; set; }
    public GenreType Genre { get; set; }
    public int AvailableCount { get; set; }
    public int TakenCount { get; set; }

    public abstract void GetDetails();
    public abstract bool CheckAvailability();


    public override bool Equals(object obj)
    {
        if (obj is Book b)
            return Title == b.Title && Author == b.Author && Year == b.Year;
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Title, Author, Year);

    public override string ToString() => $"{Title} ({Author}, {Year}) — {Genre}";
}

public class RareBook : Book
{
    public override void GetDetails()
    {
        Console.WriteLine($"Название: {Title}, Автор: {Author}, Год: {Year}, Жанр: {Genre}");
        Console.WriteLine($"Доступно: {AvailableCount}, Взято: {TakenCount}");
    }

    public override bool CheckAvailability() => AvailableCount > TakenCount;
}

public class LibraryException : Exception
{
    private readonly string _customMessage;
    public LibraryException(string message) : base(message)
    {
        _customMessage = message;
    }

    public override string Message => $"Ошибка библиотеки: {_customMessage}";
}


public record LibraryRecord(string ReaderName, string BookTitle, DateTime BorrowDate);


public class Reader
{
    public string Name { get; set; }
    public int ReaderId { get; set; }
    public List<Book> Books { get; set; } = new List<Book>();

    public void BorrowBook(Book book)
    {
        if (book.CheckAvailability())
        {
            book.TakenCount++;
            Books.Add(book);
            Console.WriteLine($"{Name} взял книгу '{book.Title}'.");
        }
        else
        {
            throw new LibraryException($"Книга '{book.Title}' в данный момент недоступна.");
        }
    }

    public void ReturnBook(Book book)
    {
        if (Books.Contains(book))
        {
            book.TakenCount--;
            Books.Remove(book);
            Console.WriteLine($"{Name} вернул книгу '{book.Title}'.");
        }
        else
        {
            throw new LibraryException($"У {Name} нет книги '{book.Title}'.");
        }
    }
}

public class Library
{
    public string Name { get; set; }
    public string Address { get; set; }
    public List<Book> Books { get; set; } = new List<Book>();
    public List<LibraryRecord> Records { get; set; } = new List<LibraryRecord>();

    public void AddBook(Book book)
    {
        Books.Add(book);
        Console.WriteLine($"Книга '{book.Title}' добавлена в библиотеку.");
    }

    public void RemoveBook(Book book)
    {
        if (Books.Contains(book))
        {
            Books.Remove(book);
            Console.WriteLine($"Книга '{book.Title}' удалена из библиотеки.");
        }
        else
        {
            Console.WriteLine($"Книга '{book.Title}' не найдена в библиотеке.");
        }
    }

    public void ListBooks()
    {
        if (Books.Count == 0)
        {
            Console.WriteLine("В библиотеке нет книг.");
        }
        else
        {
            Console.WriteLine("Список книг:");
            foreach (var book in Books)
            {
                book.GetDetails();
            }
        }
    }


    public class LibraryStatistics
    {
        public static void PrintStats(List<Book> books)
        {
            int total = books.Count;
            int available = books.Count(b => b.CheckAvailability());
            Console.WriteLine($"\n[Статистика библиотеки]");
            Console.WriteLine($"Всего книг: {total}");
            Console.WriteLine($"Доступных: {available}");
        }
    }
}

public class LibraryStaff : IBorrowable
{
    public string Name { get; set; }
    public string Position { get; set; }
    public Library Library { get; set; }

    public void IssueBook(Book book, Reader reader)
    {
        try
        {
            reader.BorrowBook(book);
            Library.Records.Add(new LibraryRecord(reader.Name, book.Title, DateTime.Now));
        }
        catch (LibraryException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public void AcceptReturn(Book book, Reader reader)
    {
        try
        {
            reader.ReturnBook(book);
        }
        catch (LibraryException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}


public class Program
{
    private static Library library = new Library() { Name = "Национальная библиотека", Address = "ул. Библиотечная, 123" };
    private static Random rnd = new Random();

    public static void Main(string[] args)
    {
        bool running = true;
        while (running)
        {
            Console.Clear();
            ShowMenu();
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1": AddBookToLibrary(); break;
                case "2": RemoveBookFromLibrary(); break;
                case "3": ListBooksInLibrary(); break;
                case "4": ManageReaderBooks(); break;
                case "0": running = false; break;
                default: Console.WriteLine("Неверный выбор."); break;
            }
        }
    }

    private static void ShowMenu()
    {
        Console.WriteLine("Меню:");
        Console.WriteLine("1. Добавить книгу");
        Console.WriteLine("2. Удалить книгу");
        Console.WriteLine("3. Показать книги");
        Console.WriteLine("4. Управление книгами читателя");
        Console.WriteLine("0. Выход");
        Console.Write("Выберите пункт: ");
    }

    private static void AddBookToLibrary()
    {
        Console.Write("Введите название: ");
        string title = Console.ReadLine();
        Console.Write("Введите автора: ");
        string author = Console.ReadLine();
        Console.Write("Введите год: ");
        int year = int.Parse(Console.ReadLine());

        Console.WriteLine("Выберите жанр (0 - Fiction, 1 - Science, 2 - History, 3 - Fantasy, 4 - Rare): ");
        GenreType genre = (GenreType)int.Parse(Console.ReadLine());

        Console.Write("Введите количество: ");
        int count = int.Parse(Console.ReadLine());

        Book newBook = new RareBook() { Title = title, Author = author, Year = year, Genre = genre, AvailableCount = count, TakenCount = 0 };
        library.AddBook(newBook);
    }

    private static void RemoveBookFromLibrary()
    {
        Console.Write("Введите название книги: ");
        string title = Console.ReadLine();
        var book = library.Books.Find(b => b.Title == title);
        if (book != null) library.RemoveBook(book);
        else Console.WriteLine("Книга не найдена.");
    }

    private static void ListBooksInLibrary()
    {
        library.ListBooks();
        Library.LibraryStatistics.PrintStats(library.Books);
        Console.WriteLine("\nНажмите любую клавишу...");
        Console.ReadKey();
    }

    private static void ManageReaderBooks()
    {
        Console.Write("Введите имя читателя: ");
        string name = Console.ReadLine();
        Reader reader = new Reader() { Name = name, ReaderId = rnd.Next(1000, 9999) };
        LibraryStaff staff = new LibraryStaff() { Name = "Мария", Position = "Библиотекарь", Library = library };

        Console.WriteLine("1. Взять книгу");
        Console.WriteLine("2. Вернуть книгу");
        string c = Console.ReadLine();

        Console.Write("Введите название книги: ");
        string title = Console.ReadLine();
        var book = library.Books.Find(b => b.Title == title);
        if (book == null)
        {
            Console.WriteLine("Книга не найдена.");
            return;
        }

        if (c == "1") staff.IssueBook(book, reader);
        else if (c == "2") staff.AcceptReturn(book, reader);
    }
}
