using SFML.Graphics;
using SFML.Window;
using vm.lib;
using vm.lib.Interop;

namespace vm.std;

public static class Windowing
{

    public class CreateWindow : ICsProcedure
    {
        public static RenderWindow? Window { get; set; } = null;

        public static Dictionary<Keyboard.Key, bool> Keys { get; set; } =
            Enum.GetValues(typeof(Keyboard.Key))
            .Cast<Keyboard.Key>()
#pragma warning disable CS0618 // Have to do this horrible thing to stop multiple keys with same value being added
        .Where(x => x != Keyboard.Key.Dash && x != Keyboard.Key.BackSpace && x != Keyboard.Key.Return && x != Keyboard.Key.BackSlash && x != Keyboard.Key.SemiColon)
#pragma warning restore CS0618
        .Select(key => new { Key = key, Pressed = false })
            .ToDictionary(x => x.Key, x => x.Pressed);

        public int ParamCount => 2;

        public bool ReturnsValue => false;

        public void Run(ref CsProcedureContext ctx)
        {
            var x = (uint)ctx.Params[0].ToI32();
            var y = (uint)ctx.Params[1].ToI32();

            var window = new RenderWindow(new VideoMode(x, y), string.Empty, Styles.Default, new ContextSettings(24, 8, 4));
            Window = window;
            window.Closed += (sender, args) =>
            {
                window.Close();
            };
            window.KeyPressed += (sender, args) =>
            {
                Keys[args.Code] = true;
            };
            window.KeyReleased += (sender, args) =>
            {
                Keys[args.Code] = false;
            };
        }
    }

    public class DrawCircle : ICsProcedure
    {
        public int ParamCount => 6;

        public bool ReturnsValue => false;

        public void Run(ref CsProcedureContext ctx)
        {
            var x = (float)ctx.Params[0].ToF64();
            var y = (float)ctx.Params[1].ToF64();

            var radius = (float)ctx.Params[2].ToF64();

            var red = (byte)ctx.Params[3].ToI64();
            var green = (byte)ctx.Params[4].ToI64();
            var blue = (byte)ctx.Params[5].ToI64();

            var circle = new CircleShape
            {
                Position = new SFML.System.Vector2f(x, y),
                Radius = radius,
                FillColor = new Color(red, green, blue)
            };
            if (CreateWindow.Window is null) throw new CsProcedureException("Window has not been opened.", this);
            CreateWindow.Window.Draw(circle);
        }
    }

    public class DrawRectangle : ICsProcedure
    {
        public int ParamCount => 7;

        public bool ReturnsValue => false;

        public void Run(ref CsProcedureContext ctx)
        {
            var x = (float)ctx.Params[0].ToF64();
            var y = (float)ctx.Params[1].ToF64();

            var width = (float)ctx.Params[2].ToF64();
            var height = (float)ctx.Params[3].ToF64();

            var red = (byte)ctx.Params[4].ToI64();
            var green = (byte)ctx.Params[5].ToI64();
            var blue = (byte)ctx.Params[6].ToI64();

            var rect = new RectangleShape
            {
                Position = new SFML.System.Vector2f(x, y),
                Size = new SFML.System.Vector2f(width, height),
                FillColor = new Color(red, green, blue)
            };
            if (CreateWindow.Window is null) throw new CsProcedureException("Window has not been opened.", this);
            CreateWindow.Window.Draw(rect);
        }
    }

    public class IsOpen : ICsProcedure
    {
        public int ParamCount => 0;

        public bool ReturnsValue => true;

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

        public bool ReturnsValue => false;

        public void Run(ref CsProcedureContext ctx)
        {
            if (CreateWindow.Window is null) throw new CsProcedureException("Window has not been opened.", this);
            CreateWindow.Window.Clear();
        }
    }

    public class Display : ICsProcedure
    {
        public int ParamCount => 0;

        public bool ReturnsValue => false;

        public void Run(ref CsProcedureContext ctx)
        {
            if (CreateWindow.Window is null) throw new CsProcedureException("Window has not been opened.", this);
            CreateWindow.Window.Display();
        }
    }

    public class DispatchEvents : ICsProcedure
    {
        public int ParamCount => 0;

        public bool ReturnsValue => false;

        public void Run(ref CsProcedureContext ctx)
        {
            if (CreateWindow.Window is null) throw new CsProcedureException("Window has not been opened.", this);
            CreateWindow.Window.DispatchEvents();
        }
    }

    public class IsKeyPressed : ICsProcedure
    {
        public int ParamCount => 1;

        public bool ReturnsValue => true;

        public void Run(ref CsProcedureContext ctx)
        {
            var code = ctx.Params[0].ToI64();
            var returnValue = CreateWindow.Keys[(Keyboard.Key)code];
            ctx.Return(Word.FromBool(returnValue));
        }
    }
}