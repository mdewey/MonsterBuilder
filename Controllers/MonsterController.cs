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
      var htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(responseBody);

      var monster = new Builders.MonsterFactory(responseBody).ShowData().Build();

      return monster;
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