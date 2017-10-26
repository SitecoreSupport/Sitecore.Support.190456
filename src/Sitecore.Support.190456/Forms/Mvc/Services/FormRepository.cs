namespace Sitecore.Support.Forms.Mvc.Services
{
  using System;
  using System.Web;
  using Sitecore;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using System.Collections.Generic;
  using Sitecore.Forms.Mvc.Data.Wrappers;
  using Sitecore.Forms.Mvc.Interfaces;
  using Sitecore.Forms.Mvc.Models;
  using Sitecore.Web;

  public class FormRepository : IRepository<FormModel>
  {
    private readonly Dictionary<Guid, FormModel> models = new Dictionary<Guid, FormModel>();

    public FormRepository(IRenderingContext renderingContext)
    {
      Assert.ArgumentNotNull(renderingContext, "renderingContext");
      this.RenderingContext = renderingContext;
    }

    public FormModel GetModel()
    {
      return this.GetModel(this.RenderingContext.Rendering.UniqueId);
    }
      
    #region Modified code

    public FormModel GetModel(Guid uniqueId)
    {
      string str;
      if ((uniqueId != Guid.Empty) && this.models.ContainsKey(uniqueId))
      {
        return (FormModel)this.models[uniqueId].Clone();
      }
      string dataSource = this.RenderingContext.Rendering.DataSource;
      if (!string.IsNullOrEmpty(dataSource) && ID.IsID(dataSource))
      {
        str = dataSource;
      }
      else
      {
        str = this.RenderingContext.Rendering.Parameters[Sitecore.Forms.Mvc.Constants.FormId];
      }
      if (!ID.IsID(str))
      {
        return null;
      }
      ID id = ID.Parse(str);
      Item item = this.RenderingContext.Database.GetItem(id);
      // Sitecore Support Fix #190456
      if (item == null)
      {
        Log.Error($"Form item: {id}, cannot be found in database ", this);
        return new FormModel(uniqueId);
      }
      // Sitecore Support Fix #190456
      FormModel model = new FormModel(uniqueId, item)
      {
        ReadQueryString = MainUtil.GetBool(this.RenderingContext.Rendering.Parameters[Sitecore.Forms.Mvc.Constants.ReadQueryString], false),
        QueryParameters = HttpUtility.ParseQueryString(WebUtil.GetQueryString())
      };
      this.models.Add(uniqueId, model);
      return model;
    }
    
    #endregion

    public IRenderingContext RenderingContext { get; private set; }
  }
}