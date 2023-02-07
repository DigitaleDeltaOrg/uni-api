namespace UniApiExportFormats;

using DigitaleDelta;
using Observation = DatabaseModel.Models.Observation;

public interface IUniApiExport<T> where T: IBaseResponseObject
{
	Task<List<T>> GenerateExportDataAsync(List<Observation> data, Dictionary<long, DatabaseModel.Models.Reference> references);
}