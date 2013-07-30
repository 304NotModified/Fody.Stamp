using System;
using LibGit2Sharp;
using Mono.Cecil;

public class FormatStringTokenResolver
{
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

        return template.Trim();
    }
}