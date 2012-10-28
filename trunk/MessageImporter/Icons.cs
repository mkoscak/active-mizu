using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MessageImporter
{
    static class Icons
    {
        public static Image Eqipped;
        public static Image NonEquipped;
        public static Image Waiting;

        static Icons()
        {
            Eqipped = Image.FromFile(@"Resources\eq16.png");
            NonEquipped = Image.FromFile(@"Resources\non16.png");
            Waiting = Image.FromFile(@"Resources\warning16.png");
        }
    }
}
