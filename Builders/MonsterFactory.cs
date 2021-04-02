using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
      rv = this.BuildAbilities(rv);
      rv = this.BuildDefenses(rv);
      return rv;
    }

    private int parseInt(string raw)
    {
      var rv = 0;
      var str = raw.Replace(",", "").Replace("+", "").Replace(";", "").Trim();
      Int32.TryParse(str, out rv);
      return rv;
    }

    private int? findStartIndex(string needle) => data
          .Select((item, index) => new { index, item, text = item.InnerHtml })
          .Where(w => w.text == needle).FirstOrDefault()?.index;

    private Monster BuildAbilities(Monster monster)
    {
      var startIndex = findStartIndex("Feats").GetValueOrDefault();
      startIndex++;
      var findingFeats = true;
      while (findingFeats)
      {
        monster.Abilities.Feats.Add(data[startIndex].InnerHtml);
        if (data[startIndex + 1].InnerHtml.Contains(","))
        {
          startIndex += 2;
        }
        else
        {
          findingFeats = false;
        }
      }

      var skillIndex = findStartIndex("Skills").GetValueOrDefault();
      monster.Abilities.Skills = data[skillIndex + 1].InnerHtml;
      var racialModifierIndex = findStartIndex("Racial Modifiers");
      if (racialModifierIndex.HasValue)
      {
        monster.Abilities.RacialModifiers = data[racialModifierIndex.GetValueOrDefault() + 1].InnerHtml;
      }

      var languageIndex = findStartIndex("Languages");
      if (languageIndex.HasValue)
      {
        monster.Abilities.Languages = data[languageIndex.GetValueOrDefault() + 1].InnerHtml;
      }

      return monster;
    }

    private Monster BuildBaseStats(Monster monster)
    {
      // find index of base stats
      var startIndex = findStartIndex("Statistics").GetValueOrDefault();
      monster.BaseStats.Strength = this.parseInt(data[startIndex + 2].InnerHtml);
      monster.BaseStats.Dexterity = this.parseInt(data[startIndex + 4].InnerHtml);
      monster.BaseStats.Constitution = this.parseInt(data[startIndex + 6].InnerHtml);
      monster.BaseStats.Intelligence = this.parseInt(data[startIndex + 8].InnerHtml);
      monster.BaseStats.Wisdom = this.parseInt(data[startIndex + 10].InnerHtml);
      monster.BaseStats.Charisma = this.parseInt(data[startIndex + 12].InnerHtml);
      monster.BaseStats.BaseAttackBonus = this.parseInt(data[startIndex + 14].InnerHtml);
      monster.BaseStats.CMB = data[startIndex + 16].InnerHtml.Trim();
      monster.BaseStats.CMD = data[startIndex + 18].InnerHtml.Trim();
      return monster;
    }

    private Monster BuildSummary(Monster monster)
    {
      monster.Summary.Name = data[0].InnerText;
      monster.Summary.ChallengeRating = data[2].InnerText.Split(" ").Last();
      // N Huge magical beast (aquatic, augmented animal)
      var initIndex = this.findStartIndex("Init").GetValueOrDefault();
      var details = data[initIndex - 1].InnerText;
      var splat = details.Split(' ');
      monster.Summary.Alignment = splat[0];
      monster.Summary.Size = splat[1];
      monster.Summary.Type = String.Join(String.Empty, details.Skip(monster.Summary.Alignment.Length + monster.Summary.Size.Length + 2)).Split($"(")[0];
      monster.Summary.Init = data[initIndex + 1].InnerHtml.Replace(";", String.Empty);
      monster.Summary.Senses = data[initIndex + 3].InnerHtml;
      if (details.Contains($"("))
      {
        monster.Summary.SubType = details.Substring(details.IndexOf($"("));
      }
      return monster;
    }

    private Monster BuildDefenses(Monster monster)
    {
      // AC 28, touch 10, flat-footed 27 (+1 Dex, +18 natural, –1 size)
      var startIndex = findStartIndex("AC").GetValueOrDefault();
      var allAc = data[startIndex + 1].InnerHtml;
      var firstSplit = allAc.Replace(")", "").Split("(");
      monster.Defenses.AcFormula = firstSplit.Last();
      var secondSplit = firstSplit.First().Split(",");
      monster.Defenses.AC = Int32.Parse(secondSplit[0].Trim());
      monster.Defenses.TouchAc = Int32.Parse(secondSplit[1].Replace("touch", "").Trim());
      monster.Defenses.FlatFlootedAc = Int32.Parse(secondSplit[2].Replace("flat-footed ", "").Trim());

      // hp 138 (12d12+60)
      var hpIndex = findStartIndex("hp").GetValueOrDefault();
      var hpData = data[hpIndex + 1].InnerHtml.Trim().Split(" ");
      Console.WriteLine(hpData[0]);


      monster.Defenses.HP = int.Parse(hpData[0].Trim());
      monster.Defenses.HpFormula = hpData[1].Replace("(", "").Replace(")", "");
      // Fort +13, Ref +9, Will +14
      var fortIndex = findStartIndex("Fort").GetValueOrDefault();
      monster.Defenses.Fort = int.Parse(data[fortIndex + 1].InnerHtml.Replace(",", ""));
      monster.Defenses.Reflex = int.Parse(data[fortIndex + 3].InnerHtml.Replace(",", ""));
      monster.Defenses.Will = int.Parse(data[fortIndex + 5].InnerHtml.Replace(",", ""));
      // DR 5/magic; Immune paralysis, sleep; SR 21
      var drIndex = findStartIndex("DR");
      if (drIndex.HasValue)
      {
        monster.Defenses.DamageReduction = data[drIndex.GetValueOrDefault() + 1].InnerHtml.Replace(";", "");
      }
      var srIndex = findStartIndex("SR");
      if (srIndex.HasValue)
      {
        monster.Defenses.SpellResistance = int.Parse(data[srIndex.GetValueOrDefault() + 1].InnerHtml.Replace(";", ""));
      }
      var immuneIndex = findStartIndex("Immune");
      if (immuneIndex.HasValue)
      {
        monster.Defenses.Immune = data[immuneIndex.GetValueOrDefault() + 1].InnerHtml.Replace(";", "");
      }
      return monster;
    }

  }
}