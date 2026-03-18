using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Dapper;
using System.Data;
using Tranglo1.Onboarding.Application.DTO;
using Microsoft.Extensions.Logging;

namespace Tranglo1.Onboarding.Application.Managers
{
    public class IntegrationManager
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<IntegrationManager> _logger;
        public IntegrationManager(IConfiguration config,
            ILogger<IntegrationManager> logger)
        {
            this.configuration = config;
            _logger = logger;
        }
        public async Task<bool> AddApiSettingsAsync(long? rspId, string username, string password,
            string hashkey, Email comapnyEmail, string callbackUrl)
        {
            bool isSuccess = false;
            if (await AddSystemUsersAsync(rspId, comapnyEmail, username, password))
            {
                if(await AddSecurityKeyAsync(rspId, hashkey))
                    isSuccess = await UpdateRSPAsync(rspId, callbackUrl);
            }
            return isSuccess;
        }
        public async Task<long?> AddSupplierPartnerAsync(string companyName, string currencyCode)
        {
            long? partnerId = null;
            try
            {
                var _connectionString = configuration.GetConnectionString("GloremitConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.ExecuteReaderAsync(
                            "Partner_Add",
                            new
                            {
                                PartnerName = companyName,
                                Remark = companyName,
                                CurrencyCode = currencyCode,
                                EWalletLimit = 0.00
                            },
                            null, null, CommandType.StoredProcedure);
                    var readerPartner = await connection.QueryMultipleAsync(
                            "Partner_Search",
                            new
                            {
                                PartnerName = companyName,
                            },
                            null, null, CommandType.StoredProcedure);
                    var supplierPartnerList = await readerPartner.ReadAsync<SupplierPartnerDTO>();
                    partnerId = supplierPartnerList.ToList().FirstOrDefault().ID;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AddSupplierPartnerAsync", ex.Message);
            }
            
            return partnerId;
        }
        public async Task<bool> UpdateSupplierPartnerDetails(long? supplierPartnerId, string companyName, string currencyCode)
        {
            bool isSuccess = false;
            try
            {
                var _connectionString = configuration.GetConnectionString("GloremitConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string sqlQuery = "UPDATE Partner set Partner_Name=@PartnerName, " +
                        "Remark=@Remark," +
                        "CurrencyCode=@CurrencyCode where Id=@Id";
                    var resultList = await connection.ExecuteReaderAsync(
                           command: new CommandDefinition(sqlQuery, parameters: new
                           {
                               PartnerName = companyName,
                               Remark = companyName,
                               CurrencyCode = currencyCode,
                               Id = supplierPartnerId
                           }));
                }
                isSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateSupplierPartnerDetails", ex.Message);
            } 
            return isSuccess;
        }
        public async Task<long?> AddRSPAsync(string currencyCode, string countryCode, string companyName)
        {
            long? rspId = null;
            long ParentId = 0;
            long? CompanyId = null;
            //string currencyCode
            //string countryCode
            string Code = companyName;
            string Description = companyName;
            int EWallet = 1000000;
            int Status = 1;
            long? ModifiedBy = null;
            DateTime? ModifiedStamp = null;
            int CreatedBy = 1;
            DateTime CreatedStamp = DateTime.Now;
            string Shortname = companyName;
            string Logo_Path = null;
            int trxSharing_FeeType = 0;
            int rsp_Type = 1;
            long? ReceiptForex = null;
            int? Alert_Threshold = null;
            long AR_Value = 262143;
            long? SecondaryAccount = null;
            string companyCode = "CC0001";

            try
            {
                var _connectionString = configuration.GetConnectionString("GloremitConnection");
                _logger.LogInformation($"[AddRSPAsync]_connectionString: {_connectionString}");
                using (var connection = new SqlConnection(_connectionString))
                {
                    var p = new DynamicParameters();
                    p.Add("@result", dbType: DbType.Int64, direction: ParameterDirection.Output);
                    p.Add("@ParentId", ParentId);
                    _logger.LogInformation($"[AddRSPAsync]ParentId: {ParentId}");
                    p.Add("@CompanyId", CompanyId);
                    _logger.LogInformation($"[AddRSPAsync]CompanyId: {CompanyId}");
                    p.Add("@CurrencyCode", currencyCode);
                    _logger.LogInformation($"[AddRSPAsync]currencyCode: {currencyCode}");
                    p.Add("@CountryCode", countryCode);
                    _logger.LogInformation($"[AddRSPAsync]countryCode: {countryCode}");
                    p.Add("@Code", Code);
                    _logger.LogInformation($"[AddRSPAsync]Code: {Code}");
                    p.Add("@Description", Description);
                    _logger.LogInformation($"[AddRSPAsync]Description: {Description}");
                    p.Add("@EWallet", EWallet);
                    _logger.LogInformation($"[AddRSPAsync]EWallet: {EWallet}");
                    p.Add("@Status", Status);
                    _logger.LogInformation($"[AddRSPAsync]Status: {Status}");
                    p.Add("@ModifiedBy", ModifiedBy);
                    _logger.LogInformation($"[AddRSPAsync]ModifiedBy: {ModifiedBy}");
                    p.Add("@ModifiedStamp", ModifiedStamp);
                    _logger.LogInformation($"[AddRSPAsync]ModifiedStamp: {ModifiedStamp}");
                    p.Add("@CreatedBy", CreatedBy);
                    _logger.LogInformation($"[AddRSPAsync]CreatedBy: {CreatedBy}");
                    p.Add("@CreatedStamp", CreatedStamp);
                    _logger.LogInformation($"[AddRSPAsync]CreatedStamp: {CreatedStamp}");
                    p.Add("@Shortname", Shortname);
                    _logger.LogInformation($"[AddRSPAsync]Shortname: {Shortname}");
                    p.Add("@Logo_Path", Logo_Path);
                    _logger.LogInformation($"[AddRSPAsync]Logo_Path: {Logo_Path}");
                    p.Add("@trxSharing_FeeType", trxSharing_FeeType);
                    _logger.LogInformation($"[AddRSPAsync]trxSharing_FeeType: {trxSharing_FeeType}");
                    p.Add("@rsp_Type", rsp_Type);
                    _logger.LogInformation($"[AddRSPAsync]rsp_Type: {rsp_Type}");
                    p.Add("@ReceiptForex", ReceiptForex);
                    _logger.LogInformation($"[AddRSPAsync]ReceiptForex: {ReceiptForex}");
                    p.Add("@Alert_Threshold", Alert_Threshold);
                    _logger.LogInformation($"[AddRSPAsync]Alert_Threshold: {Alert_Threshold}");
                    p.Add("@AR_Value", AR_Value);
                    _logger.LogInformation($"[AddRSPAsync]AR_Value: {AR_Value}");
                    p.Add("@SecondaryAccount", SecondaryAccount);
                    _logger.LogInformation($"[AddRSPAsync]SecondaryAccount: {SecondaryAccount}");
                    p.Add("@companyCode", companyCode);
                    _logger.LogInformation($"[AddRSPAsync]companyCode: {companyCode}");

                    var reader = await connection.ExecuteReaderAsync(
                            "RSP_Add",
                            p,
                            null, null, CommandType.StoredProcedure);
                    rspId = p.Get<long>("@result");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[AddRSPAsync]currencyCode: {currencyCode}");
                _logger.LogError($"[AddRSPAsync]countryCode: {countryCode}");
                _logger.LogError($"[AddRSPAsync]companyName: {companyName}");
                _logger.LogError($"[AddRSPAsync]InnerException: {ex.InnerException}");
                _logger.LogError($"[AddRSPAsync]Source: {ex.Source}");
                _logger.LogError($"[AddRSPAsync]Message: {ex.Message}");
                //_logger.LogError("AddRSPAsync", ex.Message);
            }

            return rspId;
        }
        public async Task<bool> UpdateRSPDetails(long? rspId, string currencyCode, string countryCode,
            string companyName)
        {
            bool isSuccess = false;
            try
            {
                var _connectionString = configuration.GetConnectionString("GloremitConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sqlQuery = "UPDATE RSP set Code=@Code, " +
                        "Description=@Description," +
                        "Shortname=@Shortname," +
                        "CountryCode=@CountryCode," +
                        "CurrencyCode=@CurrencyCode where Id=@Id";
                    var resultList = await connection.ExecuteReaderAsync(
                           command: new CommandDefinition(sqlQuery, parameters: new
                           {
                               Code = companyName,
                               Description = companyName,
                               Shortname = companyName,
                               CountryCode = countryCode,
                               CurrencyCode = currencyCode,
                               Id = rspId
                           }));
                }
                isSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("AddRSPAsync", ex.Message);
            }
            
            return isSuccess;
        }
        public async Task<bool> UpdateCallbackAsync(long? rspId, string callbackUrl)
        {
            bool isSuccess = false;
            try
            {
                isSuccess = await UpdateRSPAsync(rspId, callbackUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateApiSettingsAsync", ex.Message);
            }
            
            return isSuccess;
        }

        public async Task<bool> UpdateAPISettingsAsync(long? rspId,
            Email comapnyEmail, string username, string password, string hashkey)
        {
            bool isSuccess = false;
            if (await UpdateSystemUsersAsync(rspId, comapnyEmail, username, password))
            {
                if (await UpdateSecurityKeyAsync(rspId, hashkey))
                    isSuccess = true;
            }
            return isSuccess;
        }

        private async Task<bool> UpdateSecurityKeyAsync(long? rspId, string hashkey)
        {
            bool isSuccess = false;
            try
            {
                var _connectionString = configuration.GetConnectionString("GloremitConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sqlQuery = "UPDATE SecurityKey set HashKey=@HashKey where RspId=@RspId";
                    var resultList = await connection.ExecuteReaderAsync(
                           command: new CommandDefinition(sqlQuery, parameters: new
                           {
                               HashKey = hashkey,
                               RspId = rspId
                           }));
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateApiSettingsAsync", ex.Message);
            }
            
            return isSuccess;
        }

        private async Task<bool> UpdateSystemUsersAsync(long? rspId, Email companyEmail, string username, string password)
        {
            bool isSuccess = false;
            try
            {
                var _connectionString = configuration.GetConnectionString("GloremitConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sqlQuery = "UPDATE SystemUsers set User_Email=@CompanyEmail," +
                        "Username=@Username," +
                        "Password=@Password where RspId=@RspId";
                    var resultList = await connection.ExecuteReaderAsync(
                           command: new CommandDefinition(sqlQuery, parameters: new
                           {
                               CompanyEmail = companyEmail.Value,
                               Username = username,
                               Password = password,
                               RspId = rspId
                           }));
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateApiSettingsAsync", ex.Message);
            }
            
            return isSuccess;
        }

        private async Task<bool> AddSystemUsersAsync(long? rspId, Email comapnyEmail,
            string username, string password)
        {
            bool isSuccess = false;
            //params: rspId,username,password,comapnyEmail
            try
            {
                var _connectionString = configuration.GetConnectionString("GloremitConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.ExecuteReaderAsync(
                            "SystemUsers_Add",
                            new
                            {
                                Username = username,
                                Password = password,
                                RspID = rspId,
                                Category=1,
                                FULLNAME = comapnyEmail.Value,
                                Status = 1,
                                CreatedBy =1
                            },
                            null, null, CommandType.StoredProcedure);
                }
                isSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("AddSystemUsersAsync", ex.Message);
            }
            

            return isSuccess;
        }
        private async Task<bool> AddSecurityKeyAsync(long? rspId, string hashkey)
        {
            bool isSuccess = false;
            try
            {
                var _connectionString = configuration.GetConnectionString("GloremitConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.ExecuteReaderAsync(
                            "SecurityKey_Add",
                            new
                            {
                                RspID = rspId,
                                HashKey = hashkey
                            },
                            null, null, CommandType.StoredProcedure);
                }
                isSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("AddSecurityKeyAsync", ex.Message);
            }
            
            return isSuccess;
        }
        private async Task<bool> UpdateRSPAsync(long? rspId, string callbackUrl)
        {
            bool isSuccess = false;
            try
            {
                var _connectionString = configuration.GetConnectionString("GloremitConnection");
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sqlQuery = "UPDATE RSP set CallBackURL=@CallbackURL where Id=@Id";
                    var resultList = await connection.ExecuteReaderAsync(
                           command: new CommandDefinition(sqlQuery, parameters: new
                           {
                               CallbackURL = callbackUrl,
                               Id = rspId
                           }));
                }
                isSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateRSPAsync", ex.Message);
            }
            
            return isSuccess;
        }
        
        private static string CreateRandomPassword(int length = 15)
        {
            // Create a string of characters, numbers, special characters that allowed in the password  
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }
    }
}
