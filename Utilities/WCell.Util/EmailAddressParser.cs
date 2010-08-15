/*************************************************************************
 *
 *   file		: EmailSyntaxValidator.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-03-10 07:28:05 +0800 (Mon, 10 Mar 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 183 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Text.RegularExpressions;

namespace WCell.Util
{
	/// <summary>
	/// Edited by Domi for some performance gain
	/// 
	/// Implements an email validation utility class
	/// Validation based on code from
	/// http://www.aspemporium.com/aspEmporium/tutorials/emailvalidation.asp
	/// We added checks for length as defined at
	/// http://email.about.com/od/emailbehindthescenes/f/address_length.htm
	/// Validates an email address for proper syntax.
	/// </summary>
	/// <example>
	/// Validate an array of email addresses with the 
	/// <see cref="EmailAddressParser"/> class.
	/// <code>
	/// string[]             emails;
	/// EmailSyntaxValidator emailsyntaxvalidator;
	/// int                  countgood=0, countbad=0;
	/// 
	///
	/// //TODO: set emails string[] array
	///
	///
	/// //validate each email in the array
	/// foreach(string email in emails)
	/// {
	/// 	emailsyntaxvalidator = new EmailSyntaxValidator(email, true);
	/// 	if (emailsyntaxvalidator.IsValid)
	/// 	{
	/// 		countgood ++;
	/// 	}
	/// 	else
	/// 	{
	/// 		Console.WriteLine(email);
	/// 		countbad ++;
	/// 	}
	/// 	emailsyntaxvalidator = null;
	/// }
	///
	///
	/// Console.WriteLine("good: {0}\r\nbad : {1}", countgood, countbad);
	/// </code>
	/// </example>
	/// <remarks>
	/// <para>
	/// Validates emails for proper syntax as detailed here:
	/// </para>
	/// <para>
	/// <A HREF="http://www.aspemporium.com/aspEmporium/tutorials/emailvalidation.asp">Email Validation - Explained</A>
	/// </para>
	/// <para> </para>
	/// <para>
	/// Version Information
	/// </para>
	/// <para>
	/// 	    Email verification in general has had a checkered history at the ASP 
	/// 	    Emporium. It took a while but I think we finally came up with something 
	/// 	    good... Here's the short version history of all email validation 
	/// 	    software from ASP Emporium...
	/// </para>
	/// <para>
	/// All future versions of email validation from ASPEmporium will be C# classes
	/// written for the .NET framework.
	/// </para>
	/// <para>
	/// 		10/2002 v4.0 (C#)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			Added new TLD (.int). Thanks to alex.wernhardt@web.de
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			Rebuilt from the ground up as a C# class that uses
	/// 			only regular expressions for string parsing.
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			Supports all the rules as detailed here:
	/// 			<A HREF="http://www.aspemporium.com/aspEmporium/tutorials/emailvalidation.asp">Email Validation - Explained</A>
	/// 			This repairs all known issues in version 3.2 which
	/// 			was written in JScript for classic ASP.
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		10/2002 v4.0 (JScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			Added new TLD (.int). Thanks to alex.wernhardt@web.de
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			Supports all the rules as detailed here:
	/// 			<A HREF="http://www.aspemporium.com/aspEmporium/tutorials/emailvalidation.asp">Email Validation - Explained</A>
	/// 			This repairs all known issues in version 3.2 which
	/// 			was written in JScript for classic ASP.
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			This is the last edition of email software that is
	/// 			written for classic ASP (JScript/VBScript).
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		2/2002 v3.2 (JScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			fixed a problem that allows emails like test@mydomain.......com
	/// 			  to pass through. Thanks to g.falcone@mclink.it for letting
	/// 			  me know about it.
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		11/2001 v3.1 (JScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			added new tlds. thanks to alex.wernhardt@web.de for sending
	/// 			  me the list - http://www.icann.org/tlds/
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			new tlds: aero, biz, coop, info, museum, name, pro
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		9/2001  v3.0 (JScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			fixed spaced email problem. thanks to mikael@perfoatris.com 
	/// 			  for the report.
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			put the length check right in the function rather
	/// 			  than relying on a programmer to check length before
	/// 			  testing an email. thanks to eduardo.azambuja@uol.com.br for
	/// 			  bringing that to my attention.
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		7/2001  v2.5 (JScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			forgot the TLD (.gov). added now...
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			fixed @@ problem... thanks to davidchersg@yahoo.com for
	/// 			  letting me know that the problem was still there.
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		5/2001  v2.0 (JScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			added verification of all known TLDs as of
	/// 			  May 2001: http://www.behindtheurl.com/TLD/
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			added line to remove leading and trailing spaces before
	/// 			  testing email
	/// 			    http://www.aspemporium.com/aspEmporium/feedback/feedbacklib.asp?mail=200105060001
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			regular expression improvements by:
	/// 				Bj�rn Hansson  -  http://nytek.nu
	/// 			  you can view his emails here:
	/// 			    http://www.aspemporium.com/aspEmporium/feedback/feedbacklib.asp?mail=200104180005
	/// 			    http://www.aspemporium.com/aspEmporium/feedback/feedbacklib.asp?mail=200104090006
	/// 			    http://www.aspemporium.com/aspEmporium/feedback/feedbacklib.asp?mail=200104090005
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			this email verification software replaces all other email 
	/// 			  verification software at the ASP Emporium. VBScript versions 
	/// 			  have been abandoned in favor of this JScript version.
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		2/2001  v1.5 (JScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			Regular Expression introduced to validate emails. Basically 
	/// 			  a re-hashed version of the VBScript edition of IsEmail, aka 
	/// 			  the EmailVerification object 3.0 (next line below)
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		12/2000 v3.0 (VBScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			EmailVerification Class released, resolving multiple domain 
	/// 			  and user name problems.
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			Abandoned VBScript processing of emails in favor of regular 
	/// 			  expressions.
	/// 		</description>
	/// 	</item>
	/// 	<item>
	/// 		<description>
	/// 			New VBScript class structure.
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		8/2000  v1.0 (JScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			Initial Release of IsEmail for JScript is a lame function 
	/// 			  that uses weak JScript inherent functions like indexOf... 
	/// 			  This is essentually a copy of the vbscript edition of the
	/// 			  software, version 2, remembered on the next line below...
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		8/2000  v2.0 (VBScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			IsEmail function updated to resolve several issues but 
	/// 			  multiple domains still pose a problem.
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		4/2000  v1.0 (VBScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			IsEmail function introduced
	/// 			  (used in the Simple Email Verification example)
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// <para>
	/// 		4/2000  v0.1 (VBScript)
	/// <list type="bullet">
	/// 	<item>
	/// 		<description>
	/// 			First email validation code at the ASP Emporium checks only 
	/// 			  for an @ and a . (Used in the first version of the 
	/// 			  autoresponder example)
	/// 		</description>
	/// 	</item>
	/// </list>
	/// </para>
	/// </remarks>
	public class EmailAddressParser
	{
		public static readonly Regex AddressRegex = new Regex("^(.+?)\\@(.+?)$", RegexOptions.Singleline | RegexOptions.RightToLeft);
		public static readonly Regex BracketRegex = new Regex("^\\<*|\\>*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		public static readonly Regex TrimRegex = new Regex("^\\s*|\\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		public static readonly Regex TLDomainRegex = new Regex("^((([a-z0-9-]+)\\.)+)[a-z]{2,6}$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		public static readonly Regex AnyDomainRegex = new Regex("^((([a-z0-9-]+)\\.)+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		public static readonly Regex DomainExtensionRegex;

		private readonly string inputemail;
		private readonly bool syntaxvalid;
		private string account, domain;

		static EmailAddressParser()
		{
			var domainvalidatorpattern = "";

			//pattern to validate all known TLD's
			domainvalidatorpattern += "\\.(";
			domainvalidatorpattern += "a[c-gil-oq-uwz]|"; //ac,ad,ae,af,ag,ai,al,am,an,ao,aq,ar,as,at,au,aw,az
			domainvalidatorpattern += "b[a-bd-jm-or-tvwyz]|"; //ba,bb,bd,be,bf,bg,bh,bi,bj,bm,bn,bo,br,bs,bt,bv,bw,by,bz
			domainvalidatorpattern += "c[acdf-ik-orsuvx-z]|"; //ca,cc,cd,cf,cg,ch,ci,ck,cl,cm,cn,co,cr,cs,cu,cv,cz,cy,cz
			domainvalidatorpattern += "d[ejkmoz]|"; //de,dj,dk,dm,do,dz
			domainvalidatorpattern += "e[ceghr-u]|"; //ec,ee,eg,eh,er,es,et,eu
			domainvalidatorpattern += "f[i-kmorx]|"; //fi,fj,fk,fm,fo,fr,fx
			domainvalidatorpattern += "g[abd-ilmnp-uwy]|"; //ga,gb,gd,ge,gf,gg,gh,gi,gl,gm,gn,gp,gq,gr,gs,gt,gu,gw,gy
			domainvalidatorpattern += "h[kmnrtu]|"; //hk,hm,hn,hr,ht,hu
			domainvalidatorpattern += "i[delm-oq-t]|"; //id,ie,il,im,in,io,iq,ir,is,it
			domainvalidatorpattern += "j[emop]|"; //je,jm,jo,jp
			domainvalidatorpattern += "k[eg-imnprwyz]|"; //ke,kg,kh,ki,km,kn,kp,kr,kw,ky,kz
			domainvalidatorpattern += "l[a-cikr-vy]|"; //la,lb,lc,li,lk,lr,ls,lt,lu,lv,ly
			domainvalidatorpattern += "m[acdghk-z]|"; //ma,mc,md,mg,mh,mk,ml,mm,mn,mo,mp,mq,mr,ms,mt,mu,mv,mw,mx,my,mz
			domainvalidatorpattern += "n[ace-giloprtuz]|"; //na,nc,ne,nf,ng,ni,nl,no,np,nr,nt,nu,nz
			domainvalidatorpattern += "om|"; //om
			domainvalidatorpattern += "p[ae-hk-nrtwy]|"; //pa,pe,pf,pg,ph,pk,pl,pm,pn,pr,pt,pw,py
			domainvalidatorpattern += "qa|"; //qa
			domainvalidatorpattern += "r[eouw]|"; //re,ro,ru,rw
			domainvalidatorpattern += "s[a-eg-ort-vyz]|"; //sa,sb,sc,sd,se,sg,sh,si,sj,sk,sl,sm,sn,so,sr,st,su,sv,sy,sz
			domainvalidatorpattern += "t[cdf-hjkm-prtvwz]|"; //tc,td,tf,tg,th,tj,tk,tm,tn,to,tp,tr,tt,tv,tx,tz
			domainvalidatorpattern += "u[agkmsyz]|"; //ua,ug,uk,um,us,uy,uz
			domainvalidatorpattern += "v[aceginu]|"; //va,vc,ve,vg,vy,vn,vu
			domainvalidatorpattern += "w[fs]|"; //wf,ws
			domainvalidatorpattern += "y[etu]|"; //ye,yt,yu
			domainvalidatorpattern += "z[admrw]|"; //za,zd,zm,zr,zw
			domainvalidatorpattern += "com|"; //com
			domainvalidatorpattern += "edu|"; //edu
			domainvalidatorpattern += "net|"; //net
			domainvalidatorpattern += "org|"; //org
			domainvalidatorpattern += "mil|"; //mil
			domainvalidatorpattern += "gov|"; //gov
			domainvalidatorpattern += "biz|"; //biz
			domainvalidatorpattern += "pro|"; //pro
			domainvalidatorpattern += "aero|"; //aero
			domainvalidatorpattern += "coop|"; //coop
			domainvalidatorpattern += "info|"; //info
			domainvalidatorpattern += "name|"; //name
			domainvalidatorpattern += "int|"; //int
			domainvalidatorpattern += "museum"; //museum
			domainvalidatorpattern += ")$";

			DomainExtensionRegex = new Regex(
				domainvalidatorpattern,
				RegexOptions.IgnoreCase | RegexOptions.Singleline
				);
		}

		/// <summary>
		/// Determines if an email has valid syntax
		/// </summary>
		/// <param name="email">the email to test</param>
		/// <param name="TLDrequired">indicates whether or not the 
		/// email must end with a known TLD to be considered valid</param>
		/// <returns>boolean indicating if the email has valid syntax</returns>
		/// <remarks>
		/// Validates an email address specifying whether or not
		/// the email is required to have a TLD that is valid.
		/// </remarks>
		public static bool Valid(string email, bool TLDrequired)
		{
			//call syntax validator
			var v = new EmailAddressParser(email, TLDrequired);

			return v.IsValid;
		}

		/// <summary>
		/// Initializes a new instance of the EmailSyntaxValidator
		/// </summary>
		/// <param name="email">the email to test</param>
		/// <param name="TLDrequired">indicates whether or not the 
		/// email must end with a known TLD to be considered valid</param>
		/// <remarks>
		/// The initializer creates an instance of the EmailSyntaxValidator
		/// class to validate a single email. You can specify whether or not
		/// the TLD is required and should be validated.
		/// </remarks>
		public EmailAddressParser(string email, bool TLDrequired)
		{
			//save email as validated
			inputemail = email;

			//remove <brackets> if found
			var tmpmail = RemoveBrackets(email);

			//then trim
			tmpmail = Trim(tmpmail);

			account = domain = "";

			//separate account from domain, quit if unable to separate
			if (!ParseAddress(tmpmail))
			{
				return;
			}
			
			//check lengths
			if (Account.Length > 64 || Domain.Length > 255)
			{
				return;
			}

			//validate the domain, quit if domain is bad
			if (!DomainValid(TLDrequired))
			{
				return;
			}

			//if the TLD is required, validate the domain extension,
			//quit if the domain extension is bad
			if (TLDrequired && !DomainExtensionValid())
			{
				return;
			}

			//email syntax is valid
			syntaxvalid = true;
		}

		/// <summary>
		/// Gets a value indicating whether or not the email address 
		/// has valid syntax
		/// </summary>
		/// <remarks>
		/// This property returns a boolean indicating whether or not
		/// the email address has valid syntax as determined by the
		/// class.
		/// </remarks>
		/// <value>boolean indicating the validity of the email</value>
		public bool IsValid
		{
			get { return syntaxvalid; }
		}

		/// <summary>
		/// Get the domain part of the email address.
		/// </summary>
		/// <remarks>
		/// This property returns the domain part of the email
		/// address if and only if the email is considered valid
		/// by the class. Otherwise null is returned.
		/// </remarks>
		/// <value>string representing the domain of the email</value>
		public string Domain
		{
			get { return domain; }
		}

		/// <summary>
		/// Get the account part of the email address.
		/// </summary>
		/// <remarks>
		/// This property returns the account part of the email
		/// address if and only if the email is considered valid
		/// by the class. Otherwise null is returned.
		/// </remarks>
		/// <value>string representing the account of the email</value>
		public string Account
		{
			get { return account; }
		}

		/// <summary>
		/// Gets the email address as entered.
		/// </summary>
		/// <remarks>
		/// This property is filled regardless of the validity of the email.
		/// It contains the email as it was entered into the class.
		/// </remarks>
		/// <value>string representing the email address as entered</value>
		public string Address
		{
			get { return inputemail; }
		}

		/// <summary>
		/// separates email account from domain
		/// </summary>
		/// <param name="email">the email to parse</param>
		/// <returns>boolean indicating success of separation</returns>
		private bool ParseAddress(string email)
		{
			//pattern to separate email into account and domain.
			//note that we parse from right to left, thereby forcing
			//this pattern to match the last @ symbol in the email.

			//determine if email matches pattern (email contains one
			//or more @'s)
			var m = AddressRegex.Match(email);
			if (m.Success && m.Groups.Count >= 2)
			{
				//first group is the account
				account = m.Groups[1].Value;

				//second group is the domain
				domain = m.Groups[2].Value;

				//indicate success
				return true;
			}

			//return the indication of parse success or failure
			return false;
		}

		/// <summary>
		/// removes outer brackets from an email address
		/// </summary>
		/// <param name="input">the email to parse</param>
		/// <returns>the email without brackets</returns>
		private static string RemoveBrackets(string input)
		{
			// pattern to match brackets or no brackets:
			// If email matches (it will always match this pattern)

			// remove
			return BracketRegex.Replace(input, "");
		}

		/// <summary>
		/// trims any leading and trailing white space from the email
		/// </summary>
		/// <param name="input">the email to parse</param>
		/// <returns>the email with no leading or trailing white space</returns>
		private static string Trim(string input)
		{
			return TrimRegex.Replace(input, "");
		}

		private bool DomainValid(bool TLDrequired)
		{
			Regex re;
			string emaildomain;

			if (TLDrequired)
			{
				//if the TLD is required, the pattern contains
				//a basic TLD length check at the end
				re = TLDomainRegex;
				emaildomain = domain;
			}
			else
			{
				//when the tld is not required, the pattern is
				//the same except for the tld length check.
				//note the pattern ends with a . in the loop.
				//This means that we must append a . to the domain
				//temporarily to test the email.
				re = AnyDomainRegex;
				emaildomain = domain + ".";
			}

			//if the email matches, it's valid
			return re.IsMatch(emaildomain);
		}

		private bool DomainExtensionValid()
		{
			//if domain matches pattern, it has a valid TLD
			return DomainExtensionRegex.IsMatch(domain);
		}
	}
}