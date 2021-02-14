﻿using System;
using System.Collections.Generic;
using System.Xml;
using vmPing.Properties;

namespace vmPing.Classes
{
  public class Favorite
  {
    public List<string> Hostnames { get; set; }
    
    public int ColumnCount { get; set; }

    public static bool TitleExists(string title)
    {
      if (!Configuration.Exists())
      {
        return false;
      }

      var titleExists = false;

      try
      {
        var xd = new XmlDocument();
        xd.Load(Configuration.Path);

        var nodeTitleSearch = xd.SelectSingleNode($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(title)}]");
        if (nodeTitleSearch != null)
        {
          titleExists = true;
        }
      }
      catch (Exception ex)
      {
        Util.ShowError($"Failed to read configuration file. {ex.Message}");
        titleExists = false;
      }

      return titleExists;
    }


    public static List<string> GetTitles()
    {
      if (!Configuration.Exists())
      {
        return new List<string>();
      }

      var favoriteTitles = new List<string>();

      try
      {
        var xd = new XmlDocument();
        xd.Load(Configuration.Path);

        foreach (XmlNode node in xd.SelectNodes("/vmping/favorites/favorite"))
        {
          favoriteTitles.Add(node.Attributes["title"].Value);
        }
      }

      catch (Exception ex)
      {
        Util.ShowError($"Failed to read configuration file. {ex.Message}");
      }

      favoriteTitles.Sort();
      return favoriteTitles;
    }


    public static Favorite GetContents(string favoriteTitle)
    {
      if (!Configuration.Exists())
      {
        return new Favorite();
      }

      var favorite = new Favorite();

      try
      {
        favorite.Hostnames = new List<string>();

        var xd = new XmlDocument();
        xd.Load(Configuration.Path);

        var nodeFavorite = xd.SelectSingleNode($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(favoriteTitle)}]");
        if (nodeFavorite == null)
        {
          throw new Exception($"{favoriteTitle} could not be found");
        }

        favorite.ColumnCount = int.Parse(nodeFavorite.Attributes["columncount"].Value);

        foreach (XmlNode node in xd.SelectNodes($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(favoriteTitle)}]/host"))
        {
          favorite.Hostnames.Add(node.InnerText);
        }
      }

      catch (Exception ex)
      {
        Util.ShowError($"{Strings.Error_ReadConfig} {ex.Message}");
      }

      return favorite;
    }


    public static bool IsTitleInvalid(string title)
    {
      return string.IsNullOrWhiteSpace(title);
    }


    public static void Rename(string originalTitle, string newTitle)
    {
      if (Configuration.IsReady() == false)
      {
        return;
      }

      try
      {
        var xd = new XmlDocument();
        xd.Load(Configuration.Path);

        foreach (XmlNode node in xd.SelectNodes($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(originalTitle)}]"))
        {
          // Rename title attribue.
          node.Attributes["title"].Value = newTitle;
        }

        xd.Save(Configuration.Path);
      }

      catch (Exception ex)
      {
        Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
      }
    }


    public static void Save(string title, List<string> hostnames, int columnCount)
    {
      if (Configuration.IsReady() == false)
      {
        return;
      }

      try
      {
        var xd = new XmlDocument();
        xd.Load(Configuration.Path);

        var nodeRoot = xd.SelectSingleNode("/vmping/favorites");

        // Check if title already exists.
        var nodeTitleSearch = xd.SelectNodes($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(title)}]");
        foreach (XmlNode node in nodeTitleSearch)
        {
          // Title already exists.  Delete any old versions.
          nodeRoot.RemoveChild(node);
        }

        var favorite = xd.CreateElement("favorite");
        favorite.SetAttribute("title", title);
        favorite.SetAttribute("columncount", columnCount.ToString());
        foreach (var hostname in hostnames)
        {
          var xmlElement = xd.CreateElement("host");
          xmlElement.InnerText = hostname;
          favorite.AppendChild(xmlElement);
        }

        nodeRoot.AppendChild(favorite);
        xd.Save(Configuration.Path);
      }

      catch (Exception ex)
      {
        Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
      }
    }

    public static void Delete(string title)
    {
      if (!Configuration.Exists())
      {
        return;
      }

      try
      {
        var xd = new XmlDocument();
        xd.Load(Configuration.Path);

        // Search for favorite by title.
        var nodeRoot = xd.SelectSingleNode("/vmping/favorites");
        foreach (XmlNode node in xd.SelectNodes($"/vmping/favorites/favorite[@title={Configuration.GetEscapedXpath(title)}]"))
        {
          // Found title.  Delete all versions.
          nodeRoot.RemoveChild(node);
        }

        xd.Save(Configuration.Path);
      }

      catch (Exception ex)
      {
        Util.ShowError($"{Strings.Error_WriteConfig} {ex.Message}");
      }
    }
  }
}
