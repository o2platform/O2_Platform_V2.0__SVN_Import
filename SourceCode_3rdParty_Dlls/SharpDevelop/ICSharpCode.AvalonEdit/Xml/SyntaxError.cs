﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="David Srbecký" email="dsrbecky@gmail.com"/>
//     <version>$Revision: 4675 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

using ICSharpCode.AvalonEdit.Document;

namespace ICSharpCode.AvalonEdit.Xml
{
	/// <summary> Information about syntax error that occured during parsing </summary>
	public class SyntaxError: TextSegment
	{
		/// <summary> Object for which the error occured </summary>
		public AXmlObject Object { get; internal set; }
		/// <summary> Textual description of the error </summary>
		public string Message { get; internal set; }
		/// <summary> Any user data </summary>
		public object Tag { get; set; }
		
		internal SyntaxError Clone(AXmlObject newOwner)
		{
			return new SyntaxError {
				Object = newOwner,
				Message = Message,
				Tag = Tag,
				StartOffset = StartOffset,
				EndOffset = EndOffset,
			};
		}
	}
}
