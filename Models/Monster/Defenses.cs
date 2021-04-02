namespace MonsterBuilder.Models.Monster
{
  public class Defenses
  {
    public int AC { get; set; }

    public int TouchAc { get; set; }

    public int FlatFlootedAc { get; set; }

    public string AcFormula { get; set; }

    public int HP { get; set; }

    public string HpFormula { get; set; }

    public int Fort { get; set; }

    public int Reflex { get; set; }

    public int Will { get; set; }

    public string DamageReduction { get; set; }

    public string Immune { get; set; }

    public int SpellResistance { get; set; }

  }
}