namespace Sitecore.Support.ItemWebApi.Pipelines.Request
{
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.ItemWebApi;
  using Sitecore.ItemWebApi.Pipelines.Request;
  using Sitecore.Web;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.RegularExpressions;

  public class ResolveScope : RequestProcessor
  {
    private static ID PublishingTargetTemplateID = 
      new ID(
        string.IsNullOrEmpty(Settings.GetSetting("Sitecore.Support.PublishingTargetTemplateID")) ? "E130C748-C13B-40D5-B6C6-4B150DC3FAB3" : Settings.GetSetting("Sitecore.Support.PublishingTargetTemplateID"));
    public override void Process(RequestArgs arguments)
    {
      Assert.ArgumentNotNull(arguments, "arguments");
      arguments.Scope = ResolveScope.GetScope(arguments.Items);
    }

    private static bool CanReadItem(Item item)
    {
      return item.Access.CanRead() && (Sitecore.Context.Site.Name != "shell" || item.Access.CanReadLanguage() || item.TemplateID == PublishingTargetTemplateID);
    }

    private static string[] GetAxes()
    {
      string queryString = WebUtil.GetQueryString("scope", null);
      if (queryString == null)
      {
        return new string[]
        {
                    "s"
        };
      }
      string[] array = queryString.Split(new char[]
      {
                '|'
      });
      Regex regex = new Regex("^[cps]{1}$");
      List<string> list = new List<string>();
      string[] array2 = array;
      for (int i = 0; i < array2.Length; i++)
      {
        string text = array2[i];
        string text2 = text.ToLower().Trim();
        if (regex.IsMatch(text2) && !list.Contains(text2))
        {
          list.Add(text2);
        }
      }
      if (list.Count <= 0)
      {
        return new string[]
        {
                    "s"
        };
      }
      return list.ToArray();
    }

    private static IEnumerable<Item> GetItemsByAxe(Item item, string axe)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(axe, "axe");
      if (axe != null)
      {
        if (axe == "c")
        {
          return item.GetChildren();
        }
        if (!(axe == "p"))
        {
          if (axe == "s")
          {
            return new Item[]
            {
                            item
            };
          }
        }
        else
        {
          if (item.Parent == null)
          {
            return new Item[0];
          }
          return new Item[]
          {
                        item.Parent
          };
        }
      }
      throw new FormatException("Unknown axe value.");
    }

    private static IEnumerable<Item> GetScope(Item item, IEnumerable<string> axes)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(axes, "axes");
      List<Item> list = new List<Item>();
      foreach (string current in axes)
      {
        list.AddRange(ResolveScope.GetItemsByAxe(item, current));
      }
      return list.ToArray();
    }

    private static Item[] GetScope(Item[] items)
    {
      Assert.ArgumentNotNull(items, "items");
      if (items.Length == 0)
      {
        Logger.Warn("Cannot resolve the scope because the item set is empty.");
        return new Item[0];
      }
      List<Item> list = new List<Item>();
      string[] axes = ResolveScope.GetAxes();
      for (int i = 0; i < items.Length; i++)
      {
        Item item = items[i];
        list.AddRange(ResolveScope.GetScope(item, axes));
      }
      return list.Where(new Func<Item, bool>(ResolveScope.CanReadItem)).ToArray<Item>();
    }
  }
}