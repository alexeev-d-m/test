﻿using Common.DAL.Logger;
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

        FileGenerator fileGenerator = new(maxFileSizeInBytes: 50l * 1024 * 1024);
        FileProcessingResult generatedFile = await fileGenerator.GenerateAsync();
        stopwatch.Stop();

        Logger.Info($"Was created file {generatedFile.Target}");
        Logger.Info($"Creation time in seconds: {stopwatch.Elapsed.TotalSeconds}");

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