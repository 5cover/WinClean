using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;

namespace Scover.WinClean.Model.Scripts;

public abstract class MutableScriptRepository : ScriptRepository
{
    private bool _updatesEnabled = true;

    protected MutableScriptRepository(IScriptSerializer serializer, ScriptType type) : base(serializer, type)
        => Scripts.CollectionChanged += (_, e) =>
        {
            if (!_updatesEnabled)
            {
                return;
            }

            if (e.OldItems is not null)
            {
                foreach (Script s in e.OldItems)
                {
                    _ = Remove(s);
                }
            }
            if (e.NewItems is not null)
            {
                foreach (Script s in e.NewItems)
                {
                    Add(s);
                }
            }
        };

    /// <summary>Updates a script in a repository.</summary>
    /// <param name="script">The script to update.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="script"/> is not present in the repository.
    /// </exception>
    /// <exception cref="FileSystemException">A filesystem exception occured.</exception>
    public abstract void Commit(Script script);

    /// <summary>Adds a script to the repository.</summary>
    /// <exception cref="FileSystemException">A filesystem exception occured.</exception>
    /// <exception cref="ScriptAlreadyExistsException">
    /// The script already exists in the repository.
    /// </exception>
    protected abstract void Add(Script script);

    /// <summary>
    /// Adds an item to the script collection without triggering the CollectionChanged event handler.
    /// </summary>
    protected void AddItemInternally(Script script)
    {
        _updatesEnabled = false;
        Scripts.Add(script);
        _updatesEnabled = true;
    }

    /// <summary>Removes a script from a repository.</summary>
    /// <param name="script">The script to remove.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="script"/> successfully removed otherwise, <see
    /// langword="false"/>. This method also returns <see langword="false"/> if <paramref name="script"/>
    /// was not found in the repository.
    /// </returns>
    protected abstract bool Remove(Script script);
}