using System;
using System.IO;
using System.Linq;
using System.Text;
using ClassForge.Model;

namespace DocForge.Helpers
{
    internal class HtmlGenerator
    {
        private string tokenPageTitle = "[$pagetitle]";

        private string tokenClassName = "[$classname]";

        private string tokenClassInherits = "[$classinherits]";

        private string tokenClassParent = "[$classparent]";

        private string tokenClassRemark = "[$classremark]";

        private string tokenPropertiesTable = "[$properties]";

        private string tokenClassTable = "[$classes]";

        private string tokenClassInheritanceTable = "[$classesinh]";

        public void GenerateDocumentation(Model m)
        {
            foreach (var c in m.Classes)
            {
                this.GenerateClassPage(c);
            }
        }

        public void GenerateClassPage(Class c)
        {
            string template = File.ReadAllText("Templates\\classpage.html");

            // replace classname
            template = template.Replace(this.tokenPageTitle, c.Name);
            template = template.Replace(this.tokenClassName, c.Name);
            template = template.Replace(this.tokenClassInherits, c.InheritanceClass == null ? c.Inherits : c.InheritanceClass.HtmlLinkToPage());
            template = template.Replace(this.tokenClassParent, c.ContainmentParent == null ? string.Empty : c.ContainmentBreadcrumb());
            template = template.Replace(this.tokenClassRemark, c.Remark);
            template = template.Replace(this.tokenPropertiesTable, this.GeneratePropertiesTable(c));
            template = template.Replace(this.tokenClassTable, this.GenerateContainedClassesTable(c));
            template = template.Replace(this.tokenClassInheritanceTable, this.GenerateInheritanceClassesTable(c));

            var savepath = c.HtmlPage();

            Directory.CreateDirectory("output");

            File.WriteAllText("output\\" + savepath, template);

            foreach (var cc in c.Classes)
            {
                this.GenerateClassPage(cc);
            }
        }

        private string GeneratePropertiesTable(Class c)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<table class=\"table table-striped\">");
                sb.AppendLine("<thead>");
                    sb.AppendLine("<tr>");
                        sb.AppendLine("<th>Name</th>");
                        sb.AppendLine("<th>Value</th>");
                        sb.AppendLine("<th>Remark</th>");
                    sb.AppendLine("</tr>");
                sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");
            foreach (var property in c.Properties)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine(string.Format("<td>{0}</td>", property.Name));
                sb.AppendLine(string.Format("<td>{0}</td>", property.Value));
                sb.AppendLine(string.Format("<td>{0}</td>", property.Remark));
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        private string GenerateContainedClassesTable(Class c)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<table class=\"table table-striped\">");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Name</th>");
            sb.AppendLine("<th>Remark</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");
            foreach (var property in c.Classes)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine(string.Format("<td>{0}</td>", property.HtmlLinkToPage()));
                sb.AppendLine(string.Format("<td>{0}</td>", property.Remark));
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            sb.AppendLine("<h5>Simple List</h5>");

            sb.AppendLine(string.Format("<pre>{0}</pre>", string.Join(",", c.Classes.Select(cl => string.Format("\"{0}\"", cl.Name)))));

            return sb.ToString();
        }

        private string GenerateInheritanceClassesTable(Class c)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<table class=\"table table-striped\">");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Name</th>");
            sb.AppendLine("<th>Remark</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");
            foreach (var property in c.InheritanceChildren)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine(string.Format("<td>{0}</td>", property.HtmlLinkToPage()));
                sb.AppendLine(string.Format("<td>{0}</td>", property.Remark));
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            sb.AppendLine("<h5>Simple List</h5>");

            sb.AppendLine(string.Format("<pre>{0}</pre>", string.Join(",", c.InheritanceChildren.Select(cl => string.Format("\"{0}\"", cl.Name)))));

            return sb.ToString();
        }
    }
}
