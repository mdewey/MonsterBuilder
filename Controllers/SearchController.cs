using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonsterBuilder.Builders;
using MonsterBuilder.Models.Search;

namespace MonsterBuilder.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class SearchController : ControllerBase
  {

    static readonly HttpClient client = new HttpClient();


    private async Task<Haystack> GetSearchResults(string url)
    {
      Console.WriteLine(url);
      var response = await client.GetAsync(url);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();

      var results = new SearchResults(responseBody).ShowData().Build();

      return results;
    }

    [HttpGet]
    public async Task<ActionResult> GetBuildStatusAsync(string needle)
    {
      var url = $"https://www.aonprd.com/Search.aspx?Query={needle}&Filter=000000010000000000&AllTerms=True&OneLine=False&ExcludeAPModule=False&PFSLegalOnly=False";
      var results = await GetSearchResults(url);
      results.Needle = needle;
      return Ok(results);
    }
  }
}