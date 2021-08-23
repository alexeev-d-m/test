//#define USE_10GB_FILE

using Common.DAL;
using Common.DAL.Logger;
using Common.Models;
using Producer.DAL;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Producer
{
  class Program
  {
    static async Task Main(string[] args)
    {
      Console.Title = "Генерация файла заданного размера";

      try
      {
        Logger.Info("START");
        Stopwatch stopwatch = new();
        stopwatch.Start();

#if USE_10GB_FILE
        // 10 гигабайт - чтобы нагрузить алгоритмы генерации и сортировки
        FileGenerator fileGenerator = new(maxFileSizeInBytes: 10L * 1024 * 1024 * 1024, fileName: "10Gb.txt");
#else
        // 50 мегабайт - достаточно для тестирования программы
        FileGenerator fileGenerator = new(maxFileSizeInBytes: 50L * 1024 * 1024, fileName: "50mb.txt");
#endif
        FileProcessingResult generatedFile = await fileGenerator.GenerateAsync();
        stopwatch.Stop();

        Logger.Info($"Was created file {generatedFile.Target}");
        Logger.Info($"Creation time in seconds: {stopwatch.Elapsed.TotalSeconds}");

        Logger.Info("Send generated file path to kafka");
        KafkaManager kafkaManager = new();
        await kafkaManager.SendAsync("quickstart-events", generatedFile.Target);

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
