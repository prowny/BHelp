using System.Collections.Generic;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class ViewDocsViewModel
    {
        public IEnumerable<Document> Docs { get; set; }
        public string ImageData { get; set; }
    }
}