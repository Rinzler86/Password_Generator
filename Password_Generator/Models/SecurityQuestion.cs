using System.ComponentModel.DataAnnotations;

namespace Password_Generator.Models
{
    public enum SecurityQuestion
    {
        [Display(Name = "What is your mother's maiden name?")]
        WhatIsYourMotherMaidenName,

        [Display(Name = "What was the name of your first pet?")]
        WhatWasTheNameOfYourFirstPet,

        [Display(Name = "What is your favorite book?")]
        WhatIsYourFavoriteBook,

        [Display(Name = "What city were you born in?")]
        WhatCityWereYouBornIn,

        [Display(Name = "What is your favorite food?")]
        WhatIsYourFavoriteFood
    }
}

