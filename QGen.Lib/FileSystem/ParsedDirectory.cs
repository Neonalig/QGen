using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace QGen.Lib.FileSystem;

/// <summary>
/// Represents a directory which gets dynamically enumerated upon request.
/// </summary>
public class ParsedDirectory : IReadOnlyList<ParsedFile>, IReadOnlyList<ParsedDirectory> {

    /// <summary>
    /// The path to the directory.
    /// </summary>
    public readonly DirectoryInfo Path;

    /// <summary>
    /// Initialises a new instance of the <see cref="ParsedDirectory"/> class.
    /// </summary>
    /// <param name="Path">The path to the directory.</param>
    public ParsedDirectory( DirectoryInfo Path ) => this.Path = Path;

    /// <summary>
    /// Gets the files within the folder.
    /// </summary>
    /// <value>
    /// The files.
    /// </value>
    public ReadOnlyCollection<ParsedFile> Files => _Files ??= Path.GetFiles().Cast(FI => new ParsedFile(FI, this)).ToReadOnly();
    ReadOnlyCollection<ParsedFile>? _Files = null;

    /// <summary>
    /// Gets the subfolders within the folder.
    /// </summary>
    /// <value>
    /// The subfolders.
    /// </value>
    public ReadOnlyCollection<ParsedDirectory> Folders => _Folders ??= Path.GetDirectories().Cast(DI => new ParsedDirectory(DI)).ToReadOnly();
    ReadOnlyCollection<ParsedDirectory>? _Folders = null;

    #region Implementation of IEnumerable

    /// <inheritdoc />
    IEnumerator<ParsedDirectory> IEnumerable<ParsedDirectory>.GetEnumerator() => Folders.GetEnumerator();

    /// <inheritdoc />
    public IEnumerator<ParsedFile> GetEnumerator() => Files.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() {
        //return Folders.Join(Files).GetEnumerator();
        foreach ( ParsedDirectory Folder in Folders ) {
            yield return Folder;
        }
        foreach ( ParsedFile File in Files ) {
            yield return File;
        }
    }

    #endregion

    #region IReadOnlyList<ParsedFile> Implementation

    /// <summary>
    /// Gets the number of files in the folder.
    /// </summary>
    /// <value><see cref="Files"/>.<see cref="ReadOnlyCollection{T}.Count">Count</see></value>
    public int Count => Files.Count;

    /// <summary>
    /// Gets the <see cref="ParsedFile"/> at the specified index.
    /// </summary>
    /// <value>
    /// The <see cref="ParsedFile"/> at the given index.
    /// </value>
    /// <param name="Index">The index.</param>
    /// <returns><see cref="Files"/>.<see cref="IReadOnlyList{T}.this[int]">this[<see cref="int"/>]</see></returns>
    public ParsedFile this[int Index] => Files[Index];

    #endregion

    #region IReadOnlyList<ParsedDirectory> Implementation

    /// <summary>
    /// Gets the number of subfolders in the folder.
    /// </summary>
    /// <value><see cref="Folders"/>.<see cref="ReadOnlyCollection{T}.Count">Count</see></value>
    int IReadOnlyCollection<ParsedDirectory>.Count => Folders.Count;

    /// <summary>
    /// Gets the <see cref="ParsedDirectory"/> at the specified index.
    /// </summary>
    /// <value>
    /// The <see cref="ParsedDirectory"/> at the given index.
    /// </value>
    /// <param name="Index">The index.</param>
    /// <returns><see cref="Folders"/>.<see cref="IReadOnlyList{T}.this[int]">this[<see cref="int"/>]</see></returns>
    ParsedDirectory IReadOnlyList<ParsedDirectory>.this[int Index] => Folders[Index];

    #endregion

    static bool TryTread( ParsedDirectory Root, IEnumerable<string> Crumbs, [NotNullWhen(true)] out ParsedDirectory? Found ) {
        Found = Root;
        foreach ( string Crumb in Crumbs ) {
            if ( !Root.Folders.TryGetFirst(PD => PD.Path.Name.Equals(Crumb, StringComparison.CurrentCultureIgnoreCase), out Found) ) {
                Found = null;
                return false;
            }
        }
        return true;
    }

    static bool TryTread( ParsedDirectory Root, string Path, out ParsedFile? Found ) {
        string[] Crumbs = Path.Split('\\');
        int L = Crumbs.Length;
        Found = null;
        return TryTread(Root, Crumbs.Grab(L - 1), out ParsedDirectory? Parent)
               && Parent.Files.TryGetFirst(PF => PF.Path.Name.Equals(Crumbs[L - 1], StringComparison.CurrentCultureIgnoreCase), out Found);
    }

    /// <summary>
    /// Attempts to retrieve the subfolder with the given relative path.
    /// </summary>
    /// <param name="Path">The relative path. (i.e. "foo/bar", "baz")</param>
    /// <param name="Found">The found directory, or <see langword="null"/> if <see langword="false"/> (unsuccessful).</param>
    /// <returns><see langword="true"/> if the subfolder was successfully found; otherwise <see langword="false"/>.</returns>
    public bool TryGetFolder( string Path, [NotNullWhen(true)] out ParsedDirectory? Found ) {
        Path = Path.ToUpperInvariant().Replace('/', '\\').TrimEnd('\\');
        return Path.Contains('\\')
            ? TryTread(this, Path.Split('\\'), out Found)
            : Folders.TryGetFirst(PD => PD.Path.Name.Equals(Path, StringComparison.CurrentCultureIgnoreCase), out Found);
    }

    /// <summary>
    /// Gets the <see cref="ParsedFile"/> with the specified relative path.
    /// </summary>
    /// <value>
    /// The <see cref="ParsedFile"/> found with the specified relative path.
    /// </value>
    /// <param name="RelativePath">The relative path.</param>
    /// <returns>The found <see cref="ParsedFile"/>.</returns>
    /// <exception cref="FileNotFoundException">The relative path was invalid or the file could not be found.</exception>
    public ParsedFile this[ string RelativePath ] => TryGetFile(RelativePath, out ParsedFile? Found) ? Found : throw new FileNotFoundException($"The relative path ('{RelativePath}') was invalid or the file could not be found.");

    /// <summary>
    /// Attempts to retrieve the file with the given relative path.
    /// </summary>
    /// <param name="Path">The relative path. (i.e. "foo/bar/quz.mp3", "baz.txt")</param>
    /// <param name="Found">The found file, or <see langword="null"/> if <see langword="false"/> (unsuccessful).</param>
    /// <returns><see langword="true"/> if the file was successfully found; otherwise <see langword="false"/>.</returns>
    public bool TryGetFile( string Path, [NotNullWhen(true)] out ParsedFile? Found ) {
        Path = Path.ToUpperInvariant().Replace('/', '\\').TrimEnd('\\');
        return Path.Contains('\\')
            ? TryTread(this, Path, out Found)
            : Files.TryGetFirst(PF => PF.Path.Name.Equals(Path, StringComparison.CurrentCultureIgnoreCase), out Found);
    }

    /// <summary>
    /// Performs an <see langword="implicit"/> conversion from <see cref="DirectoryInfo"/> to <see cref="ParsedDirectory"/>.
    /// </summary>
    /// <param name="Path">The path to the directory.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator ParsedDirectory( DirectoryInfo Path ) => new ParsedDirectory(Path);

    /// <summary>
    /// Performs an explicit conversion from <see cref="ParsedDirectory"/> to <see cref="DirectoryInfo"/>.
    /// </summary>
    /// <param name="Dir">The parsed directory.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static explicit operator DirectoryInfo( ParsedDirectory Dir ) => Dir.Path;

    /// <inheritdoc />
    public override string ToString() => $"[{CtoStrRep()}]{{{Files.Count + Folders.Count}}}";

    string CtoStrRep() => this.CreateString(
        Obj => Obj switch {
            ParsedDirectory PD => $"📄：{PD.Path.Name}",
            ParsedFile PF      => $"📁：{PF.Path.Name}",
            _                  => "␀"
        });
}
