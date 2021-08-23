using Common.DAL;
using System;

namespace Common.Models
{
  /// <summary>
  /// Класс, представляющий структуру вида "Number. String"
  /// </summary>
  public class NumberString
  {
    /// <summary>
    /// Конструктор для инициализации статичных полей
    /// </summary>
    static NumberString()
    {
      _random = new Random();
      _randomWords = Resources.TestData.Random_words_en.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
      _comparer = new NumberStringComparer();
      MaxValue = new NumberString
      {
        Number = int.MaxValue,
        // Немного слукавим: максимальное значение для строки это нонсенс, но будем считать, что 100 заданных
        // подряд максимально возможных значений char дадут нам "максимальную" строку.
        String = new string(char.MaxValue, 100)
      };
    }

    /// <summary>
    /// Для генерации <see cref="Number"/>
    /// </summary>
    private static readonly Random _random;

    /// <summary>
    /// Для сравнения строк
    /// </summary>
    private static readonly NumberStringComparer _comparer;

    /// <summary>
    /// Слова для генерации <see cref="String"/>
    /// </summary>
    private static readonly string[] _randomWords;

    /// <summary>
    /// Разделитель числа и строки
    /// </summary>
    private const string _NUMBER_STRING_SEPARATOR = ". ";

    /// <summary>
    /// Целочисленная часть
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Строковая часть
    /// </summary>
    public string String { get; set; }

    // Метод перегружен с целью упрощённого просмотра содержимого класса при отладке, а также записи в файл.
    /// <inheritdoc/>
    public override string ToString()
      => $"{Number}{_NUMBER_STRING_SEPARATOR}{String}";

    /// <summary>
    /// Объект со случайными данными
    /// </summary>
    /// <returns>Объект с произвольным заполнением полей</returns>
    public static NumberString RandomItem
      => new()
      {
        Number = _random.Next(minValue: 1, maxValue: 1_000_000),
        String = _randomWords[_random.Next(maxValue: _randomWords.Length)]
      };

    /// <summary>
    /// Максимальное значение (т.е. при сортировке от меньшего к большему будет последним)
    /// </summary>
    /// <returns>Максимальное значение, которое может принять данный объект</returns>
    public static readonly NumberString MaxValue;

    /// <summary>
    /// Преобразование строки в тип <see cref="NumberString"/>
    /// </summary>
    /// <param name="string">Строка для преобразования</param>
    /// <returns>Новый объект типа <see cref="NumberString"/></returns>
    /// <remarks>В случае ошибки будет брошено исключение</remarks>
    public static NumberString Parse(string @string)
    {
      int separatorIndex = @string.IndexOf(_NUMBER_STRING_SEPARATOR);

      return new NumberString
      {
        Number = int.Parse(@string[..separatorIndex]),
        String = @string[(separatorIndex + _NUMBER_STRING_SEPARATOR.Length)..]
      };
    }

    /// <summary>
    /// Код, означающий, что строки равны
    /// </summary>
    private const int _STRINGS_ARE_EQUAL_CODE = 0;

    /// <summary>
    /// Сравнение: какое из значений меньше?
    /// </summary>
    /// <param name="x">Первое сравниваемое значение</param>
    /// <param name="y">Второе сравниваемое значение</param>
    /// <returns>true, если a <b>меньше</b> b</returns>
    public static bool operator <(NumberString x, NumberString y)
      => _comparer.Compare(x, y) < 0;

    /// <summary>
    /// Сравнение: какое из значений больше?
    /// </summary>
    /// <param name="x">Первое сравниваемое значение</param>
    /// <param name="y">Второе сравниваемое значение</param>
    /// <returns>true, если a <b>больше</b> b</returns>
    public static bool operator >(NumberString x, NumberString y)
      => _comparer.Compare(x, y) > 0;
  }
}
