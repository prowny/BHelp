﻿namespace BHelp.ViewModels
{
    public class DocumentsViewModel
    {
        public int Id { get; set; }
        public string[,] DocNames { get; set; }
        public int DocNamesUpperBound { get; set; }
    }
}