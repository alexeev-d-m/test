/*
 * Данный класс предназначен для того, чтобы у приложения был хоть какой-то логгер.
 * При желании можно использовать любой из существующих логгеров: log4net, Serilog.
 */
using System;
using System.Text;

namespace Common.DAL.Logger
{
  /// <summary>
  /// Логирование данных
  /// </summary>
  public static class Logger
  {
    /// <summary>
    /// Логирование сообщения <paramref name="message"/> с уровнем "Информация"
    /// </summary>
    /// <param name="message">Сообщение для логирования</param>
    public static void Info(string message)
      => _Log(nameof(Info), message);
    
    /// <summary>
    /// Логирование сообщения <paramref name="message"/> с уровнем "Предупреждение"
    /// </summary>
    /// <param name="message">Сообщение для логирования</param>
    public static void Warning(string message)
      => _Log(nameof(Warning), message);

    /// <summary>
    /// Логирование сообщения <paramref name="message"/> с уровнем "Ошибка"
    /// </summary>
    /// <param name="message">Сообщение для логирования</param>
    /// <param name="exception">Информация об ошибке</param>
    public static void Error(string message, Exception exception)
    {
      StringBuilder errorMessageBuilder = new();

      if (!string.IsNullOrWhiteSpace(message))
      {
        errorMessageBuilder.AppendLine(message);
      }

      if (!string.IsNullOrWhiteSpace(exception?.Message))
      {
        errorMessageBuilder
          .AppendLine("[Exception message]")
          .AppendLine(exception.Message);
      }

      // У InnerException может быть своё InnerException и т.д. Можно использовать рекурсию/цикл до тех
      // пор, пока не будет NULL/не превысим некий лимит вложенности, заданный разработчиком. Но задача
      // не про логирование, поэтому ограничимся одним Сообщением о вложенной ошибке.
      if (!string.IsNullOrWhiteSpace(exception?.InnerException?.Message))
      {
        errorMessageBuilder
          .AppendLine("[Inner exception message]")
          .AppendLine(exception.InnerException.Message);
      }

      _Log(nameof(Error), errorMessageBuilder.ToString());
    }

    /// <summary>
    /// Логирование сообщения <paramref name="message"/> с указанным типом сообщения <paramref name="messageType"/>
    /// </summary>
    /// <param name="messageType">Тип сообщения</param>
    /// <param name="message">Сообщение</param>
    private static void _Log(string messageType, string message)
      => Console.WriteLine($"[{messageType}, {DateTime.Now:dd.MM.yyyy HH:mm:ss}] {message}");
  }
}
