using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using MonsterBuilder.Models.Search;

namespace MonsterBuilder.Builders
{
  public class SearchResults
  {
    public HtmlDocument htmlDoc { get; set; } = new HtmlDocument();
    public List<HtmlNode> data { get; set; } = new List<HtmlNode>();

    public SearchResults(string html)
    {
      htmlDoc.LoadHtml(html);
      data = htmlDoc
        .DocumentNode
        .SelectSingleNode("//span[@id='ctl00_MainContent_SearchOutput']")
        .ChildNodes
        .Where(w =>
        {
          return !String.IsNullOrEmpty(w.InnerHtml.Trim());
        }).Select(s => s).ToList();
    }

    public SearchResults ShowData()
    {
      var i = 0;
      foreach (var item in data)
      {
        Console.WriteLine($"{i}=>{item.InnerHtml}");
        Console.WriteLine("-------");
        i++;
      }
      return this;
    }

    public Haystack Build()
    {
      var rv = new Haystack();
      var resultsPosition = findStartIndex("Monsters");
      rv.Results = data
        .Skip(resultsPosition.GetValueOrDefault() + 1)
        .Where(w => w.InnerText.Trim() != ",")
        .Select(s =>
          new Result
          {
            Name = s.InnerText,
            Slug = s
              .ChildNodes
              .FirstOrDefault()?
              .GetAttributeValue("href", null)?
              .Replace("MonsterDisplay.aspx?ItemName=", "")
          })
        .Take(15)
        .ToList();
      return rv;
    }

    private int? findStartIndex(string needle) => data
         .Select((item, index) => new { index, item, text = item.InnerHtml })
         .Where(w => w.text == needle).FirstOrDefault()?.index;

  }
}