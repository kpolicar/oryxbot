using System.Xml.Linq;
using OryxBot.GameDataExtractor.Models;
using System.Runtime.CompilerServices;

namespace OryxBot.GameDataExtractor.Extraction;

public class ClusterExtractor
{
    private IEnumerable<ClusterDefinition> ExtractFromXml(string xml, string sourceFileName)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing xml {sourceFileName}: {ex.Message}");
            yield break;
        }

        var root = doc.Root;
        if (root == null) yield break;

        var clusterElements = new List<XElement>();
        
        if (root.Name.LocalName == "world")
        {
            var clustersNode = root.Element("clusters");
            if (clustersNode != null)
            {
                clusterElements.AddRange(clustersNode.Elements("cluster"));
            }
        }
        else if (root.Name.LocalName == "cluster")
        {
            clusterElements.Add(root);
        }

        foreach (var clusterEl in clusterElements)
        {
            var id = clusterEl.Attribute("id")?.Value;
            var displayName = clusterEl.Attribute("displayname")?.Value ?? string.Empty;
            var type = clusterEl.Attribute("type")?.Value ?? "UNKNOWN";
            var fileNameAttr = clusterEl.Attribute("file")?.Value ?? sourceFileName;
            
            // Parse filename if provided to extract id, type, biome, tier, faction, zone
            string? biome = null;
            string? tier = null;
            string? faction = null;
            string? zone = null;

            if (!string.IsNullOrEmpty(fileNameAttr))
            {
                var parts = fileNameAttr.Split('_');
                // Format: {id}_{type}_{biome}_{season}_{tier}_{faction}_{zone}.cluster.xml
                // Example: 4205_WRL_MN_AUTO_T5_KPR_ROY.cluster.xml
                if (string.IsNullOrEmpty(id) && parts.Length >= 1)
                {
                    id = parts[0];
                }
                if (type == "UNKNOWN" && parts.Length >= 2)
                {
                    type = parts[1];
                }
                if (parts.Length >= 7)
                {
                    biome = parts[2];
                    tier = parts[4];
                    faction = parts[5];
                    zone = parts[6].Split('.')[0];
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            var exits = new List<ExitDefinition>();
            var exitsElement = clusterEl.Element("exits");
            if (exitsElement != null)
            {
                foreach (var exitEl in exitsElement.Elements("exit"))
                {
                    var exitId = exitEl.Attribute("id")?.Value;
                    var targetIdStr = exitEl.Attribute("targetid")?.Value;
                    
                    // targetid format is usually targetExitId@targetClusterId
                    var targetClusterId = string.Empty;
                    var targetExitId = string.Empty;
                    
                    if (!string.IsNullOrEmpty(targetIdStr))
                    {
                        var targetParts = targetIdStr.Split('@');
                        if (targetParts.Length == 2)
                        {
                            targetExitId = targetParts[0];
                            targetClusterId = targetParts[1];
                        }
                        else
                        {
                            // Some exits point to a plain ID or don't use @ format
                            targetClusterId = targetIdStr;
                        }
                    }

                    if (string.IsNullOrEmpty(exitId) || string.IsNullOrEmpty(targetClusterId))
                    {
                        continue;
                    }

                    // Look for pos attribute, format is "X Y"
                    var posStr = exitEl.Attribute("pos")?.Value;
                    float px = 0;
                    float py = 0;
                    if (!string.IsNullOrEmpty(posStr))
                    {
                        var posParts = posStr.Split(' ');
                        if (posParts.Length == 2)
                        {
                            float.TryParse(posParts[0], out px);
                            float.TryParse(posParts[1], out py);
                        }
                    }

                    exits.Add(new ExitDefinition
                    {
                        ExitId = exitId,
                        TargetClusterId = targetClusterId,
                        TargetExitId = targetExitId,
                        PositionX = px,
                        PositionY = py
                    });
                }
            }

            var internalName = fileNameAttr.EndsWith(".cluster.xml") 
                ? fileNameAttr.Substring(0, fileNameAttr.Length - 12) 
                : fileNameAttr;

            var worldMapPosStr = clusterEl.Attribute("worldmapposition")?.Value;
            float? worldX = null;
            float? worldY = null;
            if (!string.IsNullOrEmpty(worldMapPosStr))
            {
                var posParts = worldMapPosStr.Split(' ');
                if (posParts.Length == 2)
                {
                    if (float.TryParse(posParts[0], out var wx)) worldX = wx;
                    if (float.TryParse(posParts[1], out var wy)) worldY = wy;
                }
            }

            yield return new ClusterDefinition
            {
                Id = id,
                DisplayName = displayName,
                Type = type,
                Biome = biome,
                Tier = tier,
                Faction = faction,
                Zone = zone,
                WorldX = worldX,
                WorldY = worldY,
                InternalName = internalName,
                Exits = exits
            };
        }
    }

    public async IAsyncEnumerable<ClusterDefinition> ExtractAllAsync(
        string directoryPath, 
        bool isEncrypted,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var files = Directory.GetFiles(directoryPath, isEncrypted ? "*.bin" : "*.xml", SearchOption.AllDirectories);
        Console.WriteLine($"[DEBUG] Directory.GetFiles found {files.Length} files in {directoryPath}");
        foreach (var file in files)
        {
            if (ct.IsCancellationRequested) yield break;

            IEnumerable<ClusterDefinition> defs = Array.Empty<ClusterDefinition>();
            try
            {
                string xmlContent;
                if (isEncrypted)
                {
                    xmlContent = await BinDecryptor.DecryptBinFileAsync(file, ct);
                }
                else
                {
                    xmlContent = await File.ReadAllTextAsync(file, ct);
                }

                defs = ExtractFromXml(xmlContent, Path.GetFileName(file));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Failed to process file {file}: {ex.Message}");
            }

            foreach (var def in defs)
            {
                yield return def;
            }
        }
    }
}
