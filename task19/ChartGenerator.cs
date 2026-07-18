using System;
using System.IO;
using ScottPlot;

namespace Task19;

public static class ChartGenerator
{
    public static void GenerateCharts()
    {
        double[] softStopCycles = [3, 3, 3]; 
        double[] hardStopCycles = [1, 1, 0];
        
        double[] positions = [0, 1, 2];
        string[] labels = ["Задача №1", "Задача №2", "Задача №3"];

        Plot myPlot = new();
        var barsSoft = myPlot.Add.Bars(positions, softStopCycles);
        barsSoft.Color = new Color(46, 204, 113); // #2ecc71
        barsSoft.LegendText = "Плавный останов (SoftStop)";
        double[] hardPositions = [0.3, 1.3, 2.3];
        var barsHard = myPlot.Add.Bars(hardPositions, hardStopCycles);
        barsHard.Color = new Color(231, 76, 60); // #e74c3c
        barsHard.LegendText = "Жесткий останов (HardStop)";

        myPlot.Title("Сравнение стратегий останова сервера LongOperationServerThread");
        myPlot.YLabel("Количество выполненных квантов времени");
        
        double[] tickPositions = [0.15, 1.15, 2.15];
        myPlot.Axes.Bottom.SetTicks(tickPositions, labels);
        
        myPlot.Axes.SetLimitsY(0, 4);

        myPlot.ShowLegend(Alignment.UpperRight);

        string projectPath = Directory.GetCurrentDirectory();
        string outputPath = Path.Combine(projectPath, "task19_scottplot_charts.png");
        
        myPlot.SavePng(outputPath, 800, 500);
        
        Console.WriteLine($"График успешно сохранен в корневую папку task19: {outputPath}");
    }
}