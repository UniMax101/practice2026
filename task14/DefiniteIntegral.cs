using System;
using System.Threading;

public class DefiniteIntegral
{
    public static double Solve(double a, double b, Func<double, double> function, double step, int threadsNumber)
    {
        if (function == null)
            throw new ArgumentNullException(nameof(function));

        if (threadsNumber <= 0)
            throw new ArgumentException("Неверное количество потоков");

        if (step <= 0)
            throw new ArgumentException("Шаг должен быть больше нуля");

        double result = 0;
        double partLength = (b - a) / threadsNumber;

        Barrier barrier = new Barrier(threadsNumber + 1);
        Thread[] threads = new Thread[threadsNumber];

        for (int i = 0; i < threadsNumber; i++)
        {
            double start = a + i * partLength;
            double end = start + partLength;

            threads[i] = new Thread(() =>
            {
                double sum = 0;
                double x = start;

                while (x + step <= end)
                {
                    sum += (function(x) + function(x + step)) * step / 2;
                    x += step;
                }

                if (x < end)
                {
                    sum += (function(x) + function(end)) * (end - x) / 2;
                }

                double oldValue;
                double newValue;

                do
                {
                    oldValue = result;
                    newValue = oldValue + sum;
                }
                while (Interlocked.CompareExchange(ref result, newValue, oldValue) != oldValue);

                barrier.SignalAndWait();
            });

            threads[i].Start();
        }

        barrier.SignalAndWait();

        return result;
    }
    public static double SolveSingleThread(double a, double b, Func<double, double> function, double step)
    {
        double sum = 0;
        long steps = (long)((b - a) / step);

        for (long k = 0; k < steps; k++)
        {
            double x1 = a + k * step;
            double x2 = a + (k + 1) * step;
            sum += (function(x1) + function(x2)) * step / 2;
        }

        return sum;
    }
}
