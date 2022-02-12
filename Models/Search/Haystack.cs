using System.Collections.Generic;

namespace MonsterBuilder.Models.Search
{
  public class Haystack
  {
    public string Needle { get; set; }
    public List<Result> Results { get; set; } = new List<Result>();

  }
}