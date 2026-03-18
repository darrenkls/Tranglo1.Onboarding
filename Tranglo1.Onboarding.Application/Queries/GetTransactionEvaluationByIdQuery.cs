using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.Onboarding.Domain.Entities;
using AutoMapper.QueryableExtensions;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Application.DTO.AMLCFTQuestionnaire;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.DTO.TransactionEvaluation;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Application.Queries
{
    [Permission(Permission.KYCManagementTransactionEvaluation.Action_View_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Business },
        new string[] {  })]
    internal class GetTransactionEvaluationByIdQuery : BaseQuery<GetTransactionEvaluationOutputDTO>
    {
        public int BusinessProfileCode { get; set; }

        public override Task<string> GetAuditLogAsync(GetTransactionEvaluationOutputDTO result)
        {
            string _description = $"Get Transaction Evaluation for Business Profile Code: [{this.BusinessProfileCode}]";
            return Task.FromResult(_description);
        }

        public class GetTransactionEvaluationByIdQueryHandler : IRequestHandler<GetTransactionEvaluationByIdQuery, GetTransactionEvaluationOutputDTO>
        {
            private readonly IConfiguration _config;
            private readonly IBusinessProfileRepository _repository;
            public GetTransactionEvaluationByIdQueryHandler(IConfiguration config, IBusinessProfileRepository repository)
            {
                _config = config;
                _repository = repository;
            }


            async Task<GetTransactionEvaluationOutputDTO> IRequestHandler<GetTransactionEvaluationByIdQuery, GetTransactionEvaluationOutputDTO>.Handle(GetTransactionEvaluationByIdQuery request, CancellationToken cancellationToken)
            {
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                
                GetTransactionEvaluationOutputDTO transactionEvaluationDTO = new GetTransactionEvaluationOutputDTO();
                IEnumerable<TransactionEvaluationInfoDTO> transactionEvaluationInfoDTOs;
                IEnumerable<QuestionDTO> questionDTOs;
                IEnumerable<AnswerDTO> answerDTOs;
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var reader = await connection.QueryMultipleAsync(
                        "GetTransactionEvaluationByBusinessProfile",
                        new { businessProfileCode = request.BusinessProfileCode},
                        null, null, CommandType.StoredProcedure);

                    // read as IEnumerable<dynamic>
                    transactionEvaluationDTO = await reader.ReadFirstOrDefaultAsync<GetTransactionEvaluationOutputDTO>();
                    transactionEvaluationInfoDTOs = await reader.ReadAsync<TransactionEvaluationInfoDTO>();
                    questionDTOs = await reader.ReadAsync<QuestionDTO>();
                    answerDTOs = await reader.ReadAsync<AnswerDTO>();
                }

                if(transactionEvaluationDTO!=null)
                {
                    transactionEvaluationDTO.TransactionEvaluationInfos = transactionEvaluationInfoDTOs.OrderBy(x => x.TransactionEvaluationInfoCode).ToList();
                    transactionEvaluationDTO.Questions = questionDTOs.OrderBy(x => x.SequenceNo).ToList();

                    foreach (QuestionDTO questionDTO in transactionEvaluationDTO.Questions)
                    {
                        questionDTO.Answers = answerDTOs.Where(x => x.QuestionCode == questionDTO.QuestionCode).ToList();
                    }
                }

                var businessProfile = await _repository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);
                transactionEvaluationDTO.TransactionEvalConcurrencyToken = businessProfile.TransactionEvalConcurrencyToken;
                return transactionEvaluationDTO;
            }
        }

    }
}
