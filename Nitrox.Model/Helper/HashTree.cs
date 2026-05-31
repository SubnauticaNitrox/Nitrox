using System.Collections.Generic;

namespace Nitrox.Model.Helper;

public sealed class HashTree
{
    private readonly Dictionary<object, HashTree> data = new();

    public HashTree AddMoveNext(object node)
    {
        if (!data.TryGetValue(node, out HashTree next))
        {
            next = new();
            data.Add(node, next);
        }
        return next;
    }

    /// <summary>
    ///     Returns true if the given tree is entirely within the current tree.
    /// </summary>
    public bool Contains(HashTree tree, HashTree? self = null)
    {
        self ??= this;
        foreach (KeyValuePair<object, HashTree> pair in tree.data)
        {
            if (!self.data.TryGetValue(pair.Key, out HashTree next))
            {
                return false;
            }
            return Contains(pair.Value, next);
        }
        return true;
    }

    /// <summary>
    ///     Adds the branch to the tree if it doesn't already exist in its entirety. Returns true if
    ///     <see cref="branch" /> is empty.
    /// </summary>
    public bool TryAdd(HashTree branch)
    {
        if (branch.data.Count < 1)
        {
            // Nothing to do here.
            return true;
        }
        if (Contains(branch))
        {
            return false;
        }
        AddRecursive(branch, this);
        return true;

        void AddRecursive(HashTree source, HashTree destination)
        {
            foreach (KeyValuePair<object, HashTree> pair in source.data)
            {
                if (!destination.data.TryGetValue(pair.Key, out HashTree destSubTree))
                {
                    destSubTree = new();
                    destination.data.Add(pair.Key, destSubTree);
                }
                destSubTree.data[pair.Key] = pair.Value;
                AddRecursive(pair.Value, destSubTree);
            }
        }
    }
}
