namespace Avina.Models
{
    public class DataTableParameterModel
    {
        public DataTableParameterModel()
        {
            iDisplayStart = 0;
            iDisplayLength = 15;
            sColumns = "";
            sSearch = "";
        }

        /// <summary>
        /// Request sequence number sent by DataTable,
        /// same value must be returned in response
        /// </summary>       
        public string sEcho { get; set; }

        /// <summary>
        /// Text used for filtering
        /// </summary>
        public string sSearch { get; set; }

        /// <summary>
        /// An array of searchabilty flags, one per column
        /// </summary>
        public bool[] bSearchable { get; set; }

        /// <summary>
        /// Number of records that should be shown in table
        /// </summary>
        public int iDisplayLength { get; set; }

        /// <summary>
        /// First record that should be shown(used for paging)
        /// </summary>
        public int iDisplayStart { get; set; }

        /// <summary>
        /// Number of columns in table
        /// </summary>
        public int iColumns { get; set; }

        /// <summary>
        /// Number of columns that are used in sorting
        /// </summary>
        public int iSortingCols { get; set; }

        /// <summary>
        /// Array of sortability flags, one per column
        /// </summary>
        public bool[] bSortable { get; set; }

        /// <summary>
        /// Array of column ids to sort, one per sort column. The size of this array is iSortingCols
        /// </summary>
        public int[] iSortCol { get; set; }

        /// <summary>
        /// The direction of each sort, one per sort column. The size of this array is iSortingCols
        /// </summary>
        public string[] sSortDir { get; set; }

        /// <summary>
        /// Comma separated list of column names
        /// </summary>
        public string sColumns { get; set; }
    }
}