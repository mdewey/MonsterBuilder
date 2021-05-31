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
      rv = this.BuildSpecialAbilityList(rv);
      rv = this.BuildDefenses(rv);
      rv = this.BuildSpeed(rv);
      rv = this.BuildAttacks(rv);
      rv = this.BuildGear(rv);
      return rv;
    }

    private Monster BuildSpecialAbilityList(Monster rv)
    {
      var pos = findStartIndex("Special Abilities");
      if (!pos.HasValue)
      {
        return rv;
      }
      var position = pos.GetValueOrDefault();
      position++;
      var dataPoint = data[position].InnerHtml;
      while (dataPoint.Trim() != "Description")
      {
        Console.WriteLine("*****");
        Console.WriteLine(dataPoint);
        AbilityDescription ad = null;
        if (dataPoint.Contains("(Ex)") || dataPoint.Contains("(Su)") || dataPoint.Contains("(Sp)"))
        {
          // dataPoint description
          ad = new AbilityDescription();
          ad.Name = dataPoint;
          position++;
          var next = data[position + 1].InnerHtml;
          do
          {

            dataPoint = data[position].InnerHtml;
            ad.Description += dataPoint;
            next = data[position + 1].InnerHtml;
            position++;
          } while (!(next.Contains("(Ex)") || next.Contains("(Su)") || next.Contains("(Sp)") || next == "Description"));
        }
        rv.Abilities.AbilityDescriptions.Add(ad);
        dataPoint = data[position].InnerHtml;
      }
      return rv;
    }

    private Monster BuildGear(Monster rv)
    {
      var combatGear = findStartIndex("Combat Gear");
      if (combatGear.HasValue)
      {
        rv.Gear.Combat = data[combatGear.GetValueOrDefault() + 1].InnerHtml.Split(',').ToList();
      }
      var otherGear = findStartIndex("Other Gear");
      if (otherGear.HasValue)
      {
        var otherStart = otherGear.GetValueOrDefault() + 1;
        var Ecology = findStartIndex("Ecology").GetValueOrDefault();
        while (otherStart < Ecology || otherStart >= otherGear.GetValueOrDefault() + 50)
        {
          var raw = Regex
            .Split(data[otherStart].InnerHtml, "(?<![0-9]),")
            .Select(s => s.Trim())
            .Where(w => !String.IsNullOrWhiteSpace(w));
          rv.Gear.Other.AddRange(raw);
          otherStart++;
        }




      }

      return rv;
    }

    private Monster BuildSpeed(Monster rv)
    {
      var index = findStartIndex("Speed").GetValueOrDefault();
      index++;
      rv.Movements = data[index].InnerHtml.Split(",").Select(s => s.Trim()).ToList();
      return rv;
    }


    private Monster BuildAttacks(Monster rv)
    {
      var melee = findStartIndex("Melee");
      if (melee.HasValue)
      {
        melee++;
        var baseAttack = data[melee.GetValueOrDefault()].InnerHtml;
        if (!(Regex.Match(baseAttack, "[0-9]d[0-9]").Success))
        {
          var attacks = data[melee.GetValueOrDefault()].InnerHtml.Split(",").Select(s => s.Trim()).ToList();
          attacks.AddRange(data[melee.GetValueOrDefault() + 1].InnerHtml.Split(",").Select(s => s.Trim()));
          rv.Attacks.Melee.Add(String.Join(' ', attacks));
        }
        else
        {
          rv.Attacks.Melee = data[melee.GetValueOrDefault()].InnerHtml.Split(",").Select(s => s.Trim()).ToList();
        }
      }

      var reach = findStartIndex("Reach");
      if (reach.HasValue)
      {
        rv.Attacks.Reach = data[reach.GetValueOrDefault() + 1].InnerHtml.Trim();
      }
      else
      {
        rv.Attacks.Reach = "5ft";
      }

      var ranged = findStartIndex("Ranged");
      if (ranged.HasValue)
      {
        ranged++;
        var baseAttack = data[ranged.GetValueOrDefault()].InnerHtml;
        if (!(Regex.Match(baseAttack, "[0-9]d[0-9]").Success))
        {
          var attacks = data[ranged.GetValueOrDefault()].InnerHtml.Split(",").Select(s => s.Trim()).ToList();
          attacks.AddRange(data[ranged.GetValueOrDefault() + 1].InnerHtml.Split(",").Select(s => s.Trim()));
          rv.Attacks.Ranged.Add(String.Join(' ', attacks));
        }
        else
        {
          rv.Attacks.Ranged = data[ranged.GetValueOrDefault()].InnerHtml.Split(",").Select(s => s.Trim()).ToList();
        }
      }

      var special = findStartIndex("Special Attacks");
      if (special.HasValue)
      {
        special++;
        var baseAttack = data[special.GetValueOrDefault()].InnerHtml;
        rv.Attacks.Special.AddRange(Regex.Split(baseAttack, @",(?=(((?!\)).)*\()|[^\(\)]*$)", RegexOptions.IgnoreCase).Select(s => s.Trim()).Where(w => !String.IsNullOrEmpty(w)));
      }
      return rv;

    }
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

      var aura = findStartIndex("Aura");
      if (aura.HasValue)
      {
        monster.Abilities.Aura = $"{data[aura.GetValueOrDefault() + 1].InnerHtml},{data[aura.GetValueOrDefault() + 2].InnerHtml.Trim()}";
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

      // Ogre King CR 13
      monster.Summary.ChallengeRating = data.First(f => f.InnerHtml.Contains(" CR ")).InnerText.Split(" ").Last();

      // N Huge magical beast (aquatic, augmented animal)
      var initIndex = this.findStartIndex("Init").GetValueOrDefault();
      var details = data[initIndex - 1].InnerText;
      var splat = details.Split(' ');
      monster.Summary.Alignment = splat[0];
      monster.Summary.Size = splat[1];
      monster.Summary.Type = String.Join(String.Empty, details.Skip(monster.Summary.Alignment.Length + monster.Summary.Size.Length + 2)).Split($"(")[0];
      monster.Summary.Init = data[initIndex + 1].InnerHtml.Replace(";", String.Empty);
      monster.Summary.Senses = data[initIndex + 3].InnerHtml.Trim();
      if (details.Contains($"("))
      {
        monster.Summary.SubType = details.Substring(details.IndexOf($"("));
      }

      var space = findStartIndex("Space");
      if (space.HasValue)
      {
        monster.Summary.Space =
          data[space.GetValueOrDefault() + 1]
          .InnerHtml
          .Replace(".", "")
          .Replace(",", "").Trim();
      }
      else
      {
        monster.Summary.Space = "5ft";
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
      monster.Defenses.FlatFlootedAc = Int32.Parse(secondSplit[2].Replace("flat-footed ", "").Replace(';', ' ').Trim());

      // hp 138 (12d12+60)
      var hpIndex = findStartIndex("hp").GetValueOrDefault();
      var rawHp = data[hpIndex + 1];
      var hpData = rawHp.InnerHtml.Trim().Split(" ");
      var regenData = rawHp.InnerHtml.Split(';');
      if (regenData.Length > 1)
      {
        monster.Defenses.Regeneration = regenData[1].Trim();
      }

      monster.Defenses.HP = int.Parse(hpData[0].Trim());
      monster.Defenses.HpFormula = hpData[1].Replace("(", "").Replace(")", "");
      // Fort +13, Ref +9, Will +14;+2 against mind affecting
      var fortIndex = findStartIndex("Fort").GetValueOrDefault();
      monster.Defenses.Fort = data[fortIndex + 1].InnerHtml.Replace(",", "").Trim();
      monster.Defenses.Reflex = data[fortIndex + 3].InnerHtml.Replace(",", "").Trim();
      monster.Defenses.Will = data[fortIndex + 5].InnerHtml.Replace(",", "").Trim();
      // DR 5/magic; Immune paralysis, sleep; SR 21
      var drIndex = findStartIndex("DR");
      if (drIndex.HasValue)
      {
        monster.Defenses.DamageReduction = data[drIndex.GetValueOrDefault() + 1].InnerHtml.Replace(";", "").Trim();
      }
      var srIndex = findStartIndex("SR");
      if (srIndex.HasValue)
      {
        monster.Defenses.SpellResistance = int.Parse(data[srIndex.GetValueOrDefault() + 1].InnerHtml.Replace(";", ""));
      }
      var immuneIndex = findStartIndex("Immune");
      if (immuneIndex.HasValue)
      {
        monster.Defenses.Immune = data[immuneIndex.GetValueOrDefault() + 1].InnerHtml.Replace(";", "").Trim();
      }

      var resists = findStartIndex("Resist");
      if (resists.HasValue)
      {
        monster.Defenses.Resists = data[resists.GetValueOrDefault() + 1].InnerHtml.Replace(";", "").Trim();
      }
      return monster;
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
  }
}