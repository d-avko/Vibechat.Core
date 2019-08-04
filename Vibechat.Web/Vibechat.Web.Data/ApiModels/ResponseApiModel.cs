namespace Vibechat.Web.ApiModels
{
    public class ResponseApiModel<T>
    {
        public bool IsSuccessfull { get; set; }

        public string ErrorMessage { get; set; }

        public T Response { get; set; }
    }
}