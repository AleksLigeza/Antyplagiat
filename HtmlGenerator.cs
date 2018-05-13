using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plagiat
{
    public class HtmlGenerator
    {
        private string baseText;
        private List<string>[] matches;

        public void AddText(string text)
        {
            baseText = text;
        }

        public void AddMaches(List<string> newMatches, int[] matchGroups)
        {
            matches = new List<string>[matchGroups.Length];
            for (int j = 0; j < matchGroups.Length; j++)
            {
                foreach (var list in matches)
                {
                    matches[j] = new List<string>();
                }
            }


            var orderedMatches = newMatches.OrderByDescending(x => x.Split(' ').Length).Distinct().ToList();

            var i = 0;
            foreach(var match in orderedMatches)
            {
                if(match.Split(' ').Length < matchGroups[i])
                {
                    i++;
                }
                matches[i].Add(match);
            }
        }

        public string GenerateHtml()
        {
            string start = "<!doctype html>" +
                           "<html>" +
                           "<head>" +
                           "  <meta charset=\"utf-8\">" +
                           "" +
                           "  <title>The HTML5 view</title>" +
                           "<script>findString = function findText(text) { window.find(text);document.getElementById(text);copyText.select();document.execCommand('copy');}</script>" +
                           "</head>" +
                           "" +
                           "<body>";
            string end = "</ body >" +
                         "</ html >";

            StringBuilder matchesBuilder = new StringBuilder("<br /><br /><br />");
            foreach (var list in matches)
            {
                foreach (var match in list)
                {
                    //matchesBuilder.Append("<button type=\"button\" onClick='findString(\"" + match + "\")'>" + match.ToUpper() + "</button><br />");
                    matchesBuilder.Append(match + "<br />");
                }
            }
            return start 
                   + baseText
                   + matchesBuilder.ToString() 
                   + end;
        }
    }
}
