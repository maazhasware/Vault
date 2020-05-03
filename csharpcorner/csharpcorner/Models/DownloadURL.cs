using System;
using System.Collections.Generic;
using System.Text;

namespace csharpcorner.Models
{
    public class DownloadURL
    {
        public string FileName { get; set; }
        public string Url { get; set; }
        public Guid Userid { get; set; }
        public override string ToString()
        {
            return FileName;
        }
    }
}
