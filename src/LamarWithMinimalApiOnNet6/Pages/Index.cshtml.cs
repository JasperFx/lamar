namespace LamarWithMinimalApiOnNet6.Pages
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ITest test;

        public string Text { get; set; }

        public IndexModel(ILogger<IndexModel> logger, ITest test)
        {
            _logger = logger;
            this.test = test;
        }

        public void OnGet()
        {
            this.Text = this.test.SayHello();
        }
    }
}