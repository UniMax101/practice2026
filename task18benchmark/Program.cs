using System.Collections.Concurrent;
using System.Diagnostics;
using ScottPlot;
using Task17;
using Task18;

namespace Task18Benchmark;
internal static class Program
{
    private static readonly string DataDir = Path.Combine("task18", "report", "data");
    private static readonly string ImagesDir = Path.Combine("task18", "report", "images");

    private static void Main()
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(ImagesDir);

        RunFairnessScenario();
        RunLatencyScenario();
        RunScalabilityScenario();

        Console.WriteLine("Готово. CSV -> " + DataDir + ", PNG -> " + ImagesDir);
    }

    private static void RunFairnessScenario()
    {
        var scheduler = new RoundRobinScheduler();
        var serverThread = new LongOperationServerThread(scheduler);
        var order = new ConcurrentQueue<string>();
        var completed = new CountdownEvent(3);

        serverThread.Start();

        foreach (var name in new[] { "A", "B", "C" })
        {
            var cmd = new SteppedLongCommand(6, step =>
            {
                order.Enqueue($"{name}");
                if (step == 6) completed.Signal();
            });
            serverThread.Enqueue(cmd);
        }

        completed.Wait(TimeSpan.FromSeconds(5));
        serverThread.Enqueue(new Task18.SoftStopCommand(serverThread));
        serverThread.Join();

        var rows = order.Select((name, i) => (tick: i + 1, name)).ToList();
        WriteCsv(Path.Combine(DataDir, "round_robin_fairness.csv"), "tick,command",
            rows.Select(r => $"{r.tick},{r.name}"));

        var plt = new Plot();
        var names = rows.Select(r => r.name).Distinct().OrderBy(n => n).ToArray();
        var colors = new[] { Colors.SteelBlue, Colors.Coral, Colors.SeaGreen };
        for (int i = 0; i < names.Length; i++)
        {
            var xs = rows.Where(r => r.name == names[i]).Select(r => (double)r.tick).ToArray();
            var ys = xs.Select(_ => (double)i).ToArray();
            var sp = plt.Add.ScatterPoints(xs, ys);
            sp.Color = colors[i % colors.Length];
            sp.MarkerSize = 12;
            sp.LegendText = names[i];
        }

        plt.Title("Round Robin: чередование выполнения трёх длинных команд");
        plt.XLabel("Тик планировщика (вызов Execute)");
        plt.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            names.Select((n, i) => new Tick(i, n)).ToArray());
        plt.ShowLegend();
        plt.SavePng(Path.Combine(ImagesDir, "round_robin_fairness.png"), 900, 320);
    }
    private static void RunLatencyScenario()
    {
        var scheduler = new RoundRobinScheduler();
        var serverThread = new LongOperationServerThread(scheduler);
        var sw = Stopwatch.StartNew();
        var events = new ConcurrentQueue<(string name, double elapsedMs)>();
        var newCommandDone = new ManualResetEventSlim(false);
        var firstStepDone = new ManualResetEventSlim(false);

        serverThread.Start();

        var longCommand = new SteppedLongCommand(10, step =>
        {
            events.Enqueue(($"long-{step}", sw.Elapsed.TotalMilliseconds));
            if (step == 1) firstStepDone.Set();
            Thread.Sleep(15); 
        });
        serverThread.Enqueue(longCommand);

        firstStepDone.Wait(TimeSpan.FromSeconds(2));
        var enqueueTime = sw.Elapsed.TotalMilliseconds;
        serverThread.Enqueue(new InstantCommand(() =>
        {
            events.Enqueue(("new-command", sw.Elapsed.TotalMilliseconds));
            newCommandDone.Set();
        }));
        newCommandDone.Wait(TimeSpan.FromSeconds(2));

        serverThread.Enqueue(new Task18.SoftStopCommand(serverThread));
        serverThread.Join();

        var newCommandTime = events.First(e => e.name == "new-command").elapsedMs;
        var latencyMs = newCommandTime - enqueueTime;

        var rows = events.ToList();
        WriteCsv(Path.Combine(DataDir, "new_command_latency.csv"), "event,elapsed_ms",
            rows.Select(r => $"{r.name},{r.elapsedMs:F1}"));
        WriteCsv(Path.Combine(DataDir, "new_command_latency_summary.csv"), "metric,value_ms",
            new[] { $"latency_new_command,{latencyMs:F1}" });

        var plt = new Plot();
        var xs = rows.Select(r => r.elapsedMs).ToArray();
        var ys = rows.Select(r => 0.0).ToArray();
        var colors = rows.Select(r => r.name == "new-command" ? Colors.Coral : Colors.SteelBlue).ToArray();
        var bars = xs.Select((x, i) => new Bar { Position = x, Value = 1, FillColor = colors[i] }).ToArray();
        plt.Add.Bars(bars);
        plt.Title($"Новая команда подхватывается через {latencyMs:F0} мс после постановки\n" +
                  "в очередь, не дожидаясь завершения долгой операции");
        plt.XLabel("Время от старта, мс");
        plt.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual([]);
        plt.SavePng(Path.Combine(ImagesDir, "new_command_latency.png"), 900, 300);
    }
    private static void RunScalabilityScenario()
    {
        var results = new List<(int n, double elapsedMs)>();

        for (int n = 1; n <= 10; n++)
        {
            var scheduler = new RoundRobinScheduler();
            var serverThread = new LongOperationServerThread(scheduler);
            var completed = new CountdownEvent(n);
            const int stepsEach = 5;

            var sw = Stopwatch.StartNew();
            serverThread.Start();

            for (int i = 0; i < n; i++)
            {
                var cmd = new SteppedLongCommand(stepsEach, step =>
                {
                    if (step == stepsEach) completed.Signal();
                });
                serverThread.Enqueue(cmd);
            }

            completed.Wait(TimeSpan.FromSeconds(10));
            sw.Stop();

            serverThread.Enqueue(new Task18.SoftStopCommand(serverThread));
            serverThread.Join();

            results.Add((n, sw.Elapsed.TotalMilliseconds));
        }

        WriteCsv(Path.Combine(DataDir, "scalability.csv"), "long_commands_count,elapsed_ms",
            results.Select(r => $"{r.n},{r.elapsedMs:F1}"));

        var plt = new Plot();
        var xs = results.Select(r => (double)r.n).ToArray();
        var ys = results.Select(r => r.elapsedMs).ToArray();
        var sp = plt.Add.Scatter(xs, ys);
        sp.Color = Colors.SteelBlue;
        sp.MarkerSize = 8;
        plt.Title("Суммарное время выполнения растёт линейно с числом\n" +
                  "одновременно запущенных длинных команд");
        plt.XLabel("Число одновременно запущенных длинных команд");
        plt.YLabel("Время до завершения всех команд, мс");
        plt.SavePng(Path.Combine(ImagesDir, "scalability.png"), 700, 450);
    }

    private static void WriteCsv(string path, string header, IEnumerable<string> rows)
    {
        using var writer = new StreamWriter(path, append: false);
        writer.WriteLine(header);
        foreach (var row in rows) writer.WriteLine(row);
    }
}

internal sealed class SteppedLongCommand : ILongCommand
{
    private readonly int _totalSteps;
    private readonly Action<int> _onStep;
    private int _currentStep;

    public SteppedLongCommand(int totalSteps, Action<int> onStep)
    {
        _totalSteps = totalSteps;
        _onStep = onStep;
    }

    public bool IsCompleted => _currentStep >= _totalSteps;

    public void Execute()
    {
        _currentStep++;
        _onStep(_currentStep);
    }
}

internal sealed class InstantCommand : ICommand
{
    private readonly Action _action;
    public InstantCommand(Action action) => _action = action;
    public void Execute() => _action();
}
