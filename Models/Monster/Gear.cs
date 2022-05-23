using System;
using System.Collections.Generic;

namespace MonsterBuilder.Models.Monster
{
  public class Gear
  {
    public List<string> Combat { get; set; } = new List<string>();
    public List<string> Other { get; set; } = new List<string>();
    public String Treasure { get; set; }

  }
}