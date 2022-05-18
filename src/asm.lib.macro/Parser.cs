using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;
using System.Text;

namespace asm.lib.macro;

public static class Parser
{
    public static string Process(string text)
    {
        var preprocessor = new Preprocessor();
        var tokens = Tokenizer.Tokenize(text);
        while (tokens.Count() > 0)
        {
            var count = tokens.Count();
            switch (tokens.First().Kind)
            {
                case TokenType.Text:
                    preprocessor.PushText(tokens.ConsumeToken().Value.ToStringValue());
                    break;
                case TokenType.LeftHashBracket:
                    tokens.ConsumeToken(); // skip #[
                    preprocessor.PushMacroInit(tokens.ConsumeToken().Value.ToStringValue());
                    while (tokens.First().Kind != TokenType.RightBracket) // until ]
                    {
                        if (tokens.First().Kind == TokenType.LeftHashParen)
                        {
                            tokens.ConsumeToken(); // skip #(
                            preprocessor.PushMacroArgLocal(tokens.ConsumeToken().Value.ToStringValue());
                            tokens.ConsumeToken(); // skip )
                        }
                        else
                        {
                            while (tokens.First().Kind != TokenType.Comma) // stuff between commas
                            {
                                preprocessor.PushMacroArg(tokens.ConsumeToken().Value.ToStringValue());
                            }
                            tokens.ConsumeToken(); // skip ,
                        }
                    }
                    break;
                case TokenType.RightBracket:
                    preprocessor.PushMacroInvoke();
                    break;

                case TokenType.Macro:
                    tokens.ConsumeToken(); // skip [macro]
                    preprocessor.Macro(tokens.ConsumeToken().Value.ToStringValue()); // name
                    while (tokens.First().Kind == TokenType.Text) // args after name
                    {
                        preprocessor.MacroArg(tokens.ConsumeToken().Value.ToStringValue());
                    }
                    break;
                case TokenType.Begin:
                    tokens.ConsumeToken(); // skip [begin]
                    while (tokens.First().Kind != TokenType.EndMacro) // until [endmacro]
                    {
                        if (tokens.First().Kind == TokenType.LeftHashParen) // #(
                        {
                            tokens.ConsumeToken(); // skip #(
                            preprocessor.MacroPushArg(tokens.ConsumeToken().Value.ToString()); // inside parens
                            tokens.ConsumeToken(); // skip )
                        }
                        else if (tokens.First().Kind == TokenType.Varargs)
                        {
                            tokens.ConsumeToken(); // skip [varargs]
                            preprocessor.MacroPushVarargs();
                        }
                        else
                        {
                            preprocessor.MacroPushText(tokens.ConsumeToken().Value.ToStringValue());
                        }
                    }
                    break;
                case TokenType.EndMacro:
                    tokens.ConsumeToken(); // skip [endmacro]
                    preprocessor.MacroEnd();
                    break;
                case TokenType.Store:
                    tokens.ConsumeToken(); // skip [store]
                    preprocessor.SetLocal(tokens.ConsumeToken().Value.ToStringValue()); // name
                    break;
            }
        }
        return preprocessor.Program();
    }

    public static Tokenizer<TokenType> Tokenizer = new TokenizerBuilder<TokenType>()
        .Ignore(Span.EqualTo(";;").IgnoreThen(Character.ExceptIn('\n').IgnoreMany()))
        .Ignore(Span.WhiteSpace)
        .Match(Span.EqualTo("[macro]"), TokenType.Macro)
        .Match(Span.EqualTo("[begin]"), TokenType.Begin)
        .Match(Span.EqualTo("[endmacro]"), TokenType.EndMacro)
        .Match(Span.EqualTo("[varargs]"), TokenType.Varargs)
        .Match(Span.EqualTo("[store]"), TokenType.Store)
        .Match(Span.EqualTo("#["), TokenType.LeftHashBracket)
        .Match(Character.EqualTo(']'), TokenType.RightBracket)
        .Match(Span.EqualTo("#("), TokenType.LeftHashParen)
        .Match(Character.EqualTo(')'), TokenType.RightParen)
        .Match(Character.AnyChar.Many(), TokenType.Text)
        .Build();
}

public enum TokenType
{
    Text,
    LeftHashBracket,
    LeftBracket,
    RightBracket,
    Comma,
    LeftHashParen,
    RightParen,

    Macro,
    Begin,
    EndMacro,
    Varargs,
    Store,
}