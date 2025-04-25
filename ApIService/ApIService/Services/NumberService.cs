namespace APIService.Services;

public interface INumberService
{
    int GetNumber();
    Task SetNumber(int number);
    
}

public class NumberService : INumberService
{
    private readonly SemaphoreSlim _semaphore;

    public NumberService(SemaphoreSlim semaphore)
    {
        _semaphore = semaphore;
    }

    public int GetNumber()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "number.txt");

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "0");

        var content = File.ReadAllText(filePath);
        if (int.TryParse(content, out var number)) return number;

        File.WriteAllText(filePath, "0");
        return 0;
    }

    public async Task SetNumber(int number)
    {
        await _semaphore.WaitAsync();
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "number.txt");

        var content = File.Exists(filePath) ? File.ReadAllText(filePath) : "0";
        _ = int.TryParse(content, out var currentNumber);

        File.WriteAllText(filePath, (currentNumber + number).ToString());
    }
    
}