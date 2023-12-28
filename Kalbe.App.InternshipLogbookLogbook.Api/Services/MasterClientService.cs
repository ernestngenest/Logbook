using Kalbe.App.InternshipLogbookLogbook.Api.Models;
using Kalbe.App.InternshipLogbookLogbook.Api.Models.Commons;
using Kalbe.App.InternshipLogbookLogbook.Api.Utilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Services
{
    public interface IMasterClientService
    {
        Task<Mentor> GetMentorByUPN(string upn);
        Task<UserInternal> GetUserInternalByUPN(string upn);
        Task<bool> Approve(ApprovalTransactionData data);
        Task<bool> DeleteWFAsync(ApprovalTransactionData data, bool notInsertApprovalLog);
        Task<List<ApprovalTransactionDataModel>> GetApprovalMaster(string systemCode, string moduleCode);
        Task<bool> Submit(ApprovalTransactionData data);
        Task<bool> Reject(ApprovalLogModel approvalLogData);
        Task<IEnumerable<ApprovalTransactionDataModel>> GetCurrentWF(string nomorDokumen);
        Task<bool> SendEmail(Email data);
        Task<AllowanceResponse> GetAllowanceByEducation(string educationCode);

    }
    public class MasterClientService : IMasterClientService
    {
        private readonly AppSettingModel _settingModel;
        private readonly HttpClient _httpClient;

        public MasterClientService(HttpClient httpClient, IOptions<AppSettingModel> settings)
        {
            if (httpClient != null && settings != null)
            {
                _settingModel = settings.Value;
                httpClient.BaseAddress = new Uri(_settingModel.Master);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                _httpClient = httpClient;
            }
        }

        public async Task<Mentor> GetMentorByUPN(string upn)
        {
            try
            {
                //upn = WebUtility.UrlEncode(upn);
                var response = await _httpClient.GetAsync("UserExternal/GetMentorByUPN/" + upn);
                response.EnsureSuccessStatusCode();

                var JSON = (await response.Content.ReadAsStringAsync());
                Mentor result = JsonConvert.DeserializeObject<Mentor>(JSON);
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message.ToString());
            }
        }

        public async Task<UserInternal> GetUserInternalByUPN(string upn)
        {
            try
            {
                //upn = WebUtility.UrlEncode(upn);
                var response = await _httpClient.GetAsync("UserInternal/GetByUPN/" + upn);
                response.EnsureSuccessStatusCode();

                var JSON = (await response.Content.ReadAsStringAsync());
                UserInternal result = JsonConvert.DeserializeObject<UserInternal>(JSON);
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message.ToString());
            }
        }

        public async Task<bool> Approve(ApprovalTransactionData data)
        {
            try
            {
                var listJSON = JsonSerializer.Serialize(data, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
                var content = new StringContent(listJSON, Encoding.UTF8, _settingModel.MediaType);
                var response = await _httpClient.PutAsync("ApprovalDetails/Approve", content);
                response.EnsureSuccessStatusCode();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<bool> DeleteWFAsync(ApprovalTransactionData data, bool notInsertApprovalLog)
        {
            try
            {
                var listJSON = JsonSerializer.Serialize(data, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
                var content = new StringContent(listJSON, Encoding.UTF8, _settingModel.MediaType);
                var response = await _httpClient.PutAsync("ApprovalDetails/DeleteWF?notInsertApprovalLog=" + notInsertApprovalLog, content);
                response.EnsureSuccessStatusCode();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<ApprovalTransactionDataModel>> GetApprovalMaster(string systemCode, string moduleCode)
        {
            try
            {
                var response = await _httpClient.GetAsync("Approval/GetData/" + systemCode + "/" + moduleCode);
                response.EnsureSuccessStatusCode();

                var listJSON = (await response.Content.ReadAsStringAsync());
                ApprovalMasterDataList masterApproval = Newtonsoft.Json.JsonConvert.DeserializeObject<ApprovalMasterDataList>(listJSON);
                return masterApproval.Data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<IEnumerable<ApprovalTransactionDataModel>> GetCurrentWF(string nomorDokumen)
        {
            try
            {
                nomorDokumen = WebUtility.UrlEncode(nomorDokumen);
                var response = await _httpClient.GetAsync("ApprovalDetails/CurrentWF/Multiple/" + Constant.SystemCode + "/" + Constant.ModuleCode + "/" + nomorDokumen);
                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<ServiceResponse<IEnumerable<ApprovalTransactionDataModel>>>(responseStream, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                return result.Data;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message.ToString());
            }
        }

        public async Task<bool> Submit(ApprovalTransactionData data)
        {
            try
            {
                var listJSON = JsonSerializer.Serialize(data, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
                var content = new StringContent(listJSON, Encoding.UTF8, _settingModel.MediaType);
                var response = await _httpClient.PostAsync("ApprovalDetails/Submit", content);
                response.EnsureSuccessStatusCode();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception("Workflow is currently running", ex);
            }
        }
        public async Task<bool> Reject(ApprovalLogModel approvalLogData)
        {
            try
            {
                var listJSON = JsonSerializer.Serialize(approvalLogData, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
                var content = new StringContent(listJSON, Encoding.UTF8, _settingModel.MediaType);
                var response = await _httpClient.PutAsync("ApprovalDetails/Reject", content);
                response.EnsureSuccessStatusCode();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<bool> SendEmail(Email data)
        {
            try
            {
                var listJSON = JsonSerializer.Serialize(data, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
                var content = new StringContent(listJSON, Encoding.UTF8, _settingModel.MediaType);
                var response = await _httpClient.PostAsync("Email/SendEmail", content);
                response.EnsureSuccessStatusCode();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception("Email is not sent", ex);
            }
        }

        public async Task<AllowanceResponse> GetAllowanceByEducation(string educationCode)
        {
            try
            {
                var response = await _httpClient.GetAsync("Education/GetByEducation/" + educationCode);
                response.EnsureSuccessStatusCode();

                var listJSON = (await response.Content.ReadAsStringAsync());
                AllowanceResponse allowanceResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<AllowanceResponse>(listJSON);
                return allowanceResponse;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
