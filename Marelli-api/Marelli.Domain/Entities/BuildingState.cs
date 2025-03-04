namespace Marelli.Domain.Entities
{
    public class BuildingState
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int BuildId { get; set; }
        public string JenkinsBuildLogFile { get; set; }
        public DateTime Date { get; set; }
        public bool InQueue { get; set; }
        public string Starting { get; set; }
        public DateTime StartingDate { get; set; }
        public string Integrating { get; set; }
        public DateTime IntegratingDate { get; set; }
        public string ApplGen { get; set; }
        public DateTime ApplGenDate { get; set; }
        public string NvmGen { get; set; }
        public DateTime NvmGenDate { get; set; }
        public string ParametersGen { get; set; }
        public DateTime ParametersGenDate { get; set; }
        public string DiagnoseGen { get; set; }
        public DateTime DiagnoseGenDate { get; set; }
        public string NetworkGen { get; set; }
        public DateTime NetworkGenDate { get; set; }
        public string RteGen { get; set; }
        public DateTime RteGenDate { get; set; }
        public string UpdateIds { get; set; }
        public DateTime UpdateIdsDate { get; set; }
        public string Compiling { get; set; }
        public DateTime CompilingDate { get; set; }
        public string Finished { get; set; }
        public DateTime FinishedDate { get; set; }
        public string Download { get; set; }
    }
}
