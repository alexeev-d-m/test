using System.Collections.Generic;

namespace Consumer.DAL
{
  /// <summary>
  /// Для сравнения строк по общей логике
  /// </summary>
  internal class CommonStringComparer : IComparer<string>
  {
    /// <inheritdoc/>
    public int Compare(string x, string y)
      => string.CompareOrdinal(x, y);
  }
}
