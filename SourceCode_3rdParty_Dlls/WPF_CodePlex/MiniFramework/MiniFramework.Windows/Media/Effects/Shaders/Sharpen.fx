// <copyright file="Sharpen.fx" company="C�dric Belin">
// 	Copyright (c) 2007-2009, C�dric Belin &lt;contact@cedric-belin.fr&gt;
// 	GNU Lesser General Public License (LGPLv3) - http://www.gnu.org/licenses/lgpl-3.0.txt
// </copyright>
// <summary>
// 	Shader de la classe <c>MiniFramework.Windows.Media.Effects.SharpenEffect</c>.
// </summary>
// <author>$Author: cedx $</author>
// <date>$Date: 2009-09-10 19:12:59 +0200 (jeu. 10 sept. 2009) $</date>
// <version>$Revision: 1817 $</version>

//// ========================================================================================

// Constantes
float Amount: register(C0);
float Width: register(C1);

// Entr�e de surface
sampler2D Input: register(S0);

// Point d'entr�e du programme
float4 main(float2 Point: TEXCOORD): COLOR
{
	float4 Color=tex2D(Input, Point);
	Color.rgb+=tex2D(Input, Point-Width)*Amount;
	Color.rgb-=tex2D(Input, Point+Width)*Amount;
	return Color;
}