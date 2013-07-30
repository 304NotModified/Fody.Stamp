using System;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Mono.Cecil;

public class FormatStringTokenResolver
{
    static Regex reEnvironmentToken = new Regex(@"%env\[([^\]]+)]%");

    public string ReplaceTokens(string template, ModuleDefinition moduleDefinition, Repository repo)
    {
        var assemblyVersion = moduleDefinition.Assembly.Name.Version;
        var branch = repo.Head;

        template = template.Replace("%version%", assemblyVersion.ToString());
        template = template.Replace("%version1%", assemblyVersion.ToString(1));
        template = template.Replace("%version2%", assemblyVersion.ToString(2));
        template = template.Replace("%version3%", assemblyVersion.ToString(3));
        template = template.Replace("%version4%", assemblyVersion.ToString(4));

        template = template.Replace("%githash%", branch.Tip.Sha);
        
        template = template.Replace("%branch%", repo.Head.Name);
        
        template = template.Replace("%haschanges%", repo.IsClean() ? "" : "HasChanges");

        template = template.Replace("%user%", FormatUserName());
        template = template.Replace("%machine%", Environment.MachineName);

        template = reEnvironmentToken.Replace(template, FormatEnvironmentVariable);

        return template.Trim();
    }

    private string FormatUserName()
    {
        return string.IsNullOrWhiteSpace(Environment.UserDomainName)
                   ? Environment.UserName
                   : string.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName);
    }

    private string FormatEnvironmentVariable(Match match)
    {
        return Environment.GetEnvironmentVariable(match.Groups[1].Value);
    }
}