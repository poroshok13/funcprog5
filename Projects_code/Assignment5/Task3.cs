namespace Assignment5;

public static class PipeExtensions
{
    public static R Pipe<T, R>(this T value, Func<T, R> f)
    {
        return f(value);
    }
}

public static class Task3
{
    public static void Run()
    {
        Console.WriteLine("--- Task 3: Pipelining with Extension Method ---");

        Func<decimal, decimal> discount10 = Task1.Curry<decimal, decimal, decimal>(Task1.ApplyDiscount)(0.10m);
        Func<decimal, decimal> fee5 = Task1.Curry<decimal, decimal, decimal>(Task1.AddFee)(5m);
        Func<decimal, decimal> round = Task1.RoundToCents;

        decimal initialAmount = 1234.56m;
        decimal result = initialAmount
            .Pipe(discount10)
            .Pipe(fee5)
            .Pipe(round);

        Console.WriteLine($"Order amount: {initialAmount}");
        Console.WriteLine($"Piped result: {result}");
        Console.WriteLine();
    }
}
