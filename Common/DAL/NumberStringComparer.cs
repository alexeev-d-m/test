using Common.Models;
using System.Collections.Generic;

namespace Common.DAL
{
  /// <summary>
  /// Для сравнения между собой и сортировки объектов типа <see cref="NumberString"/>
  /// </summary>
  public class NumberStringComparer : IComparer<NumberString>
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="x">Первое сравниваемое значение</param>
    /// <param name="y">Второе сравниваемое значение</param>
    /// <returns>
    /// Меньше нуля, если x <b>меньше</b> y<para/>
    /// Больше нуля, если x <b>больше или равно</b> y
    /// </returns>
    public int Compare(NumberString x, NumberString y)
    {
      int STRINGS_ARE_EQUAL_CODE = 0;
      int compareResult = string.CompareOrdinal(x?.String, y?.String);

      // Строковые части равны - сравниваем числовые значения
      if (compareResult == STRINGS_ARE_EQUAL_CODE)
      {
        compareResult = (x?.Number < y?.Number)
          ? -1
          : 1;
      }

      return compareResult;
    }
  }
}
