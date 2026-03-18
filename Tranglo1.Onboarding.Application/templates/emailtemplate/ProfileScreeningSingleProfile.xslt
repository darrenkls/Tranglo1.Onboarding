<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
	<xsl:template match="ProfileScreening">
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
					text-decoration: underline;
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

					.tradeCaption {
					margin-top: 10px;
					margin-bottom: 10px;
					color: #000000;
					font-family: Montserrat, sans-serif;
					font-size: 10px;
					font-style: italic;
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
					Dear Compliance team,
				</p>
				<br></br>

				<!-- If Sanction is detected -->
				<xsl:if test="IsSanctionDetected = 'True'">
					<p>
						Below are the screening result details of the <b>new partner</b> to be reviewed via <a href="{PersonnelWatchlistUrl}">T1 portal</a>.
					</p>
					<p>
						Partner Name: <b><xsl:value-of select="PartnerName"/></b>
					</p>

					<br></br>

					<table border="1" cellpadding="5">
						<thead>
							<tr>
								<th align="left">Solution</th>
								<th align="left">Partner Name</th>
								<th align="left">Category</th>
								<th align="left">Type</th>
								<th align="left">Name</th>
								<th align="left">Nationality</th>
								<th align="left">Date of Birth / Date of Incorporation</th>
								<th align="left">Type of Alert</th>
								<th align="left">Entity ID</th>
							</tr>
						</thead>
						<xsl:for-each select="BusinessProfile">
							<tr>
								<td>
									<xsl:value-of select="SolutionNames"/>
								</td>
								<td>
									<xsl:value-of select="CompanyName"/>
								</td>
								<td>
									<xsl:value-of select="OwnershipStructureTypeNames"/>
								</td>
								<td>
									<xsl:value-of select="EntityTypeName"/>
								</td>
								<td>
									<xsl:value-of select="FullName"/>
								</td>
								<td>
									<xsl:value-of select="NationalityFullName"/>
								</td>
								<td>
									<xsl:value-of select="DateOfBirth"/>
								</td>
								<td>
									<xsl:value-of select="Type"/>
								</td>
								<td>
									<xsl:value-of select="EntityIds"/>
								</td>
							</tr>
						</xsl:for-each>
					</table>
				</xsl:if>

				<!-- If No Hit -->
				<xsl:if test="IsSanctionDetected = 'False'">
					<p>
						The screening for partner <b><xsl:value-of select="PartnerName"/></b> has been completed, and there are <b>no screening hits</b> identified.
					</p>
					<p>
						You may proceed to <b>review and approve the profile</b> via <a href="{KYCProfileApprovalUrl}">T1 portal</a>.
					</p>
				</xsl:if>
				
				<br></br>
				<br></br>
				<p class="tradeCaption">This is an autogenerated email, please do not reply.</p>
				<br></br>
				<hr class="style-six" />
				<center class="tradeMark">
					<xsl:value-of select="CurrentYear" disable-output-escaping="yes"/> Tranglo. All rights reserved.
				</center>

			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>
