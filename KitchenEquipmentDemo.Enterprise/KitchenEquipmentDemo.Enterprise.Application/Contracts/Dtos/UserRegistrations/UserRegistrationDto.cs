using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitchenEquipmentDemo.Enterprise.Application.Contracts.Dtos.UserRegistrations
{
    using System;
    using System.Collections.Generic;

    namespace KitchenEquipmentDemo.Enterprise.WPF.Dtos
    {
        public class UserRegistrationDto
        {
            public int RequestId { get; set; } // request_id (Primary key)
            public string FirstName { get; set; } // first_name (length: 100)
            public string LastName { get; set; } // last_name (length: 100)
            public string EmailAddress { get; set; } // email_address (length: 256)
            public string UserName { get; set; } // user_name (length: 100)
            public string UserType { get; set; } // user_type (length: 20)
            public byte[] PasswordHash { get; set; } // password_hash (length: 64)
            public byte[] PasswordSalt { get; set; } // password_salt (length: 16)
            public string Status { get; set; } // status (length: 20)
            public DateTime CreatedAt { get; set; } // created_at
            public int? ReviewedBy { get; set; } // reviewed_by
            public DateTime? ReviewedAt { get; set; } // reviewed_at
            public string ReviewNote { get; set; } // review_note (length: 400)
        }
    }
}
