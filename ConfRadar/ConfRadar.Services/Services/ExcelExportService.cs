using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Services
{
    public interface IExcelExportService
    {
        /// <summary>
        /// Xuất một danh sách đối tượng bất kỳ ra file Excel.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu của các đối tượng trong danh sách.</typeparam>
        /// <param name="data">Danh sách dữ liệu cần xuất.</param>
        /// <param name="sheetName">Tên của sheet trong file Excel.</param>
        /// <returns>Mảng byte của file Excel.</returns>
        Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName);
    }
    public class ExcelExportService : IExcelExportService
    {
        public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName)
        {
            ExcelPackage.License.SetNonCommercialPersonal("<My Name>");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(sheetName);

                // Nếu không có dữ liệu, trả về file trống
                if (data == null || !data.Any())
                {
                    return await package.GetAsByteArrayAsync();
                }

                // Lấy ra các thuộc tính của đối tượng T để làm header
                var properties = typeof(T).GetProperties();

                // Ghi header vào dòng đầu tiên
                for (int i = 0; i < properties.Length; i++)
                {
                    // Ưu tiên lấy tên từ [DisplayName] attribute, nếu không có thì dùng tên thuộc tính
                    var displayNameAttribute = properties[i].GetCustomAttributes(typeof(DisplayNameAttribute), true)
                                                            .FirstOrDefault() as DisplayNameAttribute;
                    worksheet.Cells[1, i + 1].Value = displayNameAttribute?.DisplayName ?? properties[i].Name;
                }

                // Ghi dữ liệu từ các dòng tiếp theo
                int row = 2;
                foreach (var item in data)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        worksheet.Cells[row, i + 1].Value = properties[i].GetValue(item);
                    }
                    row++;
                }

                // Tự động điều chỉnh độ rộng cột cho đẹp
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
        }
    }
}
