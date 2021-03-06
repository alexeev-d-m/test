using Common.Models;
using Common.Models.Enums;
using Consumer.DAL;
using NUnit.Framework;
using Producer.DAL;
using System.Threading.Tasks;

namespace Tests
{
  /// <summary>
  /// Тестирование основной логики
  /// </summary>
  public class MainTests
  {
    /// <summary>
    /// Тестирование создания файла с его последующей сортировкой
    /// </summary>
    /// <remarks>Использование Kafka исключено в данной цепочке</remarks>
    // 1 Mb - 0.6 s
    [TestCase(1L * 1024 * 1024, "1mb.txt")]
    // 50 Mb - 28 s
    [TestCase(50L * 1024 * 1024, "50mb.txt")]
    // Длинные тесты - для сбора статистики!
    //// 1 Gb - 12.5 min
    //[TestCase(1L * 1024 * 1024 * 1024, "1Gb.txt")]
    //// 2 Gb - 28 min
    //[TestCase(2L * 1024 * 1024 * 1024, "2Gb.txt")]
    public async Task GenerateAndSort_Success(long maxFileSizeInBytes, string fileName)
    {
      FileGenerator fileGenerator = new(maxFileSizeInBytes, fileName);
      FileProcessingResult generatedFile = await fileGenerator.GenerateAsync();

      Assert.AreEqual(generatedFile.Status, FileProcessingStatus.Success);
      Assert.IsNull(generatedFile.Source);

      SortManager sortManager = new(generatedFile.Target);
      FileProcessingResult sortResult = await sortManager.SortAsync();

      Assert.AreEqual(sortResult.Status, FileProcessingStatus.Success);
      Assert.AreEqual(sortResult.Source, generatedFile.Target);
    }
  }
}
