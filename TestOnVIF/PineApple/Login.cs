using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meziantou.Framework.Win32;

namespace Rujchai
{
    class Credit
    {
        public string AppName { get; set; }
        public string PassWord { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public Credit()
        {
            AppName = "Pine Apple";
            var cred = CredentialManager.ReadCredential(AppName);
            if (cred != null)
            {
                UserName = cred.UserName;
                PassWord = cred.Password;
            }
            else
            {
                createAccount();
            }
        }

        public Credit(string appname)
        {
            AppName = appname;
            var cred = CredentialManager.ReadCredential(AppName);
            if (cred != null)
            {
                UserName = cred.UserName;
                PassWord = cred.Password;
            }
            else
            {
                createAccount();
            }
        }

        public bool createAccount()
        {
            bool rval = false;
            var creds = CredentialManager.PromptForCredentials(
                    captionText: "Create Account",
                    messageText: "Create account to use PineApple App",
                    saveCredential: CredentialSaveOption.Hidden
                //userName: "Admin" // Pre-fill the UI with a default username
                );
            if (creds != null)
            {
                CredentialManager.WriteCredential(AppName, creds.UserName, creds.Password, CredentialPersistence.LocalMachine);
                rval = true;
            }
            return rval;
        }

        public bool login(out bool cancel)
        {
            cancel = false;
            bool rval = false;
            CredentialResult creds = CredentialManager.PromptForCredentials(
                    captionText: "Log In",
                    //messageText: "This will allow user to use PineApple App",
                    messageText: Message,
                    saveCredential: CredentialSaveOption.Hidden
                );
            if (creds != null)
            {
                if (creds.UserName == UserName && creds.Password == PassWord)
                {
                    rval = true;
                }
            }
            else
            {
                cancel = true;
            }
            return rval;
        }

    }
}
