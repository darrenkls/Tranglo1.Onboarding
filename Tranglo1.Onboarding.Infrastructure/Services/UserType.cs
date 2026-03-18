using System;
using System.Collections.Generic;
using System.Text;

namespace Tranglo1.Onboarding.Infrastructure.Services
{
	public enum UserType
	{
		/// <summary>
		/// Represent the current execution context is not under any human user. This could 
		/// be the scenario when a backend task, scheduler is running some processes.
		/// </summary>
		System = 0,

		/// <summary>
		/// Represent the current user is Tranglo staff
		/// </summary>
		Internal = 1,

		/// <summary>
		/// Represent the current user is external customer
		/// </summary>
		External = 2
	}
}
