using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Utils
{
    internal static class VersionEx
    {
        /// <summary>
        /// Append ".0" in case of one number versions before parsing it.
        /// </summary>
        internal static bool TryParse(string? input, out Version? result)
        {
            if (input is not null && !input.Contains('.'))
                input += ".0";
            return Version.TryParse(input, out result);
        }
    }

    internal static class DirectoryEx
    {
        /// <summary>
        /// Delete target folder before moving.
        /// </summary>
        internal static void CleanMove(string source, string target)
        {
            if (Directory.Exists(target))
                Directory.Delete(target, true);
            Directory.Move(source, target);
        }

        /// <summary>
        /// Recursively copy a folder.
        /// </summary>
        internal static void Copy(string source, string target)
        {
            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);
            foreach (string file in Directory.EnumerateFiles(source))
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
            foreach (string folder in Directory.EnumerateDirectories(source))
                Copy(folder, Path.Combine(target, Path.GetFileName(folder)));
        }

        /// <summary>
        /// Delete target folder before recursively copying source into it.
        /// </summary>
        internal static void CleanCopy(string source, string target)
        {
            if (Directory.Exists(target))
                Directory.Delete(target, true);
            Copy(source, target);
        }

        /// <summary>
        /// Delete folder if it exists.
        /// </summary>
        internal static void EnsureDeleted(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        /// <summary>
        /// Delete folder if it exists. No Exceptions
        /// </summary>
        internal static bool TryDelete(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Find paths with a folder name.
        /// </summary>
        internal static IEnumerable<string> FindFolder(string path, string folderName)
        {
            List<string> result = new();
            Queue<string> queue = new(Directory.EnumerateDirectories(path));
            while (queue.Count > 0)
            {
                string folder = queue.Dequeue();
                if (Path.GetFileName(folder) == folderName)
                {
                    result.Add(folder);
                }
                else
                {
                    foreach (var add in Directory.EnumerateDirectories(folder))
                        queue.Enqueue(add);
                }
            }
            return result;
        }
    }

    internal static class CollectionExtension
    {
        internal static IEnumerable<TResult> SelectNoNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult?> selector) where TResult : class
        {
            return source.Select(selector).Where(x => x is not null).Select(x => x!);
        }
    }
}
