using System.Collections.ObjectModel;

namespace QGen.Lib.FileSystem;

/// <summary>
/// Represents a directory which gets dynamically enumerated upon request.
/// </summary>
public class ParsedDirectory {

    /// <summary>
    /// The path to the directory.
    /// </summary>
    public readonly DirectoryInfo Path;

    /// <summary>
    /// Gets the files within the folder.
    /// </summary>
    /// <value>
    /// The files.
    /// </value>
    public ReadOnlyCollection<ParsedFile> Files => _Files ??= Path.GetFiles().CastArray<ParsedFile>().ToReadOnly();
    ReadOnlyCollection<ParsedFile>? _Files = null;

    ///// <summary>
    ///// Gets the files within the folder.
    ///// </summary>
    ///// <value>
    ///// The files.
    ///// </value>
    //public ReadOnlyCollection<ParsedDirectory> Folders => _Folders ??= Path.GetFiles().ToReadOnly();
    //ReadOnlyCollection<ParsedDirectory>? _Folders = null;

    //readonly Lazy<ReadOnlyCollection<DirectoryInfo>> _Folders;

}
