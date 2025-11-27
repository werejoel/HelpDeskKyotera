using HelpDeskKyotera.Services;
using HelpDeskKyotera.ViewModels;
using HelpDeskKyotera.ViewModels.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskKyotera.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClaimsController : Controller
    {
        private readonly IClaimsService _claimsService;
        private readonly ILogger<ClaimsController> _logger;

        public ClaimsController(IClaimsService claimsService, ILogger<ClaimsController> logger)
        {
            _claimsService = claimsService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? search, string? category, int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                var result = await _claimsService.GetPagedClaimsAsync(search, category, pageNumber, pageSize);

                ViewBag.Search = search;
                ViewBag.Category = category;

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading claims list");
                TempData["Error"] = "Something went wrong while fetching claims.";
                return View(new PagedResult<ClaimListItemViewModel>());
            }
        }

        public IActionResult Create()
        {
            return View(new ClaimEditViewModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClaimEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _claimsService.CreateClaimAsync(model);
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating claim");
                TempData["Error"] = "An unexpected error occurred.";
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var claim = await _claimsService.GetClaimByIdAsync(id);
                if (claim == null)
                {
                    TempData["Error"] = "Claim not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading claim for edit");
                TempData["Error"] = "An error occurred while loading the claim.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClaimEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _claimsService.UpdateClaimAsync(model);
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating claim");
                TempData["Error"] = "An unexpected error occurred.";
                return View(model);
            }
        }

        // GET: Delete
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var claim = await _claimsService.GetClaimByIdAsync(id);
                if (claim == null)
                {
                    TempData["Error"] = "Claim not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading claim for deletion");
                TempData["Error"] = "Something went wrong while preparing deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var success = await _claimsService.DeleteClaimAsync(id);
                if (success)
                {
                    TempData["Success"] = "Claim deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to delete claim.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting claim");
                TempData["Error"] = "Something went wrong while deleting claim.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
