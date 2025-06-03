using System;
using System.Collections.Generic;

namespace Investly.DAL.Entities;

public partial class InvestorContactRequest
{
    public int Id { get; set; }

    public int InvestorId { get; set; }

    public int BusinessId { get; set; }

    public bool Status { get; set; }

    public string? DeclineReason { get; set; }

    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }


    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Business Business { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Investor Investor { get; set; } = null!;

    public virtual User? UpdatedByNavigation { get; set; }
}
