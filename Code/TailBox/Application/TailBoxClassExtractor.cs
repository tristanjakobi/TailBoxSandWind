using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sandbox.TailBox;

public static class TailBoxClassExtractor
{
	private static readonly Regex SafelistRegex = new(
		@"tailbox\s+safelist\s*:\s*(?<value>[^\r\n*<]+)",
		RegexOptions.IgnoreCase | RegexOptions.Compiled );

	private static readonly Regex ClassAttributeRegex = new(
		@"\bclass\s*=\s*([""'])(?<value>.*?)\1",
		RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled );

	private static readonly Regex StringLiteralRegex = new(
		@"([""'])(?<value>(?:\\.|(?!\1).)*)\1",
		RegexOptions.Singleline | RegexOptions.Compiled );

	public static IReadOnlyCollection<string> ExtractClassesFromFiles( IEnumerable<string> files )
	{
		throw new NotSupportedException( "TailBox file scanning is editor-only in s&box. Use ExtractClassesFromText or TailBoxGenerator.GenerateFromSources in runtime code." );
	}

	internal static IReadOnlyCollection<TailBoxClassOccurrence> ExtractClassOccurrencesFromFiles( IEnumerable<string> files )
	{
		throw new NotSupportedException( "TailBox file scanning is editor-only in s&box. Use ExtractClassOccurrencesFromSources instead." );
	}

	internal static IReadOnlyCollection<TailBoxClassOccurrence> ExtractClassOccurrencesFromSources( IEnumerable<TailBoxSourceText> sources )
	{
		var occurrences = new List<TailBoxClassOccurrence>();

		foreach ( var source in sources ?? Enumerable.Empty<TailBoxSourceText>() )
		{
			if ( string.IsNullOrWhiteSpace( source.Text ) )
				continue;

			foreach ( var className in ExtractClassesFromText( source.Text ) )
			{
				occurrences.Add( new TailBoxClassOccurrence( className, source.Path ) );
			}
		}

		return occurrences;
	}

	public static IReadOnlyCollection<string> ExtractClassesFromText( string text )
	{
		var classes = new SortedSet<string>( StringComparer.Ordinal );
		if ( string.IsNullOrWhiteSpace( text ) )
			return classes;

		foreach ( Match match in SafelistRegex.Matches( text ) )
		{
			AddTokens( classes, match.Groups["value"].Value );
		}

		foreach ( Match match in ClassAttributeRegex.Matches( text ) )
		{
			AddTokens( classes, match.Groups["value"].Value );
		}

		// Razor class attributes often contain nested C# string literals. Scanning
		// literals broadly lets the generator pick up conditional utility classes
		// while unsupported UI copy is filtered later by the utility compiler.
		foreach ( Match match in StringLiteralRegex.Matches( text ) )
		{
			AddTokens( classes, match.Groups["value"].Value );
		}

		foreach ( var literal in ExtractOverlappingStringLiterals( text ) )
		{
			AddTokens( classes, literal );
		}

		return classes;
	}

	private static IEnumerable<string> ExtractOverlappingStringLiterals( string text )
	{
		for ( var i = 0; i < text.Length; i++ )
		{
			var quote = text[i];
			if ( quote is not ('"' or '\'') )
				continue;

			var escaped = false;
			for ( var end = i + 1; end < text.Length; end++ )
			{
				var c = text[end];
				if ( escaped )
				{
					escaped = false;
					continue;
				}

				if ( c == '\\' )
				{
					escaped = true;
					continue;
				}

				if ( c == quote )
				{
					yield return text[(i + 1)..end];
					break;
				}
			}
		}
	}

	private static void AddTokens( ISet<string> classes, string value )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
			return;

		var tokens = value.Split( new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
		foreach ( var token in tokens )
		{
			var cleaned = CleanToken( token );
			if ( IsCandidateClass( cleaned ) )
			{
				classes.Add( cleaned );
			}
		}
	}

	private static string CleanToken( string token )
	{
		return token
			.Trim()
			.Trim( '"', '\'', '`', ',', ';', '<', '>', '(', ')', '{', '}' );
	}

	private static bool IsCandidateClass( string token )
	{
		if ( string.IsNullOrWhiteSpace( token ) )
			return false;

		if ( token.StartsWith( "@", StringComparison.Ordinal ) )
			return false;

		if ( token.StartsWith( "/", StringComparison.Ordinal ) )
			return false;

		if ( token.Contains( '=', StringComparison.Ordinal ) )
			return false;

		if ( token.Contains( '<', StringComparison.Ordinal ) || token.Contains( '>', StringComparison.Ordinal ) )
			return false;

		if ( !token.Any( c => char.IsLetterOrDigit( c ) || c == '[' ) )
			return false;

		return true;
	}
}

internal readonly record struct TailBoxClassOccurrence( string ClassName, string SourcePath );
