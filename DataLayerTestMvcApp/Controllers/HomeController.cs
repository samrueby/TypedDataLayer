using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExampleNamespace.DataAccess.TableRetrieval;
using TypedDataLayer.DataAccess;

namespace DataLayerTestMvcApp.Controllers {
	public class HomeController : Controller {
		public ActionResult Index() {
			return View();
		}
		
		public ActionResult TestDb() {
			ActionResult r = null;
			new DataAccessState().ExecuteWithThis( () => DataAccessState.Current.PrimaryDatabaseConnection.ExecuteWithConnectionOpen( () => {
				r = new ContentResult(){Content = UsersTableRetrieval.GetRows().First().EmailAddress};
			}) );
			return r;
		}

		public ActionResult About() {
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact() {
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}