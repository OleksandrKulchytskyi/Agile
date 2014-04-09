using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using WebSignalR.Common.Interfaces;
using WebSignalR.Common.Interfaces.Bus;
using WebSignalR.Common.Messages;

namespace WebSignalR.ExtensionModule.Controllers.Api
{
	public class DataController : ApiController
	{
		// Content type for our body
		private static readonly MediaTypeHeaderValue _mediaType = MediaTypeHeaderValue.Parse("text/csv");

		private readonly IBus _bus;
		private readonly ICsvStatePusher _pusher;
		private readonly ICsvProvider _csvProvider;

		public DataController(IBus bus, ICsvStatePusher push, ICsvProvider provider)
		{
			WebSignalR.Common.Extension.Ensure.Argument.NotNull(bus, "bus");
			WebSignalR.Common.Extension.Ensure.Argument.NotNull(push, "push");
			WebSignalR.Common.Extension.Ensure.Argument.NotNull(provider, "provider");

			_bus = bus;
			_pusher = push;
			_csvProvider = provider;
		}

		[HttpGet]
		public HttpResponseMessage GetRoomVotes(int roomId)
		{
			var message = new ProvideCsvMessage() { RoomId = roomId, State = Common.Messages.CsvReadyState.Init, OutputId = Guid.NewGuid() };
			if (this.User != null)
				message.Issuer = User.Identity.Name;

			_bus.SendAsync<ProvideCsvMessage>(message).
				ContinueWith(t =>
				{
					if (t.IsCompleted)
					{
						System.Diagnostics.Debug.WriteLine("Message has been published.");
					}
					else if (t.IsFaulted)
					{
						System.Diagnostics.Debug.WriteLine("Exception has been occurred. {0}{1}", Environment.NewLine, t.Exception.ToString());
					}
				});
			HttpResponseMessage response = Request.CreateResponse();
			response.Content = new PushStreamContent(Cast, "text/event-stream");
			return response;
		}

		private void Cast(Stream stream, HttpContent httpContent, TransportContext tctx)
		{
			_pusher.OnStreamAvailable(stream);
		}

		[HttpGet]
		public HttpResponseMessage Download([FromUri]bool lengthOnly, [FromUri]string fileId)
		{
			string fname = fileId + ".csv";
			string path = HttpContext.Current.Server.MapPath("~/App_Data/" + fname);
			if (!File.Exists(path))
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotFound, new HttpError("The file does not exist."));
			}

			if (lengthOnly)
			{
				using (FileStream fs = File.Open(path, FileMode.Open))
				{
					return Request.CreateResponse(HttpStatusCode.OK, fs.Length);
				}
			}

			try
			{
				MemoryStream responseStream = new MemoryStream();
				Stream fileStream = File.Open(path, FileMode.Open);
				bool fullContent = true;
				if (this.Request.Headers.Range != null)
				{
					fullContent = false;
					// Currently we only support a single range.
					RangeItemHeaderValue range = this.Request.Headers.Range.Ranges.FirstOrDefault();
					// From specified, so seek to the requested position.
					if (range != null)
					{
						if (range.From != null)
						{
							fileStream.Seek(range.From.Value, SeekOrigin.Begin);
							// In this case, actually the complete file will be returned.
							if (range.From == 0 && (range.To == null || range.To >= fileStream.Length))
							{
								fileStream.CopyTo(responseStream);
								fullContent = true;
							}
						}
						if (range.To != null)
						{
							// 10-20, return the range.
							if (range.From != null)
							{
								long? rangeLength = range.To - range.From;
								int length = (int)Math.Min(rangeLength.Value, fileStream.Length - range.From.Value);
								byte[] buffer = new byte[length];
								fileStream.Read(buffer, 0, length);
								responseStream.Write(buffer, 0, length);
							}
							// -20, return the bytes from beginning to the specified value.
							else
							{
								int length = (int)Math.Min(range.To.Value, fileStream.Length);
								byte[] buffer = new byte[length];
								fileStream.Read(buffer, 0, length);
								responseStream.Write(buffer, 0, length);
							}
						}
					}
					// No Range.To
					else
					{	// 10-, return from the specified value to the end of file.
						if (range.From != null)
						{
							if (range.From < fileStream.Length)
							{
								int length = (int)(fileStream.Length - range.From.Value);
								byte[] buffer = new byte[length];
								fileStream.Read(buffer, 0, length);
								responseStream.Write(buffer, 0, length);
							}
						}
					}
				}
				// No Range header. Return the complete file.
				else
					fileStream.CopyTo(responseStream);

				fileStream.Close();
				if (responseStream.CanSeek)
					responseStream.Position = 0;

				HttpResponseMessage response = Request.CreateResponse(fullContent ? HttpStatusCode.OK : HttpStatusCode.PartialContent);
				response.Content = new StreamContent(responseStream);
				response.Content.Headers.ContentType = _mediaType;
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
				response.Content.Headers.ContentDisposition.FileName = fname;

				return response;
			}
			catch (IOException ex)
			{
				return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
			}
		}

		[HttpDelete]
		public HttpResponseMessage Delete([FromUri]Guid fileId)
		{
			_csvProvider.Purge(fileId);
			return Request.CreateResponse(HttpStatusCode.OK, "File has been deleted.");
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}