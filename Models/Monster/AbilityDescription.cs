using System;

namespace MonsterBuilder.Models.Monster
{
  public class AbilityDescription
  {
    public string Name { get; set; }

    public string Type
    {
      get
      {
        if (this.Name == null)
        {
          return "";
        }
        if (this.Name.Contains("(Su)"))
        {
          return "Supernatural";
        }
        else if (this.Name.Contains("(Sp)"))
        {
          return "Spell-Like Ability";
        }
        else if (this.Name.Contains("(Ex)"))
        { return "Extraordinary"; }
        else
        {
          return "";
        }
      }
    }

    public string Description { get; set; }


  }
}