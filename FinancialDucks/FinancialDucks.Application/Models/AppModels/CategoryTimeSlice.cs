using FinancialDucks.Application.Extensions;

namespace FinancialDucks.Application.Models.AppModels
{
    public record TimeSlice(DateTime SliceStart, DateTime SliceEnd);
    public record CategoryTimeSlice(decimal Amount, TimeInterval TimeInterval, DateTime SliceStart, DateTime SliceEnd) 
        : TimeSlice(SliceStart,SliceEnd) 
    { 
        public CategoryTimeSlice ChangeAmount(decimal newAmount) => 
            new CategoryTimeSlice(newAmount,TimeInterval, SliceStart,SliceEnd);
    }

    public record CategorySnapshot(decimal Amount, DateTime Date);
}
