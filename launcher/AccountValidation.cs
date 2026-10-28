using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Collections.Generic;
namespace DiscordSoundLauncher {
 public static class AccountValidation {
  static string L(string language,string ru,string en,string uk){return language=="EN"?en:language=="UK"?uk:ru;}
  public static string Validate(string username,string password,string language){
   if(!Regex.IsMatch(username??"","^[a-zA-Z0-9_-]{3,24}$"))return L(language,"Логин: от 3 до 24 символов — латинские буквы, цифры, _ или -. Без пробелов.","Username: 3–24 Latin letters, numbers, _ or -. No spaces.","Логін: від 3 до 24 символів — латинські літери, цифри, _ або -. Без пробілів.");
   if(password==null||password.Length<10||password.Length>128)return L(language,"Пароль должен содержать от 10 до 128 символов.","Password must contain 10–128 characters.","Пароль має містити від 10 до 128 символів.");
   return null;
  }
  public static string Translate(string message,string language){
   switch(message){
    case "Username: 3–24 letters, numbers, _ or -":return Validate("","1234567890",language);
    case "Password: 10–128 characters":return Validate("valid_user","",language);
    case "Username is already taken":return L(language,"Этот логин уже занят. Выбери другой или нажми «Войти».","This username is taken. Choose another or select Sign in.","Цей логін уже зайнято. Вибери інший або натисни «Увійти».");
    case "Invalid username or password":return L(language,"Неверный логин или пароль.","Invalid username or password.","Неправильний логін або пароль.");
    case "Too many attempts. Please try later.":return L(language,"Слишком много попыток. Попробуй позже.",message,"Забагато спроб. Спробуй пізніше.");
    case "Invalid account request":return L(language,"Сервер не смог прочитать данные аккаунта. Повтори попытку.","The server could not read the account request. Please try again.","Сервер не зміг прочитати дані акаунта. Повтори спробу.");
    case "Session expired. Sign in again.":return L(language,"Сессия истекла. Войди заново.",message,"Сесія завершилася. Увійди знову.");
    case "Confirm your email address first":return L(language,"Сначала подтверди email по ссылке в письме, затем нажми «Войти».","Confirm your email using the link in the message, then select Sign in.","Спочатку підтвердь email за посиланням у листі, потім натисни «Увійти».");
    case "Verify your email in GitHub first":return L(language,"Сначала подтверди свой email в настройках GitHub.","Verify your email in GitHub settings first.","Спочатку підтвердь свій email у налаштуваннях GitHub.");
    case "Login flow expired":return L(language,"Код GitHub истёк. Начни вход заново.","GitHub code expired. Start sign-in again.","Код GitHub завершився. Почни вхід знову.");
    case "Only the owner can delete this pack":return L(language,"Удалить пак может только его автор.","Only the pack owner can delete it.","Видалити пак може лише його автор.");
    case "GitHub sign-in is not configured":return L(language,"GitHub ещё не настроен владельцем сервера.","GitHub is not configured on this server yet.","GitHub ще не налаштовано власником сервера.");
    case "Email sign-in is not configured":return L(language,"Email-вход ещё ожидает настройки Firebase.","Email sign-in is awaiting Firebase setup.","Email-вхід ще очікує налаштування Firebase.");
    case "Use GitHub or a verified email to publish":return L(language,"Для новых публикаций войди через GitHub или подтверждённый email. Старый вход остаётся для прежних паков.","Sign in with GitHub or verified email to publish. Legacy sign-in retains access to existing packs.","Для нових публікацій увійди через GitHub або підтверджений email. Старий вхід зберігає доступ до попередніх паків.");
    case "Use GitHub or email registration":return L(language,"Регистрация доступна через GitHub или email.","Register with GitHub or email.","Реєстрація доступна через GitHub або email.");
    default:return message;
   }
  }
  public static string ReadError(WebException error,string language){
   string fallback=L(language,"Не удалось связаться с воркшопом. Проверь интернет и адрес сервера.","Could not reach the workshop. Check your connection and server address.","Не вдалося зв’язатися з воркшопом. Перевір інтернет та адресу сервера.");
   try{if(error.Response!=null)using(var stream=error.Response.GetResponseStream())using(var reader=new StreamReader(stream,Encoding.UTF8)){string text=reader.ReadToEnd();if(text.Length<=8192){var data=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(text);object value;if(data!=null&&data.TryGetValue("error",out value)&&value is string)return Translate((string)value,language);}}}catch{}
   return fallback;
  }
 }
}
