﻿// Copyright (C)2024 Nick Kastellanos

using System;
using SharpDX;

namespace Content.Pipeline.Editor
{
    public struct Package
    {
        public string Name;
        public string Version;

        public static Package Parse(string packageReference)
        {
            packageReference.Trim();

            Package package;
            package.Name = packageReference;
            package.Version = String.Empty;

            string[] split = packageReference.Split(' ');
            if (split.Length == 2)
            {
                package.Name = split[0].Trim();
                package.Version = split[1].Trim();
            }

            return package;
        }

        public override string ToString()
        {
            string result = this.Name;
            if (this.Version != String.Empty)
                result += " " + this.Version;

            return result;
        }
    }
}