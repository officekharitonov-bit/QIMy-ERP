namespace QIMy.Core.Models.ExportImport;

/// <summary>
/// Модель для экспорта/импорта клиентов в формате 'Personen index'
/// Совместима с BMD и другими бухгалтерскими системами
/// </summary>
public class ClientExportModel
{
    public string Freifeld_01 { get; set; } = string.Empty;
    public int Kto_Nr { get; set; }
    public string Nachname { get; set; } = string.Empty;
    public string Freifeld_06 { get; set; } = string.Empty;
    public string Strasse { get; set; } = string.Empty;
    public string Plz { get; set; } = string.Empty;
    public string Ort { get; set; } = string.Empty;
    public string WAE { get; set; } = string.Empty;
    public string ZZiel { get; set; } = string.Empty;
    public string SktoProz1 { get; set; } = string.Empty;
    public string SktoTage1 { get; set; } = string.Empty;
    public string UID_Nummer { get; set; } = string.Empty;
    public string Freifeld_11 { get; set; } = string.Empty;
    public string Lief_Vorschlag_Gegenkonto { get; set; } = string.Empty;
    public string Freifeld_04 { get; set; } = string.Empty;
    public string Freifeld_05 { get; set; } = string.Empty;
    public string Kunden_Vorschlag_Gegenkonto { get; set; } = string.Empty;
    public string Freifeld_02 { get; set; } = string.Empty;
    public string Freifeld_03 { get; set; } = string.Empty;
    public string Filiale { get; set; } = string.Empty;
    public string Land_Nr { get; set; } = string.Empty;
    public string Warenbeschreibung { get; set; } = string.Empty;
}
