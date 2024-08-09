using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Business.Infrastructure;
using Project.Business.Model;
using Project.Business.Service.Events;
using Project.Data.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Service
{
    public class CandidateServices : ICandidateService
    {
        private readonly IUnitOfWork _unitOfWork;


        private readonly IHttpContextAccessor _httpContextAccessor;


        private readonly ILogger<CandidateServices> _logger;

        private static readonly ConcurrentDictionary<string, List<Candidate>> _candidatePreviewStore = new ConcurrentDictionary<string, List<Candidate>>();


        public CandidateServices(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILogger<CandidateServices> logger)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }




        public async Task<IEnumerable<Candidate>> GetAllCandidates()
        {
            return await Task.Run(() => _unitOfWork.CandidateRepository.GetQuery().Where(c => (c.IsDeleted == false)).ToList());
        }

        public async Task<IEnumerable<Candidate>> GetCandidatesByEventId(Guid eventId)
        {
            return await Task.Run(() => _unitOfWork.CandidateRepository.GetQuery(c => c.EventId == eventId && (c.IsDeleted == false)).ToList());
        }

        public async Task<(string previewId, List<CandidateModelsReviewImport> previewResult)> VerifyAndPreviewImportAsync(IFormFile file, Guid eventId, string channel, string cvLink)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;
            var FileName = Path.GetFileNameWithoutExtension(file.FileName);
            bool isValidFile = VerifyFileFormat(stream);
            if (!isValidFile)
            {
                throw new Exception("Invalid file format.");
            }

            List<Candidate> candidates = ParseCandidatesFromFile(stream, eventId, channel, cvLink);
            List<CandidateModelsReviewImport> candidateModelsReview = new List<CandidateModelsReviewImport>();
            List<Candidate> mergedCandidates = await MergeCandidates(candidates, candidateModelsReview);

            // Generate a unique preview ID
            var previewId = Guid.NewGuid().ToString();
            _candidatePreviewStore[previewId] = mergedCandidates;

            _logger.LogInformation($"Preview ID {previewId} generated and stored with {mergedCandidates.Count} candidates.");

            return (previewId, candidateModelsReview);
        }

        //public async Task ConfirmImportAsync(string previewId)
        //{
        //    if (_candidatePreviewStore.TryGetValue(previewId, out var candidates))
        //    {
        //        //StringBuilder candidateDetails = new StringBuilder();

        //        foreach (var candidate in candidates)
        //        {

        //            //candidateDetails.AppendLine($"FullName: {candidate.FullName}");
        //            //candidateDetails.AppendLine($"DateOfBirth: {candidate.DateOfBirth}");
        //            //candidateDetails.AppendLine($"Gender: {candidate.Gender}");
        //            //candidateDetails.AppendLine($"PhoneNo: {candidate.PhoneNo}");
        //            //candidateDetails.AppendLine($"Email: {candidate.Email}");
        //            //candidateDetails.AppendLine($"Address: {candidate.Address}");
        //            //candidateDetails.AppendLine($"University: {candidate.University}");
        //            //candidateDetails.AppendLine($"Major: {candidate.Major}");
        //            //candidateDetails.AppendLine($"GraduationDate: {candidate.GraduationDate}");
        //            //candidateDetails.AppendLine($"GPA: {candidate.GPA}");
        //            //candidateDetails.AppendLine($"Skills: {candidate.Skills}");
        //            //candidateDetails.AppendLine($"Languages: {candidate.Languages}");
        //            //candidateDetails.AppendLine($"CVLink: {candidate.CVLink}");
        //            //candidateDetails.AppendLine($"ApplyJob: {candidate.ApplyJob}");
        //            //candidateDetails.AppendLine($"ApplicationDate: {candidate.ApplicationDate}");
        //            //candidateDetails.AppendLine($"WorkingTime: {candidate.WorkingTime}");
        //            //candidateDetails.AppendLine($"Talent: {candidate.Talent}");
        //            //candidateDetails.AppendLine($"Channel: {candidate.Channel}");
        //            //candidateDetails.AppendLine($"Recer: {candidate.Recer}");
        //            //candidateDetails.AppendLine($"Note: {candidate.Note}");
        //            //candidateDetails.AppendLine($"Status: {candidate.Status}");
        //            //candidateDetails.AppendLine($"IsDeleted: {candidate.IsDeleted}");
        //            //candidateDetails.AppendLine($"EventId: {candidate.EventId}");
        //            //candidateDetails.AppendLine(); // Add an extra newline between candidates
        //            //candidateDetails.AppendLine("---------------------------"); // Add an extra newline between candidates

        //            _unitOfWork.CandidateRepository.Add(candidate);
        //            AddAuditTrail(candidate.CandidateId, "Import confirmed by excel ");
        //        }
        //        await _unitOfWork.SaveChangesAsync();
        //        _candidatePreviewStore.TryRemove(previewId, out _);
        //        //throw new Exception();

        //        _logger.LogInformation($"Preview ID {previewId} confirmed and candidates imported. ");
        //    }
        //    else
        //    {
        //        _logger.LogError($"Invalid preview ID: {previewId}");
        //        throw new Exception("Invalid preview ID");
        //    }
        //}
        public async Task ConfirmImportAsync(string previewId)
        {
            try
            {
                if (_candidatePreviewStore.TryGetValue(previewId, out var candidates))
                {
                    string UserString = "F79CB990-1938-41C9-986D-42F0B711DBA9";
                    if (!Guid.TryParse(UserString, out var UserId))
                    {
                        _logger.LogError("Current user ID is not a valid GUID.");
                        throw new FormatException("Current user ID is not a valid GUID.");
                    }

                    foreach (var candidate in candidates)
                    {
                        try
                        {
                            candidate.PhoneNo = FormatPhoneNumber(candidate.PhoneNo);
                            _unitOfWork.CandidateRepository.Add(candidate);
                            AddAuditTrail(UserId, "Import confirmed by excel huhu");
                        }
                        catch (Exception addEx)
                        {
                            throw new Exception("Error adding candidate to repository: {addEx.Message}");

                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    _candidatePreviewStore.TryRemove(previewId, out _);

                    _logger.LogInformation($"Preview ID {previewId} confirmed and candidates imported.");
                }
                else
                {
                    _logger.LogError($"Invalid preview ID: {previewId}");
                    throw new Exception("Invalid preview ID");
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"Database update error: {dbEx.Message}");
                if (dbEx.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {dbEx.InnerException.Message}");
                }
                throw new Exception("A database update error occurred while saving the entity changes");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                throw new Exception("An unexpected error occurred. Please check the inner exception for details.", ex);
            }
        }



        public void CancelImport(string previewId)
        {
            if (_candidatePreviewStore.TryRemove(previewId, out _))
            {
                _logger.LogInformation($"Preview ID {previewId} import cancelled.");
            }
            else
            {
                _logger.LogError($"Invalid preview ID: {previewId}");
            }
        }


        private bool VerifyFileFormat(Stream fileStream)
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null || worksheet.Name != "Data")
            {
                throw new Exception("Worksheet 'Data' not found.");
            }

            var headers = new List<string>
            {
                "FullName", "Email", "DateOfBirth", "PhoneNo", "Address", "Gender",
                "University", "Major", "GraduationDate", "GPA", "Skills", "Languages",
                "CVLink", "ApplyJob", "ApplicationDate", "WorkingTime", "Talent",
                "Channel", "Recer", "Note", "Status", "IsDeleted", "EventName"
            };

            for (int i = 1; i <= headers.Count; i++)
            {
                if (worksheet.Cell(1, i).GetString() != headers[i - 1])
                {
                    throw new Exception($"Expected header '{headers[i - 1]}' not found at position {i}. Found '{worksheet.Cell(1, i).GetString()}' instead.");
                }
            }
            return true;
        }

        private List<Candidate> ParseCandidatesFromFile(Stream fileStream, Guid eventId, string channel, string cvLink)
        {
            var candidates = new List<Candidate>();
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet("Data");

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var candidate = new Candidate
                {
                    CandidateId = Guid.NewGuid(),
                    FullName = row.Cell(1).GetString(),
                    Email = row.Cell(2).GetString(),
                    DateOfBirth = DateTime.TryParse(row.Cell(3).GetString(), out DateTime parsedDateOfBirth) ? parsedDateOfBirth : default,
                    PhoneNo = row.Cell(4).GetString(),
                    Address = row.Cell(5).GetString(),
                    Gender = row.Cell(6).GetString().Equals("Male", StringComparison.OrdinalIgnoreCase),
                    University = row.Cell(7).GetString(),
                    Major = row.Cell(8).GetString(),
                    GraduationDate = DateTime.TryParse(row.Cell(9).GetString(), out DateTime parsedGraduationDate) ? parsedGraduationDate : default,
                    GPA = double.TryParse(row.Cell(10).GetString(), out double parsedGpa) ? parsedGpa : default,
                    Skills = row.Cell(11).GetString(),
                    Languages = row.Cell(12).GetString(),
                    CVLink = cvLink,
                    ApplyJob = row.Cell(14).GetString(),
                    ApplicationDate = DateTime.TryParse(row.Cell(15).GetString(), out DateTime parsedApplicationDate) ? parsedApplicationDate : default,
                    WorkingTime = row.Cell(16).GetString(),
                    Talent = row.Cell(17).GetString().Equals("Yes", StringComparison.OrdinalIgnoreCase),
                    Channel = channel,
                    Recer = row.Cell(19).GetString(),
                    Note = row.Cell(20).GetString(),
                    Status = row.Cell(21).GetString(),
                    IsDeleted = false,
                    EventId = eventId
                };

                candidates.Add(candidate);
            }
            return candidates;
        }

        private async Task<List<Candidate>> MergeCandidates(List<Candidate> newCandidates, List<CandidateModelsReviewImport> candidateModelsReview)
        {
            var existingCandidates = _unitOfWork.CandidateRepository.GetQuery().ToList();
            var mergedCandidates = new List<Candidate>();

            foreach (var newCandidate in newCandidates)
            {
                var existingCandidate = existingCandidates.FirstOrDefault(c => c.Email == newCandidate.Email && c.PhoneNo.Trim() == newCandidate.PhoneNo.Trim() && c.FullName == newCandidate.FullName);
                if (existingCandidate == null)
                {
                    mergedCandidates.Add(newCandidate);
                    candidateModelsReview.Add(new CandidateModelsReviewImport
                    {
                        FullName = newCandidate.FullName,
                        DateOfBirth = newCandidate.DateOfBirth,
                        Gender = newCandidate.Gender,
                        PhoneNo = newCandidate.PhoneNo,
                        Email = newCandidate.Email,
                        Address = newCandidate.Address,
                        University = newCandidate.University,
                        Major = newCandidate.Major,
                        GraduationDate = newCandidate.GraduationDate,
                        GPA = newCandidate.GPA,
                        Skills = newCandidate.Skills,
                        Languages = newCandidate.Languages,
                        CVLink = newCandidate.CVLink,
                        ApplyJob = newCandidate.ApplyJob,
                        ApplicationDate = newCandidate.ApplicationDate,
                        WorkingTime = newCandidate.WorkingTime,
                        Talent = newCandidate.Talent,
                        Channel = newCandidate.Channel,
                        Recer = newCandidate.Recer,
                        Note = newCandidate.Note,
                        Status = newCandidate.Status,
                        IsDeleted = false,
                        EventId = newCandidate.EventId ?? Guid.Empty,
                        ReviewImport = "New Record"
                    });
                }
                else
                {
                    existingCandidate.FullName = newCandidate.FullName;
                    existingCandidate.DateOfBirth = newCandidate.DateOfBirth;
                    existingCandidate.Gender = newCandidate.Gender;
                    existingCandidate.PhoneNo = MergeValues(existingCandidate.PhoneNo, newCandidate.PhoneNo);
                    existingCandidate.Email = newCandidate.Email;
                    existingCandidate.Address = newCandidate.Address;
                    existingCandidate.University = MergeValues(existingCandidate.University, newCandidate.University);
                    existingCandidate.Major = MergeValues(existingCandidate.Major, newCandidate.Major);
                    existingCandidate.GraduationDate = newCandidate.GraduationDate;
                    existingCandidate.GPA = newCandidate.GPA;
                    existingCandidate.Skills = MergeValues(existingCandidate.Skills, newCandidate.Skills);
                    existingCandidate.Languages = MergeValues(existingCandidate.Languages, newCandidate.Languages);
                    existingCandidate.CVLink = newCandidate.CVLink;
                    existingCandidate.ApplyJob = newCandidate.ApplyJob;
                    existingCandidate.ApplicationDate = newCandidate.ApplicationDate;
                    existingCandidate.WorkingTime = newCandidate.WorkingTime;
                    existingCandidate.Talent = newCandidate.Talent;
                    existingCandidate.Channel = newCandidate.Channel;
                    existingCandidate.Recer = newCandidate.Recer;
                    existingCandidate.Note = newCandidate.Note;
                    existingCandidate.Status = newCandidate.Status;
                    existingCandidate.IsDeleted = false;
                    existingCandidate.EventId = newCandidate.EventId;

                    mergedCandidates.Add(existingCandidate);
                    candidateModelsReview.Add(new CandidateModelsReviewImport
                    {
                        FullName = newCandidate.FullName,
                        DateOfBirth = newCandidate.DateOfBirth,
                        Gender = newCandidate.Gender,
                        PhoneNo = MergeValues(existingCandidate.PhoneNo, newCandidate.PhoneNo),
                        Email = newCandidate.Email,
                        Address = newCandidate.Address,
                        University = MergeValues(existingCandidate.University, newCandidate.University),
                        Major = MergeValues(existingCandidate.Major, newCandidate.Major),
                        GraduationDate = newCandidate.GraduationDate,
                        GPA = newCandidate.GPA,
                        Skills = MergeValues(existingCandidate.Skills, newCandidate.Skills),
                        Languages = MergeValues(existingCandidate.Languages, newCandidate.Languages),
                        CVLink = newCandidate.CVLink,
                        ApplyJob = newCandidate.ApplyJob,
                        ApplicationDate = newCandidate.ApplicationDate,
                        WorkingTime = newCandidate.WorkingTime,
                        Talent = newCandidate.Talent,
                        Channel = newCandidate.Channel,
                        Recer = newCandidate.Recer,
                        Note = newCandidate.Note,
                        Status = newCandidate.Status,
                        IsDeleted = false,
                        EventId = newCandidate.EventId ?? Guid.Empty,
                        ReviewImport = "Updated Record"
                    });
                }
            }
            return mergedCandidates;
        }

        private string MergeValues(string existingValue, string newValue)
        {
            var existingList = existingValue?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            var newList = newValue?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            foreach (var newItem in newList)
            {
                if (!existingList.Contains(newItem))
                {
                    existingList.Add(newItem);
                }
            }
            return string.Join(",", existingList);
        }

        private void AddAuditTrail(Guid candidateId, string actionDetails)
        {
            try
            {
                var createdBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

                if (string.IsNullOrEmpty(createdBy))
                {
                    _logger.LogError("Current user is not authenticated or HttpContext is not available.");
                    createdBy = "Unknown";
                }

                var audit = new AuditTrail
                {
                    CandidateID = candidateId,
                    ActionDetails = actionDetails,
                    ChangedBy = createdBy,
                    ChangeTimestamp = DateTime.Now
                };

                _unitOfWork.AuditTrailRepository.Add(audit);
                _unitOfWork.SaveChangesAsync().Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding audit trail.");
                throw;
            }
        }

        public async Task CreateCandidateAsync(CandidateCreateViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var candidate = new Candidate
            {
                CandidateId = Guid.NewGuid(),
                FullName = model.FullName,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth,
                PhoneNo = FormatPhoneNumber(model.PhoneNo),
                Address = model.Address,
                Gender = model.Gender,
                University = model.University,
                Major = model.Major,
                GraduationDate = model.GraduationDate,
                GPA = model.GPA,
                Skills = model.Skills,
                Languages = model.Languages,
                CVLink = model.CVLink,
                ApplyJob = model.ApplyJob,
                ApplicationDate = model.ApplicationDate,
                WorkingTime = model.WorkingTime,
                Talent = model.Talent,
                Channel = model.Channel,
                Recer = model.Recer,
                Note = model.Note,
                Status = model.Status,
                IsDeleted = false,
                EventId = model.EventId
            };

            _unitOfWork.CandidateRepository.Add(candidate);
            await _unitOfWork.SaveChangesAsync();
            AddAuditTrail(candidate.CandidateId, "Created new candidate");
        }


        public async Task UpdateCandidateStatusAsync(Guid id, string status)
        {
            var candidate = await _unitOfWork.CandidateRepository.GetByIdAsync(id);
            if (candidate == null)
            {
                throw new KeyNotFoundException("Candidate not found");
            }
            if (candidate.IsDeleted == true)
            {
                throw new KeyNotFoundException("Candidate has been deleted");
            }

            AddAuditTrail(candidate.CandidateId, "Update Status from '" + candidate.Status + "' to '" + status + "'");

            candidate.Status = status;
            _unitOfWork.CandidateRepository.Update(candidate);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Candidate> GetCandidateByIdAsync(Guid id)
        {
            var candidate = await _unitOfWork.CandidateRepository.GetByIdAsync(id);
            if (candidate == null)
            {
                throw new KeyNotFoundException("Candidate not found");
            }
            if (candidate.IsDeleted == true)
            {
                throw new KeyNotFoundException("Candidate has been deleted");
            }

            return candidate;
        }

        public async Task DeleteCandidateAsync(Guid id)
        {
            var candidate = await _unitOfWork.CandidateRepository.GetByIdAsync(id);
            if (candidate == null)
            {
                throw new KeyNotFoundException("Candidate not found");
            }
            if (candidate.IsDeleted == true)
            {
                throw new KeyNotFoundException("Candidate has been deleted");
            }

            candidate.IsDeleted = true;
            candidate.DeletedAt = DateTime.Now;
            _unitOfWork.CandidateRepository.Update(candidate);
            await _unitOfWork.SaveChangesAsync();
            AddAuditTrail(candidate.CandidateId, "Soft deleted candidate");
        }

        public async Task UpdateCandidateAsync(CandidateCreateViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var candidate = await _unitOfWork.CandidateRepository.GetByIdAsync(model.CandidateId);
            if (candidate == null)
            {
                throw new KeyNotFoundException("Candidate not found");
            }
            if (candidate.IsDeleted == true)
            {
                throw new KeyNotFoundException("Candidate has been deleted");
            }

            var oldCandidate = new Candidate
            {
                FullName = candidate.FullName,
                Email = candidate.Email,
                DateOfBirth = candidate.DateOfBirth,
                PhoneNo = candidate.PhoneNo,
                Address = candidate.Address,
                Gender = candidate.Gender,
                University = candidate.University,
                Major = candidate.Major,
                GraduationDate = candidate.GraduationDate,
                GPA = candidate.GPA,
                Skills = candidate.Skills,
                Languages = candidate.Languages,
                CVLink = candidate.CVLink,
                ApplyJob = candidate.ApplyJob,
                ApplicationDate = candidate.ApplicationDate,
                WorkingTime = candidate.WorkingTime,
                Talent = candidate.Talent,
                Channel = candidate.Channel,
                Recer = candidate.Recer,
                Note = candidate.Note,
                Status = candidate.Status,
                EventId = candidate.EventId
            };

            candidate.FullName = model.FullName;
            candidate.Email = model.Email;
            candidate.DateOfBirth = model.DateOfBirth;
            candidate.PhoneNo = FormatPhoneNumber(model.PhoneNo);
            candidate.Address = model.Address;
            candidate.Gender = model.Gender;
            candidate.University = model.University;
            candidate.Major = model.Major;
            candidate.GraduationDate = model.GraduationDate;
            candidate.GPA = model.GPA;
            candidate.Skills = model.Skills;
            candidate.Languages = model.Languages;
            candidate.CVLink = model.CVLink;
            candidate.ApplyJob = model.ApplyJob;
            candidate.ApplicationDate = model.ApplicationDate;
            candidate.WorkingTime = model.WorkingTime;
            candidate.Talent = model.Talent;
            candidate.Channel = model.Channel;
            candidate.Recer = model.Recer;
            candidate.Note = model.Note;
            candidate.Status = model.Status;
            candidate.EventId = model.EventId;

            _unitOfWork.CandidateRepository.Update(candidate);
            await _unitOfWork.SaveChangesAsync();

            var changes = new List<string>();
            if (oldCandidate.FullName != candidate.FullName)
                changes.Add($"FullName changed from '{oldCandidate.FullName}' to '{candidate.FullName}'");
            if (oldCandidate.Email != candidate.Email)
                changes.Add($"Email changed from '{oldCandidate.Email}' to '{candidate.Email}'");
            if (oldCandidate.DateOfBirth != candidate.DateOfBirth)
                changes.Add($"DateOfBirth changed from '{oldCandidate.DateOfBirth}' to '{candidate.DateOfBirth}'");
            if (oldCandidate.PhoneNo != candidate.PhoneNo)
                changes.Add($"PhoneNo changed from '{oldCandidate.PhoneNo}' to '{candidate.PhoneNo}'");
            if (oldCandidate.Address != candidate.Address)
                changes.Add($"Address changed from '{oldCandidate.Address}' to '{candidate.Address}'");
            if (oldCandidate.Gender != candidate.Gender)
                changes.Add($"Gender changed from '{oldCandidate.Gender}' to '{candidate.Gender}'");
            if (oldCandidate.University != candidate.University)
                changes.Add($"University changed from '{oldCandidate.University}' to '{candidate.University}'");
            if (oldCandidate.Major != candidate.Major)
                changes.Add($"Major changed from '{oldCandidate.Major}' to '{candidate.Major}'");
            if (oldCandidate.GraduationDate != candidate.GraduationDate)
                changes.Add($"GraduationDate changed from '{oldCandidate.GraduationDate}' to '{candidate.GraduationDate}'");
            if (oldCandidate.GPA != candidate.GPA)
                changes.Add($"GPA changed from '{oldCandidate.GPA}' to '{candidate.GPA}'");
            if (oldCandidate.Skills != candidate.Skills)
                changes.Add($"Skills changed from '{oldCandidate.Skills}' to '{candidate.Skills}'");
            if (oldCandidate.Languages != candidate.Languages)
                changes.Add($"Languages changed from '{oldCandidate.Languages}' to '{candidate.Languages}'");
            if (oldCandidate.CVLink != candidate.CVLink)
                changes.Add($"CVLink changed from '{oldCandidate.CVLink}' to '{candidate.CVLink}'");
            if (oldCandidate.ApplyJob != candidate.ApplyJob)
                changes.Add($"ApplyJob changed from '{oldCandidate.ApplyJob}' to '{candidate.ApplyJob}'");
            if (oldCandidate.ApplicationDate != candidate.ApplicationDate)
                changes.Add($"ApplicationDate changed from '{oldCandidate.ApplicationDate}' to '{candidate.ApplicationDate}'");
            if (oldCandidate.WorkingTime != candidate.WorkingTime)
                changes.Add($"WorkingTime changed from '{oldCandidate.WorkingTime}' to '{candidate.WorkingTime}'");
            if (oldCandidate.Talent != candidate.Talent)
                changes.Add($"Talent changed from '{oldCandidate.Talent}' to '{candidate.Talent}'");
            if (oldCandidate.Channel != candidate.Channel)
                changes.Add($"Channel changed from '{oldCandidate.Channel}' to '{candidate.Channel}'");
            if (oldCandidate.Recer != candidate.Recer)
                changes.Add($"Recer changed from '{oldCandidate.Recer}' to '{candidate.Recer}'");
            if (oldCandidate.Note != candidate.Note)
                changes.Add($"Note changed from '{oldCandidate.Note}' to '{candidate.Note}'");
            if (oldCandidate.Status != candidate.Status)
                changes.Add($"Status changed from '{oldCandidate.Status}' to '{candidate.Status}'");
            if (oldCandidate.EventId != candidate.EventId)
                changes.Add($"EventId changed from '{oldCandidate.EventId}' to '{candidate.EventId}'");

            string actionDetails = string.Join("; ", changes);
            AddAuditTrail(candidate.CandidateId, actionDetails);
        }


        public async Task<IEnumerable<Candidate>> GetAllCandidatesAsync(string searchTerm = null)
        {
            var query = _unitOfWork.CandidateRepository.GetQuery();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => (c.FullName.Contains(searchTerm) ||
                                          c.Email.Contains(searchTerm) ||
                                          c.PhoneNo.Contains(searchTerm)) &&
                                          c.IsDeleted == false);
            }
            else
            {
                query = query.Where(c => c.IsDeleted == false);
            }

            return await query.ToListAsync();
        }

        public async Task<byte[]> ExportCandidatesToExcelAsync()
        {
            var candidates = await GetAllCandidates();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Data");

            // Adding header
            var headers = new List<string>
            {
                "FullName", "Email", "DateOfBirth", "PhoneNo", "Address", "Gender",
                "University", "Major", "GraduationDate", "GPA", "Skills", "Languages",
                "CVLink", "ApplyJob", "ApplicationDate", "WorkingTime", "Talent", "Channel",
                "Recer", "Note", "Status", "IsDeleted", "EventName"
            };
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            int row = 2;
            foreach (var candidate in candidates)
            {
                worksheet.Cell(row, 1).Value = candidate.FullName;
                worksheet.Cell(row, 2).Value = candidate.Email;
                worksheet.Cell(row, 3).Value = candidate.DateOfBirth;
                worksheet.Cell(row, 4).Value = candidate.PhoneNo;
                worksheet.Cell(row, 5).Value = candidate.Address;
                worksheet.Cell(row, 6).Value = candidate.Gender ? "Male" : "Female";
                worksheet.Cell(row, 7).Value = candidate.University;
                worksheet.Cell(row, 8).Value = candidate.Major;
                worksheet.Cell(row, 9).Value = candidate.GraduationDate;
                worksheet.Cell(row, 10).Value = candidate.GPA;
                worksheet.Cell(row, 11).Value = candidate.Skills;
                worksheet.Cell(row, 12).Value = candidate.Languages;
                worksheet.Cell(row, 13).Value = candidate.CVLink;
                worksheet.Cell(row, 14).Value = candidate.ApplyJob;
                worksheet.Cell(row, 15).Value = candidate.ApplicationDate;
                worksheet.Cell(row, 16).Value = candidate.WorkingTime;
                worksheet.Cell(row, 17).Value = candidate.Talent ? "Yes" : "No";
                worksheet.Cell(row, 18).Value = candidate.Channel;
                worksheet.Cell(row, 19).Value = candidate.Recer;
                worksheet.Cell(row, 20).Value = candidate.Note;
                worksheet.Cell(row, 21).Value = candidate.Status;
                worksheet.Cell(row, 22).Value = candidate.IsDeleted ? "Yes" : "No";

                var eventCandidate = _unitOfWork.EventRepository.GetById(candidate.EventId.Value);
                worksheet.Cell(row, 23).Value = eventCandidate?.EventName ?? "N/A";

                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }



        private string FormatPhoneNumber(string phoneNo)
        {
            if (string.IsNullOrWhiteSpace(phoneNo))
            {
                throw new ArgumentException("Phone number is required.", nameof(phoneNo));
            }

            // Remove any non-digit characters
            var cleanedPhoneNo = new string(phoneNo.Where(char.IsDigit).ToArray());



            // Ensure the phone number starts with '0'
            if (!cleanedPhoneNo.StartsWith("0"))
            {
                cleanedPhoneNo = "0" + cleanedPhoneNo;
            }

            // Ensure the phone number length is between 10 and 12
            if (cleanedPhoneNo.Length < 10 || cleanedPhoneNo.Length > 12)
            {
                throw new ArgumentException("Phone number must be between 10 and 12 digits.", nameof(phoneNo));
            }

          

            return cleanedPhoneNo;
        }


        public async Task<IEnumerable<Candidate>> AdvancedSearchCandidates(CandidateSearchModel searchModel)
        {
            var query = _unitOfWork.CandidateRepository.GetQuery().Where(c => !c.IsDeleted);

            if (!string.IsNullOrEmpty(searchModel.FullName))
            {
                string fullNameSearch = searchModel.FullName.ToLower();
                query = query.Where(c => c.FullName.ToLower().Contains(fullNameSearch));
            }

            if (!string.IsNullOrEmpty(searchModel.EventName))
            {
                string eventNameSearch = searchModel.EventName.ToLower();
                query = query.Where(c => c.Event.EventName.ToLower().Contains(eventNameSearch));
            }

            if (!string.IsNullOrEmpty(searchModel.Channel))
            {
                string channelSearch = searchModel.Channel.ToLower();
                query = query.Where(c => c.Channel.ToLower().Contains(channelSearch));
            }

            if (searchModel.GraduationYearFrom.HasValue)
            {
                query = query.Where(c => c.GraduationDate.Year >= searchModel.GraduationYearFrom.Value);
            }
            if (searchModel.GraduationYearTo.HasValue)
            {
                query = query.Where(c => c.GraduationDate.Year <= searchModel.GraduationYearTo.Value);
            }

            if (searchModel.GPAFrom.HasValue)
            {
                query = query.Where(c => c.GPA >= searchModel.GPAFrom.Value);
            }
            if (searchModel.GPATo.HasValue)
            {
                query = query.Where(c => c.GPA <= searchModel.GPATo.Value);
            }

            if (searchModel.Gender.HasValue)
            {
                query = query.Where(c => c.Gender == searchModel.Gender.Value);
            }

            if (searchModel.Talent.HasValue)
            {
                query = query.Where(c => c.Talent == searchModel.Talent.Value);
            }

            var candidateList = await query.ToListAsync();

            if (!string.IsNullOrEmpty(searchModel.University))
            {
                var universitySearches = searchModel.University.ToLower().Split(',').Select(u => u.Trim());
                candidateList = candidateList.Where(c => universitySearches.All(u => c.University.ToLower().Contains(u))).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.Skills))
            {
                var skillsSearches = searchModel.Skills.ToLower().Split(',').Select(s => s.Trim());
                candidateList = candidateList.Where(c => skillsSearches.All(s => c.Skills.ToLower().Contains(s))).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.Languages))
            {
                var languagesSearches = searchModel.Languages.ToLower().Split(',').Select(l => l.Trim());
                candidateList = candidateList.Where(c => languagesSearches.All(l => c.Languages.ToLower().Contains(l))).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.WorkingTime))
            {
                string workingTimeSearch = searchModel.WorkingTime.ToLower();
                candidateList = candidateList.Where(c => c.WorkingTime.ToLower().Contains(workingTimeSearch)).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.ApplyJob))
            {  
                string applyJobSearch = searchModel.ApplyJob.ToLower();
                candidateList = candidateList.Where(c => c.ApplyJob.ToLower().Contains(applyJobSearch)).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.Recer))
            {
                string recerSearch = searchModel.Recer.ToLower();
                candidateList = candidateList.Where(c => c.Recer.ToLower().Contains(recerSearch)).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.Status))
            {
                string statusSearch = searchModel.Status.ToLower();
                candidateList = candidateList.Where(c => c.Status.ToLower().Contains(statusSearch)).ToList();
            }

            return candidateList;
        }

      
    }
}
