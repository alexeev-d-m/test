using Common.DAL;
using Common.DAL.Logger;
using Consumer.DAL;
using System;
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
        Logger.Info("START");

        KafkaManager kafkaManager = new();
        string filePath = await kafkaManager.ReadSingleMessageAsync("quickstart-events");

        Logger.Info($"Received file path ({filePath}) - sort it now!");

        Stopwatch stopwatch = new();
        stopwatch.Start();

        SortManager sortManager = new(filePath);
        await sortManager.SortAsync();

        stopwatch.Stop();

        Logger.Info($"Was sorted file {filePath}");
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
