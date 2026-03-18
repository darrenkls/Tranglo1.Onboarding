
using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveDirectoryManager.Configurations
{
	class LdapConfigurations
	{
		public string LdapServer { get; set; }
		public string LdapUser { get; set; }
		public string LdapPassword { get; set; }
		public string LdapQuery { get; set; }
		public int IntervalInMinutes { get; set; }
	}
}
