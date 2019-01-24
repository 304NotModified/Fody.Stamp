using System.Linq;
using LibGit2Sharp;

namespace Stamp.Fody.Internal
{
    internal static class LibGitEx
    {
        public static bool IsClean(this Repository repository)
        {
            var repositoryStatus = repository.RetrieveStatus();
            return 
                repositoryStatus.Added.IsEmpty() &&
                repositoryStatus.Missing.IsEmpty() &&
                repositoryStatus.Modified.IsEmpty() &&
                repositoryStatus.Removed.IsEmpty() &&
                repositoryStatus.Staged.IsEmpty();	
        }

        internal static string FindVersionTag(this Repository repository) {
            var tagMap = repository.Tags
                .Where(t => t.Target is Commit)
                .ToDictionary(t => (Commit) t.Target);

            return repository.Commits.QueryBy(new CommitFilter {
                    SortBy = CommitSortStrategies.Topological
                })
                .Where(c => tagMap.ContainsKey(c))
                .Select(c => tagMap[c].FriendlyName.Trim())
                .FirstOrDefault();
        }
    }
}