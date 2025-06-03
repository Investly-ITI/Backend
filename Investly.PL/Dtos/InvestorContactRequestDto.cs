namespace Investly.PL.Dtos
{
    public class InvestorContactRequestDto
    {
        public int Id { get; set; }
        // Founder (include the ID if you want to navigate to that specific founder)
        public string FounderName { get; set; }
        public int FounderId { get; set; }
        public string InvestorName{ get; set; }
        public int InvestorId { get; set; }
        public string BusinessTitle { get; set; }
        public int BusinessId { get; set; }
        public int Status { get; set; }
        public string? DeclineReason { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ContactRequestToggleActivationDto
    {
        public int ContactRequestId { get; set; }
        public string? DeclineReason { get; set; }
    }


}
