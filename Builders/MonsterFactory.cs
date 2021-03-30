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
      rv.Summary.Name = data[0].InnerText;
      rv.Summary.ChallengeRating = data[2].InnerText.Split(" ").Last();
      // N Huge magical beast (aquatic, augmented animal)
      var details = data[7].InnerText;
      var splat = details.Split(' ');
      rv.Summary.Alignment = splat[0];
      rv.Summary.Size = splat[1];
      rv.Summary.Type = String.Join(String.Empty, details.Skip(rv.Summary.Alignment.Length + rv.Summary.Size.Length + 2)).Split($"(")[0];
      rv.Summary.Init = data[9].InnerHtml.Replace(";", String.Empty);
      rv.Summary.Senses = data[11].InnerHtml;
      if (details.Contains($"("))
      {
        rv.Summary.SubType = details.Substring(details.IndexOf($"("));
      }
      return rv;
    }

  }
}