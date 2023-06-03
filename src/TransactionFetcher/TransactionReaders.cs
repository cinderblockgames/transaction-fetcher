using System.Globalization;
using System.Text.Json;
using TransactionFetcher.Readers;

namespace TransactionFetcher;

public class TransactionReaders
{
    public ITransactionReader[] Instances { get; }
    private CultureInfo Locale { get; }
    
    public TransactionReaders(string folder, CultureInfo locale)
    {
        Locale = locale;
        
        Console.WriteLine($"Loading accounts from {folder}.");
        Instances = Directory.EnumerateFiles(folder)
            .Select(BuildTransactionReader)
            .ToArray();
    }

    private ITransactionReader BuildTransactionReader(string path)
    {
        Console.WriteLine($"Loading account from {path}.");
        var text = File.ReadAllText(path);
        var options = JsonSerializer.Deserialize<TransactionReaderOptions>(text);

        var type = Type.GetType(options!.Type!);
        var reader = (ITransactionReader)Activator.CreateInstance(type!)!;
        reader.Initialize((TransactionReaderOptions)JsonSerializer.Deserialize(text, reader.OptionsType)!, Locale);
        return reader;
    }
}