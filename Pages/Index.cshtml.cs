using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _1000Problems.Data;
using _1000Problems.Models;

namespace _1000Problems.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationRepository _repository;

        public IndexModel(ApplicationRepository repository)
            {
                    _repository = repository;
                        }

                            public List<Application> Applications { get; set; } = new();

                                [BindProperty(SupportsGet = true)]
                                    public string? SearchTerm { get; set; }

                                        public string? ErrorMessage { get; set; }

                                            public async Task OnGetAsync(string? search)
                                                {
                                                        SearchTerm = search;
                                                                try
                                                                        {
                                                                                    Applications = await _repository.GetActiveApplicationsAsync(search);
                                                                                            }
                                                                                                    catch (Exception ex)
                                                                                                            {
                                                                                                                        ErrorMessage = "Unable to connect to the database. Please try again later.";
                                                                                                                                    Applications = new List<Application>();
                                                                                                                                            }
                                                                                                                                                }
                                                                                                                                                }
                                                                                                                                                
