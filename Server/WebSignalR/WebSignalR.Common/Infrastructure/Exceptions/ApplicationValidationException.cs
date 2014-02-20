using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSignalR.Common.Infrastructure
{
	/// <summary>
	/// The custom exception for validation errors
	/// </summary>
	public class ApplicationValidationException
		: Exception
	{
		#region Properties

		IEnumerable<string> _validationErrors;
		/// <summary>
		/// Get or set the validation errors messages
		/// </summary>
		public IEnumerable<string> ValidationErrors
		{
			get
			{
				return _validationErrors;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Create new instance of Application validation errors exception
		/// </summary>
		/// <param name="validationErrors">The collection of validation errors</param>
		public ApplicationValidationException(IEnumerable<string> validationErrors)
			: base("Invalid type, expected is RegisterTypesMapConfigurationElement")
		{
			_validationErrors = validationErrors;
		}

		#endregion
	}
}