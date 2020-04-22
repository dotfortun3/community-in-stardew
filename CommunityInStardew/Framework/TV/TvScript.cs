using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityInStardew.Television
{
    public class TvShowConfig
    {
        public string ShowId { get; set; }
        public string ShowTitle { get; set; }
        public string TexturePath { get; set; }
        public TvScript[] Scripts { get; set; }


    }
    public class TvScript
    {
        public string EpisodeTitle { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public ScriptPage[] Pages { get; set; }
    }

    public class ScriptPage
    {
        public string Text { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
