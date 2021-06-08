using CobaltConverter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using static Cobalt.Bindings.STB.ImageLoader;

namespace CobaltConverter
{
    public class Program
    {
        static void Main(string[] args)
        {
            int idx = 0;
            foreach(string arg in args)
            {
                if(arg == "-combineorm")
                {
                    string rmpath = args[idx + 1];
                    string opath = args[idx + 2];

                    ImagePayload ORMPayload = ImageConverter.ConvertToORM(rmpath, opath);

                    SaveImageAsPNG("ORMTex.png", ORMPayload);
                }
                idx++;
            }
        }
    }
}
