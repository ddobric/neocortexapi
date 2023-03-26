using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCortexApiSample
{
    public class ExcelWriter
    {
        private static int rowNum = 1; // initialize row number to 1
        string fileName = "Accuracy_Output.xlsx";

        public void WriteAccuracy(string sequenceKey , double accuracy)
        {
            // Create a new Excel package and worksheet
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Accuracy Data");

                // Get the last row number and increment it by 1
                int lastRowNum = worksheet.Dimension?.End.Row ?? 0;
                rowNum = lastRowNum + 1;

                // Write the sequence key and accuracy to the new row in the worksheet
                worksheet.Cells[rowNum, 1].Value = sequenceKey;
                worksheet.Cells[rowNum, 2].Value = accuracy;

                // Save the Excel package to a file
                FileInfo fileInfo = new FileInfo(fileName);
                package.SaveAs(fileInfo);
            }
        }
    }

}
