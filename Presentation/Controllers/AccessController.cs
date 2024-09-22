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

        [HttpPost]
        public async Task<IActionResult> Import([FromBody] List<List<AccessResponse>> multipleJsonAccesses)
        {
            List<AccessResponse> accesses = [];

            foreach (List<AccessResponse> jsonAccesses in multipleJsonAccesses)
                foreach (AccessResponse access in jsonAccesses)
                    accesses.Add(access);

            accesses = accesses.DistinctBy(ac => ac.Key).ToList();

            List<Access> finalAddingList = [];
            List<Access> accessesList = accesses.Select(ac => ac.ToAccess()).ToList();
            List<Access> rootParents = accessesList.Where(ac => ac.ParentId == 0).ToList();


            while (rootParents.Count > 0)
            {
                List<Access> addingList = [];
                (addingList, rootParents, accessesList) = await GetInsertList(accessesList, rootParents);
                finalAddingList.AddRange(addingList);
            }

            await accessesService.Add(finalAddingList);
            return Json(JsonSerializer.Serialize(finalAddingList));
        }

        private static List<Access> GetChildren(List<Access> accesses, Access access)
        {
            List<Access> children = accesses.Where(ac => ac.ParentId == access.ID).ToList();

            if (children.Count == 0)
                return children;

            foreach (Access child in children)
                children.AddRange(GetChildren(accesses, child));

            return children;
        }

        private async Task<(List<Access>, List<Access>, List<Access>)> GetInsertList(List<Access> accesses, List<Access> parents)
        {
            List<Access> insertedList = [];
            List<Access> uninsertedParents = [];

            foreach (Access parent in parents)
            {
                if (parent.Key != null)
                {
                    if (!await accessesService.HaveBaseKey(parent.Key))
                    {
                        List<Access> children = GetChildren(accesses, parent);
                        insertedList.Add(parent);
                        insertedList.AddRange(children);
                    }
                    else
                        uninsertedParents.Add(parent);
                }
            }

            foreach (Access access in insertedList)
                accesses.Remove(access);

            List<Access> uninsertedParentsChildren = [];

            foreach (Access parent in uninsertedParents)
                uninsertedParentsChildren.AddRange(accesses.Where(ac => ac.ParentId == parent.ID).ToList());

            return (insertedList, uninsertedParentsChildren, accesses);
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
