using System;
using System.Collections.Generic;

namespace MonsterBuilder.Models.Monster
{
  public class Abilities
  {
    public List<String> Feats { get; set; } = new List<String>();

    public string Skills { get; set; }

    public string RacialModifiers { get; set; }

    public string Languages { get; set; }
    public string Aura { get; set; }

    public List<AbilityDescription> AbilityDescriptions { get; set; } = new List<AbilityDescription>();




  }
}