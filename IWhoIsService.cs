using System;
using System.ServiceModel;

namespace MultiService
{
    [ServiceContract]
    public interface IWhoIsService
    {
        [OperationContract]
        int WhatsYourNum(int num);
    }

    public class WhoIsService : IWhoIsService
    {
        public int IAm;
        public bool Major;
        public int WhatsYourNum(int num)
        {
            
            return IAm;
        }

        public WhoIsService()
        {
            int retValue = 0;
            
            if (int.TryParse(System.Configuration.ConfigurationManager.AppSettings["MyNum"],out retValue))
	        {
                IAm = retValue;
	        }
            else IAm = 0;
        }
    }
}
