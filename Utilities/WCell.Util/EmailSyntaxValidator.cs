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
    /// Implements an email validation utility class
    /// Validation based on code from
    /// http://www.aspemporium.com/aspEmporium/tutorials/emailvalidation.asp
    /// We added checks for length as defined at
    /// http://email.about.com/od/emailbehindthescenes/f/address_length.htm
    /// Validates an email address for proper syntax.
    /// </summary>
    /// <example>
    /// Validate an array of email addresses with the 
    /// <see cref="EmailSyntaxValidator"/> class.
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
    public class EmailSyntaxValidator
    {
        private bool syntaxvalid = false;
        private string account, domain, inputemail;

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
            EmailSyntaxValidator v;
            bool valid;

            //call syntax validator
            v = new EmailSyntaxValidator(email, TLDrequired);

            //determine validity
            valid = v.IsValid;

            //check lengths
            if (v.Account.Length > 64)
                return false;
            if (v.Domain.Length > 255)
                return false;

            //return indication of validity
            return valid;
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
        public EmailSyntaxValidator(string email, bool TLDrequired)
        {
            string tmpmail;

            //save email as validated
            inputemail = email;

            //remove <brackets> if found
            tmpmail = RemoveBrackets(email);

            //then trim
            tmpmail = Trim(tmpmail);

        	account = domain = inputemail = "";

            //separate account from domain, quit if unable to separate
            if (!ParseAddress(tmpmail))
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
        /// separates email account from domain
        /// </summary>
        /// <param name="email">the email to parse</param>
        /// <returns>boolean indicating success of separation</returns>
        private bool ParseAddress(string email)
        {
            Regex re;
            Match m;
            bool ok = false;

            try
            {
                //pattern to separate email into account and domain.
                //note that we parse from right to left, thereby forcing
                //this pattern to match the last @ symbol in the email.
                re = new Regex(
                    "^(.+?)\\@(.+?)$",
                    RegexOptions.Singleline | RegexOptions.RightToLeft
                    );

                //determine if email matches pattern (email contains one
                //or more @'s)
                if (re.IsMatch(email))
                {
                    //if matched, separate account and domain as noted
                    //in our pattern with the () sections.
                    m = re.Match(email);

                    //first group is the account
                    account = m.Groups[1].Value;

                    //second group is the domain
                    domain = m.Groups[2].Value;

                    //cleanup
                    m = null;

                    //indicate success
                    ok = true;
                }

                //cleanup
                re = null;
            }
            catch
            {
                //catch any exceptions and just consider the string
                //unparsable
                ok = false;
            }

            //return the indication of parse success or failure
            return ok;
        }

        /// <summary>
        /// removes outer brackets from an email address
        /// </summary>
        /// <param name="input">the email to parse</param>
        /// <returns>the email without brackets</returns>
        private static string RemoveBrackets(string input)
        {
            string output = null;
            Regex re;

            //pattern to match brackets or no brackets
            re = new Regex("^\\<*|\\>*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            //if email matches (it will always match this pattern)
            if (re.IsMatch(input))
            {
                //replace them with nothing
                output = re.Replace(input, "");
            }

            //cleanup
            re = null;

            //return the email without brackets
            return output;
        }

        /// <summary>
        /// trims any leading and trailing white space from the email
        /// </summary>
        /// <param name="input">the email to parse</param>
        /// <returns>the email with no leading or trailing white space</returns>
        private static string Trim(string input)
        {
            string output = null;
            Regex re;

            //pattern to trim leading and trailing white space from string
            re = new Regex("^\\s*|\\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            //if matches
            if (re.IsMatch(input))
            {
                //remove whitespace
                output = re.Replace(input, "");
            }

            //cleanup
            re = null;

            //return the email with no leading or trailing white space
            return output;
        }

        private bool DomainValid(bool TLDrequired)
        {
            bool valid;
            Regex re;
            string pattern, emaildomain;

            if (TLDrequired)
            {
                //if the TLD is required, the pattern contains
                //a basic TLD length check at the end
                pattern = "^((([a-z0-9-]+)\\.)+)[a-z]{2,6}$";
                emaildomain = domain;
            }
            else
            {
                //when the tld is not required, the pattern is
                //the same except for the tld length check.
                //note the pattern ends with a . in the loop.
                //This means that we must append a . to the domain
                //temporarily to test the email.
                pattern = "^((([a-z0-9-]+)\\.)+)$";
                emaildomain = domain + ".";
            }

            re = new Regex(
                pattern,
                RegexOptions.IgnoreCase | RegexOptions.Singleline
                );

            //if the email matches, it's valid
            valid = re.IsMatch(emaildomain);

            //cleanup
            re = null;

            //return indication of validity
            return valid;
        }

        private bool DomainExtensionValid()
        {
            bool valid;
            Regex re;
            string domainvalidatorpattern = "";

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

            re = new Regex(
                domainvalidatorpattern,
                RegexOptions.IgnoreCase | RegexOptions.Singleline
                );

            //if domain matches pattern, it has a valid TLD
            valid = re.IsMatch(domain);

            //cleanup
            re = null;

            //return an indication of TLD validity
            return valid;
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
    }
}