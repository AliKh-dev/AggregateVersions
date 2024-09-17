using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using AggregateVersions.Presentation.Models;
using Microsoft.AspNetCore.Mvc;


namespace AggregateVersions.Presentation.Controllers
{
    public class AccessController(IAccessesService accessesService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            await accessesService.SetParent();

            List<Access> accesses = await accessesService.GetAll();

            return View(BuildJsTreeStructure(accesses));
        }

        private static List<AccessNode> BuildJsTreeStructure(List<Access> accessList)
        {
            Dictionary<long, AccessNode> nodeDictionary = [];

            foreach (Access access in accessList)
            {
                nodeDictionary[access.ID] = new AccessNode
                {
                    ID = access.Guid.ToString(),
                    DisplayName = access.Title ?? "Unnamed Access",
                    Nodes = []
                };
            }

            List<AccessNode> rootNodes = [];

            foreach (Access access in accessList)
            {
                if (access.ParentId.HasValue && access.ParentId != 0 && nodeDictionary.TryGetValue(access.ParentId.Value, out AccessNode? parentNode))
                {
                    parentNode.Nodes?.Add(nodeDictionary[access.ID]);
                }
                else
                    rootNodes.Add(nodeDictionary[access.ID]);
            }

            return rootNodes;
        }
    }
}
