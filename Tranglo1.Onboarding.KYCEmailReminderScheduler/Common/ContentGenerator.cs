using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tranglo1.Onboarding.KYCEmailReminderScheduler.Common
{
    public class ContentGenerator
    {
        private ConcurrentDictionary<string, string> _CachedTemplates = new ConcurrentDictionary<string, string>();

        public ContentGenerator(string templatesPath)
        {
            if (templatesPath == null)
            {
                throw new ArgumentException("templatesPath");
            }

            this.TemplatesPath = templatesPath;
        }


        public string TemplatesPath { get; private set; }

        public string GenerateContent(string xml, string templateName, string culture)
        {
            string xslt = this.FindTemplate(templateName, culture, true);
            string output = TransformHelper.TransformXML(xslt, xml);
            return output;
        }

        /// <summary>
        /// find the suitable xslt template inside the templates path based on the template name
        /// and culture.
        /// </summary>
        /// <param name="templateName">the xslt template name (without extention). Example: SendRequest</param>
        /// <param name="culture">the culture to be used. if null or empty string was passed in, or
        /// no template found for the given culture, default template will be used</param>
        /// <returns>the xslt string</returns>
        private string FindTemplate(string templateName, string culture, bool fallback)
        {
            /*
			 * file name search orders:
			 * 
			 * SendRequest.<culture>.xslt
			 * 
			 * if fallback = true
			 *		SendRequest.xslt
			 *		SendRequest.en-US.xslt
			 * 
			 */

            if (string.IsNullOrEmpty(culture))
            {
                if (File.Exists(templateName + ".xslt"))
                {
                    return this.LoadContent(templateName + ".xslt");
                }

                return null;
            }

            string filename = string.Format("{0}.{1}.xslt", templateName, culture);

            if (File.Exists(Path.Combine(this.TemplatesPath, filename)))
            {
                return this.LoadContent(filename);
            }

            if (fallback == true)
            {
                return this.LoadContent(templateName + ".xslt");
            }

            return null;
        }

        private string LoadContent(string filename)
        {
            string content = null;
            if (this._CachedTemplates.TryGetValue(filename, out content) == false)
            {
                using (StreamReader reader = new StreamReader(Path.Combine(this.TemplatesPath, filename)))
                {
                    content = reader.ReadToEnd();
                }

                this._CachedTemplates.TryAdd(filename, content);
                return content;
            }

            return content;
        }
    }
}
