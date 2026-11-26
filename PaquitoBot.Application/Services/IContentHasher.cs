namespace PaquitoBot.Application.Services;

public interface IContentHasher
{
    string Sha256(string content);
}