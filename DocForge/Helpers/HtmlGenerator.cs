// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlGenerator.cs" company="Red Hammer Studios">
//   Copyright (c) 2015 Red Hammer Studios
// </copyright>
// <summary>
//   Defines the HtmlGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DocForge.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ClassForge.Model;

    /// <summary>
    /// The html generator.
    /// </summary>
    internal class HtmlGenerator
    {
        /// <summary>
        /// The token page title.
        /// </summary>
        private const string tokenPageTitle = "[$pagetitle]";

        /// <summary>
        /// The token class name.
        /// </summary>
        private const string tokenClassName = "[$classname]";

        /// <summary>
        /// The token class inherits.
        /// </summary>
        private const string tokenClassInherits = "[$classinherits]";

        /// <summary>
        /// The parent class
        /// </summary>
        private const string tokenClassParent = "[$classparent]";

        /// <summary>
        /// The class comment
        /// </summary>
        private const string tokenClassRemark = "[$classremark]";

        /// <summary>
        /// The properties table.
        /// </summary>
        private const string tokenPropertiesTable = "[$properties]";

        /// <summary>
        /// The containment class table.
        /// </summary>
        private const string tokenClassTable = "[$classes]";

        /// <summary>
        /// The class inheritance table.
        /// </summary>
        private const string tokenClassInheritanceTable = "[$classesinh]";

        /// <summary>
        /// The author name.
        /// </summary>
        private const string tokenAuthorName = "[$modelname]";

        /// <summary>
        /// The model version.
        /// </summary>
        private const string tokenModelVersion = "[$modelversion]";

        /// <summary>
        /// The project name.
        /// </summary>
        private const string tokenProjectName = "[$modeldescription]";

        /// <summary>
        /// The class digram json.
        /// </summary>
        private const string tokenDigramJson = "[$digramjson]";

        /// <summary>
        /// The date.
        /// </summary>
        private const string tokenDate = "[$date]";

        /// <summary>
        /// The top level filters include table.
        /// </summary>
        private const string tokenTopLevel = "[$toplevel]";

        /// <summary>
        /// The mid level class exclude table.
        /// </summary>
        private const string tokenMidLevel = "[$midlevel]";

        /// <summary>
        /// The property includes table.
        /// </summary>
        private const string tokenLowLevel = "[$lowlevel]";

        /// <summary>
        /// The model diagram json.
        /// </summary>
        private const string tokenModelDiagram = "[$modelgraph]";

        /// <summary>
        /// The table of contents.
        /// </summary>
        private const string tokenToc = "[$toc]";

        /// <summary>
        /// The model.
        /// </summary>
        private Model model;

        /// <summary>
        /// The project name.
        /// </summary>
        private string projectNameLiteral;

        /// <summary>
        /// The model version.
        /// </summary>
        private string modelVersionLiteral;

        /// <summary>
        /// The author name.
        /// </summary>
        private string authorNameLiteral;

        /// <summary>
        /// Generates all the documentation
        /// </summary>
        /// <param name="m">
        /// The <see cref="Model"/>.
        /// </param>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        /// <param name="authorName">
        /// The author name.
        /// </param>
        /// <param name="modelVersion">
        /// The model version.
        /// </param>
        /// <param name="projectName">
        /// The project name.
        /// </param>
        /// <param name="toplevel">
        /// The toplevel.
        /// </param>
        /// <param name="midlevel">
        /// The midlevel.
        /// </param>
        /// <param name="lowlevel">
        /// The lowlevel.
        /// </param>
        public void GenerateDocumentation(Model m, string outputPath, string authorName, string modelVersion, string projectName, string[] toplevel, string[] midlevel, string[] lowlevel)
        {
            this.model = m;
            this.projectNameLiteral = projectName;
            this.modelVersionLiteral = modelVersion;
            this.authorNameLiteral = authorName;

            string template = File.ReadAllText("Templates\\indexpage.html");

            // replace classname
            template = template.Replace(tokenPageTitle, "Class Documentation");
            template = template.Replace(tokenAuthorName, authorName);
            template = template.Replace(tokenModelVersion, modelVersion);
            template = template.Replace(tokenProjectName, projectName);
            template = template.Replace(tokenModelDiagram, this.model.ModelDiagramJSON(projectName));
            template = template.Replace(tokenTopLevel, string.Join(", ", toplevel));
            template = template.Replace(tokenMidLevel, string.Join(", ", midlevel));
            template = template.Replace(tokenLowLevel, string.Join(", ", lowlevel));
            template = template.Replace(tokenDate, DateTime.Now.ToShortDateString());
           
            const string savepath = "index.html";

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

        /// <summary>
        /// Copy the directory
        /// </summary>
        /// <param name="sourceDirName">
        /// The source dir name.
        /// </param>
        /// <param name="destDirName">
        /// The dest dir name.
        /// </param>
        /// <param name="copySubDirs">
        /// The copy sub dirs.
        /// </param>
        /// <exception cref="DirectoryNotFoundException">
        /// Excption thrown due to source directory not being found.
        /// </exception>
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);
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
                    DirectoryCopy(subdir.FullName, temppath);
                }
            }
        }

        /// <summary>
        /// Generate a class page.
        /// </summary>
        /// <param name="c">
        /// The class.
        /// </param>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        public void GenerateClassPage(Class c, string outputPath)
        {
            string template = File.ReadAllText("Templates\\classpage.html");

            var toc = this.GenerateToc(c);

            template = template.Replace(tokenToc, toc);

            // replace classname
            template = template.Replace(tokenPageTitle, c.Name);
            template = template.Replace(tokenClassName, c.Name);
            template = template.Replace(tokenDigramJson, c.ClassDiagramJSON());
            template = template.Replace(tokenClassInherits, c.InheritanceClass == null ? c.Inherits : c.InheritanceClass.HtmlLinkToPage());
            template = template.Replace(tokenClassParent, c.ContainmentParent == null ? string.Empty : c.ContainmentBreadcrumb());
            template = template.Replace(tokenClassRemark, c.Remark);
            template = template.Replace(tokenPropertiesTable, this.GeneratePropertiesTable(c));
            template = template.Replace(tokenClassTable, this.GenerateContainedClassesTable(c));
            template = template.Replace(tokenClassInheritanceTable, this.GenerateInheritanceClassesTable(c));
            template = template.Replace(tokenModelDiagram, c.ModelDiagramJSON());
            template = template.Replace(tokenAuthorName, this.authorNameLiteral);
            template = template.Replace(tokenDate, DateTime.Now.ToShortDateString());

            var savepath = c.HtmlPage();

            File.WriteAllText(outputPath + "\\" + savepath, template);

            foreach (var cc in c.Classes)
            {
                this.GenerateClassPage(cc, outputPath);
            }
        }

        /// <summary>
        /// Generate table of contents
        /// </summary>
        /// <param name="c">
        /// The class.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GenerateToc(Class c)
        {
            var sb = new StringBuilder();

            var containers = c.Containers();
            containers.Reverse();

            // some code sb.AppendLine(string.Format("<img src=\"{0}\" width=\"260\"/>", "public/logo.png"));
            sb.AppendLine(string.Format("<h4><a href=\"index.html\">{0}</a></h4>", this.projectNameLiteral));
            sb.AppendLine(string.Format("<h5>Version: {0}</h5>", this.modelVersionLiteral));
            sb.AppendLine("<ul class=\"nav nav-pills nav-stacked\">");

            foreach (var modelClass in this.model.Classes)
            {
                if (modelClass != containers.First())
                {
                    sb.AppendLine(string.Format("<li>{0}</li>", modelClass.HtmlLinkToTopPage()));
                }
                else
                {
                    this.GenerateNestedToc(modelClass, containers, sb);
                }
            }

            sb.AppendLine("</ul>");

            return sb.ToString();
        }

        /// <summary>
        /// Generate the nested elements in the table of contents
        /// </summary>
        /// <param name="modelClass">
        /// The model class.
        /// </param>
        /// <param name="containers">
        /// The containers.
        /// </param>
        /// <param name="sb">
        /// The sb.
        /// </param>
        private void GenerateNestedToc(Class modelClass, List<Class> containers, StringBuilder sb)
        {
            sb.AppendLine(string.Format("<li>{0}<ul class=\"nav nav-pills nav-stacked\">", modelClass.HtmlLinkToTopPageFocused()));

            if (containers.Count == 1)
            {
                foreach (var c in modelClass.Classes)
                {
                    sb.AppendLine(string.Format("<li>{0}</li>", c.HtmlLinkToPage()));
                }
            }

            for (int i = 1; i < containers.Count; i++)
            {
                if (i == containers.Count - 1)
                {
                    sb.AppendLine(string.Format("<li>{0}<ul class=\"nav nav-pills nav-stacked\">", containers[i].HtmlLinkToPageFocused()));
                    foreach (var c in containers[i].Classes)
                    {
                        sb.AppendLine(string.Format("<li>{0}</li>", c.HtmlLinkToPage()));
                    }
                }
                else
                {
                    sb.AppendLine(string.Format("<li>{0}<ul class=\"nav nav-pills nav-stacked\">", containers[i].HtmlLinkToPage()));
                }
            }

            for (int i = 1; i < containers.Count; i++)
            {
                sb.AppendLine(string.Format("</ul></li>"));
            }

            sb.AppendLine(string.Format("</ul></li>"));
        }

        /// <summary>
        /// Generate properties table.
        /// </summary>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
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

        /// <summary>
        /// Generate contained classes table.
        /// </summary>
        /// <param name="c">
        /// The class.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
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

        /// <summary>
        /// Generate inheritance classes table.
        /// </summary>
        /// <param name="c">
        /// The class.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
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
