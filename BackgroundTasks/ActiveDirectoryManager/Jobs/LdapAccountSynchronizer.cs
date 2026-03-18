using ActiveDirectoryManager.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
//using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.CustomerIdentity.Domain.DomainServices;
using Tranglo1.CustomerIdentity.Domain.Entities.ActiveDirectory;
using Tranglo1.CustomerIdentity.Infrastructure.Services;

namespace ActiveDirectoryManager.Jobs
{
	class LdapAccountSynchronizer
	{
		public LdapAccountSynchronizer(LdapAccountManager ldapAccountManager, 
			IUnitOfWork unitOfWork, LdapConfigurations ldapConfigurations, 
			ILogger<LdapAccountSynchronizer> logger)
		{
			LdapAccountManager = ldapAccountManager ?? throw new ArgumentNullException(nameof(ldapAccountManager));
			UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			LdapConfigurations = ldapConfigurations ?? throw new ArgumentNullException(nameof(ldapConfigurations));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public LdapAccountManager LdapAccountManager { get; }
		public IUnitOfWork UnitOfWork { get; }
		public LdapConfigurations LdapConfigurations { get; }
		public ILogger<LdapAccountSynchronizer> Logger { get; }


		private LdapAccount[] QueryAccountsFromLdapServer()
		{
			try
			{
				string searchFilter = $"(&(objectCategory=person)(objectClass=user))";

				//load the self signed cert from LDAP server
				var TRGRAD01OFPRD = System.Security.Cryptography.X509Certificates.X509Certificate2.CreateFromCertFile("ldap.cer");
				var X5092 = new System.Security.Cryptography.X509Certificates.X509Certificate2(TRGRAD01OFPRD);

				var ldapOptions = new LdapConnectionOptions();
				ldapOptions.UseSsl().ConfigureRemoteCertificateValidationCallback((sender, cert, chain, sslPolicyErrors) =>
				{
					chain.ChainPolicy.ExtraStore.Clear();
					chain.ChainPolicy.ExtraStore.Add(X5092);
					var valid = chain.Build(cert as System.Security.Cryptography.X509Certificates.X509Certificate2);
					
					return true;
				});

				using (var connection = new Novell.Directory.Ldap.LdapConnection(ldapOptions))
				{
					var _Configuration = this.LdapConfigurations;

					connection.Connect(LdapConfigurations.LdapServer, Novell.Directory.Ldap.LdapConnection.DefaultSslPort);
					connection.Bind(LdapConfigurations.LdapUser, _Configuration.LdapPassword);

					//make "tranglo.net" become "DC=tranglo,DC=net"
					//var dc = _Configuration.LdapServer.Split(".", StringSplitOptions.RemoveEmptyEntries);
					//var _searchBase = string.Join(",", dc.Select(s => "DC=" + s).ToArray());

					var lsc = connection.Search(_Configuration.LdapQuery,
						Novell.Directory.Ldap.LdapConnection.ScopeSub, searchFilter, null, false,
						new LdapSearchConstraints() { ReferralFollowing = true });

					List<LdapAccount> _Accounts = new List<LdapAccount>();

					while (lsc.HasMore())
					{
						LdapEntry entry = null;

						try
						{
							entry = null; //reset this variable before trying to fetch from LDAP connection.
							entry = lsc.Next();

							if (string.IsNullOrEmpty(entry.Dn))
							{
								continue;
							}

							LdapAccount ldapAccount = new LdapAccount();

							try
							{
								ldapAccount.SamAccountName = entry.GetAttribute("sAMAccountName").StringValue;
							}
							catch (KeyNotFoundException)
							{
								Logger.LogWarning($"DN [{entry.Dn}] : sAMAccountName not found.");
								continue;
							}

							try
							{
								ldapAccount.Name = entry.GetAttribute("name").StringValue;
							}
							catch (KeyNotFoundException)
							{
								Logger.LogWarning($"DN [{entry.Dn}] : name not found.");
								continue;
							}

							try
							{
								ldapAccount.EmailAddress = entry.GetAttribute("mail").StringValue;
							}
							catch (KeyNotFoundException)
							{

							}

							try
							{
								//https://docs.microsoft.com/en-us/troubleshoot/windows-server/identity/useraccountcontrol-manipulate-account-properties
								//const int SCRIPT = 1;
								const int ACCOUNTDISABLE = 2;
								//const int HOMEDIR_REQUIRED = 8;
								//const int LOCKOUT = 16;
								//const int PASSWD_NOTREQD = 32;
								//const int PASSWD_CANT_CHANGE = 64;
								//const int ENCRYPTED_TEXT_PWD_ALLOWED = 128;
								//const int TEMP_DUPLICATE_ACCOUNT = 256;
								//const int NORMAL_ACCOUNT = 512;
								//const int INTERDOMAIN_TRUST_ACCOUNT = 2048;
								//const int DONT_EXPIRE_PASSWORD = 65536;


								var userAccountControl = entry.GetAttribute("userAccountControl").StringValue;

								var accountFlag = int.Parse(userAccountControl);

								if ((accountFlag & ACCOUNTDISABLE) == ACCOUNTDISABLE)
								{
									ldapAccount.IsEnabled = false;
								}
								else
								{
									ldapAccount.IsEnabled = true;
								}

								_Accounts.Add(ldapAccount);

							}
							catch (Exception)
							{
							}
						}
						catch (Novell.Directory.Ldap.LdapException ldapError)
						{
							Logger.LogError(ldapError.ToString());
							continue;
						}

					}

					return _Accounts.OrderBy(a => a.SamAccountName).ToArray();
				}

			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error when loading Ldap accounts from server.");
				return new LdapAccount[0];
			}
		}

		public async Task ExecuteAsync()
		{
			Logger.LogInformation($"Query user accounts from LDAP server {this.LdapConfigurations.LdapServer}");
			var _LdapAccounts = QueryAccountsFromLdapServer();

			if (_LdapAccounts.Any() == false)
			{
				Logger.LogInformation("There is no accounts loaded from LDAP.");
				return;
			}
			else
			{
				Logger.LogInformation($"There are {_LdapAccounts.Count()} accounts loaded from LDAP.");

				if (Logger.IsEnabled(LogLevel.Debug))
				{
					StringBuilder buffer = new StringBuilder();
					buffer.AppendLine();
					buffer.AppendLine($"{"No",-5}{"SamAccountName",-30}{"Name",-30}{"EmailAddress",-40}{"Status"}");

					int i = 1;
					foreach (var item in _LdapAccounts)
					{
						buffer.AppendLine($"{i + 1,-5}{item.SamAccountName,-30}{item.Name,-30}{item.EmailAddress,-40}{item.IsEnabled}");
						i++;
					}

					Logger.LogDebug(buffer.ToString());
				}
			}

			try
			{
				var _MergeResults = await LdapAccountManager.MergeLdapAccountsAsync(_LdapAccounts);

				int newCount = _MergeResults.NewAccounts.Count;
				int updateCount = _MergeResults.UpdatedAccounts.Count;
				int deleteCount = _MergeResults.DeletedAccounts.Count;

				foreach (var added in _MergeResults.NewAccounts)
				{
					await LdapAccountManager.AccountRepository.SaveAsync(added);
				}

				foreach (var updated in _MergeResults.UpdatedAccounts)
				{
					await LdapAccountManager.AccountRepository.SaveAsync(updated);
				}

				foreach (var deleted in _MergeResults.DeletedAccounts)
				{
					deleted.IsEnabled = false;
					await LdapAccountManager.AccountRepository.SaveAsync(deleted);
				}

				await this.UnitOfWork.CommitAsync();
				Logger.LogInformation($"Summaries -> Added: {newCount}, Updated: {updateCount}, Deleted: {deleteCount}");

				if (Logger.IsEnabled(LogLevel.Debug))
				{
					StringBuilder builder = new StringBuilder();
					foreach (var item in _MergeResults.NewAccounts)
					{
						builder.AppendLine($"(Added) {item.SamAccountName} ({item.Name}, {item.EmailAddress})");
					}

					foreach (var item in _MergeResults.UpdatedAccounts)
					{
						builder.AppendLine($"(Updated) {item.SamAccountName} ({item.Name}, {item.EmailAddress})");
					}

					foreach (var item in _MergeResults.DeletedAccounts)
					{
						builder.AppendLine($"(Deleted) {item.SamAccountName} ({item.Name}, {item.EmailAddress})");
					}

					Logger.LogDebug(builder.ToString());
				}

			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error when saving merge results.");
			}
		}
	}
}
