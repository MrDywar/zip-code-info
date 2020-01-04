using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZCI.Common.DTO;

namespace ZCI.Domain
{
    public interface IZipInfoService
    {
        Task<ZipCodeInfoDTO> GetZipCodeInfoAsync(string zipCode);
    }
}
