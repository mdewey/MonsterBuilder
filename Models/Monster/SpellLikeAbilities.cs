using System.Collections.Generic;

namespace MonsterBuilder.Models.Monster
{
  public class SpellLikeAbilities
  {
    public string CasterLevel { get; set; }
    public string Concentration { get; set; }
    public List<SpellLikeAbilityLevel> SpellsKnown { get; set; } = new List<SpellLikeAbilityLevel>();
  }

  public class SpellLikeAbilityLevel
  {
    public string Frequency { get; set; }

    public List<SpellLikeAbility> Spells { get; set; } = new List<SpellLikeAbility>();
  }
  public class SpellLikeAbility
  {
    public string Name { get; set; }
    public string DC { get; set; }
  }
}