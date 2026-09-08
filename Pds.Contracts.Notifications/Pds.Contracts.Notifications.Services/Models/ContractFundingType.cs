using System.ComponentModel.DataAnnotations;

namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// The types of funding that the system supports.
    /// </summary>
    public enum ContractFundingType
    {
        /// <summary>
        /// Unknown funding type.
        /// </summary>
        [Display(Name = "", Description = "Unknown")]
        Unknown = 0,

        /// <summary>
        /// Mainstream funding type.
        /// </summary>
        [Display(Name = "Mainstream", Description = "Mainstream")]
        Mainstream = 1,

        /// <summary>
        /// Esf funding type
        /// </summary>
        [Display(Name = "ESF", Description = "European social fund (ESF)")]
        Esf = 2,

        /// <summary>
        /// TwentyFourPlusLoan funding type
        /// </summary>
        [Display(Name = "24+LOANS", Description = "24+LOANS")]
        TwentyFourPlusLoan = 3,

        /// <summary>
        /// Age funding type
        /// </summary>
        [Display(Name = "AGE", Description = "AGE Grant")]
        Age = 4,

        /// <summary>
        /// Eop funding type
        /// </summary>
        [Display(Name = "EOP", Description = "EOP")]
        Eop = 5,

        /// <summary>
        /// Eof funding type
        /// </summary>
        [Display(Name = "EOF", Description = "EOF")]
        Eof = 6,

        /// <summary>
        /// CityDeals funding type.
        /// </summary>
        [Display(Name = "City Deal", Description = "City deal")]
        CityDeals = 7,

        /// <summary>
        /// LocalGrowth funding type.
        /// </summary>
        [Display(Name = "Local Growth", Description = "Local growth")]
        LocalGrowth = 8,

        /// <summary>
        /// Levy funding type.
        /// </summary>
        [Display(Name = "apprenticeship agreement", Description = "Apprenticeship agreement")]
        Levy = 9,

        /// <summary>
        /// NCS funding type.
        /// </summary>
        [Display(Name = "National Careers Service (NCS)", Description = "National Careers Service (NCS)")]
        Ncs = 10,

        /// <summary>
        /// Non Levy funding type.
        /// </summary>
        [Display(Name = "Apprenticeship contract", Description = "Apprenticeship contract")]
        NonLevy = 11,

        /// <summary>
        /// 1619Fund funding type.
        /// </summary>
        [Display(Name = "16 to 19 funding", Description = "16-19 funding")]
        SixteenNineteenFunding = 12,

        /// <summary>
        /// AEBP funding type.
        /// </summary>
        [Display(Name = "Procured adult education", Description = "Procured adult education)")]
        Aebp = 13,

        /// <summary>
        /// NLA funding type.
        /// </summary>
        [Display(Name = "Procured non levy apprenticeship contract", Description = "Procured non levy apprenticeship contract")]
        Nla = 14,

        /// <summary>
        /// Advanced Leaner Loans funding type
        /// </summary>
        [Display(Name = "Advanced Leaner Loans", Description = "Advanced Leaner Loans")]
        AdvancedLearnerLoans = 15,

        /// <summary>
        /// Education and skills funding contract.
        /// </summary>
        [Display(Name = "Education and skills funding contract", Description = "Education and skills funding contract")]
        EducationAndSkillsFunding = 16,

        /// <summary>
        /// Non-learning grant funding type.
        /// </summary>
        [Display(Name = "Non-learning grant funding agreement", Description = "Non-learning grant funding agreement")]
        NonLearningGrant = 17,

        /// <summary>
        /// 16-18 Forensic Unit.
        /// </summary>
        [Display(Name = "16 to 18 Forensic Unit", Description = "16-18 Forensic Unit")]
        SixteenEighteenForensicUnit = 18,

        /// <summary>
        /// Dance and Drama Awards.
        /// </summary>
        [Display(Name = "Dance and Drama Awards", Description = "Dance and Drama Awards")]
        DanceAndDramaAwards = 19,

        /// <summary>
        /// College Collaboration Fund Agreements.
        /// </summary>
        [Display(Name = "College Collaboration Fund", Description = "College Collaboration Fund")]
        CollegeCollaborationFund = 20,

        /// <summary>
        /// Further Education Condition Allocation agreements.
        /// </summary>
        [Display(Name = "Further Education Condition Allocation", Description = "Further Education Condition Allocation")]
        FurtherEducationConditionAllocation = 21,

        /// <summary>
        /// Procured 19 to 24 traineeships.
        /// </summary>
        [Display(Name = "Procured 19 to 24 traineeship", Description = "Procured 19 to 24 traineeship")]
        ProcuredNineteenToTwentyFourTraineeship = 22,

        /// <summary>
        /// Adult Education Budget (Contract for Service).
        /// </summary>
        [Display(Name = "Adult Education Budget (Contract for Service)", Description = "Adult Education Budget (Contract for Service)")]
        AdultEducationBudgetContractForService = 23,


        /// <summary>
        /// Higher Technical Education Provider Growth Fund.
        /// </summary>
        [Display(Name = "Higher technical education provider growth fund", Description = "Higher technical education provider growth fund")]
        HigherTechnicalEducation = 24,

        /// <summary>
        /// Skills Accelerator Development Fund.
        /// </summary>
        [Display(Name = "Skills accelerator development fund", Description = "Skills accelerator development fund")]
        SkillsAcceleratorDevelopment = 25,

        /// <summary>
        /// Further Education Professional Development Grants Pilot.
        /// </summary>
        [Display(Name = "Further education professional development grants", Description = "Further education professional development grants")]
        FurtherEducationProfessionalDevelopmentGrants = 26,

        /// <summary>
        /// Strategic development fund II.
        /// </summary>
        [Display(Name = "Strategic Development Fund II", Description = "Strategic Development Fund II")]
        StrategicDevelopmentFund2 = 27,

        /// <summary>
        /// SKILLS BOOTCAMPS.
        /// </summary>
        [Display(Name = "Skills bootcamps", Description = "Skills bootcamps")]
        SkillsBootcamps = 28,

        /// <summary>
        /// Multiply Programme.
        /// </summary>
        [Display(Name = "Multiply", Description = "Multiply")]
        Multiply = 29,

        /// <summary>
        /// Additional capital allocations.
        /// </summary>
        [Display(Name = "Additional capital allocations", Description = "Additional capital allocations")]
        AdditionalCapitalAllocations = 30,

        /// <summary>
        /// Higher technical education skills injection fund.
        /// </summary>
        [Display(Name = "Higher technical education skills injection fund", Description = "Higher technical education skills injection fund")]
        HigherTechnicalEducationSkillsInjectionFund = 31,

        /// <summary>
        /// FE Reclassification Capital Allocation.
        /// </summary>
        [Display(Name = "FE Reclassification Capital Allocation", Description = "FE Reclassification Capital Allocation")]
        FEReclassificationCapitalAllocation = 32,

        /// <summary>
        /// FE Capital Transformation Fund Allocation.
        /// </summary>
        [Display(Name = "FE Capital Transformation Fund Allocation", Description = "FE Capital Transformation Fund Allocation")]
        FECapitalTrandformationFundAllocation = 33,

        /// <summary>
        /// ESFA Adult Education Budget (procured from Aug 2023).
        /// Shares AEB2023 funding type from FCS with ESFA Adult Skills Fund (procured from Aug 2023.
        /// </summary>
        [Display(Name = "ESFA Adult Education Budget (procured from Aug 2023)", Description = "ESFA Adult Education Budget (procured from Aug 2023)")]
        AdultEducationBudgetProcured2023 = 34,

        /// <summary>
        /// Skills Bootcamps Dynamic Purchasing System.
        /// </summary>
        [Display(Name = "Skills bootcamps", Description = "Skills bootcamps dynamic purchasing system (DPS)")]
        SkillsBootcampsDPS = 35,

        /// <summary>
        /// Higher technical education skills injection fund 2.
        /// </summary>
        [Display(Name = "Higher technical education skills injection fund 2", Description = "Higher technical education skills injection fund 2")]
        HigherTechnicalEducationSkillsInjectionFund2 = 36,

        /// <summary>
        /// ESFA Adult Skills Fund (procured from Aug 2023).
        /// Shares AEB2023 funding type from FCS with ESFA Adult Education Budget (procured from Aug 2023).
        /// </summary>
        [Display(Name = "ESFA Adult Skills Fund (procured from Aug 2023)", Description = "ESFA Adult Skills Fund (procured from Aug 2023)")]
        AdultSkillsFundProcured2023 = 37,

        /// <summary>
        /// Taking teaching further.
        /// Used for 2024-25 Cohort Year 1 and 2025-26 Cohort Year 1.
        /// </summary>
        [Display(Name = "Taking teaching further", Description = "Taking teaching further")]
        TakingTeachingFurther = 38,

        /// <summary>
        /// DfE Adult Skills Fund.
        /// Shares AEB2023 funding type from FCS with ESFA Adult Education Budget (procured from Aug 2023).
        /// </summary>
        [Display(Name = "DfE Adult Skills Fund", Description = "DfE Adult Skills Fund")]
        AdultSkillsFund = 39,

        /// <summary>
        /// Taking teaching further year 2.
        /// Used for 2024-25 Cohort Year 2 and 2025-26 Cohort Year 2.
        /// </summary>
        [Display(Name = "Taking teaching further", Description = "Taking teaching further")]
        TakingTeachingFurtherYear2 = 40,

        /// <summary>
        /// Construction Technical Excellence Colleges.
        /// </summary>
        [Display(Name = "Construction Technical Excellence Colleges", Description = "Construction Technical Excellence Colleges")]
        ConstructionTechnicalExcellenceColleges = 41,

        /// <summary>
        /// Construction Skills Package: Industry Placements Fund.
        /// </summary>
        [Display(Name = "Construction Skills Package: Industry Placements Fund", Description = "Construction Skills Package: Industry Placements Fund")]
        ConstructionSkillsPackageIndustryPlacementsFund = 42,

        /// <summary>
        /// DWP Adult Skills Fund.
        /// Shares AEB2023 funding type from FCS with ESFA Adult Education Budget (procured from Aug 2023).
        /// </summary>
        [Display(Name = "DWP Adult Skills Fund", Description = "DWP Adult Skills Fund")]
        DWPAdultSkillsFund = 43,

        /// <summary>
        /// Taking teaching further year 2026-2027 cohort - Round 9.
        /// Used for 2026-27 Cohort, Year 1.
        /// </summary>
        [Display(Name = "Taking teaching further", Description = "Taking teaching further")]
        TakingTeachingFurtherRound9 = 44,
    }
}