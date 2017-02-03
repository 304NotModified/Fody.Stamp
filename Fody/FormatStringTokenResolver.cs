using System;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Mono.Cecil;

public class FormatStringTokenResolver
{
    static Regex reEnvironmentToken = new Regex(@"%env\[([^\]]+)]%");
    static Regex reNow = new Regex(@"%now:([^%]+)%");
    static Regex reUtcNow = new Regex(@"%utcnow:([^%]+)%");

    static DateTime now = DateTime.Now;
    static DateTime utcNow = DateTime.UtcNow;

    public string ReplaceTokens(string template, ModuleDefinition moduleDefinition, Repository repo, string changestring)
    {
        var assemblyVersion = moduleDefinition.Assembly.Name.Version;
        var branch = repo.Head;

        template = template.Replace("%version%", assemblyVersion.ToString());
        template = template.Replace("%version1%", assemblyVersion.ToString(1));
        template = template.Replace("%version2%", assemblyVersion.ToString(2));
        template = template.Replace("%version3%", assemblyVersion.ToString(3));
        template = template.Replace("%version4%", assemblyVersion.ToString(4));

        template = template.Replace("%now%", now.ToShortDateString());
        template = template.Replace("%utcnow%", utcNow.ToShortDateString());

        template = template.Replace("%githash%", branch.Tip.Sha);
        template = template.Replace("%shorthash%", branch.Tip.Sha.Substring(0, 8));
        template = template.Replace("%branch%", branch.FriendlyName);
        template = template.Replace("%haschanges%", repo.IsClean() ? "" : changestring);

        template = template.Replace("%user%", FormatUserName());
        template = template.Replace("%machine%", Environment.MachineName);

        template = template.Replace("%version_tag%", repo.FindVersionTag());

        template = reEnvironmentToken.Replace(template, FormatEnvironmentVariable);
        template = reNow.Replace(template, FormatTime);
        template = reUtcNow.Replace(template, FormatUtcTime);

        return template.Trim();
    }

    string FormatUserName()
    {
        return string.IsNullOrWhiteSpace(Environment.UserDomainName)
                   ? Environment.UserName
                   : $@"{Environment.UserDomainName}\{Environment.UserName}";
    }

    string FormatEnvironmentVariable(Match match)
    {
        return Environment.GetEnvironmentVariable(match.Groups[1].Value);
    }

    string FormatTime(Match match)
    {
        return now.ToString(match.Groups[1].Value);
    }

    string FormatUtcTime(Match match)
    {
        return utcNow.ToString(match.Groups[1].Value);
    }
}