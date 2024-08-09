
namespace Project.Business.Model
{
    public class LoginResponseViewModel
    {
        public  string UserInformation { get; set; }

        public  string Token { get; set; }

        public DateTime Expires { get; set; }
        public string UserName { get; set; }
        
    }
}
