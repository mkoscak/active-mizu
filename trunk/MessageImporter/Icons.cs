﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MessageImporter
{
    static class Icons
    {
        public static Image Complete;
        public static Image NonComplete;
        public static Image Warning;

        static Icons()
        {
            Complete = Image.FromFile(@"Resources\eq16.png");
            NonComplete = Image.FromFile(@"Resources\non16.png");
            Warning = Image.FromFile(@"Resources\warning16.png");
        }
    }
}
