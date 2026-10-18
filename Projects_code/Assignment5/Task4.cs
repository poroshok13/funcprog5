namespace Assignment5;

public static class Task4
{
    public static Func<decimal, decimal> Pipeline(params Func<decimal, decimal>[] steps)
    {
        return steps.Aggregate(
            (Func<decimal, decimal>)(x => x),
            (acc, step) => x => step(acc(x))
        );
    }

    public static void Run()
    {
        Console.WriteLine("--- Task 4: Dynamic Pipeline with Aggregate ---");

        Func<decimal, decimal> discount10 = Task1.Curry<decimal, decimal, decimal>(Task1.ApplyDiscount)(0.10m);
        Func<decimal, decimal> fee5 = Task1.Curry<decimal, decimal, decimal>(Task1.AddFee)(5m);
        Func<decimal, decimal> round = Task1.RoundToCents;

        decimal amount = 1234.56m;

        var standardPipeline = Pipeline(discount10, fee5, round);
        var swappedPipeline = Pipeline(fee5, discount10, round);
        var roundFirstPipeline = Pipeline(round, discount10, fee5);
        var emptyPipeline = Pipeline();

        decimal standardResult = standardPipeline(amount);
        decimal swappedResult = swappedPipeline(amount);
        decimal roundFirstResult = roundFirstPipeline(amount);
        decimal emptyResult = emptyPipeline(amount);

        Console.WriteLine($"Input amount: {amount}");
        Console.WriteLine($"1. Standard (discount -> fee -> round): {standardResult}");
        Console.WriteLine($"2. Swapped (fee -> discount -> round):   {swappedResult}");
        Console.WriteLine($"3. Round First (round -> discount -> fee): {roundFirstResult}");
        Console.WriteLine($"4. Empty Pipeline ():                     {emptyResult}");
        Console.WriteLine();
    }
}
