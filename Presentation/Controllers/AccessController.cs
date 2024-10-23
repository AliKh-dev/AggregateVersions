using AggregateVersions.Domain.DTO;
using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using AggregateVersions.Presentation.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;


namespace AggregateVersions.Presentation.Controllers
{
    public class AccessController(IAccessesService accessesService) : Controller
    {
        private readonly JsonSerializerOptions _serializerOptions = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        public async Task<IActionResult> Index()
        {
            await accessesService.SetParent();

            List<AccessResponse> accesses = await accessesService.GetAll();

            return View(BuildJsTreeStructure(accesses));
        }

        [HttpPost]
        public async Task<IActionResult> Export([FromBody] List<AccessRequest> exportRequests)
        {
            List<AccessResponse> exportResponse = await GetParents(exportRequests);

            exportResponse = exportResponse.DistinctBy(res => res.ID).ToList();

            string jsonContent = JsonSerializer.Serialize(exportResponse, _serializerOptions);

            byte[] content = Encoding.UTF8.GetBytes(jsonContent);

            return File(content, "application/json", "exported_data.json");
        }

        [HttpPost]
        public async Task<IActionResult> Import([FromBody] List<List<AccessResponse>> multipleJsonAccesses)
        {
            List<AccessResponse> accesses = [];

            foreach (List<AccessResponse> jsonAccesses in multipleJsonAccesses)
                foreach (AccessResponse access in jsonAccesses)
                    accesses.Add(access);

            accesses = accesses.DistinctBy(ac => ac.Key).ToList();

            List<Access> accessesList = accesses.Select(ac => ac.ToAccess()).ToList();
            List<Access> rootParents = accessesList.Where(ac => ac.ParentId == 0 || ac.ParentId == null).ToList();

            List<Access> result = await GetInsertList(accessesList, rootParents);

            await accessesService.Add(result);
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<Access>> GetInsertList(List<Access> accesses, List<Access> rootParents)
        {
            if (rootParents.Count == 0)
                return [];

            List<Access> insertingNodes = [];

            foreach (Access node in rootParents)
            {
                if (node.Key != null)
                    if (!await accessesService.HaveBaseKey(node.Key))
                        insertingNodes.Add(node);
            }

            List<Access> childrenNodes = [];

            foreach (Access node in rootParents)
                childrenNodes.AddRange(accesses.Where(ac => ac.ParentId == node.ID).ToList());

            insertingNodes.AddRange(await GetInsertList(accesses, childrenNodes));

            return insertingNodes;
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

        private async Task<List<AccessResponse>> GetParents(List<AccessRequest> accessRequests)
        {
            List<AccessResponse> accessesWithParents = [];

            foreach (var accessRequest in accessRequests)
            {
                AccessResponse? access = await accessesService.GetByID(accessRequest.ID);

                if (access != null)
                {
                    accessesWithParents.Add(access);

                    List<AccessResponse>? parents = await accessesService.GetParents(accessRequest);

                    if (parents == null || parents.Count == 0)
                        continue;

                    foreach (AccessResponse parent in parents)
                        accessesWithParents.Add(parent);
                }
            }
            return accessesWithParents;
        }
    }
}
