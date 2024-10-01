﻿using CampusVarbergDashBoard.Models;

namespace CampusVarbergDashBoard.Repository
{
    public interface IApplicantRepository
    {
        Task<IEnumerable<Applicant>> GetAllApplicantsAsync();
        
    }
}
