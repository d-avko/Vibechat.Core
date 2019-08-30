using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.Contacts
{
    public class GetContactsOfSpec : BaseSpecification<ContactsDataModel>
    {
        public GetContactsOfSpec(string userId) 
            : base(contact => contact.FirstUserID == userId)
        {
            AddInclude(x => x.Contact);
        }
    }

}
