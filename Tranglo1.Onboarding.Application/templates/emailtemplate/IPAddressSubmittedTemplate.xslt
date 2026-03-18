<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
	<xsl:template match="IPAddressSubmittedTemplate">
		<html xmlns="http://www.w3.org/1999/xhtml">

			<head>
				<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
				<meta name="viewport" content="width=device-width"/>
				<style type="text/css" media="screen">
					/* Linked Styles */
					body {
					padding: 0 !important;
					margin: 0 !important;
					display: block !important;
					min-width: 100% !important;
					width: 100% !important;
					background: white;
					-webkit-text-size-adjust: none;
					}

					a {
					color: #66c7ff;
					text-decoration: none;
					}

					p {
					padding: 0 !important;
					margin: 0 !important;
					}

					img {
					-ms-interpolation-mode: bicubic; /* Allow smoother rendering of resized image in Internet Explorer */
					}

					.center {
					margin: auto;
					width: 40%;
					}

					.font-style {
					color: #000000;
					font-family: Montserrat, sans-serif;
					font-size: 20px;
					line-height: 30px;
					}

					.mb-2r {
					margin-bottom: 2rem !important;
					}


					.mb-4r {
					margin-bottom: 4rem !important;
					}

					.linkButton {
					background-color: #15bdf0;
					border-radius: 28px;
					border: 1px solid #15bdf0;
					display: inline-block;
					cursor: pointer;
					color: #ffffff;
					font-family: Arial;
					font-size: 10px;
					font-weight: bold;
					padding: 8px 24px;
					text-decoration: none;
					}

					.bg {
					background-color: white;
					width: 100%;
					height: 100%;
					padding: 20px;
					}

					hr.style-six {
					border: 0;
					height: 0;
					border-top: 1px solid rgba(0, 0, 0, 0.1);
					border-bottom: 1px solid rgba(255, 255, 255, 0.3);
					}

					.tradeMark {
					margin-top: 30px;
					margin-bottom: 10px;
					color: #000000;
					font-family: Montserrat, sans-serif;
					font-size: 15px;
					font-weight: bold;
					}

					/* Mobile styles */
					@media only screen and (max-device-width: 480px), only screen and (max-width: 480px) {

					.center {
					min-width: 90%;
					margin: auto;
					}

					.linkButton {
					background-color: #15bdf0;
					border-radius: 28px;
					border: 1px solid #15bdf0;
					display: inline-block;
					cursor: pointer;
					color: #ffffff;
					font-family: Arial;
					font-size: 15px;
					font-weight: bold;
					padding: 5px 12px;
					text-decoration: none;
					}

					.font-style {
					color: #000000;
					font-family: Montserrat, sans-serif;
					font-size: 20px;
					line-height: 30px;
					}
					}
				</style>
			</head>

			<body>

				<p>
					Dear <xsl:value-of select="Recipient"/>,
				</p>
				<br></br>
				<br></br>
				<p>
					There's an IP address whitelist request from <xsl:value-of select="PartnerName"/>
				</p>
				<br></br>
				<p>
					Partner subscription details as follow:
				</p>
				<br></br>
				<br></br>
				<p>
					Partner Entity: <xsl:value-of select="PartnerEntity"/>
				</p>
				<p>
					Solution Name: <xsl:value-of select="SolutionName"/>
				</p>
				<p>
					Preferred API Connection: <xsl:value-of select="PreferredApi"/>	
				</p>
				<p>
					&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;<xsl:value-of select="PreferredApi2"/>
				</p>
				<br></br>
				<p>
					You may login to admin portal to check partner information. Kindly click the IPWhitelisted button after you have whitelisted the IP address and an email notification will be sent to the partner.

				</p>
				<br></br>
				<b>
					<xsl:element name="a">
						<xsl:attribute name="target">_blank</xsl:attribute>
						<xsl:attribute name="class">linkButton mb-1r</xsl:attribute>
						<xsl:attribute name="href">
							<xsl:value-of select="PartnerAPIUrl"/>
						</xsl:attribute>
						<span style="color:White">Login</span>
					</xsl:element>
				</b>
				<p>
					Thank You.
				</p>
				<br></br>

				<hr class="style-six" />
				<center class="tradeMark">
					<xsl:value-of select="CurrentYear" disable-output-escaping="yes"/> Tranglo. All rights reserved.
				</center>

			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>
