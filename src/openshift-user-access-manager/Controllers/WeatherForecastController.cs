using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Mvc;
using UserAccessManager.Services.Apicurio;
using Octokit;
using System.Text.Json;
using System.Xml;

namespace UserAccessManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IApiCurioRegistryClient _apiCurioRegistryClient;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IApiCurioRegistryClient apiCurioRegistryClient)
        {
            _logger = logger;
            _apiCurioRegistryClient = apiCurioRegistryClient;   
        }


        //[HttpGet(Name = "GetWeatherForecast")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add([FromBody] V1RoleBinding roleBinding)
        {
            var getSchema = _apiCurioRegistryClient.GetArtifactContentByGlobalIdAsync("17");
           
            string token = "";
            var github = new GitHubClient(new ProductHeaderValue("ocp-accessrequest"));
            github.Credentials = new Credentials(token);
            var user = await github.User.Get("sookeke");
            Console.WriteLine($"sookeke has {user.PublicRepos}");
            Console.WriteLine(user.Followers + " folks love the half ogre!");
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            //var config = KubernetesClientConfiguration
            //                .BuildConfigFromConfigFile(Environment.GetEnvironmentVariable("KUBECONFIG"));

            roleBinding.Initialize();
            config.SkipTlsVerify = true;
            var client = new Kubernetes(config);

            var roleBindingList = client.ListNamespacedRoleBinding("0137d5-dev");

            var roleexisit = roleBindingList.Items.Where(n => n == roleBinding);
            if (roleexisit == null)
            {
                roleBindingList.Items.Add(roleBinding);
            }
            return Ok(roleBindingList);

        }
        [HttpGet(Name = "GetRoleBindingList")]
        public async Task<IActionResult>  GetRbac()
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            //var config = KubernetesClientConfiguration
            //                .BuildConfigFromConfigFile(Environment.GetEnvironmentVariable("KUBECONFIG"));

            config.SkipTlsVerify = true;
            var client = new Kubernetes(config);
            var roleBindingList = client.ListNamespacedRoleBinding("0137d5-dev");


            var getSchema = _apiCurioRegistryClient.GetArtifactContentByGlobalIdAsync("17");

            string token = "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";
            var github = new GitHubClient(new ProductHeaderValue("ocp-accessrequest"));
            github.Credentials = new Credentials(token);
            var user = await github.User.Get("sookeke");
            Console.WriteLine($"sookeke has {user.PublicRepos}");
            

            var userRolebinding = roleBindingList.Items.Where(n => n.Subjects.Any(n => n.Kind.Equals("User", StringComparison.CurrentCultureIgnoreCase))).ToList();

            var userExist = userRolebinding.SelectMany(x => x.Subjects.Where(n => n.Name == "sookeke@github" && x.RoleRef.Name == "admin")).ToList().FirstOrDefault();


            var gg = from g in userRolebinding
                     from a in g.Subjects
                     where a.Name == "sookeke@github" && g.RoleRef.Name == "admin"
                     select a;

            var gg1 = from g in userRolebinding
                     from a in g.Subjects
                     where a.Name == "sookeke" && g.RoleRef.Name == "admin"
                     select a;

            foreach (var item in userRolebinding)
            {
                
               
                for (int i = 0; i < item.Subjects.Count; i++)
                {

                    //}
                    //foreach (var sub in item.Subjects)
                    //{
                    if (item.Subjects[i].Kind == "ServiceAccount")
                    {
                        item.Subjects.Remove(item.Subjects[i]);
                        //Console.WriteLine(item.Kind);
                    }
                }

            }
            var sub = new V1Subject
            {
                Name = "abc@github.com",
                ApiGroup  = "rbac.authorization.k8s.io",
                Kind = "User",
                NamespaceProperty = ""

            };
            userRolebinding.Add(new V1RoleBinding
            {
                RoleRef = new V1RoleRef
                {
                    ApiGroup = "rbac.authorization.k8s.io",
                    Name = "",//admin,edit,devops
                    Kind = "ClusterRole",

                },
                Metadata = new V1ObjectMeta
                {
                    Name = "",
                    NamespaceProperty = "",
                    Labels = new Dictionary<string, string>() { {"","" } }
                },
                Subjects = new List<V1Subject>() { sub }
            });

           
            var rb = new V1RoleBindingList()
            {
                ApiVersion = "rbac.authorization.k8s.io/v1",

            };
            rb.Items = userRolebinding;

            rb.Validate();
            var s = KubernetesYaml.Serialize(rb);
            // _logger.Log(s.ToString(),);
            Console.WriteLine(s.ToString());

            var owner = "sookeke";
            var repoName = "access-request-manager";
            var filePath = "0137d5/0137d5.yml";
            var branch = "main";
            //Repository repo = await github.Repository.Get(owner, repoName);
            //Branch branc = await github.Repository.Branch.Get(repo.Id, repo.DefaultBranch);
            //string sha = branc.Commit.Sha;

            var fileDetails = await github.Repository.Content.GetAllContentsByRef(owner, repoName, filePath, branch);   

            //Console.WriteLine(sha);
            if (fileDetails == null)
            {
                await github.Repository.Content.CreateFile(owner, repoName, filePath, new CreateFileRequest($"First commit for {filePath}", s.ToString(), branch));

            }
            else
            {
                await github.Repository.Content.UpdateFile(owner, repoName, filePath, new UpdateFileRequest($"My updated file", s.ToString(),fileDetails.First().Sha));

            }
            return  Ok(rb);
        }
    }
}