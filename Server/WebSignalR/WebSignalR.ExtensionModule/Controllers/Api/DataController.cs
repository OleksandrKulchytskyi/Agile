using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.Interfaces.Bus;

namespace WebSignalR.ExtensionModule.Controllers.Api
{
	public class DataController : ApiController
	{
		private readonly IUnityOfWork _unity;
		private readonly IBus _bus;

		public DataController(IUnityOfWork unity, IBus bus)
		{
			WebSignalR.Common.Extension.Ensure.Argument.NotNull(unity, "unity");
			_unity = unity;
		}

		// GET api/<controller>
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		public HttpResponseMessage GetRoomVotes(int roomId)
		{
			var message = new WebSignalR.Common.Messages.ProvideCsvMessage() { RoomId = roomId, State = Common.Messages.CsvReadyState.Init };
			if (this.User != null)
				message.Issuer = User.Identity.Name;

			_bus.SendAsync(message);
			return Request.CreateResponse(System.Net.HttpStatusCode.OK, message);
		}

		// POST api/<controller>
		public void Post([FromBody]string value)
		{
		}

		// PUT api/<controller>/5
		public void Put(int id, [FromBody]string value)
		{
		}

		// DELETE api/<controller>/5
		public void Delete(int id)
		{
		}

		protected override void Dispose(bool disposing)
		{
			_unity.Dispose();
			base.Dispose(disposing);
		}
	}
}