using Common.Models;
using Common.Models.Enums;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Producer.DAL
{
  /// <summary>
  /// Генератор файла
  /// </summary>
  internal class FileGenerator
  {
    /// <summary>
    /// Конструктор с возможностью указания максимальной длины размера файла в байтах
    /// </summary>
    /// <param name="maxFileSizeInBytes">Максимальный размер файла в байтах</param>
    public FileGenerator(long maxFileSizeInBytes)
    {
      if (maxFileSizeInBytes < 1)
      {
        throw new ArgumentOutOfRangeException(nameof(maxFileSizeInBytes), $"Must be greater then zero");
      }

      _maxFileSizeInBytes = maxFileSizeInBytes;
    }

    /// <summary>
    /// Максимальный размер файла в байтах
    /// </summary>
    private readonly long _maxFileSizeInBytes;

    /// <summary>
    /// Генерирование файла
    /// </summary>
    /// <returns>Результат генерации файла</returns>
    public async Task<FileProcessingResult> GenerateAsync()
    {
      const string FILE_NAME = "MessFileData_test_1Gb.txt";

      // Чтобы не писать по одной строке в файл, запись будет происходить пачками.
      // Размер подобран опытным путём.
      const long MAX_CONTENT_BATCH_LENGTH = 30_000;
      
      FileInfo fileInfo = new(FILE_NAME);
      if (fileInfo.Exists)
      {
        fileInfo.Delete();
      }

      using StreamWriter fileWriter = new(fileInfo.FullName);

      fileInfo.Refresh();

      StringBuilder fileContent = new();
      do
      {
        fileContent.AppendLine(NumberString.RandomItem.ToString());
        
        // Отправляем данные на запись пачками, чтобы не обращаться к диску каждый раз с малым объёмом данных
        if (fileContent.Length >= MAX_CONTENT_BATCH_LENGTH)
        {
          await fileWriter.WriteAsync(fileContent);
          fileContent.Clear();
          fileInfo.Refresh();
        }
        
      } while (fileInfo.Length < _maxFileSizeInBytes);

      // Дозаписываем оставшееся содержимое (не превышающее MAX_BATCH_SIZE_IN_BYTES и оттого не попавшее в файл
      // в цикле выше).
      if (fileContent.Length > 0)
      {
        await fileWriter.WriteAsync(fileContent);
      }

      return new FileProcessingResult(
        source: null,
        target: fileInfo.FullName,
        FileProcessingStatus.Success);
    }
  }
}
