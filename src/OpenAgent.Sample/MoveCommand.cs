namespace OpenAgent.Sample;

public static class MoveCommand
{
    public static (int x, int y)? Parse(string[] args)
    {
        if (args.Length != 2)
            return null;


        if (!int.TryParse(args[0], out int x))
            return null;


        if (!int.TryParse(args[1], out int y))
            return null;


        return (x, y);
    }
}