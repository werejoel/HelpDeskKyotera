namespace HelpDeskKyotera.Services;

public interface ISmsSender
{
    Task SendSmsAsync(string to, string message);
}
