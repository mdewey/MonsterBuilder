namespace MonsterBuilder.Models.Monster
{
  public class BaseStats
  {
    public int Strength { get; set; }
    public int StrMod
    {
      get
      {
        return this.GetModifier(this.Strength);
      }
    }


    public int Dexterity { get; set; }
    public int DexMod
    {
      get
      {
        return this.GetModifier(this.Dexterity);
      }
    }
    public int Constitution { get; set; }
    public int ConMod
    {
      get
      {
        return this.GetModifier(this.Constitution);
      }
    }

    public int Wisdom { get; set; }
    public int WisMod
    {
      get
      {
        return this.GetModifier(this.Wisdom);
      }
    }

    public int Charisma { get; set; }
    public int ChaMod
    {
      get
      {
        return this.GetModifier(this.Charisma);
      }
    }

    public int Intelligence { get; set; }
    public int IntMod
    {
      get
      {
        return this.GetModifier(this.Intelligence);
      }
    }

    private int GetModifier(int score) => (score - 10) / 2;


  }
}