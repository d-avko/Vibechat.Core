using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.Contacts
{
    public class GetContactsOfSpec : BaseSpecification<ContactsDataModel>
    {
        public GetContactsOfSpec(string userId) 
            : base(contact => contact.FirstUserID == userId)
        {
        }
    }
}
