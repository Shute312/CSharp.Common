using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Common.Excel.Export.Models
{
    public class RenderCell : Cell
    {

        public RenderCell(object value, int rowIndex, int colIndex)
            : base(value, rowIndex, colIndex)
        {
        }
    }
}