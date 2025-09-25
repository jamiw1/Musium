using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace localMusicPlayerTest.Models
{
    public class Song
    {
        public string Title { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public string FilePath { get; set; }
        public float Length { get; set; }
    }
}
