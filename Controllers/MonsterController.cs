using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MonsterBuilder.Models.Monster;

namespace MonsterBuilder.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class MonsterController : ControllerBase
  {

    static readonly HttpClient client = new HttpClient();


    private async Task<Monster> GetMonster(string url)
    {
      Console.WriteLine(url);


      var response = await client.GetAsync(url);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      // Above three lines can be replaced with new helper method below
      // string responseBody = await client.GetStringAsync(uri);
      var htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(responseBody);

      var rv = new Monster();
      var data = htmlDoc
        .DocumentNode
        .SelectSingleNode("//span[@id='ctl00_MainContent_DataListFeats_ctl00_Label1']")
        .ChildNodes
        .Where(w =>
        {
          return !String.IsNullOrEmpty(w.InnerHtml.Trim());
        }).Select(s => s).ToList();

      var i = 0;
      foreach (var item in data)
      {
        Console.WriteLine($"{i}=>{item.InnerText}");
        Console.WriteLine("-------");
        i++;
      }
      rv.Name = data[0].InnerText;
      rv.ChallengeRating = data[2].InnerText.Split(" ").Last();
      // N Huge magical beast (aquatic, augmented animal)
      var details = data[7].InnerText;
      var splat = details.Split(' ');
      rv.Alignment = splat[0];
      rv.Size = splat[1];
      rv.Type = String.Join(String.Empty, details.Skip(rv.Alignment.Length + rv.Size.Length + 2)).Split($"(")[0];
      if (details.Contains($"("))
      {
        rv.SubType = details.Substring(details.IndexOf($"("));
      }
      //   foreach (var item in details)
      //   {
      //     Console.WriteLine("item-----");


      //     Console.WriteLine(item);

      //   }
      //   Console.WriteLine("============================");

      //   var monsterBar = data[0].ChildNodes.Select(s => s.InnerText).ToList();
      //   rv.Name = monsterBar[0];
      //   rv.ChallengeRating = monsterBar[1].Replace("CR", "").Trim();

      //   var summary = data[1].ChildNodes.Where(w => w.InnerText.Trim().Length > 0).ToList();
      //   var alignmentAndSize = summary[1].InnerText.Split(' ');
      //   rv.Alignment = alignmentAndSize[0];
      //   rv.Size = alignmentAndSize[1];

      //   foreach (var item in summary.ChildNodes.Where(w => w.InnerHtml.Trim().Length > 0))
      //   {
      //     Console.WriteLine($"{item.InnerText.Trim()}");
      //     Console.WriteLine("~~~~~");
      //   }


      return rv;
    }

    // use  instead, d20pfsrd is dumb
    [HttpGet("{name}")]
    public async Task<ActionResult> GetBuildStatusAsync(string name)
    {
      var monster = await GetMonster($"https://aonprd.com/MonsterDisplay.aspx?ItemName={name}");

      return Ok(new { monster });
    }
  }
}