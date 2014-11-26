using System.IO;
using LibGit2Sharp;

public class GitDirFinder
{

    public static string TreeWalkForGitDir(string currentDirectory)
    {
        while (true)
        {
            var gitDir = Path.Combine(currentDirectory, @".git");
            if (Directory.Exists(gitDir)) 
            {
                return gitDir;
            } else if ( File.Exists(gitDir) ) {
                using (var repo = ModuleWeaver.GetRepo(gitDir)) {
                    if (repo.Head.Tip != null) {
                        return gitDir;
                    }
                }
            }


            try
            {
                var parent = Directory.GetParent(currentDirectory);
                if (parent == null)
                {
                    break;
                }
                currentDirectory = parent.FullName;
            }
            catch
            {
                // trouble with tree walk.
                return null;
            }
        }
        return null;
    }
}