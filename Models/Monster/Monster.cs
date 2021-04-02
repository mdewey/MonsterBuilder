namespace MonsterBuilder.Models.Monster
{
  public class Monster
  {
    public Summary Summary { get; set; } = new Summary();

    public BaseStats BaseStats { get; set; } = new BaseStats();

    public Abilities Abilities { get; set; } = new Abilities();

    public Defenses Defenses { get; set; } = new Defenses();



  }
}