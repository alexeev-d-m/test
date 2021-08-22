using Common.Models.Enums;

namespace Common.Models
{
  /// <summary>
  /// Результат обрбаотки файла
  /// </summary>
  public class FileProcessingResult
  {
    /// <summary>
    /// Конструктор для заполнения полей файла
    /// </summary>
    /// <param name="source">Файл-источник для обработки</param>
    /// <param name="target">Результирующий файл</param>
    /// <param name="isSuccess">Статус, выставленный по результату обрбаотки файла</param>
    public FileProcessingResult(string source, string target, FileProcessingStatus status)
    {
      Source = source;
      Target = target;
      Status = status;
    }

    /// <summary>
    /// Файл-источник для обработки
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Результирующий файл
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// Статус обработки файла
    /// </summary>
    public FileProcessingStatus Status { get; set; }
  }
}
