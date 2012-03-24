namespace Avina.Models
{
    using System.Web.Mvc;

    public class DataTableParameterModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            try
            {
                if (bindingContext.ModelType.IsAssignableFrom(typeof(DataTableParameterModel)))
                {
                    var model = new DataTableParameterModel();
                    model.iColumns = (int)bindingContext.ValueProvider.GetValue("iColumns").ConvertTo(typeof(int));
                    model.iDisplayLength = (int)bindingContext.ValueProvider.GetValue("iDisplayLength").ConvertTo(typeof(int));
                    model.iDisplayStart = (int)bindingContext.ValueProvider.GetValue("iDisplayStart").ConvertTo(typeof(int));
                    model.iSortingCols = (int)bindingContext.ValueProvider.GetValue("iSortingCols").ConvertTo(typeof(int));
                    model.sEcho = bindingContext.ValueProvider.GetValue("sEcho").AttemptedValue;
                    model.sColumns = bindingContext.ValueProvider.GetValue("sColumns").AttemptedValue;
                    model.sSearch = bindingContext.ValueProvider.GetValue("sSearch").AttemptedValue;
                    model.bSearchable = new bool[model.iColumns];
                    model.bSortable = new bool[model.iColumns];
                    model.iSortCol = new int[model.iSortingCols];
                    model.sSortDir = new string[model.iSortingCols];

                    for (int i = 0; i < model.iColumns; ++i)
                    {
                        model.bSearchable[i] = (bool)bindingContext.ValueProvider.GetValue("bSearchable_" + i).ConvertTo(typeof(bool));
                        model.bSortable[i] = (bool)bindingContext.ValueProvider.GetValue("bSortable_" + i).ConvertTo(typeof(bool));
                    }

                    for (int i = 0; i < model.iSortingCols; ++i)
                    {
                        model.iSortCol[i] = (int)bindingContext.ValueProvider.GetValue("iSortCol_" + i).ConvertTo(typeof(int));
                        model.sSortDir[i] = bindingContext.ValueProvider.GetValue("sSortDir_" + i).AttemptedValue;
                    }

                    return model;
                }
            }
            catch { }
            return null;
        }
    }
}