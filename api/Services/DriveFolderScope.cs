namespace api.Services;

public static class DriveFolderScope
{
    public static bool IsInsideRoot(
        string fileId,
        string rootFolderId,
        IReadOnlyDictionary<string, IReadOnlyList<string>> parentsById)
    {
        if (string.Equals(fileId, rootFolderId, StringComparison.Ordinal))
        {
            return true;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        queue.Enqueue(fileId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!seen.Add(current))
            {
                continue;
            }

            if (!parentsById.TryGetValue(current, out var parents) || parents.Count == 0)
            {
                continue;
            }

            foreach (var parentId in parents)
            {
                if (string.Equals(parentId, rootFolderId, StringComparison.Ordinal))
                {
                    return true;
                }

                queue.Enqueue(parentId);
            }
        }

        return false;
    }
}
