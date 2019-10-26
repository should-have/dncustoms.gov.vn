using DN.Core.Helpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DN.App.Controllers
{
    public class SupportController : Controller
    {
        // GET: Support
        public async Task<ActionResult> Index(int totalPage = 915)
        {
            var viewModel = await DnSupportHelper.GetByTotalPage(totalPage);
            viewModel = viewModel.Where(p => !string.IsNullOrEmpty(p.CompanyEmail)).GroupBy(p => p.CompanyEmail).Select(p => p.FirstOrDefault()).ToList();

            HttpContext.Response.AddHeader("content-disposition", "attachment; filename=dncustoms_support.xls");
            Response.ContentType = "application/vnd.ms-excel";
            Response.ContentEncoding = Encoding.GetEncoding("utf-8");
            Response.Charset = "utf-8";
            Response.BinaryWrite(Encoding.GetEncoding("utf-8").GetPreamble());

            return View(viewModel);
        }
    }
}