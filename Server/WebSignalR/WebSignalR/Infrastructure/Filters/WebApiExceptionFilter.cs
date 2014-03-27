using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace WebSignalR.Infrastructure.Filters
{
	public enum ServiceErrorCodes : int
	{
		DbError = 0,
		ArgumentError = 1,
		Unexpected = 500
	}

	public sealed class WebApiExceptionFilter : ExceptionFilterAttribute
	{
		private static Infrastructure.DynamicTracer tracer;

		static WebApiExceptionFilter()
		{
			tracer = new DynamicTracer();
		}

		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			tracer.LogToHub(actionExecutedContext.Exception);

			if (actionExecutedContext.Exception is System.Data.DBConcurrencyException)
			{
				var businessException = actionExecutedContext.Exception as System.Data.DBConcurrencyException;
				var errorMessagError = new System.Web.Http.HttpError("Database exception has been occurred.") { { "ErrorCode", (int)ServiceErrorCodes.DbError } };
				actionExecutedContext.Response =
					actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, errorMessagError);
			}
			else if (actionExecutedContext.Exception is ArgumentNullException)
			{
				var dataException = actionExecutedContext.Exception as ArgumentNullException;
				var errorMessagError = new System.Web.Http.HttpError("Argument null exception has occurred.") { { "ErrorCode", (int)ServiceErrorCodes.ArgumentError } };
				actionExecutedContext.Response =
					actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, errorMessagError);
			}
			else
			{
				var errorMessagError = new System.Web.Http.HttpError("Some internal Exception. Please contact your administrator") { { "ErrorCode", (int)ServiceErrorCodes.Unexpected } };
				actionExecutedContext.Response =
					actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessagError);
			}
		}
	}
}