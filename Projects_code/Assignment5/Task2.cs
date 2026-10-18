namespace Assignment5;

public static class Task2
{
    public static Func<A, C> Compose<A, B, C>(Func<A, B> f, Func<B, C> g)
    {
        return x => g(f(x));
    }

    public static void Run()
    {
        Console.WriteLine("--- Task 2: Function Composition ---");

        Func<decimal, decimal> discount10 = Task1.Curry<decimal, decimal, decimal>(Task1.ApplyDiscount)(0.10m);
        Func<decimal, decimal> fee5 = Task1.Curry<decimal, decimal, decimal>(Task1.AddFee)(5m);
        Func<decimal, decimal> round = Task1.RoundToCents;

        Func<decimal, decimal> checkout = Compose(Compose(discount10, fee5), round);

        decimal initialAmount = 1234.56m;
        decimal result = checkout(initialAmount);

        Console.WriteLine($"Order amount: {initialAmount}");
        Console.WriteLine($"Checkout result: {result}");
        Console.WriteLine();
    }
}
