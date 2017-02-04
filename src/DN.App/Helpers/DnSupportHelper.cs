using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using DN.App.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DN.App.Helpers
{
    public class DnSupportHelper
    {
        public static HttpClient _httpClient = new HttpClient();

        public static ConcurrentBag<SupportModel> _supportModels = null;

        public static async Task<List<SupportModel>> GetByTotalPage(int totalPage)
        {
            _supportModels = new ConcurrentBag<SupportModel>();

            var pageNumber = Convert.ToInt32(Math.Ceiling((double)totalPage / 20));
            var pageSize = 20;
            var tasks = new Task[pageNumber];
            for (int i = 0; i < pageNumber; i++)
            {
                var beginNumber = (i * pageSize) + 1;
                var endNumber = (i + 1) * pageSize;                
                if (endNumber > totalPage)
                {
                    endNumber = totalPage;
                }

                if(beginNumber > totalPage)
                {
                    break;
                }

                tasks[i] = GetByRange(beginNumber, endNumber);
            }

            foreach (var task in tasks)
            {
                await task;
            }

            return _supportModels.ToList();
        }

        public static async Task<string> GetByRange(int from, int to)
        {
            for (int i = from; i <= to; i++)
            {
                try
                {
                    var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("CURRENT_PAGE", i.ToString()) });

                    var httpResponse = await _httpClient.PostAsync("http://dncustoms.gov.vn/tu-van", content);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var htmlContent = await httpResponse.Content.ReadAsStringAsync();
                        var htmlParser = new HtmlParser();
                        var document = await htmlParser.ParseAsync(htmlContent);

                        var questionLinks = document.Body.QuerySelectorAll("div.stt a");
                        await GetDetails(questionLinks);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return string.Empty;
        }

        public static async Task<string> GetDetails(IHtmlCollection<IElement> elements)
        {
            foreach (var element in elements)
            {
                var link = element.GetAttribute("href");
                if (string.IsNullOrEmpty(link))
                {
                    continue;
                }

                var httpResponse = await _httpClient.GetAsync(link);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var htmlContnet = await httpResponse.Content.ReadAsStringAsync();
                    var htmlParser = new HtmlParser();
                    var document = await htmlParser.ParseAsync(htmlContnet);
                    ReadCompanyInfo(link, document);
                }
            }

            return string.Empty;
        }

        public static void ReadCompanyInfo(string link, IHtmlDocument document)
        {
            try
            {
                var supprtModel = new SupportModel();

                var pElements = document.QuerySelectorAll("div.dv-ct-top p");
                foreach (var pElement in pElements)
                {
                    if (pElement.TextContent.Contains("Ngày gửi:") && pElement.TextContent.Contains("Trả lời:"))
                    {
                        var days = pElement.TextContent
                            .Replace("\n", string.Empty)
                            .Replace("Ngày gửi:", string.Empty)
                            .Replace("Trả lời:", string.Empty)
                            .Split(new char[] { '-' });

                        supprtModel.QuestionDay = days[0].Trim();
                        supprtModel.AnswerDay = days[1].Trim();
                    }

                    if (pElement.TextContent.Contains("Tên doanh nghiệp:"))
                    {
                        supprtModel.CompanyName = pElement.TextContent
                            .Replace("\n", string.Empty)
                            .Replace("Tên doanh nghiệp:", string.Empty)
                            .Trim();
                    }

                    if (pElement.TextContent.Contains("Địa chỉ:") && pElement.TextContent.Contains("Email"))
                    {
                        var emailAndAddress = pElement.TextContent
                            .Replace("\n", string.Empty)
                            .Replace("Địa chỉ:", string.Empty)
                            .Split(new string[] { "- Email :" }, StringSplitOptions.RemoveEmptyEntries);

                        supprtModel.CompanyAddress = emailAndAddress[0].Trim();
                        supprtModel.CompanyEmail = emailAndAddress[1].Trim();
                    }
                }                                                

                _supportModels.Add(supprtModel);
            }
            catch (Exception ex)
            {
            }
        }
    }
}