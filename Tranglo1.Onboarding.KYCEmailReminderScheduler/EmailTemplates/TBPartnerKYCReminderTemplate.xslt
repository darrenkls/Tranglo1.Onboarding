<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

	<xsl:template match="TBPartnerKYCReminderTemplate">
		<html xmlns="http://www.w3.org/1999/xhtml">
			<head>
				<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
				<meta name="viewport" content="width=device-width" />
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

					.mb-1r {
					margin-bottom: 1rem !important;
					}

					/* Mobile styles */
					@media only screen and (max-device-width: 480px), only screen and (max-width: 480px) {

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
					}
				</style>
			</head>

			<body>
				<!-- Email content -->
				<p class="mb-1r">
					Dear <xsl:value-of select="PartnerName"/>,
				</p>
				<p class="mb-1r">
					Thank you for choosing Tranglo.
				</p>
				<p class="mb-1r">
					We’ve noticed that you’re just one step away from completing your KYC.
				</p>
				<br/>
				<p>
					<xsl:element name="a">
						<xsl:attribute name="target">_blank</xsl:attribute>
						<xsl:attribute name="class">linkButton</xsl:attribute>
						<xsl:attribute name="href">
							<xsl:value-of select="KYCReminderUri"/>
						</xsl:attribute>
						<span style="color:White">Complete KYC</span>
					</xsl:element>
				</p>
				<br/>
				<br/>
				<p class="mb-1r">
					Alternatively you can copy and paste the following link to your browser: <a href="{KYCReminderUri}">Complete KYC</a>.
				</p>
				<p class="mb-1r">
					If you have any questions, do not hesitate to contact us at <a href="mailto:hello@tranglo.com">hello@tranglo.com</a>.
				</p>

				<br/>
				<br/>
				<p class="mb-1r">Best Regards,</p>
				<p class="mb-1r">Tranglo Team</p>
				<br/>
				<p>
					<i>Note: This is an auto-generated email. Please do not reply.</i>
				</p>
				<br/>
				<br/>
				<p>
					No longer want to receive KYC reminders? Unsubscribe

					<xsl:element name="a">
						<xsl:attribute name="target">_blank</xsl:attribute>
						<xsl:attribute name="class">linkButton</xsl:attribute>
						<xsl:attribute name="href">
							<xsl:value-of select="UnsubscribeUri"/>
						</xsl:attribute>
						<span style="color:White">here</span>
					</xsl:element>
				</p>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>
