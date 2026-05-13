using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Sandbox.TailBox;

internal sealed class TailBoxGlobMatcher
{
	private readonly Regex regex;

	private TailBoxGlobMatcher( Regex regex )
	{
		this.regex = regex;
	}

	public static TailBoxGlobMatcher FromGlob( string glob )
	{
		glob = string.IsNullOrWhiteSpace( glob ) ? "**/*.razor" : glob.Replace( '\\', '/' ).TrimStart( '/' );
		var builder = new StringBuilder( "^" );

		for ( var i = 0; i < glob.Length; i++ )
		{
			var c = glob[i];
			if ( c == '*' )
			{
				var isDouble = i + 1 < glob.Length && glob[i + 1] == '*';
				if ( isDouble )
				{
					var followedBySlash = i + 2 < glob.Length && glob[i + 2] == '/';
					builder.Append( followedBySlash ? "(?:.*/)?" : ".*" );
					i += followedBySlash ? 2 : 1;
				}
				else
				{
					builder.Append( "[^/]*" );
				}
				continue;
			}

			if ( c == '?' )
			{
				builder.Append( "[^/]" );
				continue;
			}

			builder.Append( Regex.Escape( c.ToString() ) );
		}

		builder.Append( "$" );
		return new TailBoxGlobMatcher( new Regex( builder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled ) );
	}

	public bool IsMatch( string relativePath )
	{
		return regex.IsMatch( relativePath.Replace( '\\', '/' ) );
	}
}
