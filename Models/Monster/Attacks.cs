using System.Collections.Generic;

namespace MonsterBuilder.Models.Monster
{
  public class Attacks
  {
    public List<string> Melee { get; set; } = new List<string>();
    public List<string> Ranged { get; set; } = new List<string>();

  }
}