// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using O2.Interfaces.O2Core;
using O2.Kernel;
using O2.Kernel.ExtensionMethods;
using O2.DotNetWrappers.ExtensionMethods;

//O2File:C:\O2\_XRules_Local\WebAutomation\O2MediaWikiAPI.cs
//O2File:C:\O2\_XRules_Local\ExtensionMethods\extra_WebAutomation.cs
//O2File:C:\O2\_XRules_Local\ExtensionMethods\extra_VisualizationAPI.cs

namespace O2.Script
{

	public class O2PlatformWikiApi : O2MediaWikiAPI
	{

		public O2PlatformWikiApi() 
		{
			init("http://www.o2platform.com/api.php");
			this.Styles = styles();
		}
		
		// dynamically (one per session) grab the current header scripts used in OWASP
		public string styles()
		{
			var wikiStyles = "<link rel=\"stylesheet\" href=\"http://www.o2platform.com/skins/common/shared.css?207\" type=\"text/css\" media=\"screen\" />".line() +
			     		     "<link rel=\"stylesheet\" href=\"http://www.o2platform.com/skins/common/commonPrint.css?207\" type=\"text/css\" media=\"print\" /> ".line() +
				 		     "<link rel=\"stylesheet\" href=\"http://www.o2platform.com/skins/gumax/gumax_main.css?207\" type=\"text/css\" media=\"screen\" /> ".line();
			return wikiStyles;
		}
    }
}