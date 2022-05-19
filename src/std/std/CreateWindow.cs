using SFML.Graphics;
using SFML.Window;
using vm.lib;
using vm.lib.Interop;

namespace vm.std.Windowing;

public class CreateWindow : ICsProcedure
{
    public static RenderWindow? Window { get; set; }

    public int ParamCount => 2;

    public void Run(ref CsProcedureContext ctx)
    {
        var x = (uint)ctx.Params[0].ToI32();
        var y = (uint)ctx.Params[1].ToI32();

        var window = new RenderWindow(new VideoMode(x, y), string.Empty);
        Window = window;
        window.Closed += (sender, args) =>
        {
            window.Close();
        };
    }
}

public class DrawCircle : ICsProcedure
{
    public int ParamCount => 6;

    public void Run(ref CsProcedureContext ctx)
    {
        var x = (float)ctx.Params[0].ToF64();
        var y = (float)ctx.Params[1].ToF64();

        var radius = (float)ctx.Params[2].ToF64();

        var red = (byte)ctx.Params[3].ToI64();
        var green = (byte)ctx.Params[4].ToI64();
        var blue = (byte)ctx.Params[5].ToI64();

        var circle = new CircleShape(radius);
        circle.Position = new SFML.System.Vector2f(x, y);
        circle.FillColor = new Color(red, green, blue);
        if (CreateWindow.Window is null) throw new CsProcedureException("Window has not been opened.", this);
        CreateWindow.Window.Draw(circle);
    }
}

public class IsOpen : ICsProcedure
{
    public int ParamCount => 0;

    public void Run(ref CsProcedureContext ctx)
    {
        if (CreateWindow.Window is null) throw new CsProcedureException("Window has not been opened.", this);
        var returnValue = CreateWindow.Window.IsOpen;
        ctx.Return(Word.FromBool(returnValue));
    }
}

public class Clear : ICsProcedure
{
    public int ParamCount => 0;

    public void Run(ref CsProcedureContext ctx)
    {
        if (CreateWindow.Window is null) throw new CsProcedureException("Window has not been opened.", this);
        CreateWindow.Window.Clear();
    }
}

public class Display : ICsProcedure
{
    public int ParamCount => 0;

    public void Run(ref CsProcedureContext ctx)
    {
        if (CreateWindow.Window is null) throw new CsProcedureException("Window has not been opened.", this);
        CreateWindow.Window.Display();
    }
}