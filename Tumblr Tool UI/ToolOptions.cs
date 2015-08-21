﻿/* 01010011 01101000 01101001 01101110 01101111  01000001 01101101 01100001 01101011 01110101 01110011 01100001
 *
 *  Project: Tumblr Tools - Image parser and downloader from Tumblr blog system
 *
 *  Author: Shino Amakusa
 *
 *  Created: 2013
 *
 *  Last Updated: August, 2015
 *
 * 01010011 01101000 01101001 01101110 01101111  01000001 01101101 01100001 01101011 01110101 01110011 01100001 */

namespace Tumblr_Tool
{
    public class ToolOptions
    {
        public string apiMode { get; set; }

        public bool parseGIF { get; set; }

        public bool parseJPEG { get; set; }

        public bool parseOnly { get; set; }

        public bool parsePhotoSets { get; set; }

        public bool parsePNG { get; set; }

        public bool generateLog { get; set; }
    }
}