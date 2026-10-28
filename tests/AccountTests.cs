using System;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using DiscordSoundLauncher;
class AccountTests {
 static void Check(bool value){if(!value)throw new Exception("Account validation failed");}
 public static int Main(){try{
  Check(AccountValidation.Validate("","1234567890","RU").Contains("Логин"));
  Check(AccountValidation.Validate("кириллица","1234567890","UK").Contains("Логін"));
  Check(AccountValidation.Validate("valid_user","short","RU").Contains("10"));
  Check(AccountValidation.Validate("valid_user","long-password-here","EN")==null);
  Check(AccountValidation.Translate("Username is already taken","RU").Contains("занят"));
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
  using(var client=new WebClient {Encoding=new UTF8Encoding(false)}){
   client.Headers["Content-Type"]="application/json; charset=utf-8";
   try{client.UploadString("https://soundspace-workshop.cloud-workshop.workers.dev/api/auth/register","POST",new JavaScriptSerializer().Serialize(new {username="ab",password="test-password-123"}));throw new Exception("Invalid username accepted");}
   catch(WebException e){string message=AccountValidation.ReadError(e,"RU");Check(message.Contains("Логин")&&message.Contains("24"));}
  }
  Console.WriteLine("PASS: empty/invalid username, short password, valid fields, translated errors and real .NET HTTP 400 JSON response.");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
