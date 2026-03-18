using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Tranglo1.CustomerIdentity.Domain.Repositories;
using Tranglo1.CustomerIdentity.Domain.Entities.ActiveDirectory;
using Tranglo1.CustomerIdentity.Domain.DomainServices;

namespace ActiveDirectoryManager.Test
{
	[TestClass]
	public class LdapAccountManagerTest
	{
		[TestMethod]
		public async Task New_Accounts_From_Ldap_Is_Added_When_Existing_System_Users_Is_Empty()
		{
			//ARRANGE
			ServiceCollection services = new ServiceCollection();
			services.AddLdapServices();

			Mock<ILdapAccountRepository> _EmptyRepository = new Mock<ILdapAccountRepository>();
			
			_EmptyRepository.Setup(r => r.GetLdapAccountsAsync()).ReturnsAsync(new List<LdapAccount>());
			_EmptyRepository.Setup(r => r.SaveAsync(It.IsAny<LdapAccount>())).Throws(new System.Exception());

			services.AddSingleton<ILdapAccountRepository>(_EmptyRepository.Object);

			services.AddLdapRepository(options =>
			{
				options.UseInMemoryDatabase(nameof(LdapAccountManagerTest));
			});

			var provider = services.BuildServiceProvider();
			var target = provider.GetService<LdapAccountManager>();


			//ACT

			LdapAccount[] _fromLdap = new LdapAccount[]
			{
				new LdapAccount(){ EmailAddress = "bob@email.com", IsEnabled = true, Name = "Bob", SamAccountName = "Bob"},
				new LdapAccount(){ EmailAddress = "alice@email.com", IsEnabled = true, Name = "Alice", SamAccountName = "Alice"}
			};

			var mergeResult = await target.MergeLdapAccountsAsync(_fromLdap);

			//ASSERT
			Assert.IsNotNull(mergeResult);
			Assert.AreEqual(2, mergeResult.NewAccounts.Count);
			Assert.IsFalse(mergeResult.DeletedAccounts.Any());
			Assert.IsFalse(mergeResult.UpdatedAccounts.Any());
		}

		[TestMethod]
		public async Task Disabled_Account_In_Ldap_Will_Be_Ignored_When_System_Users_Is_Empty()
		{
			//ARRANGE
			ServiceCollection services = new ServiceCollection();
			services.AddLdapServices();

			Mock<ILdapAccountRepository> _EmptyRepository = new Mock<ILdapAccountRepository>();

			_EmptyRepository.Setup(r => r.GetLdapAccountsAsync()).ReturnsAsync(new List<LdapAccount>());
			_EmptyRepository.Setup(r => r.SaveAsync(It.IsAny<LdapAccount>())).Throws(new System.Exception());

			services.AddSingleton<ILdapAccountRepository>(_EmptyRepository.Object);

			services.AddLdapRepository(options =>
			{
				options.UseInMemoryDatabase(nameof(LdapAccountManagerTest));
			});

			var provider = services.BuildServiceProvider();
			var target = provider.GetService<LdapAccountManager>();


			//ACT

			LdapAccount[] _fromLdap = new LdapAccount[]
			{
				new LdapAccount(){ EmailAddress = "bob@email.com", IsEnabled = true, Name = "Bob", SamAccountName = "Bob"},
				new LdapAccount(){ EmailAddress = "alice@email.com", IsEnabled = false, Name = "Alice", SamAccountName = "Alice"}
			};

			var mergeResult = await target.MergeLdapAccountsAsync(_fromLdap);

			//ASSERT
			Assert.IsNotNull(mergeResult);
			Assert.AreEqual(1, mergeResult.NewAccounts.Count);
			Assert.AreEqual(_fromLdap[0], mergeResult.NewAccounts.First());

			Assert.IsFalse(mergeResult.DeletedAccounts.Any());
			Assert.IsFalse(mergeResult.UpdatedAccounts.Any());
		}

		[TestMethod]
		public async Task Disabled_Account_In_Ldap_Will_Caused_Existing_Record_In_System_Deleted()
		{
			//ARRANGE

			var Bob = new LdapAccount() { EmailAddress = "bob@email.com", IsEnabled = true, Name = "Bob", SamAccountName = "Bob" };
			var Alice = new LdapAccount() { EmailAddress = "alice@email.com", IsEnabled = false, Name = "Alice", SamAccountName = "Alice" };

			ServiceCollection services = new ServiceCollection();
			services.AddLdapServices();

			Mock<ILdapAccountRepository> _EmptyRepository = new Mock<ILdapAccountRepository>();

			List<LdapAccount> existings = new List<LdapAccount>();
			existings.Add(Bob);
			existings.Add(Alice);

			_EmptyRepository.Setup(r => r.GetLdapAccountsAsync()).ReturnsAsync(existings);
			_EmptyRepository.Setup(r => r.SaveAsync(It.IsAny<LdapAccount>())).Throws(new System.Exception());

			services.AddSingleton<ILdapAccountRepository>(_EmptyRepository.Object);

			//services.AddLdapRepository(options =>
			//{
			//	options.UseInMemoryDatabase(nameof(LdapAccountManagerTest));
			//});

			var provider = services.BuildServiceProvider();
			var target = provider.GetService<LdapAccountManager>();

			//ACT

			var DisabledBobFromLdap  =
				new LdapAccount() { EmailAddress = "bob@email.com", IsEnabled = false, Name = "Bob", SamAccountName = "Bob" };

			LdapAccount[] _fromLdap = new LdapAccount[]
			{
				DisabledBobFromLdap, Alice
			};

			var mergeResult = await target.MergeLdapAccountsAsync(_fromLdap);

			//ASSERT
			Assert.IsNotNull(mergeResult);
			Assert.IsFalse(mergeResult.NewAccounts.Any());

			Assert.AreEqual(1, mergeResult.DeletedAccounts.Count);
			Assert.AreEqual("bob@email.com", mergeResult.DeletedAccounts.First().EmailAddress);

			Assert.IsFalse(mergeResult.UpdatedAccounts.Any());
		}

		[TestMethod]
		public async Task UserName_From_Ldap_Will_Be_Updated_To_Existing_Record_In_System()
		{
			//ARRANGE

			var Bob = new LdapAccount() { EmailAddress = "bob@email.com", IsEnabled = true, Name = "Bob", SamAccountName = "Bob" };
			var Alice = new LdapAccount() { EmailAddress = "alice@email.com", IsEnabled = false, Name = "Alice", SamAccountName = "Alice" };

			ServiceCollection services = new ServiceCollection();
			services.AddLdapServices();

			Mock<ILdapAccountRepository> _EmptyRepository = new Mock<ILdapAccountRepository>();

			List<LdapAccount> existings = new List<LdapAccount>();
			existings.Add(Bob);
			existings.Add(Alice);

			_EmptyRepository.Setup(r => r.GetLdapAccountsAsync()).ReturnsAsync(existings);
			_EmptyRepository.Setup(r => r.SaveAsync(It.IsAny<LdapAccount>())).Throws(new System.Exception());

			services.AddSingleton<ILdapAccountRepository>(_EmptyRepository.Object);

			//services.AddLdapRepository(options =>
			//{
			//	options.UseInMemoryDatabase(nameof(LdapAccountManagerTest));
			//});

			var provider = services.BuildServiceProvider();
			var target = provider.GetService<LdapAccountManager>();

			//ACT

			var UpdatedBobFromLdap =
				new LdapAccount() { EmailAddress = "new_bob@email.com", IsEnabled = true, Name = "Bob Smith", SamAccountName = "Bob" };

			LdapAccount[] _fromLdap = new LdapAccount[]
			{
				UpdatedBobFromLdap, Alice
			};

			var mergeResult = await target.MergeLdapAccountsAsync(_fromLdap);

			//ASSERT
			Assert.IsNotNull(mergeResult);
			Assert.IsFalse(mergeResult.NewAccounts.Any());
			Assert.IsFalse(mergeResult.DeletedAccounts.Any());

			Assert.AreEqual(1, mergeResult.UpdatedAccounts.Count);
			Assert.AreEqual("new_bob@email.com", mergeResult.UpdatedAccounts.First().EmailAddress);
			Assert.AreEqual("Bob Smith", mergeResult.UpdatedAccounts.First().Name);
		}
	}
}
