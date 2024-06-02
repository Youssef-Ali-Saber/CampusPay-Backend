
using Domain.Entities;

namespace Application.DTOs;

public class HistoryTransactionDto
{
    public List<Transaction>? transactions { get; set; }
    public List<Deposition>? deposits { get; set; }
    public List<Transformation>? transferFromMe { get; set; }
    public List<Transformation>? transferToMe { get; set; }
}
