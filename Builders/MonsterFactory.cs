using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using MonsterBuilder.Models.Monster;

namespace MonsterBuilder.Builders
{
  public class MonsterFactory
  {
    public HtmlDocument htmlDoc { get; set; } = new HtmlDocument();
    public List<HtmlNode> data { get; set; } = new List<HtmlNode>();



    public MonsterFactory(string html)
    {
      htmlDoc.LoadHtml(html);
      data = htmlDoc
        .DocumentNode
        .SelectSingleNode("//span[@id='ctl00_MainContent_DataListFeats_ctl00_Label1']")
        .ChildNodes
        .Where(w =>
        {
          return !String.IsNullOrEmpty(w.InnerHtml.Trim());
        }).Select(s => s).ToList();
    }

    public MonsterFactory ShowData()
    {
      var i = 0;
      foreach (var item in data)
      {
        Console.WriteLine($"{i}=>{item.InnerText}");
        Console.WriteLine("-------");
        i++;
      }
      return this;
    }

    public Monster Build()
    {
      var rv = new Monster();
      rv = this.BuildSummary(rv);
      rv = this.BuildBaseStats(rv);
      return rv;
    }

    private int parseAbilityScore(string raw)
    {

      var rv = 0;
      Int32.TryParse(raw.Replace(",", ""), out rv);
      return rv;
    }

    private Monster BuildBaseStats(Monster monster)
    {
      // find index of base stats
      var startIndex = data
          .Select((item, index) => new { index, item, text = item.InnerHtml })
          .Where(w => w.text == "Statistics")
          .First().index;
      Console.WriteLine("stats index is" + startIndex);
      monster.BaseStats.Strength = this.parseAbilityScore(data[startIndex + 2].InnerHtml);
      monster.BaseStats.Dexterity = this.parseAbilityScore(data[startIndex + 4].InnerHtml);
      monster.BaseStats.Constitution = this.parseAbilityScore(data[startIndex + 6].InnerHtml);
      monster.BaseStats.Intelligence = this.parseAbilityScore(data[startIndex + 8].InnerHtml);
      monster.BaseStats.Wisdom = this.parseAbilityScore(data[startIndex + 10].InnerHtml);
      monster.BaseStats.Charisma = this.parseAbilityScore(data[startIndex + 12].InnerHtml);
      return monster;
    }

    private Monster BuildSummary(Monster monster)
    {
      monster.Summary.Name = data[0].InnerText;
      monster.Summary.ChallengeRating = data[2].InnerText.Split(" ").Last();
      // N Huge magical beast (aquatic, augmented animal)
      var details = data[7].InnerText;
      var splat = details.Split(' ');
      monster.Summary.Alignment = splat[0];
      monster.Summary.Size = splat[1];
      monster.Summary.Type = String.Join(String.Empty, details.Skip(monster.Summary.Alignment.Length + monster.Summary.Size.Length + 2)).Split($"(")[0];
      monster.Summary.Init = data[9].InnerHtml.Replace(";", String.Empty);
      monster.Summary.Senses = data[11].InnerHtml;
      if (details.Contains($"("))
      {
        monster.Summary.SubType = details.Substring(details.IndexOf($"("));
      }
      return monster;
    }

  }
}