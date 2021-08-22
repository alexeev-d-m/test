namespace Common.Models.Enums
{
  /// <summary>
  /// Статус обработки файла
  /// </summary>
  public enum FileProcessingStatus
  {
    /// <summary>
    /// Неизвестно: ошибочное состояние. Можно считать, что объект не был проинициализирован.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Обработка прошла успешно
    /// </summary>
    Success = 1,

    /// <summary>
    /// Обработка закончилась ошибкой
    /// </summary>
    Error = 2
  }
}
