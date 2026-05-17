using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox.TailBox;

internal static class TailBoxText
{
	public static List<string> Segment( string input, char separator )
	{
		var result = new List<string>();
		var start = 0;
		var square = 0;
		var round = 0;
		var curly = 0;
		var quote = '\0';
		var escaped = false;

		for ( var i = 0; i < input.Length; i++ )
		{
			var c = input[i];
			if ( escaped )
			{
				escaped = false;
				continue;
			}

			if ( quote != '\0' )
			{
				if ( c == '\\' )
				{
					escaped = true;
					continue;
				}

				if ( c == quote )
					quote = '\0';
				continue;
			}

			if ( c == separator && square == 0 && round == 0 && curly == 0 )
			{
				result.Add( input[start..i] );
				start = i + 1;
				continue;
			}

			if ( c == '\\' )
			{
				escaped = true;
				continue;
			}

			if ( c is '"' or '\'' )
			{
				quote = c;
				continue;
			}

			switch ( c )
			{
				case '[':
					square++;
					continue;
				case ']':
					if ( square > 0 ) square--;
					continue;
				case '(':
					round++;
					continue;
				case ')':
					if ( round > 0 ) round--;
					continue;
				case '{':
					curly++;
					continue;
				case '}':
					if ( curly > 0 ) curly--;
					continue;
			}

		}

		result.Add( input[start..] );
		return result;
	}

	public static string DecodeArbitraryValue( string input )
	{
		if ( string.IsNullOrEmpty( input ) )
			return input;

		var builder = new StringBuilder();
		var escaped = false;
		var functionStack = new List<FunctionFrame>();
		var currentIdentifier = new StringBuilder();

		for ( var i = 0; i < input.Length; i++ )
		{
			var c = input[i];
			if ( escaped )
			{
				builder.Append( c );
				escaped = false;
				currentIdentifier.Clear();
				continue;
			}

			if ( c == '\\' )
			{
				escaped = true;
				continue;
			}

			if ( char.IsLetterOrDigit( c ) || c == '-' )
			{
				currentIdentifier.Append( c );
			}
			else if ( c == '(' )
			{
				functionStack.Add( new FunctionFrame( currentIdentifier.ToString(), 0 ) );
				currentIdentifier.Clear();
			}
			else
			{
				currentIdentifier.Clear();
				if ( c == ')' && functionStack.Count > 0 )
					functionStack.RemoveAt( functionStack.Count - 1 );
				else if ( c == ',' && functionStack.Count > 0 )
				{
					var frame = functionStack[^1];
					functionStack[^1] = frame with { ArgumentIndex = frame.ArgumentIndex + 1 };
				}
			}

			if ( c == '_' )
			{
				var frame = functionStack.Count > 0 ? functionStack[^1] : default;
				builder.Append( ShouldPreserveUnderscore( frame ) ? '_' : ' ' );
				currentIdentifier.Clear();
				continue;
			}

			builder.Append( c );
		}

		if ( escaped )
			builder.Append( '\\' );

		return builder.ToString();
	}

	private static bool ShouldPreserveUnderscore( FunctionFrame frame )
	{
		if ( string.IsNullOrEmpty( frame.Name ) )
			return false;

		if ( frame.Name.Equals( "url", StringComparison.OrdinalIgnoreCase ) || frame.Name.EndsWith( "_url", StringComparison.OrdinalIgnoreCase ) )
			return true;

		if ( frame.ArgumentIndex == 0
			&& (frame.Name.Equals( "var", StringComparison.OrdinalIgnoreCase )
				|| frame.Name.EndsWith( "_var", StringComparison.OrdinalIgnoreCase )
				|| frame.Name.Equals( "theme", StringComparison.OrdinalIgnoreCase )
				|| frame.Name.EndsWith( "_theme", StringComparison.OrdinalIgnoreCase )) )
		{
			return true;
		}

		return false;
	}

	public static string EscapeIdentifier( string value )
	{
		if ( value is null )
			throw new ArgumentNullException( nameof( value ) );

		var length = value.Length;
		if ( length == 0 )
			return "";

		var result = new StringBuilder();
		var firstCodeUnit = value[0];
		if ( length == 1 && firstCodeUnit == '-' )
			return "\\" + value;

		for ( var index = 0; index < length; index++ )
		{
			var codeUnit = value[index];
			if ( codeUnit == 0 )
			{
				result.Append( '\uFFFD' );
				continue;
			}

			if ( (codeUnit >= 0x0001 && codeUnit <= 0x001f)
				|| codeUnit == 0x007f
				|| (index == 0 && codeUnit >= '0' && codeUnit <= '9')
				|| (index == 1 && codeUnit >= '0' && codeUnit <= '9' && firstCodeUnit == '-') )
			{
				result.Append( '\\' ).Append( ((int)codeUnit).ToString( "x" ) ).Append( ' ' );
				continue;
			}

			if ( codeUnit >= 0x0080
				|| codeUnit == '-'
				|| codeUnit == '_'
				|| (codeUnit >= '0' && codeUnit <= '9')
				|| (codeUnit >= 'A' && codeUnit <= 'Z')
				|| (codeUnit >= 'a' && codeUnit <= 'z') )
			{
				result.Append( codeUnit );
				continue;
			}

			result.Append( '\\' ).Append( codeUnit );
		}

		return result.ToString();
	}

	public static string EscapeIdentifierForSboxSelector( string value )
	{
		if ( value is null )
			throw new ArgumentNullException( nameof( value ) );

		if ( value.Length == 0 )
			return "";

		var result = new StringBuilder();
		for ( var index = 0; index < value.Length; index++ )
		{
			var codeUnit = value[index];
			if ( IsIdentifierSafe( codeUnit, index, value ) )
			{
				result.Append( codeUnit );
				continue;
			}

			result.Append( '\\' ).Append( ((int)codeUnit).ToString( "x6" ) ).Append( ' ' );
		}

		return result.ToString();
	}

	private static bool IsIdentifierSafe( char codeUnit, int index, string value )
	{
		if ( codeUnit >= 0x0080 || codeUnit == '_' )
			return true;

		if ( codeUnit == '-' )
			return index > 0;

		if ( index == 1 && value[0] == '-' && codeUnit >= '0' && codeUnit <= '9' )
			return false;

		if ( codeUnit >= '0' && codeUnit <= '9' )
			return index > 0;

		if ( codeUnit >= 'A' && codeUnit <= 'Z' )
			return true;

		if ( codeUnit >= 'a' && codeUnit <= 'z' )
			return true;

		return false;
	}

	private readonly record struct FunctionFrame( string Name, int ArgumentIndex );
}
