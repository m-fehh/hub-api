using System.ComponentModel.DataAnnotations;

namespace Hub.Shared.Enums
{
    public enum EGender
    {
        [Display(Name = "Nothing")]
        Nothing = 0,

        [Display(Name = "Female")]
        Female = 1,

        [Display(Name = "Male")]
        Male = 2,

        [Display(Name = "Others")]
        Others = 3
    }
}
