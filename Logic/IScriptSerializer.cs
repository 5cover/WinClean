namespace Scover.WinClean.Logic;

public interface IScriptSerializer
{
    Script Deserialize(FileInfo source);

    void Serialize(Script s);
}