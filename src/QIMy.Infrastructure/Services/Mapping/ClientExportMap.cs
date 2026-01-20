using CsvHelper.Configuration;
using QIMy.Core.Models.ExportImport;

namespace QIMy.Infrastructure.Services.Mapping;

public class ClientExportMap : ClassMap<ClientExportModel>
{
    public ClientExportMap()
    {
        Map(m => m.Freifeld_01).Name("Freifeld 01");
        Map(m => m.Kto_Nr).Name("Kto-Nr");
        Map(m => m.Nachname).Name("Nachname");
        Map(m => m.Freifeld_06).Name("Freifeld 06");
        Map(m => m.Strasse).Name("Straße");
        Map(m => m.Plz).Name("Plz");
        Map(m => m.Ort).Name("Ort");
        Map(m => m.WAE).Name("WAE");
        Map(m => m.ZZiel).Name("ZZiel");
        Map(m => m.SktoProz1).Name("SktoProz1");
        Map(m => m.SktoTage1).Name("SktoTage1");
        Map(m => m.UID_Nummer).Name("UID-Nummer");
        Map(m => m.Freifeld_11).Name("Freifeld 11");
        Map(m => m.Lief_Vorschlag_Gegenkonto).Name("Lief-Vorschlag Gegenkonto");
        Map(m => m.Freifeld_04).Name("Freifeld 04");
        Map(m => m.Freifeld_05).Name("Freifeld 05");
        Map(m => m.Kunden_Vorschlag_Gegenkonto).Name("Kunden-Vorschlag Gegenkonto");
        Map(m => m.Freifeld_02).Name("Freifeld 02");
        Map(m => m.Freifeld_03).Name("Freifeld 03");
        Map(m => m.Filiale).Name("filiale");
        Map(m => m.Land_Nr).Name("Land-Nr");
        Map(m => m.Warenbeschreibung).Name("Warenbeschreibung");
    }
}
