using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ESAM.GrowTracking.Domain.Catalogs
{
    public enum MaritalStatus : byte
    {
        [Display(Name = "Single")]
        [EnumMember(Value = "Single")]
        Single = 1,

        [Display(Name = "Married")]
        [EnumMember(Value = "Married")]
        Married = 2,

        [Display(Name = "Vidower")]
        [EnumMember(Value = "Vidower")]
        Vidower = 3,

        [Display(Name = "Divorced")]
        [EnumMember(Value = "Divorced")]
        Divorced = 4
    }
}