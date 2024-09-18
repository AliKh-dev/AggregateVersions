using AggregateVersions.Domain.DTO;
using AggregateVersions.Domain.Interfaces;
using AggregateVersions.Presentation.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;


namespace AggregateVersions.Presentation.Controllers
{
    public class AccessController(IAccessesService accessesService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            await accessesService.SetParent();

            List<AccessResponse> accesses = await accessesService.GetAll();

            return View(BuildJsTreeStructure(accesses));
        }

        [HttpPost]
        public async Task<IActionResult> Export([FromBody] List<AccessRequest> exportRequests)
        {
            List<AccessResponse> exportResponse = [];

            foreach (var exportRequest in exportRequests)
            {
                AccessResponse? access = await accessesService.GetByID(exportRequest.ID);

                if (access != null)
                {
                    exportResponse.Add(access);

                    List<AccessResponse>? parents = await accessesService.GetParents(exportRequest);

                    if (parents == null)
                        continue;

                    foreach (AccessResponse parent in parents)
                        exportResponse.Add(parent);
                }
            }

            exportResponse = exportResponse.DistinctBy(res => res.ID).ToList();

            string jsonContent = JsonSerializer.Serialize(exportResponse);

            byte[] content = Encoding.UTF8.GetBytes(jsonContent);

            return File(content, "application/json", "exported_data.json");
        }

        private static List<AccessNode> BuildJsTreeStructure(List<AccessResponse> accessList)
        {
            Dictionary<long, AccessNode> nodeDictionary = [];

            foreach (AccessResponse access in accessList)
            {
                nodeDictionary[access.ID] = new AccessNode
                {
                    ID = access.ID.ToString(),
                    DisplayName = access.Title ?? "Unnamed Access",
                    Children = [],
                    Parents = []
                };
            }

            List<AccessNode> rootNodes = [];

            foreach (AccessResponse access in accessList)
            {
                if (access.ParentId.HasValue && access.ParentId != 0 && nodeDictionary.TryGetValue(access.ParentId.Value, out AccessNode? parentNode))
                {
                    parentNode.Children?.Add(nodeDictionary[access.ID]);
                }
                else
                    rootNodes.Add(nodeDictionary[access.ID]);
            }

            return rootNodes;
        }

    }
}
