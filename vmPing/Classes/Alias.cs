﻿using System;
using System.Collections.Generic;
using System.Xml;
using vmPing.Properties;

namespace vmPing.Classes
{
    public class Alias
    {
        public static Dictionary<string, string> GetAliases()
        {
            if (!Configuration.Exists())
            {
              return new Dictionary<string, string>();
            }

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.Path);

                var aliases = new Dictionary<string, string>();
                foreach (XmlNode node in xd.SelectNodes("/vmping/aliases/alias"))
                {
                  aliases.Add(node.Attributes["host"].Value, node.InnerText);
                }

                return aliases;
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_LoadAlias} {ex.Message}");
                return new Dictionary<string, string>();
            }
        }


        public static void AddAlias(string hostname, string alias)
        {
            if (!Configuration.IsReady())
            {
              return;
            }

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.Path);

                var nodeRoot = xd.SelectSingleNode("/vmping/aliases");

                // Check if title already exists.
                foreach (XmlNode node in xd.SelectNodes($"/vmping/aliases/alias[@host={Configuration.GetEscapedXpath(hostname)}]"))
                {
                  // Title already exists.  Delete any old versions.
                  nodeRoot.RemoveChild(node);
                }

                var aliasEntry = xd.CreateElement("alias");
                aliasEntry.SetAttribute("host", hostname.ToUpper());
                aliasEntry.InnerText = alias;
                nodeRoot.AppendChild(aliasEntry);
                xd.Save(Configuration.Path);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_AddAlias} {ex.Message}");
            }
        }


        public static void DeleteAlias(string key)
        {
            if (!Configuration.Exists())
            {
              return;
            }

            try
            {
                var xd = new XmlDocument();
                xd.Load(Configuration.Path);

                // Search for alias.
                var nodeRoot = xd.SelectSingleNode("/vmping/aliases");
                foreach (XmlNode node in xd.SelectNodes($"/vmping/aliases/alias[@host={Configuration.GetEscapedXpath(key)}]"))
                {
                  // Found title.  Delete all versions.
                  nodeRoot.RemoveChild(node);
                }

                xd.Save(Configuration.Path);
            }

            catch (Exception ex)
            {
                Util.ShowError($"{Strings.Error_DeleteAlias} {ex.Message}");
            }
        }


        public static bool IsNameInvalid(string name)
        {
            return string.IsNullOrWhiteSpace(name);
        }


        public static bool IsHostInvalid(string hostname)
        {
            return string.IsNullOrWhiteSpace(hostname);
        }
    }
}
