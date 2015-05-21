using System;
using System.Collections.Generic;
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

        private string tokenModelName = "[$modelname]";
        private string tokenModelVersion = "[$modelversion]";
        private string tokenModelDescription = "[$modeldescription]";
        private string tokenDigramJson = "[$digramjson]";

        private Model model;
        private string modelNameLiteral;
        private string tokenToc = "[$toc]";
        private string modelVersionLiteral;

        public void GenerateDocumentation(Model m, string outputPath, string modelName, string modelVersion, string modelDescription)
        {
            this.model = m;
            this.modelNameLiteral = modelDescription;
            this.modelVersionLiteral = modelVersion;

            string template = File.ReadAllText("Templates\\indexpage.html");

            // replace classname
            template = template.Replace(this.tokenPageTitle, "Class Documentation");
            template = template.Replace(this.tokenModelName, modelName);
            template = template.Replace(this.tokenModelVersion, modelVersion);
            template = template.Replace(this.tokenModelDescription, modelDescription);
           
            var savepath = "index.html";

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            DirectoryCopy("Templates\\public", string.Format("{0}\\public", outputPath));

            File.WriteAllText(outputPath + "\\" + savepath, template);

            foreach (var c in m.Classes)
            {
                this.GenerateClassPage(c, outputPath);
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public void GenerateClassPage(Class c, string outputPath)
        {
            string template = File.ReadAllText("Templates\\classpage.html");

            var toc = this.GenerateToc(c);

            template = template.Replace(this.tokenToc, toc);

            // replace classname
            template = template.Replace(this.tokenPageTitle, c.Name);
            template = template.Replace(this.tokenClassName, c.Name);
            template = template.Replace(this.tokenDigramJson, c.ClassDiagramJSON());
            template = template.Replace(this.tokenClassInherits, c.InheritanceClass == null ? c.Inherits : c.InheritanceClass.HtmlLinkToPage());
            template = template.Replace(this.tokenClassParent, c.ContainmentParent == null ? string.Empty : c.ContainmentBreadcrumb());
            template = template.Replace(this.tokenClassRemark, c.Remark);
            template = template.Replace(this.tokenPropertiesTable, this.GeneratePropertiesTable(c));
            template = template.Replace(this.tokenClassTable, this.GenerateContainedClassesTable(c));
            template = template.Replace(this.tokenClassInheritanceTable, this.GenerateInheritanceClassesTable(c));

            var savepath = c.HtmlPage();

            File.WriteAllText(outputPath + "\\" + savepath, template);

            foreach (var cc in c.Classes)
            {
                this.GenerateClassPage(cc, outputPath);
            }
        }

        private string GenerateToc(Class c)
        {
            var sb = new StringBuilder();

            var containers = c.Containers();
            containers.Reverse();

            sb.AppendLine(string.Format("<img src=\"{0}\" width=\"260\"/>", "public/logo.png"));
            sb.AppendLine(string.Format("<h4>{0}</h4>",this.modelNameLiteral));
            sb.AppendLine(string.Format("<h5>Version: {0}</h5>", this.modelVersionLiteral));
            sb.AppendLine("<ul>");

            foreach (var modelClass in this.model.Classes)
            {
                if (modelClass != containers.First())
                {
                    sb.AppendLine(string.Format("<li>{0}</li>", modelClass.HtmlLinkToPage()));
                }
                else
                {
                    this.GenerateNestedToc(modelClass, containers, sb);
                }
            }

            sb.AppendLine("</ul>");

            return sb.ToString();
        }

        private void GenerateNestedToc(Class modelClass, List<Class> containers, StringBuilder sb)
        {
            sb.AppendLine(string.Format("<li>{0}<ul>", modelClass.HtmlLinkToPage()));

            if (containers.Count == 1)
            {
                foreach (var c in modelClass.Classes)
                {
                    sb.AppendLine(string.Format("<li>{0}</li>", c.HtmlLinkToPage()));
                }
            }

            for (int i = 1; i < containers.Count; i++)
            {
                sb.AppendLine(string.Format("<li>{0}<ul>", containers[i].HtmlLinkToPage()));
                
                if (i == containers.Count - 1)
                {
                    foreach (var c in containers[i].Classes)
                    {
                        sb.AppendLine(string.Format("<li>{0}</li>", c.HtmlLinkToPage()));
                    }
                }
            }

            for (int i = 1; i < containers.Count; i++)
            {
                sb.AppendLine(string.Format("</ul></li>"));
            }

            sb.AppendLine(string.Format("</ul></li>"));
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
