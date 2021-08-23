using Common.DAL;
using Common.DAL.Logger;
using Common.Models;
using Common.Models.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Consumer.DAL
{
  /// <summary>
  /// Управляющий файлом
  /// </summary>
  internal class SortManager
  {
    /// <summary>
    /// Конструктор с возможностью указания пути к обрабатываемому файлу
    /// </summary>
    /// <param name="filePath">Путь к обрабатываемому файлу</param>
    public SortManager(string filePath)
    {
      _filePath = filePath;
    }

    /// <summary>
    /// Обрабатываемый файл
    /// </summary>
    private readonly string _filePath;

    /// <summary>
    /// Сортировка файла
    /// </summary>
    public async Task<FileProcessingResult> SortAsync()
    {
      FileInfo sourceFileInfo = new(_filePath);
      if (!sourceFileInfo.Exists)
      {
        throw new FileNotFoundException("File not found", _filePath);
      }

      Logger.Info("Split source file on shards");
      IEnumerable<FileInfo> fileShards = await SplitFileOnShardsAsync(sourceFileInfo);

      Logger.Info("Sort all shards content");
      await SortShardsContentAsync(fileShards);

      Logger.Info("Create sorted file");
      FileInfo sortedDataFile = await CreateSortedFileAsync(fileShards);

      return new FileProcessingResult(
         source: _filePath,
         target: sortedDataFile.FullName,
         FileProcessingStatus.Success
        );

      static async Task<FileInfo> CreateSortedFileAsync(IEnumerable<FileInfo> shards)
      {
        const int MERGE_SHARDS_BATCH_SIZE = 20;
        const int MAX_MERGE_RUNNING_TASKS = 5;

        IEnumerable<FileInfo> currentMergingFiles = shards;
        List<FileInfo> nextMergeFiles;

        // Выполняем операцию слияния до тех пор, пока не останется всего один файл - он и будет результирующим.
        do
        {
          List<Task<FileInfo>> mergeTasks = new();
          nextMergeFiles = new List<FileInfo>();
          int mergingFilesCount = currentMergingFiles.Count();
          for (int mergeFileIndex = 0; mergeFileIndex < mergingFilesCount; mergeFileIndex += MERGE_SHARDS_BATCH_SIZE)
          {
            Logger.Info($"Merge files in range {mergeFileIndex} - {mergeFileIndex + MERGE_SHARDS_BATCH_SIZE}");

            mergeTasks.Add(MergeShardsAsync(currentMergingFiles.Skip(mergeFileIndex).Take(MERGE_SHARDS_BATCH_SIZE)));

            if (mergeTasks.Count > MAX_MERGE_RUNNING_TASKS)
            {
              Task<FileInfo> readyMergeTask = await Task.WhenAny(mergeTasks);

              nextMergeFiles.Add(readyMergeTask.GetAwaiter().GetResult());
              mergeTasks.Remove(readyMergeTask);
            }
          }

          // Дожидаемся оставшихся задач по слиянию
          await Task.WhenAll(mergeTasks);

          nextMergeFiles.AddRange(mergeTasks.Select(mergeTask => mergeTask.GetAwaiter().GetResult()));

          currentMergingFiles = nextMergeFiles;
        }
        while (nextMergeFiles.Count > 1);

        return nextMergeFiles.Single();

        static async Task<FileInfo> MergeShardsAsync(IEnumerable<FileInfo> shards)
        {
          FileInfo sortedDataFile = new($"MergeFolder/{Guid.NewGuid()}.txt");
          if (sortedDataFile.Exists)
          {
            throw new Exception("Sorted file already exists!");
          }

          if (!sortedDataFile.Directory.Exists)
          {
            sortedDataFile.Directory.Create();
          }

          /*
           * Для каждого файла-осколка создаётся поток данных, с помощью которого будет заполняться очередь.
           * Далее сравниваются данные в очередях с самым меньшим значением среди известных данных.
           */
          Dictionary<StreamReader, Queue<NumberString>> shardsReaders = new();
          try
          {
            // Инициализация потоков для чтения файлов и очередей для хранения промежуточных результатов
            foreach (FileInfo filePart in shards)
            {
              StreamReader fileDataReader = new(filePart.FullName);
              Queue<NumberString> queue = new();

              await LoadQueueAsync(queue, fileDataReader);

              shardsReaders.Add(fileDataReader, queue);
            }

            using StreamWriter sortedDataFileWriter = new(sortedDataFile.FullName);

            bool isDone = false;

            while (!isDone)
            {
              // По умолчанию задаём наибольшее возможное значение
              NumberString lowestValue = NumberString.MaxValue;
              // Поток для чтения очереди с меньшим значением
              StreamReader lowestValueReader = null;

              // Ищем "меньшее значение" в очередях
              foreach (KeyValuePair<StreamReader, Queue<NumberString>> shardReader in shardsReaders)
              {
                if (shardReader.Value is not null
                  &&
                  (
                    lowestValueReader is null
                    || shardReader.Value.Peek() < lowestValue
                  ))
                {
                  lowestValueReader = shardReader.Key;
                  lowestValue = shardReader.Value.Peek();
                }
              }

              isDone = lowestValueReader is null;

              if (!isDone)
              {
                // Записываем в итоговый файл с сортировкой меньшее значение
                sortedDataFileWriter.WriteLine(lowestValue.ToString());

                // Убираем из очереди отработанный элемент
                shardsReaders[lowestValueReader].Dequeue();

                // Если очередь очистилась, снова загружаем в неё данные
                if (shardsReaders[lowestValueReader].Count == 0)
                {
                  await LoadQueueAsync(shardsReaders[lowestValueReader], lowestValueReader);
                  // Файл был пройден до конца: считаем, что с этой очередью больше нечего делать
                  if (shardsReaders[lowestValueReader].Count == 0)
                  {
                    shardsReaders[lowestValueReader] = null;
                  }
                }
              }
            }
          }
          finally
          {
            if (shardsReaders?.Count > 0)
            {
              foreach (StreamReader reader in shardsReaders.Keys)
              {
                reader?.Dispose();
              }
            }
          }

          return sortedDataFile;

          static async Task LoadQueueAsync(Queue<NumberString> queue, StreamReader dataReader)
          {
            int queueBatchSizeCounter = 1_000;
            string line = null;
            while ((line = await dataReader.ReadLineAsync()) is not null
              && queueBatchSizeCounter > 0)
            {
              queue.Enqueue(NumberString.Parse(line));
              queueBatchSizeCounter--;
            }
          }
        }
      }

      static async Task SortShardsContentAsync(IEnumerable<FileInfo> shards)
      {
        const int MAX_SORT_CONTENT_RUNNING_TASKS = 7;

        Logger.Info($"Shards count: {shards.Count()}");

        CancellationTokenSource cancelTokenSource = new();
        CancellationToken cancelToken = cancelTokenSource.Token;
        List<Task> sortTasks = new();

        foreach (FileInfo shard in shards)
        {
          sortTasks.Add(SortShardContentAsync(shard, cancelToken));

          if (sortTasks.Count == MAX_SORT_CONTENT_RUNNING_TASKS)
          {
            Task completed = await Task.WhenAny(sortTasks);
            if (completed.Exception is not null)
            {
              cancelTokenSource.Cancel();
              throw completed.Exception;
            }
            // Исключаем выполненные задачи
            sortTasks.RemoveAll(task => task.IsCompleted);
          }
        }

        // Дожидаемся завершения оставшихся задач
        await Task.WhenAll(sortTasks);

        return;

        /*
         * Поскольку файл осколка небольшой по размеру, его можно полностью загрузить в ОЗУ и отсортировать,
         * избежав внешней сортировки на жёстком диске.
         */
        static async Task SortShardContentAsync(FileInfo shard, CancellationToken cancelToken)
        {
          NumberStringComparer numberStringComparer = new();
          if (!shard.Exists)
          {
            throw new FileNotFoundException("Shard not found", shard.FullName);
          }

          Logger.Info($"Read data from shard \"{shard.Name}\"");
          IEnumerable<NumberString> shardData = await ReadShardDataAsync(shard, cancelToken);

          if (shardData.Any())
          {
            Logger.Info($"Starting sort shard \"{shard.Name}\"");

            // Поскольку осколки имеют небольшой размер, решено применить к ним обычную сортировку из LINQ,
            // не прибегая к помощи PLINQ/Task/Thread.
            await WriteSortedDataAsync(
              shard,
              //sortedData: shardData.OrderBy(item => item.String, stringComparer).ThenBy(item => item.Number),
              sortedData: shardData.OrderBy(item => item, numberStringComparer),
              cancelToken);

            Logger.Info($"File \"{shard.Name}\" successfully sorted");
          }
          else
          {
            // Предполагается, что в каждом файле есть данные. Но немного подумав, решил ограничиться
            // предупреждением, а не пробрасыванием исключения.
            Logger.Warning($"No data available in file \"{shard.Name}\"");
          }

          return;

          static async Task WriteSortedDataAsync(
            FileInfo targetFile,
            IEnumerable<NumberString> sortedData,
            CancellationToken cancelToken)
          {
            // Код очень похож на то, что есть в FileGenerator (GenerateAsync()), но тут нет ограничения на размер
            // файла, записываемая в файл модель не создаётся на лету и нет нужды обновлять FileInfo.

            // Чтобы не писать по одной строке в файл, запись будет происходить пачками.
            // Размер подобран опытным путём.
            const long MAX_CONTENT_BATCH_LENGTH = 30_000;

            using StreamWriter fileWriter = new(targetFile.FullName);
            StringBuilder fileContent = new();
            foreach (NumberString numberString in sortedData)
            {
              fileContent.AppendLine(numberString.ToString());

              // Отправляем данные на запись пачками, чтобы не обращаться к диску каждый раз с малым объёмом данных
              if (fileContent.Length >= MAX_CONTENT_BATCH_LENGTH)
              {
                await fileWriter.WriteAsync(fileContent, cancelToken);
                fileContent.Clear();
              }
            }
          }

          static async Task<IEnumerable<NumberString>> ReadShardDataAsync(
            FileInfo shard,
            CancellationToken cancelToken)
          {
            ConcurrentQueue<NumberString> data = new();

            Parallel.ForEach(await File.ReadAllLinesAsync(shard.FullName, cancelToken),
              line =>
              {
                data.Enqueue(NumberString.Parse(line));
              });

            return data;
          }
        }
      }

      static async Task AppendRemainingLineAsync(StreamReader streamReader, StringBuilder stringBuilder)
      {
        string remainingLine = await streamReader.ReadLineAsync();
        if (!string.IsNullOrWhiteSpace(remainingLine))
        {
          stringBuilder.Append(remainingLine);
        }
      }

      static async Task<IEnumerable<FileInfo>> SplitFileOnShardsAsync(FileInfo sourceFileInfo)
      {
        const int MAX_SHARD_LENGTH = 5_000_000;
        const int CHAR_BUFFER_SIZE = 4_196;

        List<FileInfo> shards = new();
        int shardsCounter = 0;

        StringBuilder shardBuilder = new();
        using StreamReader streamReader = new(sourceFileInfo.FullName);

        // Быстрее считывать в небольшой буфер обмена, а после дочитывать до конца строки, чем читать построчно.
        int readCount;
        do
        {
          char[] buffer = new char[CHAR_BUFFER_SIZE];
          readCount = await streamReader.ReadAsync(buffer, 0, buffer.Length);

          shardBuilder.Append(new string(buffer));

          if (shardBuilder.Length > MAX_SHARD_LENGTH)
          {
            // Дочитываем оставшуюся строку
            await AppendRemainingLineAsync(streamReader, shardBuilder);

            FileInfo newShard = NewShard(shardsCounter++);
            using StreamWriter fileWriter = new(newShard.FullName);
            await fileWriter.WriteAsync(shardBuilder.ToString());
            shardBuilder.Clear();
            // Файл заполняется после формирования объекта FileInfo - как итог, без обновления состояния
            // FileInfo считает, что файла нет.
            newShard.Refresh();
            shards.Add(newShard);
            Logger.Info($"New shard created (\"{newShard.Name}\")");
          }
        } while (readCount > 0);

        return shards;

        static FileInfo NewShard(int number)
        {
          FileInfo shard = new($"Shards/Shard_{number}.txt");

          // Оставшиеся с предыдущей попытки файлы
          if (shard.Exists)
          {
            shard.Delete();
          }

          if (!shard.Directory.Exists)
          {
            shard.Directory.Create();
          }

          return shard;
        }
      }
    }
  }
}
