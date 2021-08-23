using Common.DAL.Logger;
using Common.Models;
using Consumer.DAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Consumer
{
  class Program
  {
    static async Task Main(string[] args)
    {
      Console.Title = "Сортировка файла";

      try
      {
        //string fileName = @"C:\Users\Adiy\git\test\Producer\bin\Debug\netcoreapp3.1\MessFileData_test_1Gb.txt";
        string fileName = @"C:\Users\Adiy\git\test\Producer\bin\Debug\netcoreapp3.1\MessFileData_test.txt";

        Stopwatch stopwatch = new();
        stopwatch.Start();

        SortManager sortManager = new(fileName);
        await sortManager.SortAsync();

        stopwatch.Stop();

        Logger.Info($"Was sorted file {fileName}");
        Logger.Info($"Sort time in seconds: {stopwatch.Elapsed.TotalSeconds}");

        Logger.Info("END");
      }
      catch (Exception exception)
      {
        Logger.Error("Fatal exception occured (program execution failed)", exception);
      }

      Logger.Info("Press \"Enter\" to exit");
      Console.ReadLine();
    }
  }
}
